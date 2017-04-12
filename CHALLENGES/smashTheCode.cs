using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/

namespace SmashTheCode
{
    internal class Player
    {
        private const int Lifespan = 8;
        private const int PopulationSize = 15;

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

        private class Point
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
            public Block[,] Field { get; set; }
            public int score = 0;
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
                        pos2Y = 1;
                        break;

                    case 2:
                        pos2X = -1;
                        pos2Y = 0;
                        break;

                    default:
                        pos2X = 0;
                        pos2Y = -1;
                        break;
                }
                if (move.Column + pos2X >= Width || move.Column + pos2X < 0) return false;

                bool placedA = false;
                bool placedB = false;
                for (int i = Height - 1; i >= 0; i--)
                {
                    if (i + pos2Y >= Height || i + pos2Y < 0) continue;
                    if (!placedA && Field[move.Column, i].Color == Color.Empty)
                    {
                        Field[move.Column, i].Color = blocks.BlockA.Color;
                        if (placedB) return true;
                        placedA = true;
                    }
                    if (!placedB && Field[move.Column + pos2X, i + pos2Y].Color == Color.Empty)
                    {
                        Field[move.Column + pos2X, i + pos2Y].Color = blocks.BlockB.Color;
                        if (placedA) return true;
                        placedB = true;
                    }
                }

                return false;
            }

            public void RemoveMatches(int step, bool debug = false)
            {
                var matches = CheckMatches(this);
                if (matches.Count > 0)
                {
                    int blocks = 0;
                    var colorBonus = new Dictionary<Color, int>();
                    foreach (var block in matches)
                    {
                        if (debug) Console.Error.WriteLine("Removed {0}", block);
                        colorBonus[block.Color] = 1;
                        block.Color = Color.Empty;
                        foreach (var n in block.Neighbors(this).Where(n => n.Color == Color.Skull))
                            n.Color = Color.Empty;
                        blocks += 1;
                    }
                    Gravity();
                    int chainPower = step * 8 * 2;
                    score += (10 * blocks) * Math.Max(0, Math.Min(chainPower + (colorBonus.Count - 1), 999));
                    RemoveMatches(step + 1);
                }
            }

            public void Gravity()
            {
                bool changed = false;
                do
                {
                    changed = false;
                    for (int i = 0; i < Width; i++)
                    {
                        for (int j = Height - 1; j > 1; j--)
                        {
                            if (Field[i, j].Color == Color.Empty && Field[i, j - 1].Color != Color.Empty)
                            {
                                Field[i, j].Color = Field[i, j - 1].Color;
                                Field[i, j - 1].Color = Color.Empty;
                                changed = true;
                            }
                        }
                    }
                } while (changed);
            }

            public int TallestColumn()
            {
                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        if (Field[i, j].Color != Color.Empty) return Height - j;
                    }
                }

                return 0;
            }

            internal Grid Clone()
            {
                var g = new Grid();
                Array.Copy(Field, 0, g.Field, 0, Field.Length);
                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        g.Field[i, j] = Field[i, j].Clone();
                    }
                }
                g.NuisancePoints = NuisancePoints;
                g.score = score;
                return g;
            }

            internal bool ApplyEnemyNuisance(int nuisancePoints)
            {
                while (nuisancePoints < 6)
                {
                    for (int i = 0; i < Width; i++)
                    {
                        if (Field[i, 0].Color != Color.Empty) return false;
                        Field[i, 0].Color = Color.Skull;
                    }
                    Gravity();
                    nuisancePoints -= 6;
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
                try
                {
                    if (Position.X > 0) result.Add(grid.Field[Position.X - 1, Position.Y]);
                    if (Position.X + 1 < Width - 1) result.Add(grid.Field[Position.X + 1, Position.Y]);
                    if (Position.Y > 0) result.Add(grid.Field[Position.X, Position.Y - 1]);
                    if (Position.Y + 1 < Height) result.Add(grid.Field[Position.X, Position.Y + 1]);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("ERROR: {0}", Position);
                }
                return result;
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

        private class BlockPair
        {
            public Block BlockA { get; set; }
            public Block BlockB { get; set; }

            public BlockPair(int colorA, int colorB)
            {
                BlockA = new Block(colorA, -1, -1);
                BlockB = new Block(colorB, -1, -1);
            }
        }

        private static List<Block> CheckMatches(Grid grid)
        {
            Block start;
            var blocks = new List<Block>();
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    if (grid.Field[i, j].Color != Color.Empty || grid.Field[i, j].Color != Color.Skull)
                    {
                        start = grid.Field[i, j];
                        var found = Search(grid, start);
                        if (found != null)
                        {
                            //Console.Error.WriteLine("Starting search at {0}, found {1}", start.Position, string.Join(",", found));
                            blocks.AddRange(found);
                        }
                    }
                }
            }
            blocks = blocks.Distinct().ToList();
            //if (blocks.Count > 0)
            //    Console.Error.WriteLine("Found matches {0} {1}", blocks.Count, string.Join(",", blocks));
            return blocks;
        }

        private static List<Block> Search(Grid grid, Block start)
        {
            var openList = new Queue<Block>();
            openList.Enqueue(start);

            var closedSet = new List<Block>() { start };
            var match = new Dictionary<Color, List<Block>>();
            match[start.Color] = new List<Block>() { start };

            while (openList.Count > 0)
            {
                var current = openList.Dequeue();

                foreach (var next in current.Neighbors(grid))
                {
                    if (next.Color == Color.Empty) continue;
                    if (closedSet.Any(b => b.Position == next.Position)) continue;
                    if (next.Color == current.Color)
                    {
                        if (!match.ContainsKey(current.Color)) match[current.Color] = new List<Block>();
                        match[current.Color].Add(next);
                        //if (match[current.Color].Count >= 1)
                        //    Console.Error.WriteLine("Got a color: {0} {1} {2}", next.Color, next.Position, match[current.Color].Count);
                        openList.Enqueue(next);
                        closedSet.Add(next);
                    }
                }
            }

            if (match[start.Color].Count >= MinSmash)
                return match[start.Color];

            return null;
        }

        private class Population
        {
            public DNA[] Pop { get; set; }

            public Population(int populationSize, Grid grid)
            {
                Pop = new DNA[populationSize];
                for (int i = 0; i < populationSize; i++)
                    Pop[i] = new DNA(grid.Clone());
            }

            public void CalcFitness()
            {
                for (int i = 0; i < Pop.Count(); i++)
                {
                    Pop[i].CalcFitness();
                    Pop[i].Grid = _myGrid.Clone();
                }
            }

            internal DNA GetBest(int timelimit)
            {
                for (int i = 0; i < PopulationSize; i++)
                    Pop[i].Grid.ApplyEnemyNuisance(_enemyNuisancePoints);

                CalcFitness();

                DNA best = Pop.Max<DNA>().Clone();
                Console.Error.WriteLine("Best Init: {0}", best.Fitness);
                while (_stopwatch.ElapsedMilliseconds < timelimit)
                {
                    for (int i = 0; i < PopulationSize; i++)
                    {
                        Pop[i].Grid = _myGrid.Clone();
                        Pop[i].Mutate();
                        Pop[i].CalcFitness();
                        var sb = new StringBuilder();
                        //for (int k = 0; k < Height; k++)
                        //{
                        //    for (int j = 0; j < Width; j++)
                        //    {
                        //        sb.Append(Pop[i].Grid.Field[j, k] + " ");
                        //    }

                        //    sb.AppendLine();
                        //}
                        //Console.Error.WriteLine("{0}\nCloned Score: {1}", sb.ToString(), Pop[i].Fitness);
                        if (round > 0) _simulations++;
                    }
                    var temp = Pop.Max();

                    if (temp.Fitness > best.Fitness)
                    {
                        Console.Error.WriteLine("Best: {0}, Child:{1}", best.Fitness, temp.Fitness);
                        best = temp.Clone();
                    }
                }

                return best;
            }

            internal void UpdateGrid(Grid grid)
            {
                for (int i = 0; i < Pop.Count(); i++)
                {
                    Pop[i].Grid = grid.Clone();
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

            public Grid Grid { get; set; }

            public DNA(Grid grid)
            {
                Genes = new Gene[Lifespan];
                Grid = grid;
                Fitness = int.MinValue;
                for (int i = 0; i < Lifespan; i++)
                    Mutate(i, true);
            }

            public void Mutate()
            {
                Mutate(rand.Next(Lifespan));
            }

            public void Mutate(int index, bool all = false)
            {
                int chance = rand.Next(1);

                if (all || chance == 0)
                    Genes[index].Column = rand.Next(Width);
                if (all || chance == 1)
                    Genes[index].Rotation = rand.Next(3);
            }

            public void Shift()
            {
                for (int i = 1; i < Lifespan; i++)
                {
                    Genes[i - 1] = Genes[i];
                }
                Mutate(Lifespan - 1);
            }

            public void Solve()
            {
                for (int i = 0; i < Lifespan; i++)
                {
                    int initScore = Grid.score;
                    if (!Grid.ApplyMove(Genes[i], _nextBlocks[i]))
                    {
                        Grid.score = -10000;
                        return;
                    }
                    Grid.RemoveMatches(0);
                    Grid.NuisancePoints += (Grid.score - initScore) / 70;
                }
            }

            public void CalcFitness()
            {
                Solve();
                Fitness = Grid.score + Math.Min(6, Grid.NuisancePoints) - Grid.TallestColumn();
            }

            public int CompareTo(DNA obj)
            {
                if (Fitness < obj.Fitness) return -1;
                if (Fitness == obj.Fitness) return 0;
                return 1;
            }

            internal DNA Clone()
            {
                DNA dna = new DNA(Grid.Clone());
                Genes.CopyTo(dna.Genes, 0);
                dna.Fitness = Fitness;
                return dna;
            }
        }

        private static void Main(string[] args)
        {
            Population population = null;
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

                if (round == 0)
                    population = new Population(PopulationSize, _myGrid);
                else
                    population.UpdateGrid(_myGrid);
                _stopwatch = Stopwatch.StartNew();
                var enemyPop = new Population(PopulationSize, _enemyGrid);
                var enemyDna = enemyPop.GetBest(15);
                _enemyNuisancePoints = _enemyGrid.NuisancePoints;
                var dna = population.GetBest(80);
                //dna.Grid.ApplyMove(dna.Genes[0], _nextBlocks[0]);
                //dna.Grid.RemoveMatches(0, true);
                //var sb = new StringBuilder();
                //for (int k = 0; k < Height; k++)
                //{
                //    for (int j = 0; j < Width; j++)
                //    {
                //        sb.Append(dna.Grid.Field[j, k] + " ");
                //    }

                //    sb.AppendLine();
                //}
                //Console.Error.WriteLine(sb.ToString());
                if (round > 0) Console.Error.WriteLine("Score afte 8: {0}\n{1}", dna.Fitness, _simulations / round);
                Console.WriteLine(dna.Genes[0]); // "x": the column in which to drop your blocks
            }
        }
    }
}