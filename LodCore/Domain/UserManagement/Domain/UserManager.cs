﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Common;
using Journalist;
using Journalist.Extensions;
using NHibernate.Linq;
using NHibernate.Util;
using NotificationService;
using ProjectManagement.Application;
using ProjectManagement.Infrastructure;
using UserManagement.Application;
using UserManagement.Infrastructure;
using IMailer = UserManagement.Application.IMailer;
using IUserRepository = UserManagement.Infrastructure.IUserRepository;

namespace UserManagement.Domain
{
    public class UserManager : IUserManager
    {
        private readonly IConfirmationService _confirmationService;
        private readonly IUserRepository _userRepository;
        private readonly IProjectProvider _projectProvider;
        private readonly PaginationSettings _paginationSettings;
        private readonly IProjectMembershipRepostiory _projectMembershipRepostiory;
        private readonly IMailer _mailer;
        private readonly IPasswordChangeRequestRepository _passwordChangeRequestRepository;


        public UserManager(
            IUserRepository userRepository,
            IConfirmationService confirmationService, PaginationSettings paginationSettings, IProjectProvider projectProvider, IProjectMembershipRepostiory projectMembershipRepostiory, RelativeEqualityComparer relativeEqualityComparer, IMailer mailer, IPasswordChangeRequestRepository passwordChangeRequestRepository)
        {
            Require.NotNull(userRepository, nameof(userRepository));
            Require.NotNull(confirmationService, nameof(confirmationService));

            _userRepository = userRepository;
            _confirmationService = confirmationService;
            _paginationSettings = paginationSettings;
            _projectProvider = projectProvider;
            _projectMembershipRepostiory = projectMembershipRepostiory;
            _mailer = mailer;
            _passwordChangeRequestRepository = passwordChangeRequestRepository;
        }

        public List<Account> GetUserList(Func<Account, bool> criteria = null)
        {
            return _userRepository.GetAllAccounts(criteria);
        }

        public List<Account> GetUserList(int pageNumber, Func<Account, bool> criteria = null)
        {
            var projectToSkip = _paginationSettings.PageSize*pageNumber;
            var projectsToTake = _paginationSettings.PageSize;

            return _userRepository.GetSomeAccounts(
                projectToSkip, 
                projectsToTake, 
                account => account.RegistrationTime, 
                criteria);
        }

        public Account GetUser(int userId)
        {
            Require.Positive(userId, nameof(userId));

            var account = _userRepository.GetAccount(userId);
            if (account == null)
            {
                throw new AccountNotFoundException();
            }

            return account;
        }

        public void CreateUser(CreateAccountRequest request)
        {
            Require.NotNull(request, nameof(request));

            var doesExist = EnumerableExtensions.Any(GetUserList(account => account.Email.Address == request.Email));
            if (doesExist)
            {
                throw new AccountAlreadyExistsException();
            }

            var newAccount = new Account(
                request.Firstname,
                request.Lastname,
                new MailAddress(request.Email),
                Password.FromPlainString(request.Password), 
                AccountRole.User,
                ConfirmationStatus.Unconfirmed,
                DateTime.Now,
                request.Profile,
                0,
                0);

            var userId = _userRepository.CreateAccount(newAccount);

            _confirmationService.SetupEmailConfirmation(userId);
        }

        public void UpdateUser(Account account)
        {
            Require.NotNull(account, nameof(account));

            var accountExists = _userRepository.GetAccount(account.UserId) != null;
            if (!accountExists)
            {
                throw new AccountNotFoundException();
            }

            _userRepository.UpdateAccount(account);
        }

        public void InitiatePasswordChangingProcedure(int userId)
        {
            Require.Positive(userId, nameof(userId));

            var userToInitiateProcedure = GetUser(userId);

            var request = _passwordChangeRequestRepository.GetPasswordChangeRequest(userId);

            if (_passwordChangeRequestRepository.GetPasswordChangeRequest(userId) == null)
            {
                request = new PasswordChangeRequest(userId, Extensions.GenerateToken());
            }

            var passwordChangeRequest = request;

            var link = $"http://api.lod-misis.ru/password/{passwordChangeRequest.Token}";

            _passwordChangeRequestRepository.SavePasswordChangeRequestt(request);

            _mailer.SendPasswordResertMail(link, userToInitiateProcedure.Email);
        }

        public List<Account> GetUserList(string searchString)
        {
            Require.NotEmpty(searchString, nameof(searchString));

            var allProjectMemberships = _projectMembershipRepostiory.GetAllProjectMemberships().ToList();

            var allUsers = new HashSet<Account>(_userRepository.GetAllAccounts());

            var allUsersToSearchByRole =
                new HashSet<Account>(
                    allProjectMemberships.Select(membership => allUsers.Single(account => account.UserId == membership.DeveloperId)));

            var userRolesDictionary = allUsersToSearchByRole.ToDictionary(user => user,
                user =>
                    allProjectMemberships.Where(membership => membership.DeveloperId == user.UserId)
                        .Select(that => that.Role));

            return userRolesDictionary.Where(
                pair =>
                    pair.Value.Any(role => Extensions.Contains(role, searchString)))
                .Select(pair => pair.Key)
                .Union(
                    allUsers.Where(
                        account =>
                            Extensions.Contains($"{account.Firstname} {account.Lastname}",
                                searchString))).ToList();
        }
    }
}