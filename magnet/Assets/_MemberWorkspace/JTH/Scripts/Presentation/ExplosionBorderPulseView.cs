using Cysharp.Threading.Tasks;
using JTH.Scripts.Data;
using LitMotion;
using UnityEngine;
namespace JTH.Scripts.Presentation
{
    /// <summary>
    /// N×N 클리어 시 바깥 테두리 1개 직사각형을 키우며 펄스한다.
    /// LineRenderer loop(4점) + Transform scale(1→Peak), alpha는 동일 t·별도 Ease.
    /// </summary>
    public sealed class ExplosionBorderPulseView : MonoBehaviour
    {
        private static Material _sharedLineMaterial;

        private LineRenderer _lineRenderer;
        private MotionHandle _motionHandle;
        private Color _baseColor;

        public static UniTask PlayAsync(
            BoardView boardView,
            int squareSize,
            BoardConfigSO boardConfig,
            ExplosionBorderConfigSO config)
        {
            if (boardView == null || boardConfig == null || config == null || squareSize < 3)
            {
                return UniTask.CompletedTask;
            }

            var go = new GameObject("ExplosionBorderPulse");
            go.transform.SetParent(boardView.transform, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            var view = go.AddComponent<ExplosionBorderPulseView>();
            return view.PlayInternalAsync(squareSize, boardConfig, config);
        }

        private async UniTask PlayInternalAsync(
            int squareSize,
            BoardConfigSO boardConfig,
            ExplosionBorderConfigSO config)
        {
            float baseSide = squareSize * boardConfig.CellSize;

            _baseColor = config.Color;
            SetupLine(config.LineWidth, config.SortingOrder);
            transform.localScale = new Vector3(baseSide, baseSide, 1f);

            float duration = config.Duration;
            if (duration <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            var completion = new UniTaskCompletionSource();
            float peakScale = config.PeakScale;
            float maxAlpha = config.MaxAlpha;
            Ease ease = config.Ease;

            _motionHandle = LMotion.Create(0f, 1f, duration)
                .WithOnComplete(() => completion.TrySetResult())
                .Bind(t =>
                {
                    float sizeT = EaseUtility.Evaluate(t, ease);
                    float scaleMultiplier = Mathf.Lerp(1f, peakScale, sizeT);
                    transform.localScale = new Vector3(baseSide * scaleMultiplier, baseSide * scaleMultiplier, 1f);

                    float alphaPulse = Pulse01(EaseUtility.Evaluate(t, ease));
                    Color color = _baseColor;
                    color.a = _baseColor.a * maxAlpha * alphaPulse;
                    _lineRenderer.startColor = color;
                    _lineRenderer.endColor = color;
                });

            await completion.Task;
            Destroy(gameObject);
        }

        private void SetupLine(float lineWidth, int sortingOrder)
        {
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
            _lineRenderer.useWorldSpace = false;
            _lineRenderer.loop = true;
            _lineRenderer.positionCount = 4;
            _lineRenderer.SetPosition(0, new Vector3(-0.5f, -0.5f, 0f));
            _lineRenderer.SetPosition(1, new Vector3(0.5f, -0.5f, 0f));
            _lineRenderer.SetPosition(2, new Vector3(0.5f, 0.5f, 0f));
            _lineRenderer.SetPosition(3, new Vector3(-0.5f, 0.5f, 0f));
            _lineRenderer.widthMultiplier = lineWidth;
            _lineRenderer.numCapVertices = 0;
            _lineRenderer.numCornerVertices = 0;
            _lineRenderer.material = GetLineMaterial();
            _lineRenderer.startColor = Color.clear;
            _lineRenderer.endColor = Color.clear;
            _lineRenderer.sortingOrder = sortingOrder;
            _lineRenderer.alignment = LineAlignment.View;
        }

        private static float Pulse01(float easedT)
        {
            return 1f - Mathf.Abs(2f * easedT - 1f);
        }

        private static Material GetLineMaterial()
        {
            _sharedLineMaterial ??= new Material(
                Shader.Find("Sprites/Default")
                ?? Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
            return _sharedLineMaterial;
        }

        private void OnDestroy()
        {
            if (_motionHandle.IsActive())
            {
                _motionHandle.Cancel();
            }
        }
    }
}
