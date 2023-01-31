namespace Rydo.HttpClient
{
    using System;
    using Microsoft.Extensions.Logging;
    using Polly;

    internal static class ContextPollyEx
    {
        public static ILogger? GetLogger(this Context context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (!context.TryGetValue(Constants.PolicyContextLogger, out var logger))
                return null;

            var contextLogger = logger as ILogger;
            return contextLogger;
        }
    }
}