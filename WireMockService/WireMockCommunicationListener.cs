using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System.Fabric;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using WireMock.Server;
using WireMock.Settings;

namespace WireMockService
{
    internal class WireMockCommunicationListener : ICommunicationListener
    {
        private FluentMockServer _server;
        private readonly StatefulServiceContext _context;
        private readonly IReliableStateManager _reliableStateManager;

        public WireMockCommunicationListener(StatefulServiceContext context, IReliableStateManager reliableStateManager)
        {
            _context = context;
            _reliableStateManager = reliableStateManager;
        }

        /// <summary>
        /// This method causes the communication listener to be opened. Once the Open completes, the communication listener becomes usable - accepts and sends messages.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A <see cref="Task">Task</see> that represents outstanding operation. The result of the Task is is endpoint string on which WireMock.Net is listening.</returns>
        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            string url = GetListenerUrl();
            ServiceEventSource.Current.ServiceMessage(_context, $"WireMock.Net Starting on {url}");

            _server = FluentMockServer.Start(new FluentMockServerSettings
            {
                Urls = new[] { url },
                StartAdminInterface = true,
                ReadStaticMappings = true,
                WatchStaticMappings = true,
                PreWireMockMiddlewareInit = app => { ServiceEventSource.Current.ServiceMessage(_context, $"PreWireMockMiddlewareInit : {app.GetType()}"); },
                PostWireMockMiddlewareInit = app => { ServiceEventSource.Current.ServiceMessage(_context, $"PostWireMockMiddlewareInit : {app.GetType()}"); },
                Logger = new WireMockServiceFabricLogger(_context),

                // FileSystemHandler = new WireMockServiceFabricFileSystemHandler(_reliableStateManager)
            });

            return Task.FromResult(url);
        }

        /// <summary>
        /// This method causes the communication listener to close. Close is a terminal state and this method allows the communication listener to transition to this state in a graceful manner.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A <see cref="Task">Task</see> that represents outstanding operation.</returns>
        public Task CloseAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                ServiceEventSource.Current.ServiceMessage(_context, "WireMock.Net Stopping");

                _server.Stop();
                _server.Dispose();

                ServiceEventSource.Current.ServiceMessage(_context, "WireMock.Net Stopped");

                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        /// <summary>
        /// This method causes the communication listener to close. Close is a terminal state and this method causes the transition to close ungracefully.
        /// Any outstanding operations (including close) should be canceled when this method is called.
        /// </summary>
        public void Abort()
        {
            ServiceEventSource.Current.ServiceMessage(_context, "WireMock.Net Aborting");

            _server.Stop();
            _server.Dispose();

            ServiceEventSource.Current.ServiceMessage(_context, "WireMock.Net Aborted");
        }

        /// <summary>
        /// Gets url for the listener. Listener url is created using the endpointName passed in the constructor.
        /// </summary>
        /// <returns>url for the listener.</returns>
        private string GetListenerUrl()
        {
            // Get protocol and port from endpoint resource (ServiceManifest.xml).
            var serviceEndpoint = _context.CodePackageActivationContext.GetEndpoint("ServiceEndpoint");

            return string.Format(CultureInfo.InvariantCulture, "{0}://+:{1}", serviceEndpoint.Protocol.ToString().ToLower(), serviceEndpoint.Port);
        }
    }
}
