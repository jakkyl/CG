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
        private const double MaxVerticalSpeed = -40.0;
        private const double MaxHorizontalSpeed = 20.0;
        private const int Depth = 300;
        private const int Population = 5;
        private const int maxDist = 15232;

        private static Stopwatch stopwatch;
        private static IList<Point> map = new List<Point>();
        private static Point[] landingZone = new Point[2];

        private static Random rand = new Random();
        private static int solutionsTried = 0;
        private static Point landingZoneCenter;
        private static int initFuel;

        private static int GetRandom(int min, int max)
        {
            return rand.Next(min, max);
        }

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

            public double Dot(Point p)
            {
                return X * p.X + p.Y * Y;
            }

            public Point Subtract(Point p)
            {
                return new Point(p.X - X, p.Y - Y);
            }

            public override string ToString()
            {
                return string.Format("[{0},{1}]", X, Y);
            }
        }

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
            { }

            public void Move(double angle, int thrust)
            {
                if (angle > Rotation + 15) Rotation += 15;
                else if (angle < Rotation - 15) Rotation -= 15;
                else Rotation = angle;
                Rotation = Math.Max(-90, Math.Min(Rotation, 90));

                if (thrust > Power) Power = Math.Min(4, Power + 1);
                else if (thrust < Power) Power = Math.Max(0, Power - 1);

                if (Fuel < Power)
                    Power = Fuel;
                Fuel -= Power;

                var rotRads = -Rotation * Math.PI / 180;
                var xAcc = Math.Sin(rotRads) * Power;
                var yAcc = Math.Cos(rotRads) * Power - Gravity;
                HorizontalVelocity += xAcc;
                VerticalVelocity += yAcc;
                X += HorizontalVelocity - 0.5 * xAcc;
                Y += VerticalVelocity - 0.5 * yAcc;
            }

            public void GetBestSolution(int timelimit, bool seed = false)
            {
                //if (!seed)
                //    for (int i = 0; i < Population; i++)
                //        solutions[i] = new Solution();

                for (int i = 0; i < Population; i++)
                {
                    //Solve(50, ref solutions[i]);
                }

                solutions = solutions.OrderByDescending(s => s.score).ToArray();
                sol = solutions[0];
                //Console.Error.WriteLine("best: {0}\n{1}", string.Join(",", sol.angles),
                //    //string.Join(",", solutions.Select(s => s.score)));
                //    string.Join(",\n", solutions.Select(s => string.Join(",", s.angles))));

                var mom = solutions[0];
                var dad = solutions[1];

                var mud = 5;
                if (mom.score > 100) mud = 3;
                if (mom.score > 200) mud = 1;
                for (int j = 2; j < Population; j++)
                {
                    for (int i = 0; i < Depth; i++)
                    {
                        //solutions[j].angles[i] = mom.angles[i] + rand.NextDouble() * (dad.angles[i] - mom.angles[i]);
                        //solutions[j].thrust[i] = (int)(mom.thrust[i] + rand.NextDouble() * (dad.thrust[i] - mom.thrust[i]));

                        //var progress = i / Depth;
                        //var progressChance = 0.4 + 1.0 * progress;
                        //var mutChange = 0.02 * mud * progressChance;
                        //if (rand.NextDouble() < mutChange)
                        //{
                        //    solutions[j].angles[i] += rand.Next(-10, 10);
                        //    solutions[j].thrust[i] += (int)(rand.NextDouble() - 0.5);
                        //}
                    }
                    solutions[j].Mutate();
                }

                if (stopwatch.ElapsedMilliseconds < timelimit * .90) GetBestSolution(timelimit, true);
            }

            public void Solve(int timelimit, bool seed)
            {
                Solution bestSolution;
                if (seed)
                {
                    bestSolution = sol;
                    bestSolution.Shift();
                }
                else
                {
                    bestSolution = new Solution();
                }
                GetScore(bestSolution);

                Solution child;
                while (stopwatch.ElapsedMilliseconds < timelimit)
                {
                    child = bestSolution.Clone();
                    child.Mutate();

                    if (GetScore(child) > GetScore(bestSolution))
                    {
                        bestSolution = child;
                    }
                }
                sol = bestSolution;
            }

            private double GetScore(Solution solution)
            {
                if (solution.score == -1)
                {
                    bool col = false;
                    int t = 0;
                    for (int i = 0; i < Depth; i++)
                    {
                        t++;
                        if (CheckCollisions())
                        {
                            col = true;
                            break;
                        }
                        //Console.Error.WriteLine("T: {0}, {1}", solution.Time, i);
                        Move(solution.angles[i], solution.thrust[i]);
                    }

                    solution.Time = t;
                    bool inLandingZone = X > landingZoneCenter.X - 50 && X < landingZoneCenter.X + 50;
                    if (col && inLandingZone)
                    {
                        //Console.Error.WriteLine("LANDING ZONE");
                        //crash landing
                        if (VerticalVelocity < MaxVerticalSpeed || Math.Abs(HorizontalVelocity) > MaxHorizontalSpeed)
                        {
                            var dX = 0.0;
                            var dY = 0.0;
                            var dA = 0.0;
                            //too fast horizontally
                            if (Math.Abs(HorizontalVelocity) > MaxHorizontalSpeed)
                            {
                                dX = (Math.Abs(HorizontalVelocity) - MaxHorizontalSpeed) * 0.5;
                            }
                            if (VerticalVelocity < MaxVerticalSpeed)
                            {
                                dY = (MaxVerticalSpeed - VerticalVelocity) * 0.5;
                            }
                            if (Rotation != 0)
                            {
                                //dA = -Rotation * .5;
                            }
                            solution.score = 200 - dX - dY - dA;// -(Distance(landingZoneCenter) / maxDist);
                        }
                        else
                        {
                            //Console.Error.WriteLine("Speed {0} {1}", VerticalVelocity, HorizontalVelocity);
                            solution.score = 200 + (100 * Fuel / initFuel);
                        }
                    }
                    else
                    {
                        var currentSpeed = Math.Sqrt(Math.Pow(HorizontalVelocity, 2) + Math.Pow(VerticalVelocity, 2));
                        var distanceToZone = Distance(landingZoneCenter);
                        //solution.score = -distanceToZone;
                        solution.score = 100 - (100 * distanceToZone / maxDist);
                        var speedPenalty = 0.1 * Math.Max(currentSpeed - 100, 0);
                        solution.score -= speedPenalty;
                    }

                    solutionsTried++;

                    Load();
                }

                return solution.score;
            }

            public bool CheckCollisions()
            {
                if (X < 0 || X > ZoneWidth || Y < landingZone[0].Y) return true;
                return false;
                for (int i = 0; i < map.Count - 1; i++)
                {
                    //var a = map[i - 1];
                    //var b = map[i];
                    //var p = (b.X - a.X) * (Y - a.Y) - (b.Y - a.Y) * (X - a.X);
                    var p1 = map[i];
                    var p2 = map[i + 1];
                    var line = new Point(p2.X - p1.X, p2.Y - p1.Y);
                    //var distance = new Point(X - b.X, Y - b.Y);
                    //var dot = line.X * X + line.Y * Y;

                    //var aa = line.Dot(line);
                    //var bb = 2 * distance.Dot(line);
                    //var cc = distance.Dot(distance) - 2;

                    //var disc = bb * bb - 4 * aa * cc;

                    ////var a = (map[i].X - map[i - 1].X) * (Y - map[i - 1].Y);
                    ////var b = (X - map[i - 1].X) * (map[i].X - map[i - 1].X);
                    ////var c = a - b;

                    //var lab = a.Distance(b);
                    //var dx = (b.X - a.X) / lab;
                    //var dy = (b.Y - a.Y) / lab;
                    //var t = dx * (X - a.X) + dy * (Y - a.Y);

                    //var ex = t * dx + a.X;
                    //var ey = t * dy + a.Y;

                    //var lec = new Point(ex, ey).Distance(this);
                    var rSq = 100;
                    var v = line;
                    var a = v.Dot(v);
                    var b = 2 * v.Dot(p1.Subtract(this));
                    var c = p1.Dot(p1) + this.Dot(this) - 2 * p1.Dot(this) - rSq;
                    var disc = b * b - 4 * a * c;

                    var mag = Math.Sqrt(line.X * line.X + line.Y * line.Y);
                    var normal = new Point(-line.Y, line.X);
                    var unit = new Point(normal.X / mag, normal.Y / mag);
                    var d = unit.Dot(new Point(X - line.X, Y - line.Y));
                    Console.Error.WriteLine("COL1:{0}", d);
                    //if (d > 0) return true;
                    //else continue;
                    //Console.Error.WriteLine("COL1: {2} {0} {1}", new Point(ex, ey), lec, i);
                    //if (lec <= 2)
                    //{
                    //    Console.Error.WriteLine("COL: {0} {1}", new Point(ex, ey), lec);
                    //    return true;
                    //}
                    //else continue;

                    if (disc < 0) continue;
                    else
                    {
                        var sDisc = Math.Sqrt(disc);
                        var qA = 1 / (2 * a);
                        double t1 = (-b - disc) * qA;
                        double t2 = (-b + disc) * qA;

                        //Console.Error.WriteLine("COL: {0} {1} {2} {3}", this, t1, t2, i);
                        if ((t1 >= 0.0 && t1 <= 1.0) || (t2 >= 0.0 && t2 <= 1.0))
                        {
                            //Console.Error.WriteLine("COL: {0} {1}", this, disc);
                            return true;
                        }
                    }
                }
                //Console.Error.WriteLine("NO COL: {0}", this);

                return false;
            }

            private double[] cache = new double[7];

            internal void Save()
            {
                cache[0] = X;
                cache[1] = Y;
                cache[2] = HorizontalVelocity;
                cache[3] = VerticalVelocity;
                cache[4] = Rotation;
                cache[5] = Power;
                cache[6] = Fuel;
            }

            private void Load()
            {
                X = cache[0];
                Y = cache[1];
                HorizontalVelocity = cache[2];
                VerticalVelocity = cache[3];
                Rotation = cache[4];
                Power = cache[5];
                Fuel = cache[6];
            }
        }

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
            for (int i = 0; i < surfaceN - 1; i++)
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
            Console.Error.WriteLine("Zone: {0}, {1}: {2}", landingZone[0], landingZone[1], landingZoneCenter);

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
                if (t == 0) initFuel = fuel;
                lander.X = X;
                lander.Y = Y;
                lander.HorizontalVelocity = hSpeed;
                lander.VerticalVelocity = vSpeed;
                lander.Fuel = fuel;
                lander.Rotation = rotate;
                lander.Power = power;
                lander.Save();
                stopwatch = Stopwatch.StartNew();
                //lander.Solve(140, t > 0);
                lander.Solve(140, t > 0);
                //if (t == 0)
                //{
                //Console.Error.WriteLine("Lander: {0} {1} {2} {3}", X, Y, rotate, power);
                //Console.Error.WriteLine("Solution: {0}\n{1}\nScore:{2}", string.Join(",", lander.sol.angles), string.Join(",", lander.sol.thrust), lander.sol.score);
                //for (int i = 0; i < Depth; i++)
                //{
                //    if (lander.CheckCollisions())
                //    {
                //        Console.Error.WriteLine("Col");
                //        break;
                //    }
                //    lander.Move(lander.sol.angles[i], lander.sol.thrust[i]);
                //    Console.Error.WriteLine("Lander Pos: {0} {1} V: {2} {3}", Math.Round(lander.X), Math.Round(lander.Y), Math.Round(lander.HorizontalVelocity), Math.Round(lander.VerticalVelocity));
                //}
                Console.WriteLine((int)lander.sol.angles[0] + " " + lander.sol.thrust[0]);

                //for (int i = 0; i < lander.sol.Time; i++)
                //{
                //    lander.sol.angles[i] = Math.Max(-90, Math.Min(lander.sol.angles[i], 90));
                //    lander.sol.thrust[i] = Math.Max(0, Math.Min(lander.sol.thrust[i], 4));
                //    Console.WriteLine((int)lander.sol.angles[i] + " " + lander.sol.thrust[i]);
                //}
                //}
                Console.Error.WriteLine("Score:{0} {1}", lander.sol.score, lander.sol.Time);
                Console.Error.WriteLine("Speed {0} {1}", lander.VerticalVelocity, lander.HorizontalVelocity);

                if (t > 0) Console.Error.WriteLine("Avg Sols: {0}, Avg Sims: {1}", solutionsTried / 1, solutionsTried * Depth / t);

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