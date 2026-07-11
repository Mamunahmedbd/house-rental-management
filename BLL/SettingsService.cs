using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Housing_rental.DAL;
using Housing_rental.Models;

namespace Housing_rental.BLL
{
    public class SettingsService
    {
        private readonly SettingsRepository _settingsRepository;
        private readonly UserRepository _userRepository;
        private readonly AuditRepository _auditRepository;

        public SettingsService()
        {
            _settingsRepository = new SettingsRepository();
            _userRepository = new UserRepository();
            _auditRepository = new AuditRepository();
        }

        public ServiceResult<List<AppSettingItem>> GetAllSettings()
        {
            if (!CurrentSession.IsAdmin)
            {
                return ServiceResult<List<AppSettingItem>>.Failure(
                    "Only Admin users can manage application settings.");
            }

            try
            {
                List<AppSettingItem> settings = _settingsRepository.GetAll();
                return ServiceResult<List<AppSettingItem>>.Success(
                    settings, "Settings loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<List<AppSettingItem>>.Failure(
                    "Unable to load settings. " + ex.Message);
            }
        }

        public ServiceResult<AppSettingItem> GetSettingByKey(string key)
        {
            if (!CurrentSession.IsAdmin)
            {
                return ServiceResult<AppSettingItem>.Failure(
                    "Only Admin users can manage application settings.");
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                return ServiceResult<AppSettingItem>.Failure(
                    "Setting key is required.");
            }

            try
            {
                AppSettingItem setting = _settingsRepository.GetByKey(key.Trim());

                if (setting == null)
                {
                    return ServiceResult<AppSettingItem>.Failure("Setting not found.");
                }

                return ServiceResult<AppSettingItem>.Success(
                    setting, "Setting loaded successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult<AppSettingItem>.Failure(
                    "Unable to load setting. " + ex.Message);
            }
        }

        public ServiceResult UpdateSetting(string key, string value)
        {
            if (!CurrentSession.IsAdmin)
            {
                return ServiceResult.Failure(
                    "Only Admin users can update application settings.");
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                return ServiceResult.Failure("Setting key is required.");
            }

            ServiceResult validation = ValidateSettingValue(key.Trim(), value);

            if (!validation.IsSuccess)
            {
                return validation;
            }

            try
            {
                string normalizedValue = NormalizeValue(key.Trim(), value);
                _settingsRepository.Update(
                    key.Trim(), normalizedValue, CurrentSession.User.UserId);
                return ServiceResult.Success("Setting updated successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure(
                    "Unable to update setting. " + ex.Message);
            }
        }

        public string GetDefaultCurrency()
        {
            try
            {
                string value = _settingsRepository.GetValue("DefaultCurrency", "USD");
                value = string.IsNullOrWhiteSpace(value)
                    ? "USD" : value.Trim().ToUpperInvariant();
                return value.Length == 3 ? value : "USD";
            }
            catch
            {
                return "USD";
            }
        }

        public ServiceResult ChangeMyPassword(
            string currentPassword,
            string newPassword,
            string confirmNewPassword)
        {
            if (!CurrentSession.IsAuthenticated)
            {
                return ServiceResult.Failure(
                    "You must be logged in to change your password.");
            }

            if (string.IsNullOrWhiteSpace(currentPassword))
            {
                return ServiceResult.Failure("Current password is required.");
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                return ServiceResult.Failure("New password is required.");
            }

            if (newPassword.Length < 6)
            {
                return ServiceResult.Failure(
                    "New password must be at least 6 characters.");
            }

            if (newPassword != confirmNewPassword)
            {
                return ServiceResult.Failure(
                    "New password and confirmation do not match.");
            }

            if (currentPassword == newPassword)
            {
                return ServiceResult.Failure(
                    "New password must be different from the current password.");
            }

            try
            {
                User currentUser = _userRepository.GetByUsername(
                    CurrentSession.User.Username);

                if (currentUser == null)
                {
                    return ServiceResult.Failure(
                        "Unable to verify your account.");
                }

                string currentHash = PasswordHasher.ComputeSha256Hash(
                    currentPassword, currentUser.PasswordSalt);

                if (!string.Equals(
                    currentHash,
                    currentUser.PasswordHash,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return ServiceResult.Failure(
                        "Current password is incorrect.");
                }

                string newSalt = Guid.NewGuid().ToString("N");
                string newHash = PasswordHasher.ComputeSha256Hash(newPassword, newSalt);
                _userRepository.ResetPassword(currentUser.UserId, newHash, newSalt);

                TryAudit(
                    "Change Password", "Users",
                    currentUser.UserId.ToString(),
                    "User changed their own password.");

                return ServiceResult.Success("Password changed successfully.");
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure(
                    "Unable to change password. " + ex.Message);
            }
        }

        private ServiceResult ValidateSettingValue(string settingKey, string value)
        {
            switch (settingKey)
            {
                case "ApplicationName":
                    if (string.IsNullOrWhiteSpace(value))
                        return ServiceResult.Failure("Application name is required.");
                    if (value.Trim().Length > 100)
                        return ServiceResult.Failure(
                            "Application name must not exceed 100 characters.");
                    break;

                case "DefaultCurrency":
                    if (string.IsNullOrWhiteSpace(value))
                        return ServiceResult.Failure("Default currency is required.");
                    value = value.Trim().ToUpperInvariant();
                    if (value.Length != 3
                        || !Regex.IsMatch(value, @"^[A-Z]{3}$"))
                        return ServiceResult.Failure(
                            "Currency must be a 3-letter ISO 4217 code (e.g., USD, BDT, EUR).");
                    break;

                case "RentDueDay":
                    if (string.IsNullOrWhiteSpace(value))
                        return ServiceResult.Failure("Rent due day is required.");
                    int dueDay;
                    if (!int.TryParse(value.Trim(), out dueDay)
                        || dueDay < 1 || dueDay > 28)
                        return ServiceResult.Failure(
                            "Rent due day must be a number between 1 and 28.");
                    break;

                case "ReceiptFooter":
                    if (string.IsNullOrWhiteSpace(value))
                        return ServiceResult.Failure("Receipt footer text is required.");
                    if (value.Trim().Length > 300)
                        return ServiceResult.Failure(
                            "Receipt footer must not exceed 300 characters.");
                    break;

                default:
                    if (value != null && value.Length > 300)
                        return ServiceResult.Failure(
                            "Setting value must not exceed 300 characters.");
                    break;
            }

            return ServiceResult.Success("Setting value is valid.");
        }

        private string NormalizeValue(string key, string value)
        {
            if (value == null) return null;

            switch (key)
            {
                case "DefaultCurrency":
                    return value.Trim().ToUpperInvariant();
                case "RentDueDay":
                    return value.Trim();
                default:
                    return value.Trim();
            }
        }

        private void TryAudit(
            string actionName, string tableName,
            string recordId, string description)
        {
            try
            {
                int? userId = CurrentSession.User == null
                    ? (int?)null : CurrentSession.User.UserId;
                _auditRepository.Add(userId, actionName, tableName, recordId, description);
            }
            catch
            {
                // Audit logging should not block settings operations.
            }
        }
    }
}
