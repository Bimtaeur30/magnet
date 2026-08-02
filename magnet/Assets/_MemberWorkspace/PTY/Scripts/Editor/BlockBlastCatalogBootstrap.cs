using System.Collections.Generic;
using System.IO;
using Magnet.Core.SO.Block;
using UnityEditor;
using UnityEngine;

namespace PTY.Scripts.Editor
{
    /// <summary>
    /// Block Blast 표준 블록 전부를 BlockShapes 폴더에 생성하고 BlockShapeSource에 등록한다.
    /// edge polyomino + 코너 접속 대각선(pseudo-polyomino) 포함.
    /// 스폰·배치 계산 시 0/90/180/270 회전 적용 — 대각선은 Diag2/Diag3 각 1종만 등록.
    /// </summary>
    public static class BlockBlastCatalogBootstrap
    {
        private const string ShapeFolder = "Assets/_MemberWorkspace/PTY/ScriptableObjects/BlockShapes";
        private const string SourcePath = "Assets/_MemberWorkspace/PTY/ScriptableObjects/BlockShapeSource.asset";
        private const string IconFolder = "Assets/_MemberWorkspace/PTY/Sprites/BlockIcons";

        [MenuItem("Magnet/Block Blast/Create All Block Shapes")]
        public static void CreateAllBlockShapes()
        {
            EnsureFolder(ShapeFolder);

            var definitions = GetBlockBlastDefinitions();
            var created = new List<BlockShapeSO>(definitions.Count);

            foreach (var def in definitions)
            {
                var shape = CreateOrUpdateShape(def);
                created.Add(shape);
            }

            UpdateBlockShapeSource(created);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[BlockBlastCatalogBootstrap] {created.Count}개 BlockShapeSO 생성·갱신, BlockShapeSource 등록 완료.");
        }

        private static IReadOnlyList<ShapeDefinition> GetBlockBlastDefinitions()
        {
            return new[]
            {
                Def("1x1", Cells(0, 0)),
                Def("1x2", Cells(0, 0, 1, 0), "1x2-Shape_Icon"),
                Def("1x3", Cells(0, 0, 1, 0, 2, 0), "3x1-Shape_Icon"),
                Def("1x4", Cells(0, 0, 1, 0, 2, 0, 3, 0), "4_1_Icon"),
                Def("1x5", Cells(0, 0, 1, 0, 2, 0, 3, 0, 4, 0)),
                Def("2x2", Cells(0, 0, 1, 0, 0, 1, 1, 1), "2x2-Shape_Icon"),
                Def("3x2", Cells(0, 0, 1, 0, 2, 0, 0, 1, 1, 1, 2, 1), "3x2-Shape_Icon"),
                Def("3x3", Cells(
                    0, 0, 1, 0, 2, 0,
                    0, 1, 1, 1, 2, 1,
                    0, 2, 1, 2, 2, 2), "3x3-Shape_Icon"),
                Def("L3", Cells(0, 0, 1, 0, 0, 1), "Small_L_-Shape_Icon"),
                Def("L4", Cells(0, 0, 0, 1, 1, 1, 2, 1), "L-Shape_Icon"),
                Def("J4", Cells(2, 0, 0, 1, 1, 1, 2, 1), "r-Shape_Icon"),
                Def("T4", Cells(0, 0, 1, 0, 2, 0, 1, 1)),
                Def("S4", Cells(1, 0, 2, 0, 0, 1, 1, 1)),
                Def("Z4", Cells(0, 0, 1, 0, 1, 1, 2, 1), "Z-Shape_Icon"),
                Def("L3x3", Cells(0, 0, 1, 0, 2, 0, 0, 1, 0, 2), "L_-Shape_Icon"),
                Def("Diag2", Cells(0, 0, 1, 1)),
                Def("Diag3", Cells(0, 0, 1, 1, 2, 2)),
            };
        }

        private static ShapeDefinition Def(string id, List<Vector2Int> cells, string iconFileName = null)
        {
            return new ShapeDefinition(id, cells, iconFileName);
        }

        private static List<Vector2Int> Cells(params int[] xy)
        {
            var list = new List<Vector2Int>(xy.Length / 2);
            for (var i = 0; i < xy.Length; i += 2)
            {
                list.Add(new Vector2Int(xy[i], xy[i + 1]));
            }

            return list;
        }

        private static BlockShapeSO CreateOrUpdateShape(ShapeDefinition def)
        {
            var path = $"{ShapeFolder}/{def.Id}.asset";
            var shape = AssetDatabase.LoadAssetAtPath<BlockShapeSO>(path);
            var isNew = shape == null;

            if (isNew)
            {
                shape = ScriptableObject.CreateInstance<BlockShapeSO>();
                AssetDatabase.CreateAsset(shape, path);
            }

            var serialized = new SerializedObject(shape);
            serialized.FindProperty("shapeId").stringValue = def.Id;

            var offsetsProperty = serialized.FindProperty("cellOffsets");
            offsetsProperty.arraySize = def.Cells.Count;
            for (var i = 0; i < def.Cells.Count; i++)
            {
                offsetsProperty.GetArrayElementAtIndex(i).vector2IntValue = def.Cells[i];
            }

            if (!string.IsNullOrEmpty(def.IconFileName))
            {
                var icon = LoadIconTexture(def.IconFileName);
                if (icon != null)
                {
                    serialized.FindProperty("icon").objectReferenceValue = icon;
                }
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(shape);
            return shape;
        }

        private static Texture2D LoadIconTexture(string fileName)
        {
            var path = $"{IconFolder}/{fileName}.png";
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static void UpdateBlockShapeSource(IReadOnlyList<BlockShapeSO> shapes)
        {
            var source = AssetDatabase.LoadAssetAtPath<BlockShapeSourceSO>(SourcePath);
            if (source == null)
            {
                Debug.LogError($"[BlockBlastCatalogBootstrap] BlockShapeSource not found: {SourcePath}");
                return;
            }

            var serialized = new SerializedObject(source);
            var shapesProperty = serialized.FindProperty("shapes");
            shapesProperty.arraySize = shapes.Count;
            for (var i = 0; i < shapes.Count; i++)
            {
                shapesProperty.GetArrayElementAtIndex(i).objectReferenceValue = shapes[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(source);
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            var leaf = Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, leaf);
        }

        private readonly struct ShapeDefinition
        {
            public string Id { get; }
            public List<Vector2Int> Cells { get; }
            public string IconFileName { get; }

            public ShapeDefinition(string id, List<Vector2Int> cells, string iconFileName)
            {
                Id = id;
                Cells = cells;
                IconFileName = iconFileName;
            }
        }
    }
}
