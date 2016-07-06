using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public class SIM
    {
        [DataMember]
        public NetworkProvider NetworkProvider { get; private set; }
        [DataMember]
        public Guid Id { get; private set; }
        [DataMember]
        public PhoneNumber PhoneNumber { get; private set; }

        private SIM(NetworkProvider networkProvider)
        {
            Id = Guid.NewGuid();
            NetworkProvider = networkProvider;
        }

        public async static Task<SIM> Create(NetworkProvider networkProvider)
        {
            var sim = new SIM(networkProvider);
            await sim.Initialise();
            return sim;
        }

        private async Task Initialise()
        {
            // Initialise any properties required at creation point
            var networkProviderService = ServiceFactory.CreateNetworkProviderProxy(NetworkProvider);
            PhoneNumber = await networkProviderService.AllocatePhoneNumber();
        }
    }
}
