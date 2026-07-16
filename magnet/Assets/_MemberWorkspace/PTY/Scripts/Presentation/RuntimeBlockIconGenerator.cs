using System.Collections.Generic;
using GameLib.EventChannelSystem;
using JTH.Scripts.Presentation;
using Magnet.Contracts.BlockShapes;
using Magnet.Contracts.BlockSkins;
using PMS.Scripts.Events;
using PTY.Scripts.Data;
using UnityEngine;

namespace PTY.Scripts.Presentation
{
    /// <summary>
    /// 스킨 변경 시(SkinChangedRequestEvent) 블록 아이콘을 런타임 카메라로 즉석 캡처해 재생성한다.
    /// BlockShapeIconGenerator(Editor 전용, PNG 굽기)와 달리 디스크에 저장하지 않고
    /// BlockShapeSO.icon을 메모리상에서만(SetIcon) 갱신한다. 빌드에서도 동작.
    /// </summary>
    public sealed class RuntimeBlockIconGenerator : MonoBehaviour
    {
        [SerializeField] private EventChannelSO skinEventChannel;
        [SerializeField] private ShapeBlock shapeBlockPrefab;
        [SerializeField] private BlockShapeSourceSO blockShapeSource;
        [SerializeField] private int iconSize = 256;
        [SerializeField] private float framePadding = 1.2f;

        private static readonly Vector3 RigPosition = new(10000f, 10000f, 0f);

        private IBlockSkin _currentSkin;
        private ShapeBlock _rigShapeBlock;
        private Camera _rigCamera;

        private void Awake()
        {
            Debug.Assert(skinEventChannel != null, "[RuntimeBlockIconGenerator] skinEventChannel is not assigned.", this);
            Debug.Assert(shapeBlockPrefab != null, "[RuntimeBlockIconGenerator] shapeBlockPrefab is not assigned.", this);
            Debug.Assert(blockShapeSource != null, "[RuntimeBlockIconGenerator] blockShapeSource is not assigned.", this);

            skinEventChannel.AddListener<SkinChangedEvent>(HandleSkinChanged);
            skinEventChannel.AddListener<SkinChangedRequestEvent>(HandleSkinChangedRequest);
        }

        private void OnDestroy()
        {
            skinEventChannel.RemoveListener<SkinChangedEvent>(HandleSkinChanged);
            skinEventChannel.RemoveListener<SkinChangedRequestEvent>(HandleSkinChangedRequest);

            DestroyRig();
        }

        private void HandleSkinChanged(SkinChangedEvent evt)
        {
            _currentSkin = evt.CurrentSkin;
        }

        private void HandleSkinChangedRequest(SkinChangedRequestEvent evt)
        {
            GenerateAllIcons();
            skinEventChannel.RaiseEvent(SkinEvents.SkinChangedResponseEvent);
        }

        private void GenerateAllIcons()
        {
            if (_currentSkin == null)
            {
                return;
            }

            EnsureRig();
            _rigShapeBlock.ApplySkin(_currentSkin);

            IReadOnlyList<IBlockShape> shapes = blockShapeSource.Shapes;
            for (int i = 0; i < shapes.Count; i++)
            {
                if (shapes[i] is BlockShapeSO shapeSo && shapeSo.CellOffsets.Count > 0)
                {
                    GenerateIcon(shapeSo);
                }
            }
        }

        private void GenerateIcon(BlockShapeSO shapeSo)
        {
            _rigShapeBlock.ShowCells(Vector2Int.zero, shapeSo.CellOffsets, sortingOrder: 0);

            Bounds bounds = ComputeBounds(_rigShapeBlock);
            if (bounds.size == Vector3.zero)
            {
                return;
            }

            FrameCamera(bounds);
            Texture2D texture = CaptureTexture();
            shapeSo.SetIcon(texture);
        }

        private void EnsureRig()
        {
            if (_rigShapeBlock != null)
            {
                return;
            }

            _rigShapeBlock = Instantiate(shapeBlockPrefab);
            _rigShapeBlock.transform.position = RigPosition;
            _rigShapeBlock.name = "RuntimeIconCapture_ShapeBlock";

            var cameraGo = new GameObject("RuntimeIconCapture_Camera");
            cameraGo.transform.position = RigPosition + new Vector3(0f, 0f, -10f);
            _rigCamera = cameraGo.AddComponent<Camera>();
            _rigCamera.orthographic = true;
            _rigCamera.clearFlags = CameraClearFlags.SolidColor;
            _rigCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            _rigCamera.nearClipPlane = 0.01f;
            _rigCamera.farClipPlane = 100f;
            _rigCamera.enabled = false; // Render()를 수동으로만 호출 — 매 프레임 자동 렌더 방지
        }

        private void DestroyRig()
        {
            if (_rigShapeBlock != null)
            {
                Destroy(_rigShapeBlock.gameObject);
                _rigShapeBlock = null;
            }

            if (_rigCamera != null)
            {
                Destroy(_rigCamera.gameObject);
                _rigCamera = null;
            }
        }

        private static Bounds ComputeBounds(ShapeBlock shapeBlock)
        {
            SpriteRenderer[] renderers = shapeBlock.GetComponentsInChildren<SpriteRenderer>();
            bool hasBounds = false;
            var bounds = new Bounds();

            foreach (SpriteRenderer spriteRenderer in renderers)
            {
                if (!spriteRenderer.gameObject.activeSelf)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = spriteRenderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(spriteRenderer.bounds);
                }
            }

            return bounds;
        }

        private void FrameCamera(Bounds bounds)
        {
            Vector3 position = _rigCamera.transform.position;
            position.x = bounds.center.x;
            position.y = bounds.center.y;
            _rigCamera.transform.position = position;

            _rigCamera.orthographicSize = Mathf.Max(bounds.extents.x, bounds.extents.y) * framePadding;
        }

        private Texture2D CaptureTexture()
        {
            var renderTexture = new RenderTexture(iconSize, iconSize, 24, RenderTextureFormat.ARGB32);
            _rigCamera.targetTexture = renderTexture;
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture.active = renderTexture;

            _rigCamera.Render();

            var texture = new Texture2D(iconSize, iconSize, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, iconSize, iconSize), 0, 0);
            texture.Apply();

            RenderTexture.active = previousActive;
            _rigCamera.targetTexture = null;
            renderTexture.Release();
            Destroy(renderTexture);

            return texture;
        }
    }
}
