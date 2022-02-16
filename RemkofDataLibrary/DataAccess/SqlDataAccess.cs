using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace RemkofDataLibrary.DataAccess
{
    public class SqlDataAccess : ISqlDataAccess
    {
        private readonly IConfiguration _config;

        public SqlDataAccess(IConfiguration config)
        {
            _config = config;
            DefaultTypeMap.MatchNamesWithUnderscores = true;
        }

        public string GetConnectionString(string connectionName = "PostgreSQL")
        {
            return _config.GetConnectionString(connectionName);
        }

        /// <summary>
        /// Загружает данные из БД.
        /// </summary>
        /// <typeparam name="T">Тип данных для загрузки</typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="sql">SQL код для выполнения</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<List<T>> LoadData<T, U>(string sql, U parameters)
        {
            string connectionString = GetConnectionString();
            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                var data = await connection.QueryAsync<T>(sql, parameters);
                return data.ToList();
            }
        }

        public async Task ExecuteSQLQuery<T>(string sql, T data)
        {
            using (IDbConnection connection = new NpgsqlConnection(GetConnectionString()))
            {
                try
                {
                    await connection.ExecuteAsync(sql, data);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        public async Task<int> InsertDataIntoDatabaseSingle<T>(string sql, T data)
        {
            using (IDbConnection connection = new NpgsqlConnection(GetConnectionString()))
            {
                try
                {
                    return await connection.QuerySingleOrDefaultAsync<int>(sql, data);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }
}
