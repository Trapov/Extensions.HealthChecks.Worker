using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;

namespace Extensions.HealthChecks.Worker
{
    public interface IHealthEndpoint
    {
        string Url { get; }
        string Check(HealthReport report);
    }
}