namespace Rydo.HttpClient.Exceptions
{
    using System;

    public class PolicyNotDefinedException: Exception
    {
        public PolicyNotDefinedException(string? policyName)
            : base(CreateMessage(policyName))
        {
        }
        
        internal static string CreateMessage(string? policyName) =>
            $"The entry for policy {policyName} is not configured in the settings file. Please, configure it correctly.";
    }
}