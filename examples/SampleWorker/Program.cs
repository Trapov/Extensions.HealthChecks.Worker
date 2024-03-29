namespace SampleWorker
{
    using Microsoft.Extensions.DependencyInjection;
    using Extensions.HealthChecks.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build() .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services
                        .AddHealthChecks()
                        .AddMongoDb("mongodb://localhost:27017/persons")
                        .UseHealthChecksWorker(options => { });

                    services.AddHostedService<Worker>();
                });
    }
}
