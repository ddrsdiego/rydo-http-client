namespace Rydo.HttpClient.Exceptions
{
    using System;

    public class ServiceEntryAlreadyExistsException : Exception
    {
        public ServiceEntryAlreadyExistsException(string serviceName)
            : base(CreateMessage(serviceName))
        {
        }

        private static string CreateMessage(string serviceName) =>
            $"The entry for service {serviceName} has already been added to the settings file. Please, configure it correctly";
    }
}