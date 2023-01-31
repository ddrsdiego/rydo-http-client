namespace Rydo.HttpClient.Configurations
{
    public class ServiceEntry
    {
        public string? Name { get; set; }
        public string? RepositoryName { get; set; }
        public int HandlerLifetimeInMinutes { get; set; }
        public string? BaseAddress { get; set; }
    }
}