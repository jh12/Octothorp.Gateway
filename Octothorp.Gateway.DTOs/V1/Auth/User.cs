using System;

namespace Octothorp.Gateway.DTOs.V1.Auth
{
    public class User
    {
        public string Username { get; set; }

        public User(string? username)
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
        }
    }
}