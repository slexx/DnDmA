using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMAW_DND
{
    internal static class OldConfig
    {
        //public static ulong _engineBase = Memory.GetModuleBase("engine2.dll");
        //public static int Width = 1920;
        //public static int Height = 1080;

        //public static Int32 Width = Memory.ReadValue<Int32>(Memory.EngineBase + Offsets.Engine.dwWindowWidth);
        //public static Int32 Height = Memory.ReadValue<Int32>(Memory.EngineBase + Offsets.Engine.dwWindowHeight);
        public static Int32 Width = 1920;
        public static Int32 Height = 1080;


        static OldConfig()
        {
            //while (!Memory.GameIsReady) { }
            //Width = Memory.ReadValue<Int32>(Memory.EngineBase + 0x5396D8);
            //Height = Memory.ReadValue<Int32>(Memory.EngineBase + 0x5396DC);
        }
    }
}
