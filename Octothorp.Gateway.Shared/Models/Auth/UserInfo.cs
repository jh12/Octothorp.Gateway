using System;

namespace Octothorp.Gateway.Shared.Models.Auth
{
    public class UserInfo
    {
        public Guid UserId { get; }
        public string Username { get; }
        public string Nickname { get; }
        public bool IsApproved { get; }

        public UserInfo(Guid userId, string username, string nickname, bool isApproved)
        {
            UserId = userId;
            Username = username;
            Nickname = nickname;
            IsApproved = isApproved;
        }
    }
}