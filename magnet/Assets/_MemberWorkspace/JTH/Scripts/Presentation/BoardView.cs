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

            const string widthStr = "width";
            const string lengthStr = "length";
            
            for (int i = 1; i < cellCount; i++)
            {
                LineRenderer width = InstantiateLine(widthStr, 2);
                width.SetPosition(0, new Vector3(0, cellSize * i, 0));
                width.SetPosition(1, new Vector3(lineLength, cellSize * i, 0));
                
                LineRenderer length = InstantiateLine(lengthStr, 2);
                length.SetPosition(0, new Vector3(cellSize * i, 0, 0));
                length.SetPosition(1, new Vector3(cellSize * i, lineLength, 0));
            }
            
            LineRenderer square = InstantiateLine("square", 4);
            square.loop = true;
            square.SetPosition(0, new Vector3(0, 0, 0));
            square.SetPosition(1, new Vector3(lineLength, 0, 0));
            square.SetPosition(2, new Vector3(lineLength, lineLength, 0));
            square.SetPosition(3, new Vector3(0, lineLength, 0));
        }

        private LineRenderer InstantiateLine(string lineName, int positionCount)
        {
            LineRenderer line = Instantiate(linePrefab, transform);
            line.gameObject.name = lineName;
            line.positionCount = positionCount;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;

            return line;
        }
    }
}
