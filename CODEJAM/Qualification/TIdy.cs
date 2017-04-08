using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CODEJAM.Qualification
{
    class TIdy
    {
        public static void Main(string[] args)
        {
            int cases = int.Parse(Console.ReadLine());
            for (int i = 0; i < cases; i++)
            {
                var input = Console.ReadLine();

                string last = input;

                var lastTidy = long.Parse(FindLastTidy(last));
                
                Console.WriteLine(string.Format("Case #{0}: {1}", i + 1, lastTidy));
            }
        }

        private static string FindLastTidy(string last)
        {
            if (last.Length == 1) return last;
            var value = Int64.Parse(last);
            bool tidy = false;
            string lastTidy = "";

            while (!tidy)
            {
                tidy = true;
                for (int i = 0; i < last.Length - 1; i++)
                {
                    if (last[i] > last[i + 1])
                    {
                        long.Parse(last);
                        var intVal = (char.GetNumericValue(last[i]) - 1).ToString();
                        last = last.Substring(0, i) + intVal.PadRight(last.Length - i, '9');

                        tidy = false;
                        break;
                    }
                }
            }
            //    for (var j = value; j > 10; j--)
            //{
            //    tidy = true;
            //    var v = j;
            //    var length = (int)Math.Floor(Math.Log10(j)+1);
            //    var curr = j.ToString();// new long[length];
            //    //int index = length-1;
            //    //do
            //    //{
            //    //    curr[index] = v % 10;
            //    //    v /= 10;
            //    //    index--;
            //    //} while (v != 0);
            //    //char[] curr = j.ToString().ToCharArray();
            //    for (int i = 0; i < curr.Length - 1; i++)
            //    {
            //        if (curr[i] > curr[i + 1])                        
            //        {
            //            tidy = false;                        
            //            break;
            //        }
            //    }
            //    if (tidy) return (j+1).ToString();
                
            //}

            return last;
        }
    }
}
