using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public class NetworkEvent
    {
        public NetworkEvent()
        {
            Id = Guid.NewGuid();
        }

        [DataMember]
        public Guid Id { get; private set; }
        [DataMember]
        public String CallerPhoneNumber { get; set; }
        [DataMember]
        public String CallerNetworkProvider { get; set; }
        [DataMember]
        public String CalleePhoneNumber { get; set; }
        [DataMember]
        public String CalleeNetworkProvider { get; set; }
        [DataMember]
        public double CallDurationInMs { get; set; }
        [DataMember]
        public DateTime CallStartTime { get; set; }
    }
}
