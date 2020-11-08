namespace Octothorp.Gateway.DTOs.V1.Auth
{
    public class CurrentUser : User
    {
        public string Issuer { get; set; }
        public string Identifier { get; set; }

        public CurrentUser(string? username, string issuer, string identifier) : base(username)
        {
            Issuer = issuer;
            Identifier = identifier;
        }
    }
}