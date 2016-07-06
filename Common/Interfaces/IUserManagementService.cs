using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface IUserManagementService : IService
    {
        Task AddUser(User user);
        Task<bool> RemoveUser(Guid userId);
        Task<User> GetUser(Guid userId);
        Task<List<User>> GetUsers();
        Task<bool> UpdateUser(User user);
    }
}
