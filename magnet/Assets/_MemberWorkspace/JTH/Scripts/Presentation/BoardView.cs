using JTH.Scripts.Data;
using JTH.Scripts.Domain;
using UnityEngine;

namespace JTH.Scripts.Presentation
{
    /// <summary>
    /// 보드 격자·자석 축 시각화. 이 Transform이 보드 로컬 공간의 월드 원점이다.
    /// Presentation은 격자 좌표를 <see cref="GridToLocal"/> / <see cref="SetAtBoardLocal"/>로 배치한다.
    /// </summary>
    public sealed class BoardView : MonoBehaviour
    {
        [Tooltip("격자 크기·색상 등 보드 시각화 설정")]
        [SerializeField] private BoardConfigSO config;
        [Tooltip("격자·자석 축 LineRenderer의 부모 Transform. 비우면 자동 생성")]
        [SerializeField] private Transform linesRoot;
        [SerializeField] private float lineWidth = 0.04f;

        private static Material _sharedLineMaterial;

        /// <summary>격자 → 보드 로컬 (자석 = 0,0).</summary>
        public Vector2 GridToLocal(int gridX, int gridY, float cellSize)
        {
            return BoardCoordinates.GridToWorld(gridX, gridY, cellSize);
        }

        /// <summary>월드 위치 → 보드 로컬 XY.</summary>
        public Vector2 WorldToBoardLocal(Vector3 world)
        {
            Vector3 local = transform.InverseTransformPoint(world);
            return new Vector2(local.x, local.y);
        }

        public float WorldToBoardLocalX(Vector3 world) => WorldToBoardLocal(world).x;

        /// <summary>보드 로컬 좌표로 <paramref name="target"/>을 배치한다. 중간 부모 오프셋을 보정한다.</summary>
        public void SetAtBoardLocal(Transform target, Vector2 boardLocal)
        {
            Vector3 world = transform.TransformPoint(new Vector3(boardLocal.x, boardLocal.y, 0f));
            if (target.parent == null)
            {
                target.position = world;
                return;
            }

            target.localPosition = target.parent.InverseTransformPoint(world);
        }

        private void Start()
        {
            Debug.Assert(config != null, "[BoardView] BoardConfigSO is not assigned.", this);

            BuildBoardLines();
        }

        private void BuildBoardLines()
        {
            EnsureLinesRoot();
            ClearLines();

            int half = config.CellsPerSide;
            float cellSize = config.CellSize;
            float min = (-half - 0.5f) * cellSize;
            float max = (half + 0.5f) * cellSize;
            int lineCount = config.BoardSize + 1;

            var gridRoot = CreateChild("Grid");
            for (int i = 0; i < lineCount; i++)
            {
                float t = min + i * cellSize;
                AddLineSegment(
                    gridRoot,
                    config.CellColor,
                    new Vector3(t, min, 0f),
                    new Vector3(t, max, 0f));
                AddLineSegment(
                    gridRoot,
                    config.CellColor,
                    new Vector3(min, t, 0f),
                    new Vector3(max, t, 0f));
            }

            float magnetHalf = cellSize * 0.5f;
            var magnetRoot = CreateChild("MagnetAxis");
            AddLineLoop(
                magnetRoot,
                config.MagnetAxisColor,
                sortingOrder: 1,
                new Vector3(-magnetHalf, -magnetHalf, 0f),
                new Vector3(magnetHalf, -magnetHalf, 0f),
                new Vector3(magnetHalf, magnetHalf, 0f),
                new Vector3(-magnetHalf, magnetHalf, 0f));
        }

        private void EnsureLinesRoot()
        {
            if (linesRoot == null)
            {
                var root = new GameObject("Lines");
                root.transform.SetParent(transform, false);
                linesRoot = root.transform;
            }
        }

        private Transform CreateChild(string childName)
        {
            var child = new GameObject(childName);
            child.transform.SetParent(linesRoot, false);
            return child.transform;
        }

        private void ClearLines()
        {
            for (int i = linesRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(linesRoot.GetChild(i).gameObject);
            }
        }

        private void AddLineSegment(Transform parent, Color color, Vector3 start, Vector3 end, int sortingOrder = 0)
        {
            var lineGo = new GameObject("Line");
            lineGo.transform.SetParent(parent, false);
            ConfigureLine(lineGo.AddComponent<LineRenderer>(), color, sortingOrder, false, start, end);
        }

        private void AddLineLoop(Transform parent, Color color, int sortingOrder, params Vector3[] corners)
        {
            var lineGo = new GameObject("Loop");
            lineGo.transform.SetParent(parent, false);
            ConfigureLine(lineGo.AddComponent<LineRenderer>(), color, sortingOrder, true, corners);
        }

        private void ConfigureLine(
            LineRenderer lineRenderer,
            Color color,
            int sortingOrder,
            bool closedLoop,
            params Vector3[] points)
        {
            lineRenderer.useWorldSpace = false;
            lineRenderer.loop = closedLoop;
            lineRenderer.positionCount = points.Length;
            for (int i = 0; i < points.Length; i++)
            {
                lineRenderer.SetPosition(i, points[i]);
            }

            lineRenderer.widthMultiplier = lineWidth;
            lineRenderer.numCapVertices = 0;
            lineRenderer.numCornerVertices = 0;
            lineRenderer.material = GetLineMaterial();
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.sortingOrder = sortingOrder;
            lineRenderer.alignment = LineAlignment.View;
        }

        private static Material GetLineMaterial()
        {
            _sharedLineMaterial ??= new Material(
                Shader.Find("Sprites/Default")
                ?? Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
            return _sharedLineMaterial;
        }
    }
}
