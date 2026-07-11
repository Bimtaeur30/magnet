using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Mvvm.Editor
{
    public static class MvvmPostGenerateWireProcessor
    {
        private const string PendingProfilesKey = "Mvvm.PendingWireProfiles";

        public static void Schedule(MvvmBindingProfile profile)
        {
            if (profile == null)
            {
                return;
            }

            var path = AssetDatabase.GetAssetPath(profile);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var paths = ReadPendingPaths();
            if (!paths.Contains(path))
            {
                paths.Add(path);
            }

            WritePendingPaths(paths);
        }

        [DidReloadScripts]
        private static void WirePendingProfiles()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            var paths = ReadPendingPaths();
            if (paths.Count == 0)
            {
                return;
            }

            WritePendingPaths(new List<string>());

            foreach (var path in paths)
            {
                var profile = AssetDatabase.LoadAssetAtPath<MvvmBindingProfile>(path);
                if (profile == null)
                {
                    continue;
                }

                try
                {
                    MvvmViewWirer.Wire(profile);
                    Debug.Log($"MVVM: Wired generated view fields for {path}.");
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        private static List<string> ReadPendingPaths()
        {
            var value = SessionState.GetString(PendingProfilesKey, string.Empty);
            return value
                .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .ToList();
        }

        private static void WritePendingPaths(List<string> paths)
        {
            SessionState.SetString(PendingProfilesKey, string.Join("|", paths));
        }
    }
}
