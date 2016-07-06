using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface INetworkEventSource : IService
    {
        Task QueueNetworkEvent(NetworkEvent e);
        Task<List<NetworkEvent>> GetNetworkLogs();
    }
}
