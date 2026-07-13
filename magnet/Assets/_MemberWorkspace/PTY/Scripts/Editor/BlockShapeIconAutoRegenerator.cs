using System.Linq;
using UnityEditor;

namespace PTY.Scripts.Editor
{
    /// <summary>
    /// ShapeBlock 프리팹(스킨 색상/스프라이트 정의처)이 저장될 때마다 모든 블록 아이콘을 자동으로 다시 찍는다.
    /// </summary>
    public sealed class BlockShapeIconAutoRegenerator : AssetPostprocessor
    {
        private const string ShapeBlockPrefabPath = "Assets/_MemberWorkspace/JTH/Prefabs/ShapeBlock.prefab";

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (!importedAssets.Contains(ShapeBlockPrefabPath))
            {
                return;
            }

            EditorApplication.delayCall += BlockShapeIconGenerator.GenerateAllIcons;
        }
    }
}
