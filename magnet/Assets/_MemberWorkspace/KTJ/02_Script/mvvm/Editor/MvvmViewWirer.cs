using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Mvvm.Editor
{
    public static class MvvmViewWirer
    {
        public static void Wire(MvvmBindingProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (profile.targetPrefab == null)
            {
                throw new InvalidOperationException("Target is not assigned.");
            }

            var viewType = FindType(profile.namespaceName, profile.viewClassName);
            if (viewType == null)
            {
                throw new InvalidOperationException($"Could not find generated view type: {profile.namespaceName}.{profile.viewClassName}. Let Unity compile after code generation, then try again.");
            }

            var assetPath = AssetDatabase.GetAssetPath(profile.targetPrefab);
            if (!string.IsNullOrEmpty(assetPath))
            {
                WirePrefabAsset(profile, viewType, assetPath);
            }
            else
            {
                WireSceneObject(profile, viewType, profile.targetPrefab);
            }
        }

        private static void WirePrefabAsset(MvvmBindingProfile profile, Type viewType, string assetPath)
        {
            var root = PrefabUtility.LoadPrefabContents(assetPath);
            try
            {
                WireRoot(profile, viewType, root);
                PrefabUtility.SaveAsPrefabAsset(root, assetPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void WireSceneObject(MvvmBindingProfile profile, Type viewType, GameObject root)
        {
            Undo.RegisterFullObjectHierarchyUndo(root, "Wire MVVM View");
            WireRoot(profile, viewType, root);
            EditorUtility.SetDirty(root);
        }

        private static void WireRoot(MvvmBindingProfile profile, Type viewType, GameObject root)
        {
            var view = root.GetComponent(viewType);
            if (view == null)
            {
                view = root.AddComponent(viewType);
            }

            var serializedObject = new SerializedObject(view);
            foreach (var binding in profile.bindings.Where(x => x.enabled))
            {
                var property = serializedObject.FindProperty(binding.fieldName);
                if (property == null)
                {
                    continue;
                }

                var child = FindTransform(root.transform, binding.objectPath);
                if (child == null)
                {
                    continue;
                }

                var component = child.GetComponents<Component>().FirstOrDefault(x => x != null && x.GetType().FullName == binding.componentType);
                if (component != null)
                {
                    property.objectReferenceValue = component;
                }
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Transform FindTransform(Transform root, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            var normalized = path.Replace("\\", "/");
            if (normalized == root.name)
            {
                return root;
            }

            var prefix = root.name + "/";
            if (normalized.StartsWith(prefix, StringComparison.Ordinal))
            {
                normalized = normalized.Substring(prefix.Length);
            }

            return root.Find(normalized);
        }

        private static Type FindType(string namespaceName, string className)
        {
            var fullName = string.IsNullOrWhiteSpace(namespaceName) ? className : namespaceName + "." + className;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }
    }
}
