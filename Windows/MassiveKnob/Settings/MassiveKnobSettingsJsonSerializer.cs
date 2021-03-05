using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MassiveKnob.Settings
{
    public static class MassiveKnobSettingsJsonSerializer
    {
        private static readonly JsonSerializerSettings DefaultSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter()
            }
        };
        
        
        public static string GetDefaultFilename()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"MassiveKnob");
            Directory.CreateDirectory(path);

            return Path.Combine(path, "Settings.json");
        }
        
        
        public static Task Serialize(MassiveKnobSettings settings)
        {
            return Serialize(settings, GetDefaultFilename());
        }

        public static async Task Serialize(MassiveKnobSettings settings, string filename)
        {
            var json = JsonConvert.SerializeObject(settings, DefaultSettings);

            using (var stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, true))
            using (var streamWriter = new StreamWriter(stream, Encoding.UTF8))
            {
                await streamWriter.WriteAsync(json);
                await streamWriter.FlushAsync();
            }
        }


        public static MassiveKnobSettings Deserialize()
        {
            return Deserialize(GetDefaultFilename());
        }

        public static MassiveKnobSettings Deserialize(string filename)
        {
            MassiveKnobSettings settings = null;

            if (File.Exists(filename))
            {
                string json;

                using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, true))
                using (var streamReader = new StreamReader(stream, Encoding.UTF8))
                {
                    json = streamReader.ReadToEnd();
                }

                if (!string.IsNullOrEmpty(json))
                    settings = JsonConvert.DeserializeObject<MassiveKnobSettings>(json, DefaultSettings);
            }
            
            if (settings == null)
                settings = new MassiveKnobSettings();

            settings.Verify();
            return settings;
        }
    }
}
