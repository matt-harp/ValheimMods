using System.Threading;
using HarmonyLib;

namespace ExampleMod
{
    public class ExampleMod
    {
        /*
         * Entry point for InSlimVML to load our mod
         */
        public static void Main()
        {
            new Thread(() =>
            {
                /*
                 * We need to wait for assembly_valheim to be loaded before we can use methods from it
                 */
                while (AccessTools.Method(typeof(Console), "Print") == null || Console.instance == null)
                {
                    Thread.Sleep(100);
                }

                /*
                 * Ready to print to console, run Harmony patches, etc
                 */
                Console.instance.Print("Good to go!");
            }).Start();
        }
    }
}