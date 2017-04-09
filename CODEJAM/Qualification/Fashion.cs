using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CODEJAM.Qualification
{
    class Fashion
    {
        class Model
        {
            public bool Perm = false;
            public char PermVal = '.';
            public char Val = '.';
        }
        public static void Main(string[] args)
        {
            int cases = int.Parse(Console.ReadLine());
            for (int i = 0; i < cases; i++)
            {
                var input = Console.ReadLine().Split(' ');
                int n = int.Parse(input[0]);
                var show = new Model[n, n];
                for (int r = 0; r < show.Length; r++)
                    for (int c = 0; c < show.Length; c++)
                        show[r, c] = new Model();
                int models = int.Parse(input[1]);
                for (int j = 0; j < models; j++)
                {
                    var s = Console.ReadLine().Split();
                    var m = char.Parse(s[0]);
                    int row = int.Parse(s[1]);
                    int col = int.Parse(s[2]);
                    show[row, col].Val = m;
                    show[row, col].PermVal = m;
                    show[row, col].Perm = true;
                }

                Maximize(show);

                Console.WriteLine(string.Format("Case #{0}: {1} {2}", i + 1));
            }
        }

        private static void Maximize(Model[,] show)
        {
            Model[,] copy = null;
            show.CopyTo(copy, 0);
            int minScore = GetScore(copy);

            for (int i = 0; i < show.Length; i++)
            {
                for (int j = 0; j < show.Length; j++)
                {
                    copy[i, j].Val = 'o';
                }
            }

            while (!Valid(copy))
            {

            }
        }

        private static bool Valid(Model[,] copy)
        {
            for (int i = 0; i < copy.Length; i++)
            {
                for (int j = 0; j < copy.Length; j++)
                {

                }
            }

            return true;
        }

        private static int GetScore(Model[,] show)
        {
            int score = 0;
            for (int i = 0; i < show.Length; i++)
            {
                for (int j = 0; j < show.Length; j++)
                {
                    if (show[i, j].Val == '.') continue;
                    score += show[i, j].Val == 'o' ? 2 : 1;
                }
            }

            return score;
        }
    }
}
