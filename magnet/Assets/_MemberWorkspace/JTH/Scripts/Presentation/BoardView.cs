using JTH.Scripts.Data;
using UnityEngine;

namespace JTH.Scripts.Presentation
{
    public sealed class BoardView : MonoBehaviour
    {
        [Tooltip("격자 크기·색상 등 보드 시각화 설정")]
        [SerializeField] private BoardConfigSO config;
        [SerializeField] private float lineWidth = 0.04f;
        [SerializeField] private LineRenderer linePrefab;
        
        private static Material _sharedLineMaterial;
        
        private void Start()
        {
            Debug.Assert(config != null, "[BoardView] BoardConfigSO is not assigned.", this);
        
            BuildBoardLines();
        }

        private void BuildBoardLines()
        {
            float lineLength = config.CellCount * config.CellSize;
            int cellCount = config.CellCount;
            float cellSize = config.CellSize;

            for (int i = 0; i <= cellCount; i++)
            {
                LineRenderer width = Instantiate(linePrefab, transform);
                width.positionCount = 2;
                width.SetPosition(0, new Vector3(0, cellSize * i, 0));
                width.SetPosition(1, new Vector3(lineLength, cellSize * i, 0));
                width.startWidth = lineWidth;
                width.endWidth = lineWidth;
                
                LineRenderer length = Instantiate(linePrefab, transform);
                length.positionCount = 2;
                length.SetPosition(0, new Vector3(cellSize * i, 0, 0));
                length.SetPosition(1, new Vector3(cellSize * i, lineLength, 0));
                length.startWidth = lineWidth;
                length.endWidth = lineWidth;
            }
        }
    }
}
