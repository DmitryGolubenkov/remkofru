using RemkofDataLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RemkofDataLibrary.BusinessLogic
{
    public interface IPricesService
    {
        Task AddPriceToDatabase(ServicePrice price);
        Task<List<ServicePrice>> GetPrices();
        Task SaveNewPriceList(List<ServicePrice> prices);
        Task UpdatePrices(List<ServicePrice> prices);
        Task DeletePrice(ServicePrice servicePrice);
    }
}