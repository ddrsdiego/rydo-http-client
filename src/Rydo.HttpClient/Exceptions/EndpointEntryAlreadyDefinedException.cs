namespace Rydo.HttpClient.Exceptions
{
    using System;

    public class EndpointEntryAlreadyDefinedException : Exception
    {
        public EndpointEntryAlreadyDefinedException(string? serviceName, string? endpointName)
            : base(CreateMessage(serviceName, endpointName))
        {
        }
        
        internal static string CreateMessage(string? serviceName, string? endpointName) =>
            $"The entry for endpoint {endpointName}, already has an entry in service {serviceName} in the settings file. Please, configure it correctly";
    }
}