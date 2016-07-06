using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class ServiceFactory
    {
        private readonly static String NETWORK_PROVIDER_URI = "fabric:/CellNetworkSimulation/NetworkProviderService";
        private readonly static String NETWORK_PROVIDER_MANAGER_URI = "fabric:/CellNetworkSimulation/NetworkProviderManagerService";
        private readonly static String USER_MANAGEMENT_URI = "fabric:/CellNetworkSimulation/UserManagementService";
        private readonly static String NETWORK_EVENT_SOURCE_URI = "fabric:/CellNetworkSimulation/NetworkEventSource";

        public static INetworkProviderService CreateNetworkProviderProxy(NetworkProvider networkProvider)
        {
            // TODO: build partition key from network provider id
            ServicePartitionKey partitionKey = null;
            return ServiceProxy.Create<INetworkProviderService>(new Uri(NETWORK_PROVIDER_URI), new ServicePartitionKey(1));
        }

        public static INetworkProviderManagerService CreateNetworkProviderManagerProxy()
        {
            return ServiceProxy.Create<INetworkProviderManagerService>(new Uri(NETWORK_PROVIDER_MANAGER_URI), new ServicePartitionKey(1));
        }

        public static IUserManagementService CreateUserManagementProxy()
        {
            return ServiceProxy.Create<IUserManagementService>(new Uri(USER_MANAGEMENT_URI), new ServicePartitionKey(1));
        }

        public static INetworkEventSource CreateNetworkEventSourceProxy()
        {
            return ServiceProxy.Create<INetworkEventSource>(new Uri(NETWORK_EVENT_SOURCE_URI), new ServicePartitionKey(1));
        }
    }
}
