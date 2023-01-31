namespace Rydo.HttpClient.Configurations
{
    using System;
    using System.Collections.Immutable;

    public static class EndpointEntryPathFactory
    {
        private static readonly object SynLock;
        internal static ImmutableDictionary<string, Uri> Uris;

        static EndpointEntryPathFactory()
        {
            SynLock = new object();
            Uris = ImmutableDictionary<string, Uri>.Empty;
        }

        public static Uri GetEndpointPath(this EndpointEntry endpointEntry)
        {
            Helper.GuardNull(endpointEntry.Name, nameof(endpointEntry.Name));

            if (Uris.TryGetValue(endpointEntry.Name, out var uri))
                return uri;

            lock (SynLock)
            {
                var endpointString = endpointEntry.Path;
                var baseAddressString = endpointEntry.BaseAddress;

                Helper.GuardNull(baseAddressString, nameof(baseAddressString));

                if (!baseAddressString.EndsWith("/"))
                    baseAddressString += @"/";

                Helper.GuardNull(endpointString, nameof(endpointString));

                if (endpointString.StartsWith("/"))
                    endpointString = endpointString.Remove(0, 1);

                if (Uris.TryGetValue(endpointEntry.Name, out uri))
                    return uri;

                uri = new Uri($"{baseAddressString}{endpointString}");
                Uris = Uris.Add(endpointEntry.Name, uri);
            }

            return uri;
        }
    }
}