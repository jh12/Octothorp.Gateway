using System;
using System.Threading.Tasks;
using Octothorp.Gateway.DTOs.V1.Auth;
using Octothorp.Gateway.Shared.Repositories.Public.Services.Interfaces;

namespace Octothorp.Gateway.DataAccess.Public.Services
{
    public class AuthService : IAuthService
    {
        public async Task<CurrentUser?> GetCurrentUserAsync()
        {
            throw new NotImplementedException();
        }
    }
}