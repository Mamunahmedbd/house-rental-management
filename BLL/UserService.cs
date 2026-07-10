using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Housing_rental.DAL;
using Housing_rental.Models;

namespace Housing_rental.BLL
{
    public class UserService
    {
        private readonly UserRepository _userRepository;
        private readonly RoleRepository _roleRepository;
        private readonly AuditRepository _auditRepository;

        public UserService()
        {
            _userRepository = new UserRepository();
            _roleRepository = new RoleRepository();
            _auditRepository = new AuditRepository();
        }

        public ServiceResult<List<User>> SearchUsers(string searchText)
        {
            if (!CurrentSession.IsAdmin)
            {
                return ServiceResult<List<User>>.Failure("Only Admin users can manage system users.");
            }

            try
            {
                List<User> users = _userRepository.Search((searchText ?? string.Empty).Trim());
                return ServiceResult<List<User>>.Success(users, "Users loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<List<User>>.Failure("Unable to load users. " + ex.Message);
            }
        }

        public ServiceResult<List<Role>> GetActiveRoles()
        {
            if (!CurrentSession.IsAdmin)
            {
                return ServiceResult<List<Role>>.Failure("Only Admin users can manage roles.");
            }

            try
            {
                List<Role> roles = _roleRepository.GetActiveRoles();
                return ServiceResult<List<Role>>.Success(roles, "Roles loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<List<Role>>.Failure("Unable to load roles. " + ex.Message);
            }
        }

        public ServiceResult CreateUser(User user, string password, string confirmPassword)
        {
            ServiceResult validation = ValidateUser(user, true, password, confirmPassword);

            if (!validation.IsSuccess)
            {
                return validation;
            }

            try
            {
                if (_userRepository.UsernameExists(user.Username.Trim(), 0))
                {
                    return ServiceResult.Failure("Username already exists.");
                }

                string salt = Guid.NewGuid().ToString("N");
                user.Username = user.Username.Trim();
                user.FullName = user.FullName.Trim();
                user.Phone = CleanOptional(user.Phone);
                user.Email = CleanOptional(user.Email);
                user.PasswordSalt = salt;
                user.PasswordHash = PasswordHasher.ComputeSha256Hash(password, salt);

                int userId = _userRepository.Create(user);
                TryAudit("Create User", "Users", userId.ToString(), "Created user " + user.Username + ".");

                return ServiceResult.Success("User created successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure("Unable to create user. " + ex.Message);
            }
        }

        public ServiceResult UpdateUser(User user)
        {
            ServiceResult validation = ValidateUser(user, false, null, null);

            if (!validation.IsSuccess)
            {
                return validation;
            }

            try
            {
                if (_userRepository.UsernameExists(user.Username.Trim(), user.UserId))
                {
                    return ServiceResult.Failure("Username already exists.");
                }

                user.Username = user.Username.Trim();
                user.FullName = user.FullName.Trim();
                user.Phone = CleanOptional(user.Phone);
                user.Email = CleanOptional(user.Email);

                _userRepository.Update(user);
                TryAudit("Update User", "Users", user.UserId.ToString(), "Updated user " + user.Username + ".");

                return ServiceResult.Success("User updated successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure("Unable to update user. " + ex.Message);
            }
        }

        public ServiceResult SetActiveStatus(int userId, bool isActive)
        {
            if (!CurrentSession.IsAdmin)
            {
                return ServiceResult.Failure("Only Admin users can change user status.");
            }

            if (userId <= 0)
            {
                return ServiceResult.Failure("Please select a valid user.");
            }

            if (CurrentSession.User != null && CurrentSession.User.UserId == userId && !isActive)
            {
                return ServiceResult.Failure("You cannot deactivate your own account while logged in.");
            }

            try
            {
                _userRepository.SetActiveStatus(userId, isActive);
                TryAudit(isActive ? "Activate User" : "Deactivate User", "Users", userId.ToString(), "Changed user active status.");
                return ServiceResult.Success(isActive ? "User activated successfully." : "User deactivated successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure("Unable to change user status. " + ex.Message);
            }
        }

        public ServiceResult ResetPassword(int userId, string password, string confirmPassword)
        {
            if (!CurrentSession.IsAdmin)
            {
                return ServiceResult.Failure("Only Admin users can reset passwords.");
            }

            if (userId <= 0)
            {
                return ServiceResult.Failure("Please select a valid user.");
            }

            ServiceResult passwordValidation = ValidatePassword(password, confirmPassword);

            if (!passwordValidation.IsSuccess)
            {
                return passwordValidation;
            }

            try
            {
                string salt = Guid.NewGuid().ToString("N");
                string passwordHash = PasswordHasher.ComputeSha256Hash(password, salt);
                _userRepository.ResetPassword(userId, passwordHash, salt);
                TryAudit("Reset Password", "Users", userId.ToString(), "Reset user password.");
                return ServiceResult.Success("Password reset successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure("Unable to reset password. " + ex.Message);
            }
        }

        private ServiceResult ValidateUser(User user, bool requirePassword, string password, string confirmPassword)
        {
            if (!CurrentSession.IsAdmin)
            {
                return ServiceResult.Failure("Only Admin users can manage system users.");
            }

            if (user == null)
            {
                return ServiceResult.Failure("User information is required.");
            }

            if (user.UserId < 0)
            {
                return ServiceResult.Failure("Invalid user selected.");
            }

            if (user.RoleId <= 0)
            {
                return ServiceResult.Failure("Please select a user role.");
            }

            if (string.IsNullOrWhiteSpace(user.FullName))
            {
                return ServiceResult.Failure("Full name is required.");
            }

            if (string.IsNullOrWhiteSpace(user.Username))
            {
                return ServiceResult.Failure("Username is required.");
            }

            if (user.Username.Trim().Length < 3)
            {
                return ServiceResult.Failure("Username must be at least 3 characters.");
            }

            if (!IsValidOptionalEmail(user.Email))
            {
                return ServiceResult.Failure("Please enter a valid email address.");
            }

            if (requirePassword)
            {
                return ValidatePassword(password, confirmPassword);
            }

            return ServiceResult.Success("User information is valid.");
        }

        private ServiceResult ValidatePassword(string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return ServiceResult.Failure("Password is required.");
            }

            if (password.Length < 6)
            {
                return ServiceResult.Failure("Password must be at least 6 characters.");
            }

            if (password != confirmPassword)
            {
                return ServiceResult.Failure("Password and confirm password do not match.");
            }

            return ServiceResult.Success("Password is valid.");
        }

        private string CleanOptional(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private bool IsValidOptionalEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return true;
            }

            return Regex.IsMatch(
                email.Trim(),
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase);
        }

        private void TryAudit(string actionName, string tableName, string recordId, string description)
        {
            try
            {
                int? userId = CurrentSession.User == null ? (int?)null : CurrentSession.User.UserId;
                _auditRepository.Add(userId, actionName, tableName, recordId, description);
            }
            catch
            {
                // Audit logging should not block Admin user-management work.
            }
        }
    }
}
