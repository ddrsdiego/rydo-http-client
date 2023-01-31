namespace Rydo.HttpClient.Policies
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using Configurations;

    internal static class HttpMessageHandlerFactory
    {
        public static HttpMessageHandler GetHttpMessageHandler(this EndpointEntry endpointEntry)
        {
            Helper.GuardNull(endpointEntry, nameof(endpointEntry));

            var handler = new StandardSocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(1)
            };

            if (!CertificateExists(endpointEntry, out var certificatePath))
                return handler;

            var sslOptions = new SslClientAuthenticationOptions
                { ClientCertificates = new X509Certificate2Collection() };

            sslOptions.ClientCertificates.Add(new X509Certificate2(certificatePath!));
            handler.SslOptions = sslOptions;

            return handler;
        }

        private static bool CertificateExists(EndpointEntry endpointEntry, out string? certificatePath)
        {
            const string pathCertificateFile = "Certificates/Creed/nuinvest_cert.pfx";
            certificatePath = null;

            if(!endpointEntry.Certificate)
                return false;
            
            var certPath = Path.Combine(Directory.GetCurrentDirectory(), pathCertificateFile);
            
            certificatePath = certPath;

            return File.Exists(certPath);
        }
    }
}