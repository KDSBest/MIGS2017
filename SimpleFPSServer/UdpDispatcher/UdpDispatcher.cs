using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using ReliableUdp;
using ReliableUdp.Logging;
using Utility;

namespace UdpDispatcher
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class UdpDispatcher : StatelessService
    {
        private UdpManagerListener listener;

        private static readonly int fps = 20;

        public UdpDispatcher(StatelessServiceContext context)
            : base(context)
        {
            FactoryRegistrations.Register();
            Factory.Register<IUdpLogger>(() => new EventSourceLogger(), FactoryLifespan.Singleton);
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext =>
                {
                    listener = new UdpManagerListener(serviceContext, ServiceEventSource.Current, "ServiceEndpoint");
                    return this.listener;
                })
            };
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
                this.listener.PollEvents();
                Thread.Sleep(1000 / fps);
            }

            base.RunAsync(cancellationToken);
        }
    }
}
