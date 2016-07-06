using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Common;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;

namespace NetworkEventSource
{
    internal sealed class NetworkEventSource : StatefulService, INetworkEventSource
    {
        private Task<IReliableQueue<NetworkEvent>> _eventQueue => StateManager.GetOrAddAsync<IReliableQueue<NetworkEvent>>("EventQueue");
        private Task<IReliableDictionary<String, NetworkEvent>> _eventLog
            => StateManager.GetOrAddAsync<IReliableDictionary<String, NetworkEvent>>("EventLog");

        public NetworkEventSource(StatefulServiceContext context)
            : base(context)
        { }

        public async Task QueueNetworkEvent(NetworkEvent e)
        {
            var queue = await _eventQueue;
            using (var tx = StateManager.CreateTransaction())
            {
                await queue.EnqueueAsync(tx, e);
                await tx.CommitAsync();
            }
        }

        public async Task<List<NetworkEvent>> GetNetworkLogs()
        {
            List<NetworkEvent> logList = new List<NetworkEvent>();
            var log = await _eventLog;
            var cancellationToken = new CancellationToken();
            using (var tx = StateManager.CreateTransaction())
            {
                var enumerable = await log.CreateEnumerableAsync(tx);
                using (var e = enumerable.GetAsyncEnumerator())
                {
                    while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                    {
                        logList.Add(e.Current.Value);
                    }
                }
            }
            return logList;
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]
            {
                new ServiceReplicaListener(context =>
                    this.CreateServiceRemotingListener(context))
            };
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            var queue = await _eventQueue;
            var log = await _eventLog;

            while (!cancellationToken.IsCancellationRequested)
            {
                using (var tx = StateManager.CreateTransaction())
                {
                    var queueSize = queue.GetCountAsync(tx).Result;
                    ServiceEventSource.Current.Message($"Current queue size: {queueSize}");

                    if (queueSize > 0)
                    {
                        var condition = await queue.TryDequeueAsync(tx);
                        if (condition.HasValue)
                        {
                            var ev = condition.Value;
                            try
                            {
                                await log.AddAsync(tx, ev.Id.ToString(), ev);
                            }
                            catch(Exception ex)
                            {
                                ServiceEventSource.Current.Message($"Error processing event: {ex.Message}");
                                throw ex;
                            }
                            await tx.CommitAsync();
                        }
                    }
                    else
                    {
                        // Release CPU and allow queue to grow
                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                }
            }
        }
    }
}
