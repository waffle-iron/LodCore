﻿using UserManagement.Domain;

namespace UserManagement.Infrastructure
{
    public interface IPasswordChangeRequestRepository
    {
        void SavePasswordChangeRequestt(PasswordChangeRequest request);

        PasswordChangeRequest GetPasswordChangeRequest(string token);

        PasswordChangeRequest GetPasswordChangeRequest(int userId);

        void DeletePasswordChangeRequest(PasswordChangeRequest request);
    }
}