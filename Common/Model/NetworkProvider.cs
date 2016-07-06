using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public class NetworkProvider
    {
        [DataMember]
        public String Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
