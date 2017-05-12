using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/

namespace SmashTheCode
{
    internal class Player
    {
        private const int Lifespan = 4;
        private const int PopulationSize = 100;

        private const int Width = 6;
        private const int Height = 12;
        private const int FutureTurns = 8;
        private const int MinSmash = 4;

        private static int _score = 0;
        private static int _nuisancePoints = 0;
        private static int _enemyScore = 0;
        private static int _enemyNuisancePoints = 0;

        private static BlockPair[] _nextBlocks = new BlockPair[8];

        private static Grid _myGrid = new Grid();
        private static Grid _enemyGrid = new Grid();

        private static Random rand = new Random();
        private static Stopwatch _stopwatch;

        private static int round = -1;
        private static int _simulations = 0;

        public enum Color
        {
            Empty = -1,
            Skull = 0,
            Blue = 1,
            Green,
            Pink,
            Red,
            Yellow = 5
        }

        private static int g_seed = 42;

        internal struct FastRandom
        {
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

        private struct Point
        {
            public int X { get; set; }
            public int Y { get; set; }

            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }

            public override string ToString()
            {
                return string.Format("[{0},{1}]", X, Y);
            }
        }

        private class Grid
        {
            private Grid cache;
            public int score = 0;

            public Block[,] Field { get; set; }
            public int NuisancePoints { get; set; }

            public Grid()
            {
                Field = new Block[Width, Height];
            }

            public bool ApplyMove(Gene move, BlockPair blocks)
            {
                //Console.Error.WriteLine("Moving to {0}", move);
                int pos2X, pos2Y;
                switch (move.Rotation)
                {
                    case 0:
                        pos2X = 1;
                        pos2Y = 0;
                        break;

                    case 1:
                        pos2X = 0;
                        pos2Y = -1;
                        break;

                    case 2:
                        pos2X = -1;
                        pos2Y = 0;
                        break;

                    default:
                        pos2X = 0;
                        pos2Y = 1;
                        break;
                }
                if (move.Column + pos2X >= Width || move.Column + pos2X < 0) return false;

                bool placedA = false;
                bool placedB = false;
                bool aFirst = pos2Y < 0;
                bool bFirst = pos2Y > 0;
                for (int i = Height - 1; i >= 0; i--)
                {
                    if (i + pos2Y >= Height || i + pos2Y < 0) continue;
                    if (!placedA && !bFirst && Field[move.Column, i].Color == Color.Empty)
                    {
                        Field[move.Column, i].Color = blocks.BlockA.Color;
                        aFirst = false;
                        if (placedB) return true;
                        placedA = true;
                    }
                    if (!placedB && !aFirst && Field[move.Column + pos2X, i + pos2Y].Color == Color.Empty)
                    {
                        Field[move.Column + pos2X, i + pos2Y].Color = blocks.BlockB.Color;
                        bFirst = false;
                        if (placedA) return true;
                        placedB = true;
                    }
                }

                return false;
            }

            private Dictionary<Block, List<Block>> CheckMatches()
            {
                Block start;
                var blocks = new Dictionary<Block, List<Block>>();
                var seen = new List<Block>();
                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        if (Field[i, j].Color != Color.Empty && Field[i, j].Color != Color.Skull)
                        {
                            start = Field[i, j];
                            if (seen.Contains(start)) continue;
                            seen.Add(start);
                            //check if we've already got this one
                            if (blocks.Values.Any(b => b.Contains(start))) continue;

                            var found = Search(this, start);
                            if (found != null)
                            {
                                seen.AddRange(found);
                                //Console.Error.WriteLine("Starting search at {0}, found {1}", start, string.Join(",", found));
                                blocks[start] = found;
                            }
                        }
                    }
                }
                //blocks = blocks.Distinct().ToList();
                //if (blocks.Count > 0)
                //    Console.Error.WriteLine("Found matches {0} {1}", blocks.Count, string.Join(",", blocks));
                return blocks;
            }

            public void RemoveMatches(int step, bool debug = false)
            {
                var matches = CheckMatches();
                if (matches.Count > 0)
                {
                    //if (step > 0)
                    //{
                    //var sb = new StringBuilder();
                    //for (int k = 0; k < Height; k++)
                    //{
                    //    for (int j = 0; j < Width; j++)
                    //    {
                    //        sb.Append(Field[j, k] + " ");
                    //    }

                    //    sb.AppendLine();
                    //}
                    //Console.Error.WriteLine("{0}\nActive: {1} {2}", sb.ToString(), string.Join(",", matches), string.Join(",", matches.Values));
                    //}

                    int blocks = 0;
                    int groupBonus = 0;
                    var colorBonus = new Dictionary<Color, int>();
                    foreach (var entry in matches)
                    {
                        for (int i = 0; i < entry.Value.Count; i++)
                        {
                            var block = entry.Value[i];
                            if (debug) Console.Error.WriteLine("Removed {0}", block);
                            colorBonus[block.Color] = 1;
                            block.Color = Color.Empty;
                            foreach (var n in block.Neighbors(this).Where(n => n.Color == Color.Skull))
                            {
                                n.Color = Color.Empty;
                            }
                            blocks += 1;
                        }
                        groupBonus += Math.Min(entry.Value.Count - 4, 8);
                    }
                    Gravity();
                    int chainPower = step * 8 * (step > 1 ? 2 : 1);
                    score += (10 * blocks) * Math.Max(0, Math.Min(chainPower + (int)Math.Pow(2, colorBonus.Count - 1) + groupBonus, 999));
                    RemoveMatches(step + 1);
                    //if (step > 0) Console.Error.WriteLine("Step: {0} {1}", step, score);
                }
            }

            public void Gravity()
            {
                bool changed = false;
                //int activeCount = ActiveBlocks.Count;
                //while (changed)
                //{
                //    changed = false;
                //    for (int i = 0; i < activeCount; i++)
                //    {
                //        var blockBelow = ActiveBlocks[i].GetNeighborBelow(this);
                //        if (blockBelow != null && blockBelow.Color == Color.Empty)
                //        {
                //            var temp = ActiveBlocks[i];
                //            ActiveBlocks[i] = blockBelow;
                //            blockBelow = temp;

                //            changed = true;
                //        }
                //    }
                //}
                do
                {
                    changed = false;
                    for (int j = Height - 2; j >= 0; j--)
                    {
                        for (int i = 0; i < Width; i++)
                        {
                            int y = j;
                            while (y + 1 < Height && Field[i, y + 1].Color == Color.Empty)
                                y++;

                            if (y != j)
                            {
                                Field[i, y].Color = Field[i, j].Color;
                                Field[i, j].Color = Color.Empty;
                            }
                            //if (Field[i, j].Color == Color.Empty && Field[i, j - 1].Color != Color.Empty)
                            //{
                            //    Field[i, j].Color = Field[i, j - 1].Color;
                            //    Field[i, j - 1].Color = Color.Empty;
                            //    changed = true;
                            //}
                        }
                    }
                } while (changed);
            }

            public int TallestColumn()
            {
                int h1 = 0;
                for (int j = 0; j < Height; j++)
                {
                    for (int i = 0; i < Width; i++)
                    {
                        if (Field[i, j].Color != Color.Empty)
                        {
                            h1 = Height - j;
                            return h1;
                        }
                    }
                }

                return h1;
            }

            internal void Save()
            {
                cache = new Grid();
                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        cache.Field[i, j] = new Block((int)Field[i, j].Color, Field[i, j].Position.X, Field[i, j].Position.Y);
                    }
                }

                cache.score = score;
                cache.NuisancePoints = NuisancePoints;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void Load()
            {
                //Array.Copy(Field, 0, g.Field, 0, Field.Length);
                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        Field[i, j] = new Block((int)cache.Field[i, j].Color, cache.Field[i, j].Position.X, cache.Field[i, j].Position.Y);
                    }
                }

                score = cache.score;
                NuisancePoints = cache.NuisancePoints;
            }

            internal bool ApplyEnemyNuisance(int nuisancePoints)
            {
                while (nuisancePoints > 6)
                {
                    for (int i = 0; i < Width; i++)
                    {
                        if (Field[i, 0].Color != Color.Empty) return false;
                        Field[i, 0].Color = Color.Skull;
                        nuisancePoints--;
                    }
                    Gravity();
                }
                return true;
            }
        }

        private class Block
        {
            public Color Color { get; set; }
            public Point Position { get; set; }

            public Block(int color, int x, int y)
            {
                Color = (Color)color;
                Position = new Point(x, y);
            }

            public Block(Color color, Point pos)
            {
                Color = color;
                Position = pos;
            }

            public List<Block> Neighbors(Grid grid)
            {
                var result = new List<Block>(4);
                //Console.Error.WriteLine("{0} {1}", Position.X, Position.Y);

                if (Position.X > 0) result.Add(grid.Field[Position.X - 1, Position.Y]);
                if (Position.X + 1 < Width - 1) result.Add(grid.Field[Position.X + 1, Position.Y]);
                if (Position.Y > 0) result.Add(grid.Field[Position.X, Position.Y - 1]);
                if (Position.Y + 1 < Height) result.Add(grid.Field[Position.X, Position.Y + 1]);

                return result;
            }

            public Block GetNeighborBelow(Grid grid)
            {
                if (Position.Y + 1 < Height) return grid.Field[Position.X, Position.Y + 1];
                return null;
            }

            public override string ToString()
            {
                return Color.ToString();
            }

            internal Block Clone()
            {
                return new Block(Color, Position);
            }
        }

        private struct BlockPair
        {
            public Block BlockA { get; set; }
            public Block BlockB { get; set; }

            public BlockPair(int colorA, int colorB)
            {
                BlockA = new Block(colorA, -1, -1);
                BlockB = new Block(colorB, -1, -1);
            }
        }

        private static List<Block> Search(Grid grid, Block start)
        {
            var openList = new Queue<Block>();
            openList.Enqueue(start);

            var match = new List<Block>();
            match = new List<Block>() { start };

            while (openList.Count > 0)
            {
                var current = openList.Dequeue();

                foreach (var next in current.Neighbors(grid))
                {
                    if (next.Color == Color.Empty || next.Color == Color.Skull) continue;
                    if (match.Contains(next)) continue;
                    if (next.Color == current.Color)
                    {
                        match.Add(next);
                        //if (match.Count >= MinSmash)
                        //    Console.Error.WriteLine("Got a color: {0} {1} {2}", next.Color, next.Position, match.Count);
                        openList.Enqueue(next);
                    }
                }
            }

            if (match.Count >= MinSmash)
                return match;

            return null;
        }

        private class Population
        {
            public DNA[] Pop { get; set; }

            public Population(int populationSize, Grid grid)
            {
                Pop = new DNA[populationSize];
                for (int i = 0; i < populationSize; i++)
                    Pop[i] = new DNA(grid);
            }

            public void CalcFitness()
            {
                for (int i = 0; i < PopulationSize; i++)
                {
                    Pop[i].CalcFitness();
                }
            }

            internal DNA GetBest(int timelimit, int np)
            {
                for (int i = 0; i < PopulationSize; i++)
                {
                    Pop[i].Grid.ApplyEnemyNuisance(np);
                    if (round > 0)
                        Pop[i].Shift();
                }

                //Console.Error.WriteLine("Applied: {0}", np);
                CalcFitness();

                DNA best = Pop.Max().Clone();
                //for (int i = 1; i < PopulationSize; i++)
                //    if (Pop[i].Fitness > best.Fitness) best = Pop[i];

                //Console.Error.WriteLine("Best Init: {0}", best.Fitness);
                while (_stopwatch.ElapsedMilliseconds < timelimit)
                {
                    for (int i = 0; i < PopulationSize; i++)
                    {
                        var mom = Pop[FastRandom.Rnd(PopulationSize)];
                        var dad = Pop[FastRandom.Rnd(PopulationSize)];
                        var child = mom.Crossover(dad);
                        child.Mutate();
                        child.CalcFitness();
                        Pop[i] = child;
                        //var sb = new StringBuilder();
                        //for (int k = 0; k < Height; k++)
                        //{
                        //    for (int j = 0; j < Width; j++)
                        //    {
                        //        sb.Append(Pop[i].Grid.Field[j, k] + " ");
                        //    }

                        //    sb.AppendLine();
                        //}
                        //Console.Error.WriteLine("{0}\nCloned Score: {1}", sb.ToString(), Pop[i].Fitness);
                    }

                    //var temp = Pop.Max();
                    //if (temp.Fitness > best.Fitness)
                    //{
                    //    Console.Error.WriteLine("B: {0} C: {1}", best.Fitness, temp.Fitness);
                    //    best = temp.Clone();
                    //}
                    int bestIndex = -1;
                    int maxFitness = best.Fitness;
                    for (int i = 0; i < PopulationSize; i++)
                    {
                        if (Pop[i].Fitness > best.Fitness)
                        {
                            //Console.Error.WriteLine("B: {0} C: {1} M:{2}", best.Fitness, Pop[i].Fitness, Pop[i].Genes[0]);
                            bestIndex = i;
                            maxFitness = Pop[i].Fitness;
                        }
                    }
                    if (bestIndex > -1) best = Pop[bestIndex].Clone();
                }

                return best;
            }

            internal void UpdateGrid(Grid grid)
            {
                for (int i = 0; i < Pop.Count(); i++)
                {
                    Pop[i].Grid = grid;
                }
            }
        }

        private struct Gene
        {
            public int Column;
            public int Rotation;

            public override string ToString()
            {
                return string.Format("{0} {1}", Column, Rotation);
            }
        }

        private class DNA : IComparable<DNA>
        {
            public Gene[] Genes { get; set; }
            public int Fitness { get; set; }
            public int Nuisance { get; set; }

            public Grid Grid { get; set; }

            public DNA(Grid grid, bool init = true)
            {
                Genes = new Gene[Lifespan];
                Grid = grid;
                if (init)
                {
                    Fitness = int.MinValue;
                    Nuisance = 0;
                    for (int i = 0; i < Lifespan; i++)
                        Mutate(i, true);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Mutate()
            {
                Mutate(FastRandom.Rnd(Lifespan));
            }

            public void Mutate(int index, bool all = false)
            {
                int chance = FastRandom.Rnd(1);

                if (all || chance == 0)
                    Genes[index].Column = FastRandom.Rnd(Width - 1);
                if (all || chance == 1)
                    Genes[index].Rotation = FastRandom.Rnd(3);

                Fitness = int.MinValue;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Shift()
            {
                for (int i = 1; i < Lifespan; i++)
                {
                    Genes[i - 1] = Genes[i];
                }
                Mutate(Lifespan - 1, true);
            }

            public void CalcFitness()
            {
                if (Fitness == int.MinValue)
                {
                    for (int i = 0; i < Lifespan; i++)
                    {
                        int initScore = Grid.score;
                        if (!Grid.ApplyMove(Genes[i], _nextBlocks[i]))
                        {
                            Grid.score = -1000;
                            break;
                        }
                        Grid.RemoveMatches(0);
                        //if (Grid.score > initScore)
                        //    Console.Error.WriteLine("Move for score: {0}", Genes[i]);
                        Grid.NuisancePoints += (Grid.score - initScore) / 70;
                        //Grid.score *= (int)Math.Pow(0.8, i);
                    }

                    //Console.Error.WriteLine("S-NP:{0} GS {1}", Grid.score, Grid.score);
                    Fitness = Grid.score + Grid.NuisancePoints - 100 * Grid.TallestColumn();
                    Nuisance = Grid.NuisancePoints;
                    //if (Grid.TallestColumn() > 5) Fitness -= 10000;

                    Grid.Load();
                    if (round > 0) _simulations++;
                }
                //Console.Error.WriteLine("S-NP:{0} GS {1}", Grid.NuisancePoints, Fitness);
            }

            public int CompareTo(DNA obj)
            {
                if (Fitness < obj.Fitness) return -1;
                if (Fitness == obj.Fitness) return 0;
                return 1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal DNA Clone()
            {
                DNA dna = new DNA(Grid, false);
                Genes.CopyTo(dna.Genes, 0);
                dna.Fitness = Fitness;
                dna.Nuisance = Nuisance;
                return dna;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal DNA Crossover(DNA partner)
            {
                var genes = new Gene[Lifespan];
                var mid = (int)FastRandom.Rnd(Lifespan);
                for (int i = 0; i < Lifespan; i++)
                {
                    if (i > mid)
                        genes[i] = Genes[i];
                    else
                        genes[i] = partner.Genes[i];
                }

                return new DNA(Grid, false) { Genes = genes };
            }
        }

        private static void Main(string[] args)
        {
            Population population = null;
            Population enemyPop = null;

            // game loop
            while (true)
            {
                round++;

                for (int i = 0; i < 8; i++)
                {
                    string[] inputs = Console.ReadLine().Split(' ');
                    int colorA = int.Parse(inputs[0]); // color of the first block
                    int colorB = int.Parse(inputs[1]); // color of the attached block
                    _nextBlocks[i] = new BlockPair(colorA, colorB);
                }
                _score = int.Parse(Console.ReadLine());
                for (int i = 0; i < 12; i++)
                {
                    var row = Console.ReadLine(); // One line of the map ('.' = empty, '0' = skull block, '1' to '5' = colored block)
                    for (int j = 0; j < Width; j++)
                    {
                        if (row[j] == '.')
                            _myGrid.Field[j, i] = new Block(-1, j, i);
                        else
                            _myGrid.Field[j, i] = new Block(int.Parse(row[j].ToString()), j, i);
                    }
                }
                _enemyScore = int.Parse(Console.ReadLine());
                for (int i = 0; i < 12; i++)
                {
                    var row = Console.ReadLine(); // One line of the map ('.' = empty, '0' = skull block, '1' to '5' = colored block)
                    for (int j = 0; j < Width; j++)
                    {
                        if (row[j] == '.')
                            _enemyGrid.Field[j, i] = new Block(-1, j, i);
                        else
                            _enemyGrid.Field[j, i] = new Block(int.Parse(row[j].ToString()), j, i);
                    }
                }

                //var sb = new StringBuilder();
                //for (int k = 0; k < Height; k++)
                //{
                //    for (int i = 0; i < Width; i++)
                //    {
                //        sb.Append(_myGrid.Field[i, k] + " ");
                //    }

                //    sb.AppendLine();
                //}
                //Console.Error.WriteLine(sb.ToString());

                _myGrid.Save();
                _enemyGrid.Save();
                if (round == 0)
                {
                    population = new Population(PopulationSize, _myGrid);
                    enemyPop = new Population(PopulationSize, _enemyGrid);
                }
                else
                {
                    population.UpdateGrid(_myGrid);
                    enemyPop.UpdateGrid(_enemyGrid);
                }
                _stopwatch = Stopwatch.StartNew();

                var enemyDna = enemyPop.GetBest(15, 0);
                _enemyNuisancePoints += enemyDna.Nuisance;
                Console.Error.WriteLine("NP: {0}", _enemyNuisancePoints);
                var dna = population.GetBest(80, _enemyNuisancePoints);
                Console.WriteLine(dna.Genes[0]); // "x": the column in which to drop your blocks
                //Console.Error.WriteLine("TALLEST: {0}", dna.Grid.TallestColumn());
                if (!true)
                {
                    _myGrid.ApplyMove(dna.Genes[0], _nextBlocks[0]);
                    _myGrid.RemoveMatches(0, true);
                    var sb = new StringBuilder();
                    for (int k = 0; k < Height; k++)
                    {
                        for (int j = 0; j < Width; j++)
                        {
                            sb.Append(_myGrid.Field[j, k] + " ");
                        }

                        sb.AppendLine();
                    }
                    Console.Error.WriteLine("{0} {1} M:{2}", sb.ToString(), _myGrid.score, dna.Genes[0]);
                }
                if (_enemyNuisancePoints >= 6) _enemyNuisancePoints /= 6;
                Console.Error.WriteLine("Score after 8: {0}\nAvg Sims: {1}", dna.Fitness, round > 0 ? _simulations / round : 0);
            }
        }
    }
}