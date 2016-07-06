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

namespace UserManagementService
{
    internal sealed class UserManagementService : StatefulService, IUserManagementService
    {
        private Task<IReliableDictionary<Guid, User>> _users
            => this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, User>>("Users");

        public UserManagementService(StatefulServiceContext context)
            : base(context)
        { }

        public async Task AddUser(User user)
        {
            var users = await _users;
            using (var tx = CreateTransaction())
            {
                await users.AddAsync(tx, user.Id, user);
                await tx.CommitAsync();
            }
        }

        public async Task<User> GetUser(Guid userId)
        {
            User user = null;
            var users = await _users;
            using (var tx = CreateTransaction())
            {
                var condition = await users.TryGetValueAsync(tx, userId);

                if (condition.HasValue)
                    user = condition.Value;
            }
            return user;
        }

        public async Task<List<User>> GetUsers()
        {
            List<User> userList = new List<User>();
            var cancellationToken = new CancellationToken();
            var users = await _users;
            using (var tx = CreateTransaction())
            {
                var enumerable = await users.CreateEnumerableAsync(tx);
                var e = enumerable.GetAsyncEnumerator();
                while(await e.MoveNextAsync(cancellationToken).ConfigureAwait(false))
                {
                    userList.Add(e.Current.Value);
                }
            }
            return userList;
        }

        public async Task<bool> RemoveUser(Guid userId)
        {
            bool success = false;
            var users = await _users;
            using (var tx = CreateTransaction())
            {
                var condition = await users.TryRemoveAsync(tx, userId);
                if (condition.HasValue)
                    success = true;
                await tx.CommitAsync();
            }
            return success;
        }

        public async Task<bool> UpdateUser(User user)
        {
            bool success = false;
            var users = await _users;
            using (var tx = CreateTransaction())
            {
                var existingUser = await GetUser(user.Id);
                success = await users.TryUpdateAsync(tx, existingUser.Id, user, existingUser);
                await tx.CommitAsync();
            }
            return success;
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]
            {
                new ServiceReplicaListener(context =>
                    this.CreateServiceRemotingListener(context))
            };
        }

        private ITransaction CreateTransaction()
        {
            return StateManager.CreateTransaction();
        }

    }
}
