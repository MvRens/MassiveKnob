using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace MassiveKnob.Settings
{
    public class Settings
    {
        public DeviceSettings Device { get; set; }
        public List<ActionSettings> Actions { get; set; }
        
        
        public static Settings Default()
        {
            return new Settings
            {
                Device = null,
                Actions = new List<ActionSettings>()
            };
        }


        public class DeviceSettings
        {
            public Guid? PluginId { get; set; }
            public Guid? DeviceId { get; set; }
            public JObject Settings { get; set; }
        }
        

        public class ActionSettings
        {
            public Guid PluginId { get; set; }
            public Guid ActionId { get; set; }
            public JObject Settings { get; set; }
        }
    }
}
