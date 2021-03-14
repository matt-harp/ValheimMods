using System;
using System.IO;
using System.Reflection;
using System.Threading;
using BepInEx;
using HarmonyLib;

namespace MinimapPlus
{
    [BepInPlugin("me.Night.valheim.MinimapPlus", "MinimapPlus", "2.0.0")]
    public class MinimapPlus : BaseUnityPlugin
    {
        public static MinimapSettings MapConfig;
        public static string LocalPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string ConfigDir = Path.Combine(LocalPath, "config");
        public static string ConfigPath = Path.Combine(ConfigDir, "MinimapPlus.ini");

        public void Start()
        {
            Directory.CreateDirectory(ConfigDir);
            if (!File.Exists(ConfigPath))
            {
                Logger.LogWarning("[Minimap+] MapConfig not found. Creating...");
                MinimapSettings.SaveDefault(ConfigPath);
            }
            Logger.LogInfo("[Minimap+] Loading config...");
            LoadConfig();
            Logger.LogInfo("[Minimap+] Loaded!");
            new Harmony(nameof(MinimapPlus)).PatchAll(Assembly.GetExecutingAssembly());
        }

        public static void LoadConfig()
        {
            MapConfig = MinimapSettings.ReadFile(ConfigPath);
        }
    }
}