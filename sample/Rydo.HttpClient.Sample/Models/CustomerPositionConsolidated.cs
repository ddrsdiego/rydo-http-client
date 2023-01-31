namespace Rydo.HttpClient.Sample.Models
{
    public record PlayCustomer(string customerId);
    
    public class CustomerPositionConsolidated
    {
        public Customer Customer { get; set; }
        public CustomerPosition Position { get; set; }
    }
}