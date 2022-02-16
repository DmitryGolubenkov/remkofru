using RemkofDataLibrary.DataAccess;
using RemkofDataLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RemkofDataLibrary.BusinessLogic.Admininstraror
{
    public class UsersService : IUsersService
    {
        private readonly ISqlDataAccess _db;

        public UsersService(ISqlDataAccess db)
        {
            _db = db;
        }

        public async Task<List<User>> GetUsers()
        {
            string sql = "SELECT * FROM users";
            var usersList = await _db.LoadData<User, dynamic>(sql, new { });
            return usersList;
        }

        public async Task UpdateUser(User user)
        {
            string sql = $"UPDATE users SET username='{user.Username}', email='{user.Email}', is_activated='{user.IsActivated}' WHERE user_id={user.UserId}";
            await _db.ExecuteSQLQuery(sql, new { });

        }

        public async Task DeleteUser(User user)
        {
            string sql = $"DELETE FROM users WHERE (user_id={user.UserId})";
            await _db.ExecuteSQLQuery(sql, new { });
        }
    }
}
