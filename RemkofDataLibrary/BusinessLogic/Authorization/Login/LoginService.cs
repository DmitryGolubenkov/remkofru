using RemkofDataLibrary.DataAccess;
using RemkofDataLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RemkofDataLibrary.BusinessLogic.Authorization.Login
{
    public class LoginService : ILoginService
    {
        private readonly ISqlDataAccess _db;

        public LoginService(ISqlDataAccess db)
        {
            _db = db;
        }

        public async Task<LoginStatus> Login(string username, string password)
        {
            //1. Проверяем, существует ли такой пользователь, и загружаем соль
            string sql = $"SELECT * FROM users WHERE (username='{username}');";
            List<User> usersList = await _db.LoadData<User, dynamic>(sql, new { });
            if (usersList.Count == 0)
                return LoginStatus.IncorrectLogin;

            User user = usersList[0];
            //2. Солим пароль, проверяем, совпадают ли хэш в базе и хэш переданного пароля
            string saltedPassword = PasswordUtilities.SaltPassword(password, user.PasswordSalt);
            string hash = PasswordUtilities.ComputeSha256Hash(saltedPassword);

            if (hash != user.PasswordHash)
                return LoginStatus.IncorrectPassword;

            return LoginStatus.Success;
        }
    }
}
