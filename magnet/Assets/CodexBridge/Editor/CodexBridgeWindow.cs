#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodexBridge
{
    public sealed class CodexBridgeWindow : EditorWindow
    {
        private Vector2 _scroll;
        private string _lastMessage = "Ready";

        [MenuItem("Tools/Codex/Bridge Panel")]
        public static void Open()
        {
            GetWindow<CodexBridgeWindow>("Codex Bridge");
        }

        [MenuItem("Tools/Codex/Ping")]
        public static void Ping()
        {
            EnsureBridgeFolders();
            var path = Path.Combine(Application.dataPath, "CodexBridge", "Reports", "ping.txt");
            File.WriteAllText(path, $"Codex Bridge OK\nProject: {Application.dataPath}\nUnity: {Application.unityVersion}\nTime: {DateTime.Now:O}\n");
            AssetDatabase.Refresh();
            Debug.Log($"Codex Bridge ping written: {path}");
        }

        [MenuItem("Tools/Codex/Write Scene Snapshot")]
        public static void WriteSceneSnapshot()
        {
            EnsureBridgeFolders();
            var scene = SceneManager.GetActiveScene();
            var reportPath = Path.Combine(Application.dataPath, "CodexBridge", "Reports", "scene-snapshot.txt");
            var builder = new StringBuilder();
            builder.AppendLine($"Unity: {Application.unityVersion}");
            builder.AppendLine($"Project: {Application.dataPath}");
            builder.AppendLine($"Scene: {scene.name}");
            builder.AppendLine($"ScenePath: {scene.path}");
            builder.AppendLine($"RootCount: {scene.rootCount}");
            builder.AppendLine();

            foreach (var root in scene.GetRootGameObjects())
            {
                AppendGameObject(builder, root.transform, 0);
            }

            File.WriteAllText(reportPath, builder.ToString());
            AssetDatabase.Refresh();
            Debug.Log($"Codex scene snapshot written: {reportPath}");
        }

        [MenuItem("Tools/Codex/Create Empty Working Scene")]
        public static void CreateEmptyWorkingScene()
        {
            EnsureBridgeFolders();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            scene.name = "CodexWorkingScene";
            var scenePath = "Assets/CodexBridge/CodexWorkingScene.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.Refresh();
            Debug.Log($"Codex working scene created: {scenePath}");
        }

        [MenuItem("Tools/Codex/Reload External Scene Changes")]
        public static void ReloadExternalSceneChanges()
        {
            EnsureBridgeFolders();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var scene = SceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(scene.path))
            {
                Debug.LogWarning("Codex reload skipped: active scene has no saved path.");
                return;
            }

            if (scene.isDirty && !EditorUtility.DisplayDialog(
                    "Reload External Scene Changes",
                    "The active scene has unsaved Unity-side changes. Reloading will discard those unsaved changes and load the scene file from disk.",
                    "Reload From Disk",
                    "Cancel"))
            {
                Debug.Log("Codex reload canceled.");
                return;
            }

            EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
            Debug.Log($"Codex reloaded scene from disk: {scene.path}");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Codex Bridge", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Project", Application.dataPath);
            EditorGUILayout.LabelField("Unity", Application.unityVersion);
            EditorGUILayout.Space();

            if (GUILayout.Button("Ping"))
            {
                Ping();
                _lastMessage = "Ping report written.";
            }

            if (GUILayout.Button("Write Scene Snapshot"))
            {
                WriteSceneSnapshot();
                _lastMessage = "Scene snapshot written.";
            }

            if (GUILayout.Button("Create Empty Working Scene"))
            {
                CreateEmptyWorkingScene();
                _lastMessage = "Working scene created.";
            }

            if (GUILayout.Button("Reload External Scene Changes"))
            {
                ReloadExternalSceneChanges();
                _lastMessage = "External scene changes reloaded.";
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Status", _lastMessage);
            EditorGUILayout.Space();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.HelpBox("This bridge gives Codex a stable Unity-side utility surface. Codex can edit project files directly, then Unity imports them. Use the snapshot command when Codex needs a readable view of the open scene. Use reload after Codex edits a scene file while Unity is already open.", MessageType.Info);
            EditorGUILayout.EndScrollView();
        }

        private static void EnsureBridgeFolders()
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "CodexBridge"));
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "CodexBridge", "Editor"));
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "CodexBridge", "Reports"));
        }

        private static void AppendGameObject(StringBuilder builder, Transform transform, int depth)
        {
            var indent = new string(' ', depth * 2);
            var components = transform.GetComponents<Component>()
                .Where(component => component != null)
                .Select(component => component.GetType().Name);

            builder.AppendLine($"{indent}- {transform.name}");
            builder.AppendLine($"{indent}  Active: {transform.gameObject.activeSelf}");
            builder.AppendLine($"{indent}  Position: {transform.localPosition}");
            builder.AppendLine($"{indent}  Rotation: {transform.localEulerAngles}");
            builder.AppendLine($"{indent}  Scale: {transform.localScale}");
            builder.AppendLine($"{indent}  Components: {string.Join(", ", components)}");

            for (var i = 0; i < transform.childCount; i++)
            {
                AppendGameObject(builder, transform.GetChild(i), depth + 1);
            }
        }
    }
}
#endif
