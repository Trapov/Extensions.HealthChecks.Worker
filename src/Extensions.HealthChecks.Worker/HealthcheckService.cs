namespace Extensions.HealthChecks.Worker
{
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    public class HealthCheckWorker : BackgroundService
    {
        private readonly HttpListener _httpListener = new HttpListener();
        private readonly HealthcheckOptions _options;
        private readonly HealthCheckService _service;
        private readonly ILogger _logger;

        private readonly IList<string> _prefixes = new List<string>();

        public HealthCheckWorker(IOptions<HealthcheckOptions> options, HealthCheckService service, ILogger<HealthCheckWorker> logger)
        {
            _options = options.Value;
            _service = service;
            _logger = logger;

            var s = _options.UseHttps ? "s" : "";

            foreach (var endpoint in _options.Endpoints)
            {
                var prefix = $"http{s}://{_options.Hostname}:{_options.Port}/{endpoint.Url.TrimStart('/').TrimEnd('/') + "/"}";

                _prefixes.Add(prefix);
                _httpListener.Prefixes.Add(prefix);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _httpListener.Start();
            }
            catch (HttpListenerException exc)
            {
                if (exc.ErrorCode == 5) // access denied
                {
                    _logger.LogCritical(
                        "HealthcheckService didn't start because you have no administrator rights " +
                        "to open a web server on the specified port. To resolve this issue you need:\n" +
                        string.Join(Environment.NewLine, _prefixes.Select(p => $"netsh http add urlacl url={p} user=[domain]\\[username]")));

                    await Task.CompletedTask;
                }

                throw;
            }

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var getContext = _httpListener.GetContextAsync();
                    getContext.Wait(stoppingToken);

                    var context = getContext.Result;
                    var request = context.Request;
                    var response = context.Response;

                    var endpoint = _options.Endpoints.FirstOrDefault(end => end.Url == request.Url.LocalPath);

                    if (endpoint == null)
                        continue;

                    var checkResult = await _service.CheckHealthAsync(stoppingToken);

                    var result = endpoint.Check(checkResult);

                    try
                    {
                        var statusCode = _options.StatusCodesMapping[checkResult.Status];

                        response.StatusCode = statusCode;
                        response.ContentType = "application/json";

                        using (var writer = new StreamWriter(response.OutputStream))
                            writer.Write(result);
                    }
                    catch (Exception ex) when (!(ex is OperationCanceledException))
                    {
                        Trace.WriteLine($"Error in HealthcheckService: {ex}");

                        try
                        {
                            response.StatusCode = 500;
                        }
                        catch
                        {
                            // ignore errors
                        }
                    }
                    finally
                    {
                        response.Close();
                    }
                }
            }
            finally
            {
                _httpListener.Stop();
                _httpListener.Close();
            }
        }

    }
}
