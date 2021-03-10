using System;
using System.IO;
using System.Reflection;
using System.Threading;
using HarmonyLib;

namespace MinimapPlus
{
    public class MinimapPlus
    {
        public static MinimapSettings Config;
        public static string LocalPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string ConfigDir = Path.Combine(LocalPath, "config");
        public static string ConfigPath = Path.Combine(ConfigDir, "MinimapPlus.ini");

        public static void Main()
        {
            new Thread(() =>
            {
                while (AccessTools.Method(typeof(Console), "Print") == null || Console.instance == null)
                {
                    Thread.Sleep(10);
                }

                Init();
            }).Start();
        }

        public static void Init()
        {
            Directory.CreateDirectory(ConfigDir);
            if (!File.Exists(ConfigPath))
            {
                Console.instance.Print("[Minimap+] Config not found. Creating...");
                MinimapSettings.SaveDefault(ConfigPath);
            }
            Console.instance.Print("[Minimap+] Loading config...");
            LoadConfig();
            Console.instance.Print("[Minimap+] Loaded!");
            new Harmony(nameof(MinimapPlus)).PatchAll(Assembly.GetExecutingAssembly());
        }

        public static void LoadConfig()
        {
            Config = MinimapSettings.ReadFile(ConfigPath);
        }
    }
}