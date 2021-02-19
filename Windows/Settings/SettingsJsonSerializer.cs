using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MassiveKnob.Settings
{
    public static class SettingsJsonSerializer
    {
        public static string GetDefaultFilename()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"MassiveKnob");
            Directory.CreateDirectory(path);

            return Path.Combine(path, "Settings.json");
        }
        
        
        public static Task Serialize(Settings settings)
        {
            return Serialize(settings, GetDefaultFilename());
        }

        public static async Task Serialize(Settings settings, string filename)
        {
            var serializedSettings = SerializedSettings.FromSettings(settings);
            var json = JsonConvert.SerializeObject(serializedSettings);

            using (var stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, true))
            using (var streamWriter = new StreamWriter(stream, Encoding.UTF8))
            {
                await streamWriter.WriteAsync(json);
                await streamWriter.FlushAsync();
            }
        }


        public static Task<Settings> Deserialize()
        {
            return Deserialize(GetDefaultFilename());
        }

        public static async Task<Settings> Deserialize(string filename)
        {
            if (!File.Exists(filename))
                return Settings.Default();

            string json;

            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, true))
            using (var streamReader = new StreamReader(stream, Encoding.UTF8))
            {
                json = await streamReader.ReadToEndAsync();
            }

            if (string.IsNullOrEmpty(json))
                return Settings.Default();

            var serializedSettings = JsonConvert.DeserializeObject<SerializedSettings>(json);
            return serializedSettings.ToSettings();
        }


        private class SerializedSettings
        {
            // ReSharper disable MemberCanBePrivate.Local - used for JSON serialization
            public string SerialPort;
            public SerializedKnobSettings[] Knobs;
            // ReSharper restore MemberCanBePrivate.Local


            public static SerializedSettings FromSettings(Settings settings)
            {
                return new SerializedSettings
                {
                    SerialPort = settings.SerialPort,
                    Knobs = settings.Knobs.Select(knob => new SerializedKnobSettings
                    {
                        DeviceId = knob.DeviceId
                    }).ToArray()
                };
            }

            
            public Settings ToSettings()
            {
                return new Settings
                {
                    SerialPort = SerialPort,
                    Knobs = Knobs.Select(knob => new Settings.KnobSettings
                    {
                        DeviceId = knob.DeviceId
                    }).ToList()
                };
            }
            

            public class SerializedKnobSettings
            {
                public Guid? DeviceId;
            }
        }
    }
}
