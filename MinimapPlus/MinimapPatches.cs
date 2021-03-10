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
            var walkingMultiplier = 1f;
            var boatMultiplier = 1f;
            if (!EnvMan.instance.GetCurrentEnvironment().m_name.Contains("lear"))
            {
                boatMultiplier = MinimapPlus.Config.BoatWeatherMultiplier;
                walkingMultiplier = MinimapPlus.Config.WalkingWeatherMultiplier;
            }
            if (Player.m_localPlayer.GetStandingOnShip() != null || Player.m_localPlayer.GetControlledShip() != null)
            {
                Minimap.instance.m_exploreRadius = MinimapPlus.Config.BoatRange * boatMultiplier;
            }
            else
            {
                Minimap.instance.m_exploreRadius = MinimapPlus.Config.WalkingRange * walkingMultiplier;
            }
        }

        static void Postfix()
        {
            if (!MinimapPlus.Config.MapShareEnabled) return;
            foreach (var pos in from playerInfo in ZNet.instance.GetPlayerList()
                where playerInfo.m_characterID != Player.m_localPlayer.GetZDOID() && playerInfo.m_publicPosition
                select playerInfo.m_position)
            {
                var playerExploreRadius = MinimapPlus.Config.WalkingRange;
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
            if (!MinimapPlus.Config.MapEnabled)
            {
                Minimap.instance.m_largeRoot.SetActive(false);
            }

            if (!MinimapPlus.Config.MinimapEnabled)
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
            if (MinimapPlus.Config.ShowPlayerMarkers) return;
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
            if (MinimapPlus.Config.ShowPlayerMarkers) return;
            
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
}