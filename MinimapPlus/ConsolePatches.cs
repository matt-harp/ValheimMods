using System;
using HarmonyLib;
using UnityEngine.UI;

namespace MinimapPlus
{
    [HarmonyPatch(typeof(Console), "InputText")]
    public class Console_InputText
    {
        static void Postfix(InputField ___m_input)
        {
            if (!___m_input.text.StartsWith("minimap")) return;
            var args = ___m_input.text.Split(' ');
            if (args.Length < 2 || !___m_input.text.Contains(" "))
            {
                Console.instance.Print("");
                Console.instance.Print("[Minimap+] Commands:");
                Console.instance.Print("reload - reloads the config");
                Console.instance.Print("enablemap - enable the large map");
                Console.instance.Print("disablemap - disable the large map");
                Console.instance.Print("enableminimap - enable the minimap");
                Console.instance.Print("disableminimap - disable the minimap");
                Console.instance.Print("walkingrange <range> - minimap explore radius when on foot");
                Console.instance.Print("boatrange <range> - minimap explore radius when in a boat");
                Console.instance.Print("* Recommended range is between 100(default) and 500(very far)");
                Console.instance.Print("enablemapshare - enable sharing between all online players");
                Console.instance.Print("disablemapshare - disable map sharing");
                Console.instance.Print("showplayermarkers - show player markers on map");
                Console.instance.Print("hideplayermarkers - hide player markers on map");
                Console.instance.Print("");
            }
            if (args[1].Equals("reload", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.instance.Print("[Minimap+] Reloading config...");
                MinimapPlus.LoadConfig();
                return;
            }
            if (args[1].Equals("enablemap", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.instance.Print("[Minimap+] Map enabled!");
                MinimapPlus.Config.MapEnabled = true;
                return;
            }
            if (args[1].Equals("disablemap", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.instance.Print("[Minimap+] Map disabled!");
                MinimapPlus.Config.MapEnabled = false;
                return;
            }
            if (args[1].Equals("enableminimap", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.instance.Print("[Minimap+] Minimap enabled!");
                MinimapPlus.Config.MinimapEnabled = true;
                if (Minimap.instance != null)
                {
                    Minimap.instance.m_smallRoot.SetActive(true);
                }
                return;
            }
            if (args[1].Equals("disableminimap", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.instance.Print("[Minimap+] Minimap disabled!");
                MinimapPlus.Config.MinimapEnabled = false;
                return;
            }
            if (args[1].Equals("walkingrange", StringComparison.InvariantCultureIgnoreCase))
            {
                if (float.TryParse(args[2], out var result))
                {
                    Console.instance.Print($"[Minimap+] Walking range set to {result}!");
                    MinimapPlus.Config.WalkingRange = result;
                }
                else
                {
                    Console.instance.Print($"[Minimap+] Invalid argument: {args[2]}!");
                }
                return;
            }
            if (args[1].Equals("boatrange", StringComparison.InvariantCultureIgnoreCase))
            {
                if (float.TryParse(args[2], out var result))
                {
                    Console.instance.Print($"[Minimap+] Boat range set to {result}!");
                    MinimapPlus.Config.BoatRange = result;
                }
                else
                {
                    Console.instance.Print($"[Minimap+] Invalid argument: {args[2]}!");
                }
            }
            if (args[1].Equals("enablemapshare", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.instance.Print("[Minimap+] Map share enabled!");
                MinimapPlus.Config.MapShareEnabled = false;
                return;
            }
            if (args[1].Equals("disablemapshare", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.instance.Print("[Minimap+] Map share disabled!");
                MinimapPlus.Config.MapShareEnabled = false;
                return;
            }
            if (args[1].Equals("showplayermarkers", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.instance.Print("[Minimap+] Player markers shown!");
                MinimapPlus.Config.ShowPlayerMarkers = true;
                return;
            }
            if (args[1].Equals("hideplayermarkers", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.instance.Print("[Minimap+] Player markers hidden!");
                MinimapPlus.Config.ShowPlayerMarkers = false;
                return;
            }
        }
    }
}