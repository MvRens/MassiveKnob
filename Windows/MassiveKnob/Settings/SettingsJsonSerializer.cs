using System;
using System.IO;
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
            var json = JsonConvert.SerializeObject(settings);

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

            return JsonConvert.DeserializeObject<Settings>(json);
        }
    }
}
