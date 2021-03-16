using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MinimapPlus
{
    public class MinimapSettings
    {
        public float WalkingRange { get; set; } = 200f;

        public float BoatRange { get; set; } = 400f;

        public bool MapEnabled { get; set; } = true;

        public bool MinimapEnabled { get; set; } = true;
        
        public bool MapShareEnabled { get; set; } = true;

        public bool ShowPlayerMarkers { get; set; } = true;

        public float WalkingWeatherMultiplier { get; set; } = 0.5f;
        
        public float BoatWeatherMultiplier { get; set; } = 0.4f;

        public bool ExploringSkillEnabled { get; set; } = false;
        public float ExploringSkillRate { get; set; } = 0.05f;

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
            var props = settings.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList();
            using (var sr = new StreamReader(path))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var keyvalue = line.Split('=');
                    if (keyvalue.Length != 2) continue;
                    for (var i = props.Count - 1; i >= 0; i--)
                    {
                        var prop = props[i];
                        // Only assign matching property
                        if (prop.Name != keyvalue[0]) continue;
                        props.RemoveAt(i);
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

            if (props.Count == 0) return settings;
            
            using (var sw = File.AppendText(path))
            {
                sw.Write("\n");
                foreach (var prop in props)
                {
                    sw.WriteLine($"{prop.Name}={prop.GetValue(settings)}");
                }
            }

            return settings;
        }
    }
}