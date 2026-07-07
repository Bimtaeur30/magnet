using System.Collections.Generic;
using PTY.Scripts.Data;
using UnityEditor;
using UnityEngine;

namespace PTY.Scripts.Editor
{
    public sealed class BlockShapeEditorWindow : EditorWindow
    {
        private const string SaveFolder = "Assets/_MemberWorkspace/PTY/ScriptableObjects/BlockShapes";
        private const float MinCellSize = 8f;
        private static readonly Color ActiveCellColor = new(0.35f, 0.75f, 1f, 1f);

        private readonly HashSet<Vector2Int> activeCells = new();

        private int width = 3;
        private int height = 3;
        private string shapeId = "";
        private BlockShapeSO shapeToLoad;

        [MenuItem("Magnet/Block Shape Editor")]
        private static void Open()
        {
            GetWindow<BlockShapeEditorWindow>("Block Shape Editor");
        }

        private void OnGUI()
        {
            DrawLoadControls();

            EditorGUILayout.Space();
            width = Mathf.Clamp(EditorGUILayout.IntField("Width", width), 1, 20);
            height = Mathf.Clamp(EditorGUILayout.IntField("Height", height), 1, 20);

            activeCells.RemoveWhere(cell => cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height);

            EditorGUILayout.Space();
            DrawGrid();

            EditorGUILayout.Space();
            DrawPreview();

            EditorGUILayout.Space();
            DrawSaveControls();
        }

        private void DrawLoadControls()
        {
            EditorGUILayout.BeginHorizontal();

            shapeToLoad = (BlockShapeSO)EditorGUILayout.ObjectField(
                "Load Shape", shapeToLoad, typeof(BlockShapeSO), false);

            using (new EditorGUI.DisabledScope(shapeToLoad == null))
            {
                if (GUILayout.Button("Load", GUILayout.Width(60)))
                {
                    LoadShape();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void LoadShape()
        {
            activeCells.Clear();

            var maxX = 0;
            var maxY = 0;
            foreach (var offset in shapeToLoad.CellOffsets)
            {
                activeCells.Add(offset);
                maxX = Mathf.Max(maxX, offset.x);
                maxY = Mathf.Max(maxY, offset.y);
            }

            width = Mathf.Clamp(maxX + 1, 1, 20);
            height = Mathf.Clamp(maxY + 1, 1, 20);
            shapeId = shapeToLoad.ShapeId;
        }

        private void DrawGrid()
        {
            var gridRect = GUILayoutUtility.GetRect(
                GUIContent.none, GUIStyle.none,
                GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            var cellSize = Mathf.Max(MinCellSize, Mathf.Min(gridRect.width / width, gridRect.height / height));
            var originX = gridRect.x + (gridRect.width - cellSize * width) * 0.5f;
            var originY = gridRect.y + (gridRect.height - cellSize * height) * 0.5f;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var cell = new Vector2Int(x, y);
                    var isActive = activeCells.Contains(cell);

                    var cellRect = new Rect(
                        originX + x * cellSize,
                        originY + (height - 1 - y) * cellSize,
                        cellSize, cellSize);

                    var previousColor = GUI.backgroundColor;
                    if (isActive) GUI.backgroundColor = ActiveCellColor;

                    if (GUI.Button(cellRect, GUIContent.none))
                    {
                        if (isActive) activeCells.Remove(cell);
                        else activeCells.Add(cell);
                    }

                    GUI.backgroundColor = previousColor;
                }
            }
        }

        private void DrawPreview()
        {
            foreach (var offset in GetNormalizedOffsets())
            {
                EditorGUILayout.LabelField(offset.ToString());
            }
        }

        private void DrawSaveControls()
        {
            shapeId = EditorGUILayout.TextField("Shape Id", shapeId);

            var canSave = !string.IsNullOrWhiteSpace(shapeId) && activeCells.Count > 0;
            if (!canSave)
            {
                EditorGUILayout.HelpBox("Shape Id를 입력하고 최소 한 칸 이상 선택해야 저장할 수 있습니다.", MessageType.Info);
            }

            EditorGUILayout.BeginHorizontal();

            using (new EditorGUI.DisabledScope(!canSave))
            {
                if (GUILayout.Button("Save"))
                {
                    SaveShape();
                }
            }

            if (GUILayout.Button("Clear"))
            {
                activeCells.Clear();
                shapeId = "";
            }

            EditorGUILayout.EndHorizontal();
        }

        private void SaveShape()
        {
            if (!AssetDatabase.IsValidFolder(SaveFolder))
            {
                Debug.LogError($"Save folder not found: {SaveFolder}");
                return;
            }

            var offsets = GetNormalizedOffsets();

            var shape = CreateInstance<BlockShapeSO>();
            var serializedShape = new SerializedObject(shape);
            serializedShape.FindProperty("shapeId").stringValue = shapeId;

            var offsetsProperty = serializedShape.FindProperty("cellOffsets");
            offsetsProperty.arraySize = offsets.Count;
            for (var i = 0; i < offsets.Count; i++)
            {
                offsetsProperty.GetArrayElementAtIndex(i).vector2IntValue = offsets[i];
            }

            serializedShape.ApplyModifiedPropertiesWithoutUndo();

            var path = AssetDatabase.GenerateUniqueAssetPath($"{SaveFolder}/{shapeId}.asset");
            AssetDatabase.CreateAsset(shape, path);
            AssetDatabase.SaveAssets();

            Selection.activeObject = shape;
            EditorGUIUtility.PingObject(shape);
        }

        private List<Vector2Int> GetNormalizedOffsets()
        {
            var offsets = new List<Vector2Int>(activeCells.Count);
            if (activeCells.Count == 0) return offsets;

            var minX = int.MaxValue;
            var minY = int.MaxValue;
            foreach (var cell in activeCells)
            {
                minX = Mathf.Min(minX, cell.x);
                minY = Mathf.Min(minY, cell.y);
            }

            foreach (var cell in activeCells)
            {
                offsets.Add(new Vector2Int(cell.x - minX, cell.y - minY));
            }

            offsets.Sort((a, b) => a.y != b.y ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));
            return offsets;
        }
    }
}
