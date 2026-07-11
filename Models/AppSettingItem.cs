using System;

namespace Housing_rental.Models
{
    public class AppSettingItem
    {
        public int SettingId { get; set; }
        public string SettingKey { get; set; }
        public string SettingValue { get; set; }
        public string Description { get; set; }
    }
}
