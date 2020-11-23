using Autofac;
using Octothorp.Gateway.DataAccess.Repositories;
using Octothorp.Gateway.Shared.Repositories.Interfaces;

namespace Octothorp.Gateway
{
    public class DataAccessModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AuthRepository>().As<IAuthRepository>();
        }
    }
}