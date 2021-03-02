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
                Console.instance.Print("[Minimap+] Commands:");
                Console.instance.Print("reload - reloads the config");
                Console.instance.Print("enablemap - enable the minimap and large map");
                Console.instance.Print("disablemap - disable the minimap and large map");
                Console.instance.Print("walkingrange <range> - minimap explore radius when on foot");
                Console.instance.Print("boatrange <range> - minimap explore radius when in a boat");
                Console.instance.Print("Recommended range is between 100(default) and 500(very far)");
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
                    Console.instance.Print($"[Minimap+] Boat argument: {args[2]}!");
                }
            }
        }
    }
}