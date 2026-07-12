using System;

namespace PTY.Scripts.Save.Local
{
    /// <summary>
    /// TODO(SCRUM-28 기능 구현): Application.persistentDataPath에 JSON 파일로 GameSaveData를 읽고 쓴다.
    /// </summary>
    public class JsonFileSaveRepository : ILocalSaveRepository
    {
        public GameSaveData Load()
        {
            throw new NotImplementedException();
        }

        public void Save(GameSaveData data)
        {
            throw new NotImplementedException();
        }
    }
}
