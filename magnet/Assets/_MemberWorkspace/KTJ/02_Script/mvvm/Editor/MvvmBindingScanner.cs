using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Mvvm.Editor
{
    public static class MvvmBindingScanner
    {
        public static List<MvvmBindingEntry> Scan(GameObject root)
        {
            var result = new List<MvvmBindingEntry>();
            if (root == null)
            {
                return result;
            }

            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                var path = GetPath(root.transform, transform);
                foreach (var component in transform.GetComponents<Component>())
                {
                    if (component == null)
                    {
                        continue;
                    }

                    AddCandidate(result, path, transform.name, component.GetType().FullName);
                }
            }

            return result
                .GroupBy(x => x.objectPath + "|" + x.componentType + "|" + x.targetProperty)
                .Select(x => x.First())
                .ToList();
        }

        private static void AddCandidate(List<MvvmBindingEntry> entries, string path, string objectName, string componentType)
        {
            if (componentType == "TMPro.TMP_Text" || componentType == "TMPro.TextMeshProUGUI" || componentType == "TMPro.TextMeshPro" || componentType == "UnityEngine.UI.Text")
            {
                entries.Add(Create(path, componentType, "text", MvvmBindingMode.OneWay, GuessPropertyName(objectName, "Text"), "string"));
                AddGraphicCandidates(entries, path, objectName, componentType);
            }
            else if (componentType == "UnityEngine.UI.Button")
            {
                entries.Add(Create(path, componentType, "onClick", MvvmBindingMode.Command, GuessCommandName(objectName), "Mvvm.IRelayCommand"));
                AddSelectableCandidates(entries, path, objectName, componentType);
            }
            else if (componentType == "UnityEngine.UI.Slider")
            {
                entries.Add(Create(path, componentType, "value", MvvmBindingMode.TwoWay, GuessPropertyName(objectName, "Slider"), "float"));
                entries.Add(Create(path, componentType, "minValue", MvvmBindingMode.OneWay, GuessPropertyName(objectName, "Slider") + "MinValue", "float"));
                entries.Add(Create(path, componentType, "maxValue", MvvmBindingMode.OneWay, GuessPropertyName(objectName, "Slider") + "MaxValue", "float"));
                AddSelectableCandidates(entries, path, objectName, componentType);
            }
            else if (componentType == "UnityEngine.UI.Toggle")
            {
                entries.Add(Create(path, componentType, "isOn", MvvmBindingMode.TwoWay, GuessPropertyName(objectName, "Toggle"), "bool"));
                AddSelectableCandidates(entries, path, objectName, componentType);
            }
            else if (componentType == "UnityEngine.UI.InputField" || componentType == "TMPro.TMP_InputField")
            {
                entries.Add(Create(path, componentType, "text", MvvmBindingMode.TwoWay, GuessPropertyName(objectName, "Input"), "string"));
                AddSelectableCandidates(entries, path, objectName, componentType);
            }
            else if (componentType == "UnityEngine.UI.Image")
            {
                entries.Add(Create(path, componentType, "sprite", MvvmBindingMode.OneWay, GuessPropertyName(objectName, "Image"), "UnityEngine.Sprite"));
                AddGraphicCandidates(entries, path, objectName, componentType);
            }
            else if (componentType == "UnityEngine.UI.RawImage")
            {
                entries.Add(Create(path, componentType, "texture", MvvmBindingMode.OneWay, GuessPropertyName(objectName, "Image") + "Texture", "UnityEngine.Texture"));
                AddGraphicCandidates(entries, path, objectName, componentType);
            }
            else if (componentType == "UnityEngine.CanvasGroup")
            {
                entries.Add(Create(path, componentType, "alpha", MvvmBindingMode.OneWay, GuessPropertyName(objectName, "Group") + "Alpha", "float"));
                entries.Add(Create(path, componentType, "interactable", MvvmBindingMode.OneWay, GuessPropertyName(objectName, "Group") + "Interactable", "bool"));
                entries.Add(Create(path, componentType, "blocksRaycasts", MvvmBindingMode.OneWay, GuessPropertyName(objectName, "Group") + "BlocksRaycasts", "bool"));
            }
            else if (componentType == "UnityEngine.RectTransform")
            {
                var name = GuessPropertyName(objectName, "RectTransform");
                entries.Add(Create(path, componentType, "anchoredPosition", MvvmBindingMode.OneWay, name + "Position", "UnityEngine.Vector2"));
                entries.Add(Create(path, componentType, "anchoredPosition.x", MvvmBindingMode.OneWay, name + "X", "float"));
                entries.Add(Create(path, componentType, "anchoredPosition.y", MvvmBindingMode.OneWay, name + "Y", "float"));
                entries.Add(Create(path, componentType, "sizeDelta", MvvmBindingMode.OneWay, name + "Size", "UnityEngine.Vector2"));
                entries.Add(Create(path, componentType, "sizeDelta.x", MvvmBindingMode.OneWay, name + "Width", "float"));
                entries.Add(Create(path, componentType, "sizeDelta.y", MvvmBindingMode.OneWay, name + "Height", "float"));
                entries.Add(Create(path, componentType, "localEulerAngles.z", MvvmBindingMode.OneWay, name + "RotationZ", "float"));
                entries.Add(Create(path, componentType, "localScale", MvvmBindingMode.OneWay, name + "Scale", "UnityEngine.Vector3"));
                entries.Add(Create(path, componentType, "localScale.x", MvvmBindingMode.OneWay, name + "ScaleX", "float"));
                entries.Add(Create(path, componentType, "localScale.y", MvvmBindingMode.OneWay, name + "ScaleY", "float"));
            }
        }

        private static void AddGraphicCandidates(List<MvvmBindingEntry> entries, string path, string objectName, string componentType)
        {
            var name = GuessPropertyName(objectName, "Graphic");
            entries.Add(Create(path, componentType, "color", MvvmBindingMode.OneWay, name + "Color", "UnityEngine.Color"));
            entries.Add(Create(path, componentType, "color.a", MvvmBindingMode.OneWay, name + "Alpha", "float"));
        }

        private static void AddSelectableCandidates(List<MvvmBindingEntry> entries, string path, string objectName, string componentType)
        {
            entries.Add(Create(path, componentType, "interactable", MvvmBindingMode.OneWay, GuessPropertyName(objectName, "Selectable") + "Interactable", "bool"));
        }

        private static MvvmBindingEntry Create(string path, string componentType, string targetProperty, MvvmBindingMode mode, string member, string valueType)
        {
            return new MvvmBindingEntry
            {
                objectPath = path,
                componentType = componentType,
                targetProperty = targetProperty,
                mode = mode,
                viewModelMember = member,
                valueType = valueType,
                fieldName = ToCamelCase(SanitizeIdentifier(path.Replace("/", "_") + "_" + targetProperty))
            };
        }

        private static string GetPath(Transform root, Transform current)
        {
            if (root == current)
            {
                return root.name;
            }

            var stack = new Stack<string>();
            var node = current;
            while (node != null)
            {
                stack.Push(node.name);
                if (node == root)
                {
                    break;
                }

                node = node.parent;
            }

            return string.Join("/", stack);
        }

        private static string GuessPropertyName(string objectName, string suffix)
        {
            var name = SanitizeIdentifier(objectName);
            if (name.EndsWith(suffix))
            {
                name = name.Substring(0, name.Length - suffix.Length);
            }

            return string.IsNullOrWhiteSpace(name) ? suffix : ToPascalCase(name);
        }

        private static string GuessCommandName(string objectName)
        {
            var name = SanitizeIdentifier(objectName);
            foreach (var suffix in new[] { "Button", "Btn" })
            {
                if (name.EndsWith(suffix))
                {
                    name = name.Substring(0, name.Length - suffix.Length);
                }
            }

            name = string.IsNullOrWhiteSpace(name) ? "Click" : ToPascalCase(name);
            return name + "Command";
        }

        public static string SanitizeIdentifier(string value)
        {
            value = Regex.Replace(value ?? string.Empty, "[^a-zA-Z0-9_]", "_");
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Item";
            }

            if (char.IsDigit(value[0]))
            {
                value = "_" + value;
            }

            return value;
        }

        public static string ToPascalCase(string value)
        {
            value = SanitizeIdentifier(value);
            var parts = value.Split('_').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            if (parts.Length == 0)
            {
                return "Item";
            }

            return string.Concat(parts.Select(x => char.ToUpperInvariant(x[0]) + x.Substring(1)));
        }

        public static string ToCamelCase(string value)
        {
            value = ToPascalCase(value);
            return char.ToLowerInvariant(value[0]) + value.Substring(1);
        }
    }
}
