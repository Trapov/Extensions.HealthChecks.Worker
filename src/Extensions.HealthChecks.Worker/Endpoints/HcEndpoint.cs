namespace Extensions.HealthChecks.Worker.Endpoints
{
    using System;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Newtonsoft.Json;

    public sealed class HcEndpoint : IHealthEndpoint
    {
        public string Url => "/hc";

        string IHealthEndpoint.Check(HealthReport report)
        {
            if (report == null)
                return JsonConvert.SerializeObject(new { });

            var result = JsonConvert.SerializeObject(new
            {
                Status = report.Status,
                TotalDuration = report.TotalDuration
            }, Formatting.Indented);

            return result;
        }
    }
}