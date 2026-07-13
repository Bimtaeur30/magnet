namespace PTY.Scripts.Save
{
    public interface ILocalSaveRepository
    {
        GameSaveData Load();
        void Save(GameSaveData data);
    }
}
