namespace Extensions.HealthChecks.DependencyInjection
{
    using Extensions.HealthChecks.Worker;
    using Extensions.HealthChecks.Worker.Endpoints;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;

    public static class IHealthChecksBuilderExtensions
    {
        public static IHealthChecksBuilder UseHealthChecksWorker(this IHealthChecksBuilder services, Action<HealthcheckOptions> configure)
        {
            var options = new HealthcheckOptions
            {
                Endpoints = new List<IHealthEndpoint>
                {
                    new HcEndpoint(),
                    new HealthcheckEndpoint()
                },
                Hostname = "+",
                Port = 4045,
                UseHttps = false
            };

            configure(options);

            services.Services.AddOptions();
            services.Services.Configure<HealthcheckOptions>(d =>
            {
                d.Endpoints = options.Endpoints;
                d.Hostname = options.Hostname;
                d.Port = options.Port;
                d.UseHttps = options.UseHttps;
            });

            services.Services.AddHostedService<HealthCheckWorker>();
            return services;
        }
    }
}
