using RemkofDataLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RemkofDataLibrary.BusinessLogic.Admininstraror
{
    public interface IUsersService
    {
        Task DeleteUser(User user);
        Task<List<User>> GetUsers();
        Task UpdateUser(User user);
    }
}