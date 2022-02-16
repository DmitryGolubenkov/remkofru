using RemkofDataLibrary.DataAccess;
using RemkofDataLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemkofDataLibrary.BusinessLogic
{
    public class OptionsService : IOptionsService
    {
        private readonly ISqlDataAccess _db;

        public OptionsService(ISqlDataAccess db)
        {
            _db = db;
        }

        public async Task<List<OptionModel>> GetOptions()
        {
            List<OptionModel> options = await _db.LoadData<OptionModel, dynamic>("SELECT * FROM options", new { });
            return options;
        }

        public async Task<bool> GetRegistrationSetting()
        {
            List<string> result = await _db.LoadData<string, dynamic>("SELECT key_value FROM options WHERE (key_name='is_registration_enabled') LIMIT 1", new { });
            if (result.Count > 0)
                return result[0].Equals("True", System.StringComparison.InvariantCultureIgnoreCase) ? true : false;

            //Выполнение переходит сюда, если в базе данных не оказалось данной настройки
            await SetRegistrationSettingFirstTime();
            return true;
        }

        private async Task SetRegistrationSettingFirstTime()
        {
            await _db.ExecuteSQLQuery($"INSERT INTO options (key_name, key_value) VALUES ('is_registration_enabled', true)", new { });
        }

        public async Task SetRegistrationSetting(bool value)
        {
            await _db.ExecuteSQLQuery($"UPDATE options SET key_value='{value}' WHERE (key_name='is_registration_enabled')", new { });
        }

        public async Task SaveOptions(List<OptionModel> optionList)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"INSERT INTO options (key_name, key_value) VALUES ");
            foreach (var option in optionList)
            {
                stringBuilder.Append($"('{option.KeyName}', '{option.KeyValue}'), ");
                //await _db.ExecuteSQLQuery($"INSERT INTO options (key_name, key_value) VALUES  ")
            }
            stringBuilder.Remove(stringBuilder.Length - 2, 2);
            stringBuilder.Append(" ON CONFLICT (key_name) DO UPDATE SET key_value=excluded.key_value;");

            await _db.ExecuteSQLQuery(stringBuilder.ToString(), new { });
        }
    }
}
