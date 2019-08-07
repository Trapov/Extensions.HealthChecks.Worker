namespace Extensions.HealthChecks.Worker.Endpoints
{
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Newtonsoft.Json;
    using System;
    using System.Linq;

    public sealed class HealthcheckEndpoint : IHealthEndpoint
    {
        public string Url => "/healthcheck";

        public string Check(HealthReport report)
        {
            if (report == null)
                return JsonConvert.SerializeObject(new { });

            var result = JsonConvert.SerializeObject(new
            {
                Status = report.Status,
                TotalDuration = report.TotalDuration,
                Entries = report.Entries.ToDictionary(
                    pair => pair.Key,
                    pair => new
                    {
                        Data = pair.Value.Data,
                        Description = pair.Value.Description,
                        Duration = pair.Value.Duration,
                        Exception = pair.Value.Exception?.Message,
                        Status = pair.Value.Status
                    })
            }, Formatting.Indented);

            return result;
        }
    }
}