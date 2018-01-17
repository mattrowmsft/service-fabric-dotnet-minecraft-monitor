using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace MinecraftPopulationMonitor
{
    using System.Net.Http;
    using Microsoft.ApplicationInsights;
    using Newtonsoft.Json;

    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class MinecraftPopulationMonitor : StatelessService
    {
        public MinecraftPopulationMonitor(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            var telemetryClient = new TelemetryClient();
            var httpClient = new HttpClient();
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var response = await httpClient.GetAsync("https://mcapi.us/server/status?ip=104.45.229.103", cancellationToken);
                var answerString = await response.Content.ReadAsStringAsync();
                var answer = JsonConvert.DeserializeObject<Answer>(answerString);
                telemetryClient.TrackMetric("Population", answer.Players.Now);
                telemetryClient.TrackMetric("Capacity", answer.Players.Max - answer.Players.Now);

                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }

        public class Answer
        {
            public string Status { get; set; }
            public string Online { get; set; }
            public string Motd { get; set; }
            public string Error { get; set; }
            public PlayerStats Players { get; set; }
            public ServerStats Server { get; set; }
            public string Last_Online { get; set; }
            public string Last_Updated {get; set; }
            public int Duration { get; set; }
        }

        public class PlayerStats
        {
            public int Max { get; set; }
            public int Now { get; set; }
        }

        public class ServerStats
        {
            public string Name { get; set; }
            public int Protocol { get; set; }
        }
    }
}
