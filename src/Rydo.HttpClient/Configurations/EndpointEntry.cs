namespace Rydo.HttpClient.Configurations
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    public class EndpointEntry
    {
        private ClientEntry? _clientEntry;
        private ServiceEntry? _serviceEntry;
        private int _handlerLifetimeInMinutes;

        public string? Name { get; set; }
        public string? Path { get; set; }

        public string? PolicyName { get; set; }
        
        public string? CaseStyle => !(_clientEntry is null) ? _clientEntry.CaseStyle : string.Empty;

        public bool Certificate { get; set; }

        public string? BaseAddress => !(_serviceEntry is null) ? _serviceEntry.BaseAddress : string.Empty;

        public int TimeoutInSeconds { get; set; } = 30;
        
        public RetryPolicyEntry? Retry { get; set; }
        
        public CircuitBreakPolicyEntry? CircuitBreak { get; set; }

        /// <summary>
        /// The default value is two minutes
        /// </summary>
        public int HandlerLifetimeInMinutes
        {
            get => GetHandlerLifetime();
            set => _handlerLifetimeInMinutes = value;
        }

        private int GetHandlerLifetime()
        {
            var handlerLifetime = 2;

            if (_serviceEntry is null)
            {
                handlerLifetime = _handlerLifetimeInMinutes <= 0 ? handlerLifetime : _handlerLifetimeInMinutes;
                return handlerLifetime;
            }

            if (_handlerLifetimeInMinutes <= 0 && _serviceEntry.HandlerLifetimeInMinutes <= 0)
                return handlerLifetime;

            handlerLifetime = _handlerLifetimeInMinutes > 0
                ? _handlerLifetimeInMinutes
                : _serviceEntry.HandlerLifetimeInMinutes;

            return handlerLifetime;
        }

        public void BindService([NotNull] ServiceEntry? serviceEntry) =>
            _serviceEntry = serviceEntry ?? throw new ArgumentNullException(nameof(serviceEntry));

        public void BindClient([NotNull] ClientEntry? clientEntry) =>
            _clientEntry = clientEntry ?? throw new ArgumentNullException(nameof(clientEntry));
    }
}