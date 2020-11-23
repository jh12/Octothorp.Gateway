using System.Threading.Tasks;
using Octothorp.Gateway.Shared.Models.Auth;

namespace Octothorp.Gateway.Shared.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<UserInfo> TrySigninAsync(OAuthUserInfo authUserInfo);
    }
}