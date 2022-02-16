using RemkofDataLibrary.DataAccess;
using RemkofDataLibrary.Models;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RemkofDataLibrary.BusinessLogic
{
    public class PricesService : IPricesService
    {
        private readonly ISqlDataAccess _db;

        public PricesService(ISqlDataAccess db)
        {
            _db = db;
        }

        public async Task<List<ServicePrice>> GetPrices()
        {
            string sql = "SELECT * FROM prices";
            return await _db.LoadData<ServicePrice, dynamic>(sql, new { });
        }

        public async Task SaveNewPriceList(List<ServicePrice> prices)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("DELETE FROM prices;");
                foreach(ServicePrice pr in prices)
            {
                builder.AppendLine($"INSERT INTO prices (service_name, price, view_priority) VALUES ('{pr.ServiceName}','{pr.Price}','{pr.ViewPriority}');");
            }
                
            string sql = builder.ToString();
            await _db.ExecuteSQLQuery(sql, prices);
        }

        public async Task AddPriceToDatabase(ServicePrice price)
        {
            string sql = $"INSERT INTO prices (service_name, price, view_priority) VALUES ('{price.ServiceName}', '{price.Price}', '{price.ViewPriority}')";
            await _db.ExecuteSQLQuery(sql, new { });
        }

        public async Task UpdatePrices(List<ServicePrice> prices)
        {
            foreach (ServicePrice price in prices)
            {
                string sql = $"UPDATE prices SET service_name='{price.ServiceName}', price='{price.Price}', view_priority='{price.ViewPriority}' WHERE price_id='{price.PriceId}'";
                await _db.ExecuteSQLQuery(sql, new { });
            }
        }

        public async Task DeletePrice(ServicePrice servicePrice)
        {
            string sql = $"DELETE FROM prices WHERE price_id={servicePrice.PriceId};";
            await _db.ExecuteSQLQuery(sql, new { });
        }
    }
}
