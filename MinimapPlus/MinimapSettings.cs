using System.IO;
using System.Reflection;

namespace MinimapPlus
{
    public class MinimapSettings
    {
        public float WalkingRange { get; set; } = 200f;

        public float BoatRange { get; set; } = 400f;

        public bool MapEnabled { get; set; } = true;

        public static void SaveDefault(string path)
        {
            var settings = new MinimapSettings();
            
            File.Create(path).Dispose();
            using (var sw = new StreamWriter(path))
            {
                foreach (var prop in settings.GetType().GetProperties())
                {
                    sw.WriteLine($"{prop.Name}={prop.GetValue(settings)}");
                }
            }
        }

        public static MinimapSettings ReadFile(string path)
        {
            var settings = new MinimapSettings();
            using (var sr = new StreamReader(path))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var keyvalue = line.Split('=');
                    if (keyvalue.Length != 2) continue;
                    foreach (var prop in settings.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    {
                        if (prop.Name != keyvalue[0]) continue;
                        if (prop.PropertyType == typeof(float))
                        {
                            prop.SetValue(settings, float.Parse(keyvalue[1]));
                            continue;
                        }

                        if (prop.PropertyType == typeof(bool))
                        {
                            prop.SetValue(settings, bool.Parse(keyvalue[1]));
                            continue;
                        }

                        if (prop.PropertyType == typeof(int))
                        {
                            prop.SetValue(settings, int.Parse(keyvalue[1]));
                        }
                    }
                }
            }

            return settings;
        }
    }
}