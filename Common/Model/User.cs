using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public class User
    {
        public User()
        {
            Id = Guid.NewGuid();
        }

        [DataMember]
        public Guid Id { get; private set; }
        [DataMember]
        public String Name { get; set; }
        [DataMember]
        public EmailAddress  Email { get; set; }
        [DataMember]
        public MobilePhone MobilePhone { get; set; }

        public void MakeCall(PhoneNumber phoneNumber)
        {
            if(phoneNumber != null)
                MobilePhone?.TransmitCall(phoneNumber);
        }

    }
}
