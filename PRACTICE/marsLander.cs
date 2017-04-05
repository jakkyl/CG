using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/

namespace MarsLander
{
    internal class Player
    {
        private const int ZoneWidth = 7000;
        private const int ZoneHeight = 3000;
        private const int MinFlatSurface = 1000;
        private const double Gravity = 3.711;
        private const double MaxVerticalSpeed = 40.0;
        private const double MaxHorizontalSpeed = 20.0;
        private const int Depth = 75;
        private const int Population = 30;
        private const int maxDist = 15232;

        private static Stopwatch stopwatch = new Stopwatch();
        private static IList<Point> map = new List<Point>();
        private static Point[] landingZone = new Point[2];

        private static Random rand = new Random();
        private static int solutionsTried = 0;
        private static Point landingZoneCenter;

        private static int GetRandom(int min, int max)
        {
            return rand.Next(min, max);
        }

        [Serializable]
        internal class Point
        {
            public Point(double x, double y)
            {
                X = x;
                Y = y;
            }

            public double X { get; set; }

            public double Y { get; set; }

            public double Distance2(Point p)
            {
                return Math.Pow(X - p.X, 2) + Math.Pow(Y - p.Y, 2);
            }

            public double Distance(Point p)
            {
                return Math.Sqrt(Distance2(p));
            }

            public Point Closest(Point a, Point b)
            {
                var deltaA = b.Y - a.Y;
                var deltaB = a.X - b.X;
                var c1 = deltaA * a.X + deltaB * a.Y;
                var c2 = -deltaB * X + deltaA * Y;
                var det = deltaA * deltaA + deltaB * deltaB;
                double cY = 0;
                double cX = 0;

                if (det != 0)
                {
                    cX = (deltaA * c1 - deltaB * c2) / det;
                    cY = (deltaA * c2 + deltaB * c1) / det;
                }
                else
                {
                    cX = X;
                    cY = Y;
                }

                return new Point(cX, cY);
            }

            public override string ToString()
            {
                return string.Format("[{0},{1}]", X, Y);
            }
        }

        [Serializable]
        private class Lander : Point
        {
            public double HorizontalVelocity { get; set; }
            public double VerticalVelocity { get; set; }
            public double Fuel { get; set; }
            public double Rotation { get; set; }
            public double Power { get; set; }

            public Solution sol;
            private Solution[] solutions = new Solution[Population];

            public Lander(double x, double y)
                : base(x, y)
            {
            }

            public void Move(double angle, int thrust)
            {
                if (angle > Rotation + 15) Rotation += 15;
                else if (angle < Rotation - 15) Rotation -= 15;
                else Rotation = angle;
                Rotation = Math.Max(-90, Math.Min(Rotation, 90));

                if (thrust > Power) Power = Math.Min(4, Power + 1);
                else if (thrust < Power) Power = Math.Max(0, Power - 1);

                var rotRads = -Rotation * Math.PI / 180;
                var sin = Math.Sin(rotRads) * Power;
                var cos = Math.Cos(rotRads) * Power - Gravity;
                HorizontalVelocity += sin;
                VerticalVelocity += cos;
                X += HorizontalVelocity - 0.5 * sin;
                Y += VerticalVelocity - 0.5 * cos;
                //X += HorizontalVelocity - 0.5 * sin;
                //Y += VerticalVelocity + 0.5 * cos;
                //HorizontalVelocity *= sin;
                //VerticalVelocity *= cos;
            }

            public void GetBestSolution(int timelimit, bool seed = false)
            {
                //if (!seed)
                //    for (int i = 0; i < Population; i++)
                //        solutions[i] = new Solution();

                for (int i = 0; i < Population; i++)
                {
                    solutions[i] = Solve(50, solutions[i]);
                }

                solutions = solutions.OrderByDescending(s => s.score).ToArray();
                sol = solutions[0];

                var mom = solutions[0];
                var dad = solutions[1];

                var mud = 5;
                if (mom.score > 100) mud = 3;
                if (mom.score > 200) mud = 1;
                for (int j = 2; j < Population; j++)
                {
                    for (int i = 0; i < Depth; i++)
                    {
                        solutions[j].angles[i] = mom.angles[i] + rand.NextDouble() * (dad.angles[i] - mom.angles[i]);
                        solutions[j].thrust[i] = (int)(mom.thrust[i] + rand.NextDouble() * (dad.thrust[i] - mom.thrust[i]));

                        var progress = i / Depth;
                        var progressChance = 0.4 + 1.0 * progress;
                        var mutChange = 0.02 * mud * progressChance;
                        if (rand.NextDouble() < mutChange)
                        {
                            solutions[j].angles[i] += rand.Next(-10, 10);
                            solutions[j].thrust[i] += (int)(rand.NextDouble() - 0.5);
                        }
                    }
                    //solutions[j].Mutate();
                }

                if (stopwatch.ElapsedMilliseconds < timelimit * .90) GetBestSolution(timelimit, true);
            }

            public Solution Solve(int timelimit, Solution bestSolution)
            {
                //Solution bestSolution = null;
                if (bestSolution != null)
                {
                    //bestSolution = sol;
                    //bestSolution.Shift();
                }
                else
                {
                    bestSolution = new Solution();
                }
                GetScore(ref bestSolution);
                return bestSolution;
                /*while (stopwatch.ElapsedMilliseconds < timelimit)
                {
                    var child = bestSolution.Clone();
                    child.Mutate();

                    //Console.Error.WriteLine("best: {0}\n{1}\nChild:{2}\n{3}", string.Join(",", bestSolution.angles), string.Join(",", bestSolution.thrust), string.Join(",", child.angles), string.Join(",", child.thrust));

                    if (GetScore(child) > GetScore(bestSolution))
                    {
                        bestSolution = sol = child;
                        //Console.Error.WriteLine("Better Solution: {0}\n{1}\nScore:{2}", string.Join(",", bestSolution.angles), string.Join(",", bestSolution.thrust), bestSolution.score);
                        //Console.Error.WriteLine("Better Solution: {0}", bestSolution.score);
                    }
                }*/
            }

            private Point lastPos = new Point(0, 0);

            private double GetScore(ref Solution solution)
            {
                if (solution.score == -1)
                {
                    bool col = false;
                    solution.Time = 0;
                    for (int i = 0; i < Depth; i++)
                    {
                        solution.Time++;
                        //Console.Error.WriteLine("T: {0}, {1}", solution.Time, i);
                        Move(solution.angles[i], solution.thrust[i]);
                        if (CheckCollisions())
                        {
                            col = true;
                            break;
                        }
                        Console.Error.WriteLine("T: {0}, {1}", solution.Time, i);
                    }

                    bool inLandingZone = X > landingZone[0].X && X < landingZone[1].X;
                    if (col && inLandingZone)
                    {
                        //if (this.X != lastPos.X || this.Y != lastPos.Y)
                        //    Console.Error.WriteLine("Last Pos: {0}", this);
                        //lastPos.X = X;
                        //lastPos.Y = Y;
                        //crash landing
                        if (VerticalVelocity >= MaxVerticalSpeed || Math.Abs(HorizontalVelocity) >= MaxHorizontalSpeed)
                        {
                            //Console.Error.WriteLine("Speed {0} {1}", VerticalVelocity, HorizontalVelocity);
                            var dX = 0.0;
                            var dY = 0.0;
                            var dA = 0.0;
                            //too fast horizontally
                            if (Math.Abs(HorizontalVelocity) > MaxHorizontalSpeed)
                            {
                                dX = (Math.Abs(HorizontalVelocity) - MaxHorizontalSpeed) / 2;
                            }
                            if (VerticalVelocity > MaxVerticalSpeed)
                            {
                                dY = (VerticalVelocity - MaxVerticalSpeed);
                            }
                            if (Rotation != 0)
                            {
                                dA = Math.Abs(Rotation) / 2;
                            }
                            solution.score = 200 - dX - dY - dA;
                        }
                        else
                        {
                            solution.score = 300;
                        }
                    }
                    else
                    {
                        var currentSpeed = Math.Sqrt(Math.Pow(HorizontalVelocity, 2) + Math.Pow(VerticalVelocity, 2));
                        var distanceToZone = Distance(landingZoneCenter);
                        solution.score = 100 - (100 * distanceToZone / maxDist);
                        var speedPenalty = 0.1 * Math.Max(currentSpeed - 100, 0);
                        solution.score -= speedPenalty;
                    }

                    solutionsTried++;

                    Load();
                }

                return solution.score;
            }

            private bool CheckCollisions()
            {
                for (int i = 1; i < map.Count; i++)
                {
                    var a = map[i - 1];
                    var b = map[i];
                    var p = (b.X - a.X) * (Y - a.Y) - (b.Y - a.Y) * (X - a.X);
                    //var a = (map[i].X - map[i - 1].X) * (Y - map[i - 1].Y);
                    //var b = (X - map[i - 1].X) * (map[i].X - map[i - 1].X);
                    //var c = a - b;

                    if (p < 0)
                    {
                        //Console.Error.WriteLine("COL: {0}", this);
                        return true;
                    }
                }

                return false;
            }

            private double[] cache = new double[6];

            internal void Save()
            {
                cache[0] = X;
                cache[1] = Y;
                cache[2] = HorizontalVelocity;
                cache[3] = VerticalVelocity;
                cache[4] = Rotation;
                cache[5] = Power;
            }

            private void Load()
            {
                X = cache[0];
                Y = cache[1];
                HorizontalVelocity = cache[2];
                VerticalVelocity = cache[3];
                Rotation = cache[4];
                Power = cache[5];
            }
        }

        [Serializable]
        private class Solution
        {
            public double[] angles = new double[Depth];
            public int[] thrust = new int[Depth];
            public double score = -1;

            public Solution(bool init = true)
            {
                if (!init) return;

                Init();
            }

            public void Init()
            {
                for (int i = 0; i < Depth; i++)
                {
                    Mutate(i, true);
                }
            }

            public void Mutate()
            {
                Mutate(GetRandom(0, Depth - 1));
            }

            public void Mutate(int i, bool all = false)
            {
                int r = GetRandom(0, 1);
                if (all || r == 0)
                {
                    angles[i] += GetRandom(-15, 15);
                    angles[i] = Math.Max(-90, Math.Min(angles[i], 90));
                }
                if (all || r == 1)
                {
                    thrust[i] = Math.Max(0, Math.Min(GetRandom(0, 8), 4));
                }
                score = -1;
            }

            public void Shift()
            {
                for (int i = 1; i < Depth; i++)
                {
                    angles[i - 1] = angles[i];
                    thrust[i - 1] = thrust[i];
                }

                Mutate(Depth - 1, true);
                score = -1;
            }

            public Solution Clone()
            {
                var s = new Solution(false);
                angles.CopyTo(s.angles, 0);
                thrust.CopyTo(s.thrust, 0);
                return s;
            }

            public int Time;
        }

        private static void Main(string[] args)
        {
            string[] inputs;
            int surfaceN = int.Parse(Console.ReadLine()); // the number of points used to draw the surface of Mars.

            for (int i = 0; i < surfaceN; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int landX = int.Parse(inputs[0]); // X coordinate of a surface point. (0 to 6999)
                int landY = int.Parse(inputs[1]); // Y coordinate of a surface point. By linking all the points together in a sequential fashion, you form the surface of Mars.
                map.Add(new Point(landX, landY));
            }

            double maxDistance = 0;
            //find landing zone
            for (int i = 0; i < surfaceN - 1; i += 2)
            {
                if (map[i].Y == map[i + 1].Y) //flat
                {
                    var distance = map[i].Distance(map[i + 1]);
                    if (distance > maxDistance)
                    {
                        landingZone[0] = map[i];
                        landingZone[1] = map[i + 1];
                        maxDistance = distance;
                    }
                }
            }
            landingZoneCenter = new Point((landingZone[1].X - landingZone[0].X) * 0.5 + landingZone[0].X, landingZone[0].Y);
            Console.Error.WriteLine("Zone: {0}, {1}", landingZone[0], landingZone[1]);

            Lander lander = new Lander(0, 0);
            int t = -1;
            // game loop
            while (true)
            {
                t++;

                inputs = Console.ReadLine().Split(' ');
                int X = int.Parse(inputs[0]);
                int Y = int.Parse(inputs[1]);
                int hSpeed = int.Parse(inputs[2]); // the horizontal speed (in m/s), can be negative.
                int vSpeed = int.Parse(inputs[3]); // the vertical speed (in m/s), can be negative.
                int fuel = int.Parse(inputs[4]); // the quantity of remaining fuel in liters.
                int rotate = int.Parse(inputs[5]); // the rotation angle in degrees (-90 to 90).
                int power = int.Parse(inputs[6]); // the thrust power (0 to 4).

                lander.X = X;
                lander.Y = Y;
                lander.HorizontalVelocity = hSpeed;
                lander.VerticalVelocity = vSpeed;
                lander.Fuel = fuel;
                lander.Rotation = rotate;
                lander.Power = power;
                lander.Save();

                if (t == 0)
                {
                    //Console.Error.WriteLine("Lander: {0} {1} {2} {3}", X, Y, rotate, power);
                    stopwatch.Restart();
                    //lander.Solve(140, t > 0);
                    lander.GetBestSolution(95, t > 0);
                    //Console.Error.WriteLine("Solution: {0}\n{1}\nScore:{2}", string.Join(",", lander.sol.angles), string.Join(",", lander.sol.thrust), lander.sol.score);
                    Console.Error.WriteLine("Score:{0} {1}", lander.sol.score, lander.sol.Time);

                    for (int i = 0; i < lander.sol.Time; i++)
                    {
                        lander.sol.angles[i] = Math.Max(-90, Math.Min(lander.sol.angles[i], 90));
                        lander.sol.thrust[i] = Math.Max(0, Math.Min(lander.sol.thrust[i], 4));
                        Console.WriteLine((int)lander.sol.angles[i] + " " + lander.sol.thrust[i]);
                    }
                    Console.WriteLine(0 + " " + 4);
                    Console.WriteLine(0 + " " + 4);
                    Console.WriteLine(0 + " " + 4);
                    Console.WriteLine(0 + " " + 4);
                }
                if (t > 0) Console.Error.WriteLine("Avg Sols: {0}, Avg Sims: {1}", solutionsTried / t, solutionsTried * Depth / t);

                //Console.WriteLine((int)lander.sol.angles[0] + " " + lander.sol.thrust[0]);
                /*
                if (lander.X < landingZone[0].X)
                {
                    //go right
                    if (lander.HorizontalVelocity < MaxHorizontalSpeed)
                    {
                        lander.Rotation = -21.9;
                    }
                    var distanceToDest = landingZone[0].X - lander.X;

                    lander.Power = 4;
                }
                else if (lander.X > landingZone[1].X)
                {
                    //go left
                    if (lander.HorizontalVelocity > -MaxHorizontalSpeed)
                    {
                        lander.Rotation = 21.9;
                    }
                    lander.Power = 4;
                }
                else
                {
                    //zero out horizontal velocity
                    var deltaRot = lander.HorizontalVelocity;
                    var error = 5;
                    if (deltaRot < 0)
                    {
                        lander.Rotation *= 0.50;
                    }
                    else if (deltaRot > 0)
                    {
                        lander.Rotation *= 0.50;
                    }

                    if (vSpeed < -40 && power < 4)
                    {
                        lander.Power += 1;
                    }
                }

                // 2 integers: rotate power. rotate is the desired rotation angle, power is the
                // desired thrust power (0 to 4).
                lander.Rotation = Math.Max(-90, Math.Min(90, (int)lander.Rotation));
                Console.WriteLine(lander.Rotation + " " + lander.Power);
                copy.Move(lander.Rotation, (int)lander.Power);
                Console.Error.WriteLine("Lander (SIM): {0} {1} {2} {3}", copy.X, copy.Y, copy.Rotation, copy.Power);
                 * */
            }
        }
    }
}