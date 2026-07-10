using System;
using Housing_rental.DAL;
using Housing_rental.Models;

namespace Housing_rental.BLL
{
    public class AuthService
    {
        private readonly UserRepository _userRepository;
        private readonly AuditRepository _auditRepository;

        public AuthService()
        {
            _userRepository = new UserRepository();
            _auditRepository = new AuditRepository();
        }

        public ServiceResult<User> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return ServiceResult<User>.Failure("Username is required.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return ServiceResult<User>.Failure("Password is required.");
            }

            User user = _userRepository.GetByUsername(username.Trim());

            if (user == null)
            {
                return ServiceResult<User>.Failure("Invalid username or password.");
            }

            if (!user.IsActive)
            {
                return ServiceResult<User>.Failure("This user account is inactive.");
            }

            string passwordHash = PasswordHasher.ComputeSha256Hash(password, user.PasswordSalt);

            if (!string.Equals(passwordHash, user.PasswordHash, StringComparison.OrdinalIgnoreCase))
            {
                TryAudit(user.UserId, "Login Failed", "Users", user.UserId.ToString(), "Invalid password.");
                return ServiceResult<User>.Failure("Invalid username or password.");
            }

            _userRepository.UpdateLastLogin(user.UserId);
            CurrentSession.Start(user);
            TryAudit(user.UserId, "Login Success", "Users", user.UserId.ToString(), "User logged in successfully.");

            return ServiceResult<User>.Success(user, "Login successful.");
        }

        private void TryAudit(int? userId, string actionName, string tableName, string recordId, string description)
        {
            try
            {
                _auditRepository.Add(userId, actionName, tableName, recordId, description);
            }
            catch
            {
                // Audit logging should not block login during a university lab demo.
            }
        }
    }
}
