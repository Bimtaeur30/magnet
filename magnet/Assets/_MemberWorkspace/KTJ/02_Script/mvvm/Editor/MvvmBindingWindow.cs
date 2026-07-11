using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Mvvm.Editor
{
    public sealed class MvvmBindingWindow : EditorWindow
    {
        private const string ProfileFolderPrefsKey = "Mvvm.BindingWindow.ProfileFolder";
        private const string GeneratedRootFolderPrefsKey = "Mvvm.BindingWindow.GeneratedRootFolder";

        private MvvmBindingProfile profile;
        private Vector2 scroll;
        private string profileFolder;
        private string generatedRootFolder;
        private string bindingSearch = string.Empty;
        private readonly Dictionary<string, bool> foldouts = new Dictionary<string, bool>();

        [MenuItem("Tools/MVVM/Binding Tool")]
        public static void Open()
        {
            GetWindow<MvvmBindingWindow>("MVVM Binding");
        }

        private void OnEnable()
        {
            profileFolder = EditorPrefs.GetString(ProfileFolderPrefsKey, "Assets/mvvm/Profiles");
            generatedRootFolder = EditorPrefs.GetString(GeneratedRootFolderPrefsKey, "Assets/mvvm/Generated");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("MVVM Binding Tool", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawCreationSettings();

            profile = (MvvmBindingProfile)EditorGUILayout.ObjectField("Profile", profile, typeof(MvvmBindingProfile), false);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Create Profile From Selection"))
                {
                    CreateProfileFromSelection();
                }

                if (GUILayout.Button("Load Selected Profile"))
                {
                    profile = Selection.activeObject as MvvmBindingProfile;
                }
            }

            if (profile == null)
            {
                EditorGUILayout.HelpBox("Select a UGUI prefab or scene root, then create a binding profile.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space();
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                profile.targetPrefab = (GameObject)EditorGUILayout.ObjectField("Target", profile.targetPrefab, typeof(GameObject), true);
                profile.namespaceName = EditorGUILayout.TextField("Namespace", profile.namespaceName);
                profile.viewClassName = EditorGUILayout.TextField("View Class", profile.viewClassName);
                profile.viewModelClassName = EditorGUILayout.TextField("ViewModel Class", profile.viewModelClassName);
                profile.outputFolder = EditorGUILayout.TextField("Output Folder", profile.outputFolder);

                if (change.changed)
                {
                    EditorUtility.SetDirty(profile);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Scan UI"))
                {
                    Scan();
                }

                if (GUILayout.Button("Validate"))
                {
                    Validate();
                }

                if (GUILayout.Button("Generate Code"))
                {
                    Generate();
                }

                if (GUILayout.Button("Wire View Fields"))
                {
                    Wire();
                }
            }

            EditorGUILayout.Space();
            DrawBindings();
        }

        private void DrawCreationSettings()
        {
            EditorGUILayout.LabelField("Create Settings", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                profileFolder = EditorGUILayout.TextField("Profile Folder", profileFolder);
                if (GUILayout.Button("Select", GUILayout.Width(64)))
                {
                    profileFolder = SelectAssetFolder(profileFolder);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                generatedRootFolder = EditorGUILayout.TextField("Generated Root", generatedRootFolder);
                if (GUILayout.Button("Select", GUILayout.Width(64)))
                {
                    generatedRootFolder = SelectAssetFolder(generatedRootFolder);
                }
            }

            EditorPrefs.SetString(ProfileFolderPrefsKey, NormalizeAssetFolder(profileFolder, "Assets/mvvm/Profiles"));
            EditorPrefs.SetString(GeneratedRootFolderPrefsKey, NormalizeAssetFolder(generatedRootFolder, "Assets/mvvm/Generated"));
            EditorGUILayout.Space();
        }

        private void DrawBindings()
        {
            EditorGUILayout.LabelField("Bindings", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                bindingSearch = EditorGUILayout.TextField("Search", bindingSearch);

                if (GUILayout.Button("Clear", GUILayout.Width(56)))
                {
                    bindingSearch = string.Empty;
                    GUI.FocusControl(null);
                }

                if (GUILayout.Button("Unselect All", GUILayout.Width(96)))
                {
                    UnselectAllBindings();
                }
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);

            var groups = profile.bindings
                .Select((binding, index) => new BindingRow(binding, index))
                .Where(row => MatchesSearch(row.Binding, bindingSearch))
                .GroupBy(x => GetFoldoutKey(x.Binding))
                .ToList();

            if (groups.Count == 0)
            {
                EditorGUILayout.HelpBox("No bindings match the current search.", MessageType.Info);
                EditorGUILayout.EndScrollView();
                return;
            }

            foreach (var group in groups)
            {
                var first = group.First().Binding;
                var key = group.Key;
                if (!foldouts.ContainsKey(key))
                {
                    foldouts[key] = true;
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                using (new EditorGUILayout.HorizontalScope())
                {
                    foldouts[key] = EditorGUILayout.Foldout(foldouts[key], $"{first.objectPath}  ({ShortComponentName(first.componentType)})", true, EditorStyles.foldoutHeader);
                    EditorGUILayout.LabelField($"{group.Count()} bindings", GUILayout.Width(80));
                    if (GUILayout.Button("Remove All", GUILayout.Width(88)))
                    {
                        foreach (var index in group.Select(x => x.Index).OrderByDescending(x => x))
                        {
                            profile.bindings.RemoveAt(index);
                        }

                        EditorUtility.SetDirty(profile);
                        EditorGUILayout.EndVertical();
                        break;
                    }
                }

                if (foldouts[key])
                {
                    EditorGUILayout.LabelField("Component", first.componentType);

                    foreach (var row in group.ToList())
                    {
                        var binding = row.Binding;
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        using (var change = new EditorGUI.ChangeCheckScope())
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                binding.enabled = EditorGUILayout.Toggle(binding.enabled, GUILayout.Width(18));
                                EditorGUILayout.LabelField(binding.targetProperty, EditorStyles.boldLabel);
                                if (GUILayout.Button("Remove", GUILayout.Width(72)))
                                {
                                    profile.bindings.RemoveAt(row.Index);
                                    EditorUtility.SetDirty(profile);
                                    EditorGUILayout.EndVertical();
                                    break;
                                }
                            }

                            binding.mode = (MvvmBindingMode)EditorGUILayout.EnumPopup("Mode", binding.mode);
                            binding.viewModelMember = EditorGUILayout.TextField("ViewModel Member", binding.viewModelMember);
                            binding.valueType = EditorGUILayout.TextField("Value Type", binding.valueType);
                            binding.fieldName = EditorGUILayout.TextField("Field Name", binding.fieldName);

                            if (change.changed)
                            {
                                EditorUtility.SetDirty(profile);
                            }
                        }

                        EditorGUILayout.EndVertical();
                    }
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();
        }

        private void UnselectAllBindings()
        {
            Undo.RecordObject(profile, "Unselect MVVM Bindings");

            foreach (var binding in profile.bindings)
            {
                binding.enabled = false;
            }

            EditorUtility.SetDirty(profile);
        }

        private static bool MatchesSearch(MvvmBindingEntry binding, string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return true;
            }

            search = search.Trim();
            return ContainsIgnoreCase(binding.objectPath, search)
                || ContainsIgnoreCase(ShortComponentName(binding.componentType), search)
                || ContainsIgnoreCase(binding.componentType, search)
                || ContainsIgnoreCase(binding.targetProperty, search)
                || ContainsIgnoreCase(binding.viewModelMember, search)
                || ContainsIgnoreCase(binding.valueType, search)
                || ContainsIgnoreCase(binding.fieldName, search);
        }

        private static bool ContainsIgnoreCase(string value, string search)
        {
            return !string.IsNullOrEmpty(value)
                && value.IndexOf(search, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string GetFoldoutKey(MvvmBindingEntry binding)
        {
            return binding.objectPath + "|" + binding.componentType;
        }

        private static string ShortComponentName(string componentType)
        {
            if (string.IsNullOrWhiteSpace(componentType))
            {
                return "Component";
            }

            var index = componentType.LastIndexOf('.');
            return index >= 0 ? componentType.Substring(index + 1) : componentType;
        }

        private readonly struct BindingRow
        {
            public BindingRow(MvvmBindingEntry binding, int index)
            {
                Binding = binding;
                Index = index;
            }

            public MvvmBindingEntry Binding { get; }
            public int Index { get; }
        }

        private void CreateProfileFromSelection()
        {
            var target = Selection.activeGameObject;
            if (target == null)
            {
                EditorUtility.DisplayDialog("MVVM Binding", "Select a prefab or scene GameObject first.", "OK");
                return;
            }

            var className = MvvmBindingScanner.ToPascalCase(target.name);
            var profileRoot = NormalizeAssetFolder(profileFolder, "Assets/mvvm/Profiles");
            var generatedRoot = NormalizeAssetFolder(generatedRootFolder, "Assets/mvvm/Generated");
            EnsureAssetFolder(profileRoot);
            EnsureAssetFolder(generatedRoot);
            var path = AssetDatabase.GenerateUniqueAssetPath($"{profileRoot}/{className}BindingProfile.asset");

            profile = CreateInstance<MvvmBindingProfile>();
            profile.targetPrefab = target;
            profile.viewClassName = className + "View";
            profile.viewModelClassName = className + "ViewModel";
            profile.outputFolder = $"{generatedRoot}/{className}";
            profile.bindings = MvvmBindingScanner.Scan(target);

            AssetDatabase.CreateAsset(profile, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = profile;
        }

        private static string SelectAssetFolder(string currentFolder)
        {
            var normalized = NormalizeAssetFolder(currentFolder, "Assets");
            var absolute = ToAbsoluteProjectPath(normalized);
            var selected = EditorUtility.OpenFolderPanel("Select Asset Folder", absolute, string.Empty);
            if (string.IsNullOrEmpty(selected))
            {
                return normalized;
            }

            var assetPath = ToAssetRelativePath(selected);
            if (!string.IsNullOrEmpty(assetPath))
            {
                return assetPath;
            }

            EditorUtility.DisplayDialog("MVVM Binding", $"Please select a folder inside this project's Assets folder.\n\nCurrent project Assets:\n{Application.dataPath}", "OK");
            return normalized;
        }

        private static string NormalizeAssetFolder(string folder, string fallback)
        {
            if (string.IsNullOrWhiteSpace(folder))
            {
                return fallback;
            }

            folder = folder.Replace("\\", "/").TrimEnd('/');
            if (folder.StartsWith("Assets"))
            {
                return folder;
            }

            var assetPath = ToAssetRelativePath(folder);
            return string.IsNullOrEmpty(assetPath) ? fallback : assetPath;
        }

        private static string ToAssetRelativePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            path = path.Replace("\\", "/").TrimEnd('/');
            var dataPath = Application.dataPath.Replace("\\", "/").TrimEnd('/');
            if (string.Equals(path, dataPath, System.StringComparison.OrdinalIgnoreCase))
            {
                return "Assets";
            }

            if (path.StartsWith(dataPath + "/", System.StringComparison.OrdinalIgnoreCase))
            {
                return "Assets" + path.Substring(dataPath.Length);
            }

            return string.Empty;
        }

        private static string ToAbsoluteProjectPath(string assetPath)
        {
            assetPath = NormalizeAssetFolder(assetPath, "Assets");
            var projectRoot = Directory.GetParent(Application.dataPath).FullName.Replace("\\", "/");
            return Path.GetFullPath(Path.Combine(projectRoot, assetPath)).Replace("\\", "/");
        }

        private static void EnsureAssetFolder(string folder)
        {
            folder = NormalizeAssetFolder(folder, "Assets");
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            var parts = folder.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private void Scan()
        {
            if (profile.targetPrefab == null)
            {
                EditorUtility.DisplayDialog("MVVM Binding", "Assign a target prefab or scene GameObject first.", "OK");
                return;
            }

            Undo.RecordObject(profile, "Scan MVVM Bindings");
            profile.bindings = MvvmBindingScanner.Scan(profile.targetPrefab);
            EditorUtility.SetDirty(profile);
        }

        private void Validate()
        {
            var message = MvvmBindingValidator.Validate(profile);
            EditorUtility.DisplayDialog("MVVM Binding Validation", message, "OK");
        }

        private void Generate()
        {
            try
            {
                MvvmCodeGenerator.Generate(profile);
                EditorUtility.DisplayDialog("MVVM Binding", "Code generated. After Unity compiles, click Wire View Fields to add the View component and assign serialized fields.", "OK");
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                EditorUtility.DisplayDialog("MVVM Binding Error", ex.Message, "OK");
            }
        }

        private void Wire()
        {
            try
            {
                MvvmViewWirer.Wire(profile);
                EditorUtility.DisplayDialog("MVVM Binding", "View component and serialized fields wired.", "OK");
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                EditorUtility.DisplayDialog("MVVM Wiring Error", ex.Message, "OK");
            }
        }
    }
}
