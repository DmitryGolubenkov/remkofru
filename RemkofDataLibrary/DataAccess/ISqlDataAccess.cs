using System.Collections.Generic;
using System.Threading.Tasks;

namespace RemkofDataLibrary.DataAccess
{
    public interface ISqlDataAccess
    {
        public Task<List<T>> LoadData<T, U>(string sql, U parameters);
        public Task ExecuteSQLQuery<T>(string sql, T data);
        public Task<int> InsertDataIntoDatabase<T>(string sql, T data);
    }
}
