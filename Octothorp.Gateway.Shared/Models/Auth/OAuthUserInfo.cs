namespace Octothorp.Gateway.Shared.Models.Auth
{
    public class OAuthUserInfo
    {
        public string Provider { get; }
        public string Id { get; }
        public string Name { get; }

        public OAuthUserInfo(string provider, string id, string name)
        {
            Provider = provider;
            Id = id;
            Name = name;
        }
    }
}