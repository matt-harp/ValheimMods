using System;
using System.IO;
using System.Reflection;
using System.Threading;
using BepInEx;
using HarmonyLib;
using Pipakin.SkillInjectorMod;
using UnityEngine;

namespace MinimapPlus
{
    [BepInPlugin("me.Night.valheim.MinimapPlus", "MinimapPlus", "2.1.1")]
    [BepInDependency("com.pipakin.SkillInjectorMod")]
    public class MinimapPlus : BaseUnityPlugin
    {
        public static readonly int SkillId = 231;
        public static MinimapSettings MapConfig;
        public static string ConfigDir = Paths.ConfigPath;
        public static string ConfigPath = Path.Combine(ConfigDir, "MinimapPlus.ini");
        public static bool LegitimateExploration { get; set; } = true;

        public void Start()
        {
            Directory.CreateDirectory(ConfigDir);
            if (!File.Exists(ConfigPath))
            {
                Logger.LogWarning("[Minimap+] Config not found. Creating...");
                MinimapSettings.SaveDefault(ConfigPath);
            }
            Logger.LogInfo("[Minimap+] Loading config...");
            LoadConfig();
            Logger.LogInfo("[Minimap+] Loaded!");
            if (MapConfig.ExploringSkillEnabled)
            {
                SkillInjector.RegisterNewSkill(SkillId, "Exploring",
                    "Your keen eye helps you see farther into the unknown", 1.0f, LoadSprite(), Skills.SkillType.Run);
                Logger.LogInfo("[Minimap+] Skill injected!");
            }
            new Harmony(nameof(MinimapPlus)).PatchAll(Assembly.GetExecutingAssembly());
        }
        
        private static Sprite LoadSprite()
        {
            var directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filepath = Path.Combine(directoryName, "exploring.png");
            if (File.Exists(filepath))
            {
                var texture2D = new Texture2D(0, 0);
                texture2D.LoadImage(File.ReadAllBytes(filepath));
                return Sprite.Create(texture2D, new Rect(0f, 0f, 32f, 32f), Vector2.zero);
            }
            Debug.LogError("Unable to load skill icon! Make sure exploring.png is next to the plugin dll in the plugin's folder!");
            return null;
        }

        public static void LoadConfig()
        {
            MapConfig = MinimapSettings.ReadFile(ConfigPath);
        }
    }
}