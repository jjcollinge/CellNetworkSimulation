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
using Microsoft.ServiceFabric.Data;

namespace NetworkProviderService
{
    internal sealed class NetworkProviderService : StatefulService, INetworkProviderService
    {
        private readonly string _serviceName;
        private readonly long _instanceId;

        private Task<IReliableDictionary<String, MobilePhone>> _activePhones
            => this.StateManager.GetOrAddAsync<IReliableDictionary<String, MobilePhone>>("PhoneNumbers");

        private PhoneNumberRange _phoneNumberRange;

        public NetworkProviderService(StatefulServiceContext context)
            : base(context)
        {
            _serviceName = context.ServiceTypeName;
            _instanceId = context.ReplicaId;
        }
        public Task InitialisePhoneRange(PhoneNumberRange phoneNumberRange)
        {
            if (_phoneNumberRange == null)
                _phoneNumberRange = phoneNumberRange;
            return Task.FromResult(true);
        }

        public Task<PhoneNumber> AllocatePhoneNumber()
        {
            return Task.FromResult<PhoneNumber>(_phoneNumberRange.AllocatePhoneNumber());
        }

        public async Task ActivateMobilePhone(MobilePhone mobile)
        {
            ServiceEventSource.Current.Message($"[{_serviceName}][{_instanceId}][VERBOSE] Attempting to activate {mobile.Sim.PhoneNumber}");

            var phones = await _activePhones;
            using (var tx = StateManager.CreateTransaction())
            {
                var exists = await phones.ContainsKeyAsync(tx, mobile.Sim.PhoneNumber.ToString());

                if(exists)
                {
                    ServiceEventSource.Current.Message($"[{_serviceName}][{_instanceId}][VERBOSE] The phone number {mobile.Sim.PhoneNumber} is already activated");
                }
                else
                {
                    var success = await phones.TryAddAsync(tx, mobile.Sim.PhoneNumber.ToString(), mobile);

                    if(success)
                        ServiceEventSource.Current.Message($"[{_serviceName}][{_instanceId}][VERBOSE] Added {mobile.Sim.PhoneNumber} to the registry");
                    else
                        ServiceEventSource.Current.Message($"[{_serviceName}][{_instanceId}][ERROR] Failed to add {mobile.Sim.PhoneNumber} to the registry");
                }
                await tx.CommitAsync();
            }
        }

        public async Task<bool> Connect(PhoneNumber caller, PhoneNumber callee, NetworkEvent networkEvent)
        {
            var phones = await _activePhones;
            using (var tx = CreateTransaction())
            {
                var condition = await phones.TryGetValueAsync(tx, callee.ToString());
                if (!condition.HasValue)
                {
                    // Callee is not registered with this network provider
                    // therefore we need to query the register to find
                    // the correct network provider
                    ServiceEventSource.Current.Message($"Callee {callee} is not in the current network provider");

                    var networkManager = ServiceFactory.CreateNetworkProviderManagerProxy();
                    var networkProvider = await networkManager.FindNetworkProvider(callee);

                    if (networkProvider == null)
                        return false;

                    var networkProviderService = ServiceFactory.CreateNetworkProviderProxy(networkProvider);
                    return await networkProviderService.Connect(caller, callee, networkEvent);
                }
                else
                {
                    // Callee is also registered with this provider
                    ServiceEventSource.Current.Message($"Callee {callee} is in the current network provider");

                    var calleePhone = condition.Value;
                    calleePhone.ReceiveCall(caller, networkEvent);
                    return true;
                }
            }
        }

        private ITransaction CreateTransaction()
        {
            return StateManager.CreateTransaction();
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
