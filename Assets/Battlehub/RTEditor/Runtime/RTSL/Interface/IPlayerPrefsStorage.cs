namespace Battlehub.RTSL.Interface
{
    public interface IPlayerPrefsStorage 
    {
        ProjectAsyncOperation<T> GetValue<T>(string key, ProjectEventHandler<T> callback = null);
        ProjectAsyncOperation SetValue<T>(string key, T obj, ProjectEventHandler callback = null);
        ProjectAsyncOperation DeleteValue<T>(string key, ProjectEventHandler callback = null);
    }
}


