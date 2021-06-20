using System.Threading.Tasks;

namespace RemkofDataLibrary.BusinessLogic.Authorization.Login
{
    public interface ILoginService
    {
        Task<LoginStatus> Login(string username, string password);
    }
}