using TMPro;
using UnityEditor;
using UnityEngine;

namespace Magnet.KTJ.Editor
{
    public static class ExtrudedTextMaterialMenu
    {
        private const string ShaderName = "Magnet/KTJ/TMP Extruded Outline";

        [MenuItem("Tools/KTJ/Text/Create Extruded Blue Material")]
        private static void Blue() => Create(false);

        [MenuItem("Tools/KTJ/Text/Create Extruded Yellow Material")]
        private static void Yellow() => Create(true);

        private static void Create(bool yellow)
        {
            var text = Selection.activeGameObject != null
                ? Selection.activeGameObject.GetComponent<TMP_Text>() : null;
            if (text == null || text.fontSharedMaterial == null)
            {
                Debug.LogWarning("Select a GameObject with a TMP text component first.");
                return;
            }
            var shader = Shader.Find(ShaderName);
            if (shader == null || ShaderUtil.ShaderHasError(shader))
            {
                Debug.LogError("Extruded text shader is missing or has compilation errors.");
                return;
            }
            // Clone the font material: atlas, gradient scale and font metrics must match.
            var material = new Material(text.fontSharedMaterial) { shader = shader };
            material.EnableKeyword("OUTLINE_ON");
            material.EnableKeyword("UNDERLAY_ON");
            material.DisableKeyword("UNDERLAY_INNER");
            material.SetFloat("_FaceDilate", 0);
            material.SetFloat("_OutlineWidth", 0.22f);
            material.SetFloat("_OutlineSoftness", 0);
            material.SetColor("_OutlineColor", new Color(0.12f, 0.25f, 0.8f));
            material.SetColor("_FaceColor", yellow ? new Color(1, 0.91f, 0.14f) : new Color(0.9f, 0.98f, 1));
            material.SetColor("_FaceBottomColor", yellow ? new Color(1, 0.78f, 0.65f) : new Color(0.65f, 0.88f, 1));
            material.SetColor("_SideColor", new Color(0.075f, 0.06f, 0.32f));
            material.SetColor("_SideTopColor", new Color(0.12f, 0.2f, 0.62f));
            material.SetFloat("_ExtrusionFraction", 0.75f);
            material.SetFloat("_UnderlayOffsetX", 0.08f);
            material.SetFloat("_UnderlayOffsetY", -0.6f);
            material.SetFloat("_UnderlayDilate", 0.22f);
            material.SetFloat("_UnderlaySoftness", 0.12f);
            material.SetColor("_UnderlayColor", new Color(0.025f, 0.015f, 0.12f, 0.5f));
            material.SetFloat("_EdgeWidth", 0.035f);
            material.SetFloat("_EdgeStrength", 0.45f);
            material.SetColor("_EdgeLightColor", new Color(0.75f, 0.9f, 1));
            material.SetColor("_EdgeShadeColor", new Color(0.13f, 0.23f, 0.55f));
            material.SetVector("_LightDirection", new Vector4(-0.5f, 1, 0, 0));
            text.ForceMeshUpdate();
            var bounds = text.textBounds;
            material.SetVector("_FaceGradientRange", new Vector4(bounds.min.y, bounds.max.y, 0, 0));
            const string folder = "Assets/_MemberWorkspace/KTJ/04_Asset/Shaders";
            var path = AssetDatabase.GenerateUniqueAssetPath(folder + (yellow ? "/Extruded Yellow.mat" : "/Extruded Blue.mat"));
            AssetDatabase.CreateAsset(material, path);
            Undo.RecordObject(text, "Apply extruded text material");
            text.fontSharedMaterial = material;
            text.extraPadding = true;
            text.UpdateMeshPadding();
            text.SetMaterialDirty();
            EditorUtility.SetDirty(text);
            PrefabUtility.RecordPrefabInstancePropertyModifications(text);
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(material);
        }
    }
}
