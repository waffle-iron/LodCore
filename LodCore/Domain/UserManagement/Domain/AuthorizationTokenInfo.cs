﻿using System;
using Journalist;

namespace UserManagement.Domain
{
    public class AuthorizationTokenInfo
    {
        public AuthorizationTokenInfo(int userId, string token, DateTime creationTime, AccountRole role)
        {
            Require.Positive(userId, nameof(userId));
            Require.NotEmpty(token, nameof(token));

            UserId = userId;
            Token = token;
            CreationTime = creationTime;
            Role = role;
        }

        public int UserId { get; private set; }

        public AccountRole Role { get; private set; }

        public string Token { get; private set; }

        public DateTime CreationTime { get; set; }
    }
}