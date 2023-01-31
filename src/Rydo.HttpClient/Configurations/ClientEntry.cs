namespace Rydo.HttpClient.Configurations
{
    public class ClientEntry
    {
        public string? Service { get; set; }

        public int TimeoutInSeconds { get; set; }

        public EndpointEntry[]? Endpoints { get; set; }

        //Pensar em como obter do repositório do Nubank ou do Vault
        public string? Token { get; set; }

        public string? CaseStyle { get; set; }
    }
}