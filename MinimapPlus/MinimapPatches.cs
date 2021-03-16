using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace MinimapPlus
{
    [HarmonyPatch(typeof(Minimap), "Start")]
    public class Minimap_Start
    {
        static void Prefix()
        {
            Console.instance.Print("[Minimap+] Minimap ready!");
        }
    }


    [HarmonyPatch(typeof(Minimap), "UpdateExplore")]
    public class Minimap_UpdateExplore
    {
        static void Prefix()
        {
            if (Player.m_localPlayer == null) return;
            var skillFactor = 1f;
            if (MinimapPlus.MapConfig.ExploringSkillEnabled)
            {
                skillFactor = Player.m_localPlayer.GetSkillFactor((Skills.SkillType) MinimapPlus.SkillId);
            }
            var walkingMultiplier = 1f;
            var boatMultiplier = 1f;
            if (!EnvMan.instance.GetCurrentEnvironment().m_name.Contains("lear"))
            {
                walkingMultiplier = MinimapPlus.MapConfig.WalkingWeatherMultiplier;
                boatMultiplier = MinimapPlus.MapConfig.BoatWeatherMultiplier;
            }
            if (Player.m_localPlayer.GetStandingOnShip() != null || Player.m_localPlayer.GetControlledShip() != null)
            {
                Minimap.instance.m_exploreRadius = ((MinimapPlus.MapConfig.BoatRange - 100) * skillFactor + 100) * boatMultiplier;
            }
            else
            {
                Minimap.instance.m_exploreRadius = ((MinimapPlus.MapConfig.WalkingRange - 100) * skillFactor + 100) * walkingMultiplier;
            }
        }

        static void Postfix()
        {
            if (!MinimapPlus.MapConfig.MapShareEnabled) return;
            foreach (var pos in from playerInfo in ZNet.instance.GetPlayerList()
                where playerInfo.m_characterID != Player.m_localPlayer.GetZDOID() && playerInfo.m_publicPosition
                select playerInfo.m_position)
            {
                var playerExploreRadius = MinimapPlus.MapConfig.WalkingRange;
                MinimapReverse.Explore(Minimap.instance, pos, playerExploreRadius);
            }
        }
    }

    [HarmonyPatch(typeof(Minimap))]
    public class MinimapReverse
    {
        [HarmonyReversePatch]
        [HarmonyPatch("Explore", typeof(Vector3), typeof(float))]
        public static void Explore(object instance, Vector3 p, float radius)
        {
            throw new NotImplementedException("Stub!");
        }
        
        [HarmonyReversePatch]
        [HarmonyPatch("CenterMap", typeof(Vector3))]
        public static void CenterMap(object instance, Vector3 center)
        {
            throw new NotImplementedException("Stub!");
        }
    }

    [HarmonyPatch(typeof(Minimap), "UpdateMap")]
    public class Minimap_UpdateMap
    {
        static void Postfix()
        {
            if (!MinimapPlus.MapConfig.MapEnabled)
            {
                Minimap.instance.m_largeRoot.SetActive(false);
            }

            if (!MinimapPlus.MapConfig.MinimapEnabled)
            {
                Minimap.instance.m_smallRoot.SetActive(false);
            }
        }
    }
    
    [HarmonyPatch(typeof(Minimap), "SetMapMode")]
    public class Minimap_SetMapMode
    {
        static void Prefix(Minimap __instance, ref string __state)
        {
            // ReSharper disable once RedundantAssignment
            __state = Traverse.Create(__instance).Field("m_mode").GetValue<Enum>().ToString();
        }
        static void Postfix(Minimap __instance, ref string __state)
        {
            if (MinimapPlus.MapConfig.ShowPlayerMarkers) return;
            var traverse = Traverse.Create(__instance);
            if (!__state.Equals(traverse.Field("m_mode").GetValue<Enum>().ToString()))
            {
                traverse.Field("m_mapOffset").SetValue(-Player.m_localPlayer.transform.position);
            }
        }
    }
    
    [HarmonyPatch(typeof(Minimap), "Update")]
    public class Minimap_Update
    {
        static void Postfix()
        {
            if (MinimapPlus.MapConfig.ShowPlayerMarkers) return;
            
            // Disable all player location pins
            Minimap.instance.m_largeMarker.gameObject.SetActive(false);
            Minimap.instance.m_largeShipMarker.gameObject.SetActive(false);
            Minimap.instance.m_smallMarker.gameObject.SetActive(false);
            Minimap.instance.m_smallShipMarker.gameObject.SetActive(false);
            var traverse = Traverse.Create(Minimap.instance);
            traverse.Field("m_playerPins").GetValue<List<Minimap.PinData>>().ForEach(pin => Minimap.instance.RemovePin(pin));
            traverse.Field("m_pingPins").GetValue<List<Minimap.PinData>>().ForEach(pin => Minimap.instance.RemovePin(pin));
            traverse.Field("m_shoutPins").GetValue<List<Minimap.PinData>>().ForEach(pin => Minimap.instance.RemovePin(pin));
        }
    }

    [HarmonyPatch(typeof(Minimap), "ExploreAll")]
    public class Minimap_ExploreAll
    {
        static void Prefix()
        {
            MinimapPlus.LegitimateExploration = false;
        }

        static void Postfix()
        {
            MinimapPlus.LegitimateExploration = true;
        }
    }
    
    [HarmonyPatch(typeof(Minimap), "Explore", typeof(int), typeof(int))]
    public class Minimap_Explore
    {
        static void Postfix(bool __result)
        {
            if (__result && MinimapPlus.LegitimateExploration && Player.m_localPlayer != null && MinimapPlus.MapConfig.ExploringSkillEnabled)
            {
                var player = Player.m_localPlayer;
                player.RaiseSkill((Skills.SkillType)MinimapPlus.SkillId, MinimapPlus.MapConfig.ExploringSkillRate);
            }
        }
    }
}