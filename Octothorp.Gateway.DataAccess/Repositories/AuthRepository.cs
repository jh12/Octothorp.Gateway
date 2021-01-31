using System;
using System.Threading.Tasks;
using Octothorp.Gateway.Shared.Exceptions.Auth;
using Octothorp.Gateway.Shared.Models.Auth;
using Octothorp.Gateway.Shared.Repositories.Interfaces;

namespace Octothorp.Gateway.DataAccess.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        public async Task<UserInfo> TrySigninAsync(OAuthUserInfo authUserInfo)
        {
            throw new UserNotYetApprovedException();
        }
    }
}