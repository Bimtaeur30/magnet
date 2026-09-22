using System.IO;
using PTY.Scripts.Save.Local;
using UnityEditor;
using UnityEngine;

namespace PTY.Scripts.Editor
{
    /// <summary>
    /// 에디터에서 로컬 저장 데이터(save.json)를 초기화하거나 저장 폴더를 연다.
    /// </summary>
    public static class SaveDataEditorMenu
    {
        private const string ResetMenuPath = "Magnet/Save/Reset Save Data";
        private const string OpenFolderMenuPath = "Magnet/Save/Open Save Folder";

        [MenuItem(ResetMenuPath)]
        private static void ResetSaveData()
        {
            string path = JsonFileSaveRepository.FilePath;
            if (!File.Exists(path))
            {
                EditorUtility.DisplayDialog("저장 데이터 초기화", "삭제할 저장 데이터가 없습니다.\n" + path, "확인");
                return;
            }

            if (!EditorUtility.DisplayDialog(
                    "저장 데이터 초기화",
                    "최고 점수, 해금 스킨, 장착 스킨 등 모든 저장 데이터를 삭제합니다.\n" + path,
                    "삭제",
                    "취소"))
            {
                return;
            }

            File.Delete(path);
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log($"[SaveDataEditorMenu] 저장 데이터 초기화 완료: {path}");
        }

        // 플레이 중엔 SaveService가 메모리에 든 데이터를 다시 써버리므로 막는다.
        [MenuItem(ResetMenuPath, true)]
        private static bool ValidateResetSaveData()
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode;
        }

        [MenuItem(OpenFolderMenuPath)]
        private static void OpenSaveFolder()
        {
            string path = JsonFileSaveRepository.FilePath;
            EditorUtility.RevealInFinder(File.Exists(path) ? path : Application.persistentDataPath);
        }
    }
}
