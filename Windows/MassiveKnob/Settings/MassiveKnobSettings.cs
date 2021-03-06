using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Serilog.Events;

namespace MassiveKnob.Settings
{
    public enum SettingsMenuItem
    {
        None,
        Device,
        AnalogInputs,
        DigitalInputs,
        AnalogOutputs,
        DigitalOutputs,
        Logging,
        Startup
    }


    
    public class MassiveKnobSettings
    {
        public DeviceSettings Device { get; set; }
        public List<ActionSettings> AnalogInput { get; set; }
        public List<ActionSettings> DigitalInput { get; set; }
        public List<ActionSettings> AnalogOutput { get; set; }
        public List<ActionSettings> DigitalOutput { get; set; }

        private UISettings ui;
        public UISettings UI
        {
            get => ui ?? (ui = new UISettings());
            set => ui = value ?? new UISettings();
        }

        private LogSettings log;
        public LogSettings Log
        {
            get => log ?? (log = new LogSettings());
            set => log = value ?? new LogSettings();
        }


        public void Verify()
        {
            if (AnalogInput == null) AnalogInput = new List<ActionSettings>();
            if (DigitalInput == null) DigitalInput = new List<ActionSettings>();
            if (AnalogOutput == null) AnalogOutput = new List<ActionSettings>();
            if (DigitalOutput == null) DigitalOutput = new List<ActionSettings>();
        }
        
        
        public MassiveKnobSettings Clone()
        {
            return new MassiveKnobSettings
            {
                Device = Device?.Clone(),
                AnalogInput = AnalogInput.Select(a => a?.Clone()).ToList(),
                DigitalInput = DigitalInput.Select(a => a?.Clone()).ToList(),
                AnalogOutput = AnalogOutput.Select(a => a?.Clone()).ToList(),
                DigitalOutput = DigitalOutput.Select(a => a?.Clone()).ToList(),
                UI = UI.Clone(),
                Log = Log.Clone()
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


        public class UISettings
        {
            public SettingsMenuItem ActiveMenuItem { get; set; } = SettingsMenuItem.None;

            public UISettings Clone()
            {
                return new UISettings
                {
                    ActiveMenuItem = ActiveMenuItem
                };
            }
        }


        public class LogSettings
        {
            public bool Enabled { get; set; } = true;
            public LogEventLevel Level { get; set; } = LogEventLevel.Information;

            public LogSettings Clone()
            {
                return new LogSettings
                {
                    Enabled = Enabled,
                    Level = Level
                };
            }
        }
    }
}
