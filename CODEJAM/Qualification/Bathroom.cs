using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CODEJAM.Qualification
{
    class Bathroom
    {
        class Stall
        {
            public int Id { get; set; }
            public bool Occupied { get; set; }
            public int Cost { get; set; }
            public Stall(int id) { Id = id; Neighbors = new List<Stall>(); }

            public List<Stall> Neighbors { get; set; }
        }
        public static void Main(string[] args)
        {
            int cases = int.Parse(Console.ReadLine());
            for (int i = 0; i < cases; i++)
            {
                var input = Console.ReadLine().Split(' ');
                int numStalls = int.Parse(input[0]) + 2;
                int people = int.Parse(input[1]);

                var stalls = new Stall[numStalls];
                for (int j = 0; j < numStalls; j++)
                    stalls[j] = new Stall(j);

                for (int j = 0; j < numStalls; j++)
                {
                    if (j > 0)
                        stalls[j].Neighbors.Add(stalls[j - 1]);
                    if (j < numStalls - 1)
                        stalls[j].Neighbors.Add(stalls[j + 1]);
                }
                stalls[0].Occupied = true;
                stalls[numStalls - 1].Occupied = true;

                var result = OccupyStalls(stalls, people);
                Console.WriteLine(string.Format("Case #{0}: {1} {2}", i + 1, Math.Max(result[0], result[1]), Math.Min(result[0], result[1])));
            }
        }

        private static int[] OccupyStalls(Stall[] stalls, int people)
        {

            //foreach (var s in stalls) s.Cost = 0;
            int min = stalls.Length;
            int max = 0;

            int start = 0;
            int index = 0;
            for (int i = 0; i < stalls.Length; i++)
            {
                if (!stalls[i].Occupied)
                {
                    start++;
                }
                else if (start > 0)
                {
                    if (start > max)
                    {
                        max = start;
                        index = i - (int)Math.Round(start * 0.5, MidpointRounding.AwayFromZero);
                        //index = i - index - 1;                        
                    }
                    if (start < min)
                    {
                        min = start;
                    }
                        start = 0;
                }
            }
            if (stalls[index].Occupied)
            {
                if (stalls[index - 1].Occupied) index--;
                else index++;
            }
            stalls[index].Occupied = true;
            stalls[index].Cost = start;
            //int c = stalls.Count(s => !s.Occupied);
            if (people == 1)
            {
                int right = 0;
                int left = 0;
                for (int i = index + 1; i < stalls.Length; i++)
                {
                    if (stalls[i].Occupied) break;
                    right++;
                }
                for (int i = index - 1; i > 0; i--)
                {
                    if (stalls[i].Occupied) break;
                    left++;
                }
                return new int[] { left, right };
            }
            return OccupyStalls(stalls, people - 1);
        }
    }
}
