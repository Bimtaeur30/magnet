using TMPro;
using UnityEditor;
using UnityEngine;

namespace Magnet.KTJ.Editor
{
    public sealed class ExtrudedTextShaderGUI : ShaderGUI
    {
        public override void OnGUI(MaterialEditor editor, MaterialProperty[] properties)
        {
            EditorGUILayout.HelpBox("Border Offset X/Y controls total depth. Extrusion / Total Shadow Offset controls the solid portion. Keep font atlas and metrics matched to the TMP font asset.", MessageType.Info);
            EditorGUI.BeginChangeCheck();
            base.OnGUI(editor, properties);
            if (!EditorGUI.EndChangeCheck()) return;
            foreach (var target in editor.targets)
            {
                var material = (Material)target;
                material.EnableKeyword("OUTLINE_ON");
                material.EnableKeyword("UNDERLAY_ON");
                material.DisableKeyword("UNDERLAY_INNER");
                TMPro_EventManager.ON_MATERIAL_PROPERTY_CHANGED(true, material);
                EditorUtility.SetDirty(material);
            }
        }
    }
}
