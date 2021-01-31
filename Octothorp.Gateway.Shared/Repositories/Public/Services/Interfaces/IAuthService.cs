using System.Threading.Tasks;
using Octothorp.Gateway.DTOs.V1.Auth;

namespace Octothorp.Gateway.Shared.Repositories.Public.Services.Interfaces
{
    public interface IAuthService
    {
        Task<CurrentUser?> GetCurrentUserAsync();
    }
}