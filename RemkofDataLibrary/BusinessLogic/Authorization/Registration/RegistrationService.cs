using RemkofDataLibrary.DataAccess;
using RemkofDataLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RemkofDataLibrary.BusinessLogic.Authorization.Registration
{
    public class RegistrationService : IRegistrationService
    {
        private readonly ISqlDataAccess _db;
        public RegistrationService(ISqlDataAccess db)
        {
            _db = db;
        }

        public async Task<RegistrationStatus> RegisterUser(string username, string email, string password, bool isActivated = false)
        {
            //1. Проверяем, существует ли этот пользователь
            // Если существует - возвращаем соответствующий статус
            string sql = $"SELECT * FROM users WHERE (email='{email}' OR username='{username}');";
            List<User> alreadyExistingUsers = await _db.LoadData<User, dynamic>(sql, new { });
            if (alreadyExistingUsers.Count > 0)
            {
                if (alreadyExistingUsers[0].Email == email && alreadyExistingUsers[0].Username == username)
                    return RegistrationStatus.UsernameAndEmailExists;

                return alreadyExistingUsers[0].Email == email ? RegistrationStatus.EmailExists : RegistrationStatus.UsernameExists;
            }

            string salt = PasswordUtilities.CreateRandomSalt();
            string saltedPassword = PasswordUtilities.SaltPassword(password, salt);

            User user = new User()
            {
                Username = username,
                Email = email,
                PasswordHash = PasswordUtilities.ComputeSha256Hash(saltedPassword),
                PasswordSalt = salt,
                IsActivated = isActivated
            };

            //2. Добавляем нового пользователя в базу данных
            sql = $"INSERT INTO users (username, email, password_hash, password_salt, is_activated) VALUES ('{user.Username}', '{user.Email}', '{user.PasswordHash}', '{user.PasswordSalt}', '{user.IsActivated}')";
            int? result = await _db.InsertDataIntoDatabaseSingle(sql, user);

            if (result is null || result == -1)
                return RegistrationStatus.Fail;

            return RegistrationStatus.Success;
        }
    }
}
