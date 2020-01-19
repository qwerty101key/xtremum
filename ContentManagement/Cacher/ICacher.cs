namespace ContentManagement
{
    public interface ICacher
    {
        object GetValue(string key);
        void SetValue(string key, object item);
    }
}