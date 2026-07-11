using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Housing_rental.Models;

namespace Housing_rental.DAL
{
    public class SettingsRepository
    {
        public List<AppSettingItem> GetAll()
        {
            const string sql = "EXEC dbo.sp_Settings_GetAll;";

            List<AppSettingItem> settings = new List<AppSettingItem>();

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        settings.Add(MapSetting(reader));
                    }
                }
            }

            return settings;
        }

        public AppSettingItem GetByKey(string settingKey)
        {
            const string sql = "EXEC dbo.sp_Settings_GetByKey @SettingKey;";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@SettingKey", settingKey);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return MapSetting(reader);
                }
            }
        }

        public string GetValue(string settingKey, string defaultValue)
        {
            const string sql =
                "SELECT SettingValue FROM dbo.AppSettings WHERE SettingKey = @SettingKey;";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@SettingKey", settingKey);
                connection.Open();
                object value = command.ExecuteScalar();
                return value == null || value == DBNull.Value
                    ? defaultValue
                    : Convert.ToString(value);
            }
        }

        public AppSettingItem Update(
            string settingKey, string settingValue, int updatedByUserId)
        {
            const string sql =
                "EXEC dbo.sp_Settings_Update @SettingKey, @SettingValue, @UpdatedByUserId;";

            using (SqlConnection connection = DbConnectionFactory.CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@SettingKey", settingKey);
                command.Parameters.Add(SqlHelper.Parameter("@SettingValue", settingValue));
                command.Parameters.AddWithValue("@UpdatedByUserId", updatedByUserId);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return MapSetting(reader);
                }
            }
        }

        private static AppSettingItem MapSetting(SqlDataReader reader)
        {
            return new AppSettingItem
            {
                SettingId = Convert.ToInt32(reader["SettingId"]),
                SettingKey = Convert.ToString(reader["SettingKey"]),
                SettingValue = Convert.ToString(reader["SettingValue"]),
                Description = Convert.ToString(reader["Description"])
            };
        }
    }
}
