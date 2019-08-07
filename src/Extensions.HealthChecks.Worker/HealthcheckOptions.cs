namespace Extensions.HealthChecks.Worker
{
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using System.Collections.Generic;

    public sealed class HealthcheckOptions
    {
        public bool UseHttps { get; set; } = false;

        public string Hostname { get; set; } = "+";

        public int Port { get; set; } = 4045;

        public IEnumerable<IHealthEndpoint> Endpoints { get; set; }

        public IDictionary<HealthStatus, int> StatusCodesMapping = new Dictionary<HealthStatus, int>
        {
            { HealthStatus.Healthy, 200 },
            { HealthStatus.Degraded, 200 },
            { HealthStatus.Unhealthy, 503 }
        };

    }
}