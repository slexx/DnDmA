using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DMAW_DND
{
    public static class Extensions
    {
        public static MapPosition ToMapPos(this Vector3 vector, Map map)
        {
            return new MapPosition
            {
                Y = map.ConfigFile.X - (vector.X * map.ConfigFile.Scale),
                X = map.ConfigFile.Y + (vector.Y * map.ConfigFile.Scale),
                Height = vector.Z
            };
        }

        public static double ToRadians(this float degrees)
        {
            return 0.017453292519943295 * (double)degrees;
        }

        public static double ToRadians(this double degrees)
        {
            return 0.017453292519943295 * degrees;
        }

        public static uint GetColor(byte r, byte g, byte b, byte a)
        {
            uint num = (uint)((uint)a << 8);
            num += (uint)b;
            num <<= 8;
            num += (uint)g;
            num <<= 8;
            return num + (uint)r;
        }




    }
}
