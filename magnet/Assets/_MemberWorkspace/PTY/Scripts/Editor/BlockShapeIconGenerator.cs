using System.IO;
using JTH.Scripts.Presentation;
using PTY.Scripts.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PTY.Scripts.Editor
{
    /// <summary>
    /// 프로젝트 내 모든 BlockShapeSO를 임시 프리뷰 씬에서 카메라로 촬영해 아이콘 PNG를 생성하고,
    /// 각 SO의 icon 필드에 할당한다. 카메라·ShapeBlock 인스턴스는 순회 내내 재사용한다.
    /// </summary>
    public static class BlockShapeIconGenerator
    {
        private const string IconFolder = "Assets/_MemberWorkspace/PTY/Sprites/BlockIcons";
        private const string ShapeBlockPrefabPath = "Assets/_MemberWorkspace/JTH/Prefabs/ShapeBlock.prefab";
        private const int IconSize = 256;
        private const float FramePadding = 1.2f;

        public static void GenerateAllIcons()
        {
            var shapePrefab = AssetDatabase.LoadAssetAtPath<ShapeBlock>(ShapeBlockPrefabPath);
            if (shapePrefab == null)
            {
                Debug.LogError($"[BlockShapeIconGenerator] ShapeBlock prefab not found at {ShapeBlockPrefabPath}");
                return;
            }

            var shapeGuids = AssetDatabase.FindAssets("t:BlockShapeSO");
            if (shapeGuids.Length == 0)
            {
                Debug.LogWarning("[BlockShapeIconGenerator] No BlockShapeSO assets found.");
                return;
            }

            if (!AssetDatabase.IsValidFolder(IconFolder))
            {
                Directory.CreateDirectory(IconFolder);
                AssetDatabase.Refresh();
            }

            var scene = EditorSceneManager.NewPreviewScene();
            Camera camera = null;
            ShapeBlock shapeInstance = null;

            try
            {
                camera = CreateCamera(scene);
                shapeInstance = Object.Instantiate(shapePrefab);
                SceneManager.MoveGameObjectToScene(shapeInstance.gameObject, scene);

                var generated = 0;
                foreach (var guid in shapeGuids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var shapeSo = AssetDatabase.LoadAssetAtPath<BlockShapeSO>(path);
                    if (shapeSo == null || shapeSo.CellOffsets.Count == 0)
                    {
                        continue;
                    }

                    GenerateIcon(shapeSo, shapeInstance, camera);
                    generated++;
                }

                AssetDatabase.SaveAssets();
                Debug.Log($"[BlockShapeIconGenerator] {generated}개 블록 아이콘 생성 완료.");
            }
            finally
            {
                if (shapeInstance != null)
                {
                    Object.DestroyImmediate(shapeInstance.gameObject);
                }

                if (camera != null)
                {
                    Object.DestroyImmediate(camera.gameObject);
                }

                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        private static Camera CreateCamera(Scene scene)
        {
            var cameraGo = new GameObject("IconCaptureCamera");
            SceneManager.MoveGameObjectToScene(cameraGo, scene);

            var camera = cameraGo.AddComponent<Camera>();
            camera.scene = scene;
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 100f;
            camera.transform.position = new Vector3(0f, 0f, -10f);

            return camera;
        }

        private static void GenerateIcon(BlockShapeSO shapeSo, ShapeBlock shapeInstance, Camera camera)
        {
            shapeInstance.ShowCells(Vector2Int.zero, shapeSo.CellOffsets, sortingOrder: 0);

            var bounds = ComputeBounds(shapeInstance);
            if (bounds.size == Vector3.zero)
            {
                return;
            }

            FrameCamera(camera, bounds);

            var texture = CaptureTexture(camera);
            var assetPath = $"{IconFolder}/{SanitizeFileName(shapeSo.ShapeId)}_Icon.png";
            File.WriteAllBytes(assetPath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            ConfigureImporter(assetPath);

            var iconTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            var serializedShape = new SerializedObject(shapeSo);
            serializedShape.FindProperty("icon").objectReferenceValue = iconTexture;
            serializedShape.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(shapeSo);
        }

        private static string SanitizeFileName(string name)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new char[name.Length];
            for (var i = 0; i < name.Length; i++)
            {
                var c = name[i];
                sanitized[i] = System.Array.IndexOf(invalidChars, c) >= 0 ? '_' : c;
            }

            return new string(sanitized);
        }

        private static Bounds ComputeBounds(ShapeBlock shapeInstance)
        {
            var renderers = shapeInstance.GetComponentsInChildren<SpriteRenderer>();
            var hasBounds = false;
            var bounds = new Bounds();

            foreach (var spriteRenderer in renderers)
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

        private static void FrameCamera(Camera camera, Bounds bounds)
        {
            var position = camera.transform.position;
            position.x = bounds.center.x;
            position.y = bounds.center.y;
            camera.transform.position = position;

            // 렌더 타겟이 항상 IconSize x IconSize 정사각형(aspect 1)이므로
            // camera.aspect(타겟 지정 전엔 Game 뷰 비율)에 의존하지 않고 x/y extent를 그대로 비교한다.
            camera.orthographicSize = Mathf.Max(bounds.extents.x, bounds.extents.y) * FramePadding;
        }

        private static Texture2D CaptureTexture(Camera camera)
        {
            var renderTexture = new RenderTexture(IconSize, IconSize, 24, RenderTextureFormat.ARGB32);
            camera.targetTexture = renderTexture;
            var previousActive = RenderTexture.active;
            RenderTexture.active = renderTexture;

            camera.Render();

            var texture = new Texture2D(IconSize, IconSize, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, IconSize, IconSize), 0, 0);
            texture.Apply();

            RenderTexture.active = previousActive;
            camera.targetTexture = null;
            renderTexture.Release();
            Object.DestroyImmediate(renderTexture);

            return texture;
        }

        private static void ConfigureImporter(string assetPath)
        {
            if (AssetImporter.GetAtPath(assetPath) is not TextureImporter importer)
            {
                return;
            }

            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }
    }
}
