using ImGuiNET;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using Vortice.Mathematics;

namespace DMAW_DND
{
    internal class Program
    {
        static void Main(string[] args)
        {

            //Security.Authentication.Authenticate();
            //while (Security.Authentication._isAuthenticating)
            //{
            //    // infinite loop - waiting for authentication to complete
            //}
            //Thread.Sleep(100);
            //if (!Security.Authentication.IsAuthenticated())
            //{
            //    Program.Log($"[INIT] Authentication failed. Shutting down.");
            //    Shutdown();
            //}

            while (!Config.Ready) // Calling this will initialise the static class
            {
                Thread.Sleep(500);
            }

            while (!Memory.Ready) // Calling this will initialise the static class
            {
                Thread.Sleep(5);
            };

            RadarWindow window = new RadarWindow();
            //window.UpdateFrequency = 240f; // Frame limiter
            window.Run();
            while (true)
            {
                Thread.SpinWait(0);
            }
        }

        public static void Log(string message)
        {
#if DEBUG
            Console.WriteLine(message);
            Debug.WriteLine(message);
#endif
        }

        public static void Shutdown()
        {
            //if (Memory.Shutdown()) Console.WriteLine("[SHUTDOWN] Memory Shutdown");

            //KMBox.DisposeComPort();
            //Console.WriteLine("[SHUTDOWN] KMBox Shutdown");
            //if (_isEspRunning)
            //{
            //    Console.WriteLine("[SHUTDOWN] ESP Shutdown");
            //    renderClass.Close();
            //}

            //if(LootManager.Shutdown()) Console.WriteLine("[SHUTDOWN] Loot Manager Shutdown");
            Process.GetCurrentProcess().Kill();
        }
    }
}