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

namespace NetworkProviderManagerService
{
    internal sealed class NetworkProviderManagerService : StatefulService, INetworkProviderManagerService
    {
        Task<IReliableDictionary<String, PhoneNumberRange>> _phoneNumberMap
            => this.StateManager.GetOrAddAsync<IReliableDictionary<String, PhoneNumberRange>>("PhoneNumberMap");

        public NetworkProviderManagerService(StatefulServiceContext context)
            : base(context)
        { }

        public async Task CreateNetwork()
        {
            // Create and initialise a single network provider
            long minPhoneNumber = 10000000000;
            long maxPhoneNumber = 20000000000;
            var phoneNumberRange = new PhoneNumberRange(minPhoneNumber, maxPhoneNumber);

            NetworkProvider networkProvider = new NetworkProvider();
            networkProvider.Name = "Contoso";

            var networkProviderService = ServiceFactory.CreateNetworkProviderProxy(networkProvider);
            await networkProviderService.InitialisePhoneRange(phoneNumberRange);

            // Store the network name against it's allocated phone number range
            var phoneNumberMap = await _phoneNumberMap;
            using(var tx = StateManager.CreateTransaction())
            {
                await phoneNumberMap.AddAsync(tx, networkProvider.Name, phoneNumberRange);
                await tx.CommitAsync();
            }
        }

        public async Task<NetworkProvider> FindNetworkProvider(PhoneNumber phoneNumber)
        {
            NetworkProvider networkProvider = null;
            var phoneNumberMap = await _phoneNumberMap;
            var cancellationToken = new CancellationToken();
            using(var tx = StateManager.CreateTransaction())
            {
                var enumerable = await phoneNumberMap.CreateEnumerableAsync(tx);
                using(var e = enumerable.GetAsyncEnumerator())
                {
                    while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                    {
                        if(phoneNumber > new PhoneNumber(e.Current.Value.Min)
                           && phoneNumber < new PhoneNumber(e.Current.Value.Max))
                        {
                            networkProvider = new NetworkProvider();
                            networkProvider.Name = e.Current.Key;
                            ServiceEventSource.Current.Message($"Found network provider for phone number: {phoneNumber}");
                        }
                    }
                }
            }
            return networkProvider;
        }
        public async Task<List<NetworkProvider>> GetNetworkProviders()
        {
            List<NetworkProvider> networkProviders = new List<NetworkProvider>();
            var phoneNumberMap = await _phoneNumberMap;
            var cancellationToken = new CancellationToken();
            using (var tx = StateManager.CreateTransaction())
            {
                var enumerable = await phoneNumberMap.CreateEnumerableAsync(tx);
                using (var e = enumerable.GetAsyncEnumerator())
                {
                    while (await e.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                    {
                        var networkProvider = new NetworkProvider();
                        networkProvider.Name = e.Current.Key;
                        networkProviders.Add(networkProvider);
                    }
                }
            }
            return networkProviders;
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]
            {
                new ServiceReplicaListener(context =>
                    this.CreateServiceRemotingListener(context))
            };
        }
    }
}
