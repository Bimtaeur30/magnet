using System.IO;
using UnityEngine;

namespace PTY.Scripts.Save.Local
{
    /// <summary>
    /// Application.persistentDataPath에 GameSaveData를 JSON 파일로 읽고 쓴다.
    /// </summary>
    public class JsonFileSaveRepository : ILocalSaveRepository
    {
        private const string FileName = "save.json";

        private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public GameSaveData Load()
        {
            if (!File.Exists(FilePath))
            {
                return null;
            }

            string json = File.ReadAllText(FilePath);
            return JsonUtility.FromJson<GameSaveData>(json);
        }

        public void Save(GameSaveData data)
        {
            string json = JsonUtility.ToJson(data);
            File.WriteAllText(FilePath, json);
        }
    }
}
