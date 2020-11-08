namespace Octothorp.Gateway.DTOs.V1.Auth
{
    public class AuthProvider
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }

        public AuthProvider()
        {
        }

        public AuthProvider(string name, string displayName)
        {
            Name = name;
            DisplayName = displayName;
        }
    }
}