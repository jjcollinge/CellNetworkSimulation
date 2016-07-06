using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public class MobilePhone
    {
        [DataMember]
        public SIM Sim { get; set; }
        [DataMember]
        public Location Location { get; set; }

        public async Task Activate()
        {
            var networkProvider = ServiceFactory.CreateNetworkProviderProxy(Sim.NetworkProvider);
            await networkProvider.ActivateMobilePhone(this);
        }

        public async void TransmitCall(PhoneNumber callee)
        {
            var networkEvent = new NetworkEvent
            {
                CallerPhoneNumber = Sim.PhoneNumber.ToString(),
                CallerNetworkProvider = Sim.NetworkProvider.ToString(),
                CallStartTime = DateTime.Now,
            };
            var networkProvider = ServiceFactory.CreateNetworkProviderProxy(Sim.NetworkProvider);
            await networkProvider.Connect(Sim.PhoneNumber, callee, networkEvent);
        }

        public async void ReceiveCall(PhoneNumber caller, NetworkEvent networkEvent)
        {
            networkEvent.CallDurationInMs = ((DateTime.Now) - networkEvent.CallStartTime).TotalMilliseconds;
            networkEvent.CalleePhoneNumber = Sim.PhoneNumber.ToString();
            networkEvent.CalleeNetworkProvider = Sim.NetworkProvider.ToString();

            // Log successful call
            var eventLog = ServiceFactory.CreateNetworkEventSourceProxy();
            await eventLog.QueueNetworkEvent(networkEvent);
        }
    }
}
