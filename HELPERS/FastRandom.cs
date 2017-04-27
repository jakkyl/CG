using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solution.HELPERS
{
    internal class FastRandom
    {
        private static int g_seed = 42;

        public static int fastrand()
        {
            g_seed = (214013 * g_seed + 2531011);
            return (g_seed >> 16) & 0x7FFF;
        }

        public static int Rnd(int b)
        {
            return fastrand() % b;
        }

        public static int Rnd(int a, int b)
        {
            return a + Rnd(b - a + 1);
        }
    }
}