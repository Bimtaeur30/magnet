namespace Magnet.Contracts.Save
{
    public interface ILocalSaveRepository
    {
        GameSaveData Load();
        void Save(GameSaveData data);
    }
}
