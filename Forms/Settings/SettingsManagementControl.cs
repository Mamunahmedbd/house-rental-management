using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Housing_rental.BLL;
using Housing_rental.Models;

namespace Housing_rental.Forms.Settings
{
    public partial class SettingsManagementControl : UserControl
    {
        private readonly SettingsService _settingsService;

        public SettingsManagementControl()
        {
            _settingsService = new SettingsService();
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Show/hide Application Settings tab based on role
            if (!CurrentSession.IsAdmin)
            {
                tabControl.TabPages.Remove(tabSettings);
            }

            LoadCurrentSettings();
            LoadUserProfile();
        }

        private void LoadCurrentSettings()
        {
            if (!CurrentSession.IsAdmin) return;

            ServiceResult<List<AppSettingItem>> result =
                _settingsService.GetAllSettings();

            if (!result.IsSuccess)
            {
                SetSettingsStatus(result.Message, true);
                return;
            }

            foreach (AppSettingItem setting in result.Data)
            {
                switch (setting.SettingKey)
                {
                    case "ApplicationName":
                        txtApplicationName.Text = setting.SettingValue;
                        break;
                    case "DefaultCurrency":
                        txtDefaultCurrency.Text = setting.SettingValue;
                        break;
                    case "RentDueDay":
                        int dueDay;
                        nudRentDueDay.Value = int.TryParse(
                            setting.SettingValue, out dueDay)
                            ? Math.Max(1, Math.Min(28, dueDay))
                            : 5;
                        break;
                    case "ReceiptFooter":
                        txtReceiptFooter.Text = setting.SettingValue;
                        break;
                }
            }

            SetSettingsStatus("Settings loaded.", false);
        }

        private void LoadUserProfile()
        {
            if (CurrentSession.User == null) return;

            lblFullNameValue.Text = CurrentSession.User.FullName;
            lblUsernameValue.Text = CurrentSession.User.Username;
            lblRoleValue.Text = CurrentSession.User.RoleName;
            lblEmailValue.Text = string.IsNullOrWhiteSpace(
                CurrentSession.User.Email)
                ? "(not set)" : CurrentSession.User.Email;
            lblLastLoginValue.Text = CurrentSession.User.LastLoginAt.HasValue
                ? CurrentSession.User.LastLoginAt.Value
                    .ToString("yyyy-MM-dd HH:mm")
                : "(first login)";
        }

        private void BtnSaveSettings_Click(object sender, EventArgs e)
        {
            var updates = new Dictionary<string, string>
            {
                { "ApplicationName", txtApplicationName.Text },
                { "DefaultCurrency", txtDefaultCurrency.Text },
                { "RentDueDay", nudRentDueDay.Value.ToString() },
                { "ReceiptFooter", txtReceiptFooter.Text }
            };

            foreach (var kvp in updates)
            {
                ServiceResult result = _settingsService.UpdateSetting(
                    kvp.Key, kvp.Value);

                if (!result.IsSuccess)
                {
                    SetSettingsStatus(result.Message, true);
                    return;
                }
            }

            SetSettingsStatus("All settings saved successfully.", false);
        }

        private void BtnChangePassword_Click(object sender, EventArgs e)
        {
            ServiceResult result = _settingsService.ChangeMyPassword(
                txtCurrentPassword.Text,
                txtNewPassword.Text,
                txtConfirmNewPassword.Text);

            SetProfileStatus(result.Message, !result.IsSuccess);

            if (result.IsSuccess)
            {
                txtCurrentPassword.Clear();
                txtNewPassword.Clear();
                txtConfirmNewPassword.Clear();
            }
        }

        private void SetSettingsStatus(string message, bool isError)
        {
            lblSettingsStatus.Text = message;
            lblSettingsStatus.ForeColor = isError
                ? Color.Firebrick : Color.ForestGreen;
        }

        private void SetProfileStatus(string message, bool isError)
        {
            lblProfileStatus.Text = message;
            lblProfileStatus.ForeColor = isError
                ? Color.Firebrick : Color.ForestGreen;
        }
    }
}
