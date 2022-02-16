using RemkofDataLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RemkofDataLibrary.BusinessLogic
{
    public interface IOptionsService
    {
        Task<List<OptionModel>> GetOptions();
        Task<bool> GetRegistrationSetting();
        Task SetRegistrationSetting(bool value);
        Task SaveOptions(List<OptionModel> optionList);
    }
}