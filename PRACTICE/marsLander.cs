using System;
using System.Collections.Generic;
using System.Diagnostics;

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
        private const int Depth = 25;

        private static Stopwatch stopwatch = new Stopwatch();
        private static IList<Point> map = new List<Point>();
        private static Point[] landingZone = new Point[2];

        private static Random rand = new Random();
        private static int solutionsTried = 0;

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

                var rotRads = Rotation * Math.PI / 180;
                var sin = Math.Sin(Rotation * rotRads) * Power;
                var cos = Math.Cos(Rotation * rotRads) * Power - Gravity;
                X += HorizontalVelocity - 0.5 * sin;
                Y += VerticalVelocity + 0.5 * cos;
                HorizontalVelocity *= sin;
                VerticalVelocity *= cos;
            }

            public void Solve(int timelimit, bool seed = false)
            {
                Solution bestSolution = null;
                if (seed)
                {
                    bestSolution = sol;
                    bestSolution.Shift();
                }
                else
                {
                    bestSolution = sol = new Solution();
                }
                GetScore(bestSolution);

                while (stopwatch.ElapsedMilliseconds < timelimit)
                {
                    var child = bestSolution.Clone();
                    child.Mutate();

                    //Console.Error.WriteLine("best: {0}\n{1}\nChild:{2}\n{3}", string.Join(",", bestSolution.angles), string.Join(",", bestSolution.thrust), string.Join(",", child.angles), string.Join(",", child.thrust));

                    if (GetScore(child) > GetScore(bestSolution))
                    {
                        bestSolution = sol = child;
                        Console.Error.WriteLine("Better Solution: {0}\n{1}\nScore:{2}", string.Join(",", bestSolution.angles), string.Join(",", bestSolution.thrust), bestSolution.score);
                    }
                }
            }

            private Point lastPos = new Point(0, 0);

            private double GetScore(Solution solution)
            {
                if (solution.score == -1)
                {
                    bool col = false;
                    for (int i = 0; i < Depth; i++)
                    {
                        Move(solution.angles[i], solution.thrust[i]);
                        if (CheckCollisions())
                        {
                            col = true;
                            break;
                        }
                    }
                    bool inLandingZone = X > landingZone[0].X && X < landingZone[1].X;
                    if (col)
                    {
                        if (inLandingZone)
                        {
                            if (this as Point != lastPos)
                                Console.Error.WriteLine("Last Pos: {0}", this);
                            //crash landing
                            if (VerticalVelocity < MaxVerticalSpeed || Math.Abs(HorizontalVelocity) > MaxHorizontalSpeed)
                            {
                                var dX = 0.0;
                                var dY = 0.0;
                                //too fast horizontally
                                if (Math.Abs(HorizontalVelocity) > MaxHorizontalSpeed)
                                {
                                    dX = (Math.Abs(HorizontalVelocity) - MaxHorizontalSpeed) / 2;
                                }
                                if (VerticalVelocity < MaxVerticalSpeed)
                                {
                                    dY = (-MaxVerticalSpeed - VerticalVelocity) / 2;
                                }
                                solution.score = 200 - dX - dY;
                            }
                            else
                            {
                                solution.score = 200;
                            }
                        }
                        else
                        {
                            solution.score = int.MinValue;
                        }
                    }

                   // else if (Rotation != 0) solution.score = int.MinValue;
                    //else if (X > landingZone[0].X || X < landingZone[1].Y) solution.score = int.MinValue;
                    //else if (Y < landingZone[0].Y) solution.score = int.MinValue;
                    else
                    {
                        var currentSpeed = Math.Sqrt(Math.Pow(HorizontalVelocity, 2) + Math.Pow(VerticalVelocity, 2));
                        var maxDistance = landingZone[1].X - landingZone[0].X;
                        var distanceToZone = Distance(new Point((landingZone[1].X - landingZone[0].X) * 0.5, landingZone[0].Y));
                        solution.score = 100 - (100 * distanceToZone / (2 * ZoneWidth));
                        var speedPenalty = 0.1 * Math.Max(currentSpeed - 100, 0);
                        solution.score -= speedPenalty;
                        //solution.score =
                        //solution.score = -Distance(new Point((landingZone[1].X - landingZone[0].X) * 0.5, Y));
                        //solution.score += 5000 / (distanceToZone + 1);
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
                    var a = (map[i].X - map[i - 1].X) * (Y - map[i - 1].Y);
                    var b = (X - map[i - 1].X) * (map[i].X - map[i - 1].X);
                    var c = a - b;

                    if (c >= 0) return true;
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
                    angles[i] = Math.Max(-90, Math.Min(GetRandom(-90, 90), 90));
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
            //Console.Error.WriteLine("Zone: {0}, {1}", landingZone[0], landingZone[1]);

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

                //Console.Error.WriteLine("Lander: {0} {1} {2} {3}", X, Y, rotate, power);
                stopwatch.Restart();
                lander.Solve(120, t > 0);
                Console.Error.WriteLine("Solution: {0}\n{1}", string.Join(",", lander.sol.angles), string.Join(",", lander.sol.thrust));
                if (t > 0) Console.Error.WriteLine("Avg Sols: {0}, Avg Sims: {1}", solutionsTried / t, solutionsTried * Depth / t);

                Console.WriteLine((int)lander.sol.angles[0] + " " + lander.sol.thrust[0]);
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