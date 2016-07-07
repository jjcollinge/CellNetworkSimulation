using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Common;

namespace NetworkActivitySimulator
{
    internal sealed class NetworkActivitySimulator : StatelessService
    {
        private readonly String _serviceName;

        public NetworkActivitySimulator(StatelessServiceContext context)
            : base(context)
        {
            _serviceName = Context.ServiceTypeName;
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // Allow cluster services to start up
            Thread.Sleep(TimeSpan.FromSeconds(30));

            ServiceEventSource.Current.Message($"[{_serviceName}][INFO] Began simulation initialisation");

            var networkManager = ServiceFactory.CreateNetworkProviderManagerProxy();
            await networkManager.CreateNetwork();
            var networks = await networkManager.GetNetworkProviders();
            var userManager = ServiceFactory.CreateUserManagementProxy();
            var phoneNumbers = new List<PhoneNumber>();

            ServiceEventSource.Current.Message($"[{_serviceName}][INFO] Created service proxies and network infrastructure");

            for (int i = 0; i < 5; i++)
            {
                // Create a new mobile phone and register a network provided SIM 
                // TODO: Change this to builder pattern
                MobilePhone phone = new MobilePhone(await SIM.Create(networks.First()));

                phone.AddLocation(new Location()
                {
                    Longitude = i * 10.0d,
                    Latitude = i * 10.0d,
                    Altitude = 2.0d
                });


                ServiceEventSource.Current.Message($"[{_serviceName}][INFO] Created mobile phone {phone.Sim.PhoneNumber}");

                try
                {
                    await phone.Activate();
                }
                catch(Exception ex)
                {
                    ServiceEventSource.Current.Message($"[{_serviceName}][ERROR] Exception thrown whilst activating the phone: {ex.Message}");
                    throw ex;
                }

                ServiceEventSource.Current.Message($"[{_serviceName}][INFO] Activated mobile phone {phone.Sim.PhoneNumber}");

                // Store phone numbers to call later
                phoneNumbers.Add(phone.Sim.PhoneNumber);

                // Create a new user and give activated mobile phone
                var user = new User();
                user.Email = new EmailAddress($"user{i}@contoso.com");
                user.Name = $"user{i}";
                user.MobilePhone = phone;
                ServiceEventSource.Current.Message($"[{_serviceName}][INFO] Created user {user.Name}");

                // Add user to management plane
                await userManager.AddUser(user);
                ServiceEventSource.Current.Message($"[{_serviceName}][INFO] Registered user {user.Name} with management service");
            }

            ServiceEventSource.Current.Message($"[{_serviceName}][INFO] Simulation intialised succesfully, starting simulation loop");
            long iterations = 0L;

            while (!cancellationToken.IsCancellationRequested)
            {
                var users = await userManager.GetUsers();
                ServiceEventSource.Current.Message($"[{_serviceName}][VERBOSE] Successfully retrieved {users.Count} users");

                foreach (var user in users)
                {
                    phoneNumbers.Remove(user.MobilePhone.Sim.PhoneNumber);
                    var rand = new Random().Next(0, phoneNumbers.Count);
                    var phoneNumberToCall = phoneNumbers[rand];
                    user.MakeCall(phoneNumberToCall);
                    ServiceEventSource.Current.Message($"[{_serviceName}][VERBOSE] {user.MobilePhone.Sim.PhoneNumber} has attempted to call {phoneNumberToCall}");
                    phoneNumbers.Add(user.MobilePhone.Sim.PhoneNumber);
                }

                ServiceEventSource.Current.Message($"[{_serviceName}][VERBOSE] Simluation loop iteration: {iterations++}");
                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }

            ServiceEventSource.Current.Message($"[{_serviceName}][INFO] Finished simulation");
        }
    }
}
