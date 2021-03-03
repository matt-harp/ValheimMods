using System.Reflection;
using System.Threading;
using HarmonyLib;
using UnityEngine;

namespace NoPortals
{
    public class NoPortals
    {
        private static GameObject _removedPortal;
        
        public static void Main()
        {
            new Thread(() =>
            {
                while (AccessTools.Method(typeof(Console), "Print") == null || Console.instance == null)
                {
                    Thread.Sleep(100);
                }
                Console.instance.Print("[NoPortals] Loaded!");
                new Harmony(nameof(NoPortals)).PatchAll(Assembly.GetExecutingAssembly());
            }).Start();
        }

        public static void DisablePortalCraft()
        {
            var table = ObjectDB.instance.GetItemPrefab("Hammer").GetComponent<ItemDrop>().m_itemData.m_shared
                .m_buildPieces;
            _removedPortal = table.m_pieces.Find(x => x.name.Contains("portal"));
            table.m_pieces.Remove(_removedPortal);
        }
    }
    
    [HarmonyPatch(typeof(Inventory), "IsTeleportable")]
    class Inventory_IsTeleportable_Patch
    {
        static void Postfix(ref bool __result)
        {
            __result = false;
        }
    }

    [HarmonyPatch(typeof(Player), "SetLocalPlayer")]
    class Player_SetLocalPlayer_Patch
    {
        static void Postfix()
        {
            NoPortals.DisablePortalCraft();
        }
    }
}