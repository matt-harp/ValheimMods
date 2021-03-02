using System.Linq;
using System.Reflection;
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
            if (Player.m_localPlayer.GetStandingOnShip() != null)
            {
                Minimap.instance.m_exploreRadius = MinimapPlus.Config.BoatRange;
            }
            else
            {
                Minimap.instance.m_exploreRadius = MinimapPlus.Config.WalkingRange;
            }
        }

        static void Postfix()
        {
            foreach (var pos in from playerInfo in ZNet.instance.GetPlayerList()
                where playerInfo.m_characterID != Player.m_localPlayer.GetZDOID() && playerInfo.m_publicPosition
                select playerInfo.m_position)
            {
                var playerExploreRadius = MinimapPlus.Config.WalkingRange;
                AccessTools.Method(typeof(Minimap), "Explore", new[] {typeof(Vector3), typeof(float)})
                    .Invoke(Minimap.instance, new object[] {pos, playerExploreRadius});
            }
        }
    }

    [HarmonyPatch(typeof(Minimap), "Update")]
    public class Minimap_Update
    {
        private static object MapMode_None = typeof(Minimap).GetNestedType("MapMode", BindingFlags.NonPublic)
            .GetField("None").GetValue(Minimap.instance);

        private static MethodInfo SetMapMode = typeof(Minimap).GetMethod("SetMapMode",
            BindingFlags.NonPublic | BindingFlags.Instance,
            null, new[] {typeof(Minimap).GetNestedType("MapMode", BindingFlags.NonPublic)}, null);

        static void Postfix()
        {
            if (!MinimapPlus.Config.MapEnabled)
            {
                SetMapMode?.Invoke(Minimap.instance, new[] { MapMode_None });
            }
        }
    }
}