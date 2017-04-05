using System;
using System.Collections.Generic;
using System.Linq;

namespace CODEJAM
{
    internal class LastWord
    {
        public static void Main(string[] args)
        {
            var a = Console.ReadLine();
            int n = int.Parse(a);
            //string[] words = new string[n];
            for (int i = 0; i < n; i++)
            {
                var word = Console.ReadLine();

                //var allWords = GetWords(words[i], new List<string>(),0);
                //var sorted = allWords.OrderBy(w => w);

                Console.WriteLine("Case #{0}: {1}", i + 1, GetWords(word));
            }
        }

        private static string GetWords(string p)
        {
            var ca = p.ToCharArray();
            ca.OrderBy(c => c);

            var result = new List<char>();
            result.Add(p[0]);
            for (int i = 1; i < p.Length; i++)
            {
                if (p[i] >= result[0]) result.Insert(0, p[i]);
                else result.Add(p[i]);
            }

            return string.Join("", result);
        }
    }
}