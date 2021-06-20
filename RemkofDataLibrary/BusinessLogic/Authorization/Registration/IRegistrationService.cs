using RemkofDataLibrary.Models;
using System.Threading.Tasks;

namespace RemkofDataLibrary.BusinessLogic.Authorization.Registration
{
    public interface IRegistrationService
    {
        Task<RegistrationStatus> RegisterUser(string username, string email, string password);
    }
}