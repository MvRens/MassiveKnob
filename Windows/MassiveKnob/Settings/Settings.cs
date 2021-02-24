using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace MassiveKnob.Settings
{
    public class Settings
    {
        public DeviceSettings Device { get; set; }
        public List<ActionSettings> AnalogInput { get; set; }
        public List<ActionSettings> DigitalInput { get; set; }
        public List<ActionSettings> AnalogOutput { get; set; }
        public List<ActionSettings> DigitalOutput { get; set; }


        public void Verify()
        {
            if (AnalogInput == null) AnalogInput = new List<ActionSettings>();
            if (DigitalInput == null) DigitalInput = new List<ActionSettings>();
            if (AnalogOutput == null) AnalogOutput = new List<ActionSettings>();
            if (DigitalOutput == null) DigitalOutput = new List<ActionSettings>();
        }
        
        
        public Settings Clone()
        {
            return new Settings
            {
                Device = Device?.Clone(),
                AnalogInput = AnalogInput.Select(a => a?.Clone()).ToList(),
                DigitalInput = DigitalInput.Select(a => a?.Clone()).ToList(),
                AnalogOutput = AnalogOutput.Select(a => a?.Clone()).ToList(),
                DigitalOutput = DigitalOutput.Select(a => a?.Clone()).ToList()
            };
        }


        public class DeviceSettings
        {
            public Guid? DeviceId { get; set; }
            public JObject Settings { get; set; }
            
            public DeviceSettings Clone()
            {
                return new DeviceSettings
                {
                    DeviceId = DeviceId,
                    
                    // This is safe, as the JObject itself is never manipulated, only replaced
                    Settings = Settings
                };
            }
        }
        

        public class ActionSettings
        {
            public Guid ActionId { get; set; }
            public JObject Settings { get; set; }

            public ActionSettings Clone()
            {
                return new ActionSettings
                {
                    ActionId = ActionId,

                    // This is safe, as the JObject itself is never manipulated, only replaced
                    Settings = Settings
                };
            }
        }
    }
}
