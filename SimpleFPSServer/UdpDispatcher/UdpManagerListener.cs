using System;
using System.Fabric;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using ReliableUdp;

namespace UdpDispatcher
{
    internal class UdpManagerListener : ICommunicationListener
    {
        private readonly ServiceEventSource eventSource;
        private readonly StatelessServiceContext serviceContext;
        private readonly string endpointName;

        private UdpManager udp;
        private string publishAddress;
        private string listeningAddress;

        private UdpEventListener chatServerListener;

        public UdpManagerListener(StatelessServiceContext serviceContext, ServiceEventSource eventSource, string endpointName)
        {
            if (serviceContext == null)
            {
                throw new ArgumentNullException(nameof(serviceContext));
            }

            if (endpointName == null)
            {
                throw new ArgumentNullException(nameof(endpointName));
            }

            if (eventSource == null)
            {
                throw new ArgumentNullException(nameof(eventSource));
            }

            this.serviceContext = serviceContext;
            this.endpointName = endpointName;
            this.eventSource = eventSource;
        }

        public bool ListenOnSecondary { get; set; }

        public void PollEvents()
        {
            if (this.chatServerListener != null)
                this.chatServerListener.PollEvents();
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            var serviceEndpoint = this.serviceContext.CodePackageActivationContext.GetEndpoint(this.endpointName);
            int port = serviceEndpoint.Port;

            this.listeningAddress = string.Format(
                CultureInfo.InvariantCulture,
                "udp://+:{0}",
                port);

            this.publishAddress = this.listeningAddress.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);

            try
            {
                this.eventSource.ServiceMessage(this.serviceContext, "Starting server on " + this.listeningAddress);

                this.chatServerListener = new UdpEventListener();
                this.udp = new UdpManager(this.chatServerListener, "kds");

                if (this.udp.Start(port))
                {
                    this.eventSource.ServiceMessage(this.serviceContext, "Listening on " + this.publishAddress);
                }
                else
                {
                    throw new Exception("Udp Server couldn't start.");
                }
                return Task.FromResult(this.publishAddress);
            }
            catch (Exception ex)
            {
                this.eventSource.ServiceMessage(this.serviceContext, "Udp server failed to open. " + ex.ToString());

                this.StopUdpServer();

                throw;
            }
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            this.eventSource.ServiceMessage(this.serviceContext, "Closing udp server");

            this.StopUdpServer();

            return Task.FromResult(true);
        }

        public void Abort()
        {
            this.eventSource.ServiceMessage(this.serviceContext, "Aborting udp server");

            this.StopUdpServer();
        }

        private void StopUdpServer()
        {
            if (this.udp != null)
            {
                this.chatServerListener = null;
                this.udp.Stop();
            }
        }
    }
}