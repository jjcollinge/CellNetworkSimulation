using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface INetworkProviderService : IService
    {
        Task InitialisePhoneRange(PhoneNumberRange phoneNumberRange);
        Task<PhoneNumber> AllocatePhoneNumber();
        Task ActivateMobilePhone(MobilePhone mobile);
        Task<bool> Connect(PhoneNumber caller, PhoneNumber callee, NetworkEvent networkEvent);
    }
}
