using System;
using System.Collections.Generic;

namespace MassiveKnob.Settings
{
    public class Settings
    {
        public string SerialPort { get; set; }
        public List<KnobSettings> Knobs { get; set; }
        
        
        public static Settings Default()
        {
            return new Settings
            {
                Knobs = new List<KnobSettings>()
            };
        }


        public class KnobSettings
        {
            public Guid? DeviceId { get; set; }
        }
    }
}
