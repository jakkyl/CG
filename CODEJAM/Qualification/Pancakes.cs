using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CODEJAM.Qualification
{
    class Pancakes
    {
        public static void Main(string[] args)
        {
            int cases = int.Parse(Console.ReadLine());
            Console.InputEncoding = Encoding.ASCII;
            Console.OutputEncoding = Encoding.ASCII;
            for (int i = 0; i < cases; i++)
            {
                var input = Console.ReadLine().Split(' ');

                char[] pancakes = input[0].ToCharArray();
                int capacity = int.Parse(input[1]);

                int times = 0;
                int index = pancakes.ToList().IndexOf('-');
                try
                {
                    times = Flip(pancakes, capacity, index, 0);
                }
                catch (Exception e)
                {
                    times = -1;
                }

                if (times == -1)
                    Console.WriteLine(string.Format("Case #{0}: {1}", i+1, "IMPOSSIBLE"));
                else
                    Console.WriteLine(string.Format("Case #{0}: {1}", i+1, times));
            }                       
        }

        private static int Flip(char[] pancakes, int capacity, int index, int count)
        {
            if (!pancakes.Any(p => p == '-')) return count;
            if (count > 1000) return -1;
            if (index + capacity > pancakes.Length) index = pancakes.Length - capacity - 1;
            index = Math.Max(0, Math.Min(index, pancakes.Length-1));
            
            for (int i = index; i < index + capacity; i++)
            {
                pancakes[i] = pancakes[i] == '-' ? '+' : '-';
            }
            index = pancakes.ToList().IndexOf('-');
            return Flip(pancakes, capacity, index, count + 1);
        }
    }
}
