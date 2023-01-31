namespace Rydo.HttpClient.Serialization
{
    public interface ISerializer
    {
        string Serialize<T>(T source);
        T Deserialize<T>(string json);
    }
}