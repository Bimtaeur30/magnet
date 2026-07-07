namespace HwanLib.SaveSystem
{
    public interface IStorable : ISaveable
    {
        string StoreData();
    }
}