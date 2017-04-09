using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

/**
 * Grab Snaffles and try to throw them through the opponent's goal!
 * Move towards a Snaffle and use your team id to determine where you need to throw it.
 **/
namespace FantasticBits
{

    class Player
    {
        private const int Population = 1;
        private const int Chromosones = 6;

        private const int Width = 16001;
        private const int Height = 7501;
        private const int GoalY = 3750;
        private const int MaxRounds = 200;

        private const int PoleRadius = 300;
        private const int SnaffleRadius = 150;
        private const int MaxThrust = 150;
        private const int MaxPower = 500;

        private static int _myTeamId = 0;
        private static int _myScore = 0;
        private static int _oppScore = 0;

        private static List<Wizard> _myTeam = new List<Wizard>();
        private static List<Wizard> _oppTeam = new List<Wizard>();
        private static List<Snaffle> _snaffles = new List<Snaffle>();

        private static Random rand = new Random();
        private static Stopwatch _stopwatch;

        public enum EntityType
        {
            Wizard,
            Opponent_Wizard,
            Snaffle,
            Bludger
        }
        public class Point
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

            public Point Normalized()
            {
                var mag = Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2));
                return new Point(X / mag, Y / mag);
            }
        }

        public class Entity : Point
        {
            public int Id { get; private set; }
            public int Radius { get; set; }
            public double Mass { get; set; }
            public double Friction { get; set; }
            public Point Velocity { get; set; }
            private object[] cache = new object[7];

            public Entity(int id, double x, double y, double vx, double vy) : base(x, y)
            {
                Id = id;
                Velocity = new Point(vx, vy);
            }

            public void Thrust(int power, Point dest)
            {
                var vec = Subtract(dest);
                var normal = vec.Normalized();
                var thrust = power / Mass;
                Velocity.X *= thrust;
                Velocity.Y *= thrust;
            }
            public void Move(double t)
            {
                X += Velocity.X * t;
                Y += Velocity.Y * t;
            }

            public void End()
            {
                X = Math.Round(X);
                Y = Math.Round(Y);
                Velocity.X = Math.Truncate(Velocity.X * Friction);
                Velocity.Y = Math.Truncate(Velocity.Y * Friction);
            }

            public void Bounce(Entity unit)
            {
                var mcoeff = (Mass + unit.Mass) / (Mass * unit.Mass);

                var nx = X - unit.X;
                var ny = Y - unit.Y;

                // Square of the distance between the 2 pods. This value could be hardcoded because it is
                // always 800²
                var nxnysquare = nx * nx + ny * ny;

                var dvx = Velocity.X - unit.Velocity.X;
                var dvy = Velocity.Y - unit.Velocity.Y;

                // fx and fy are the components of the impact vector. product is just there for
                // optimisation purposes
                var product = (nx * dvx + ny * dvy) / (nxnysquare * mcoeff);
                var fx = nx * product;
                var fy = ny * product;
                var m1Inverse = 1.0 / Mass;
                var m2Inverse = 1.0 / unit.Mass;

                // We apply the impact vector once
                Velocity.X -= fx * m1Inverse;
                Velocity.Y -= fy * m1Inverse;
                unit.Velocity.X += fx * m2Inverse;
                unit.Velocity.Y += fy * m2Inverse;

                // If the norm of the impact vector is less than 120, we normalize it to 120
                var impulse = Math.Sqrt(fx * fx + fy * fy);
                if (impulse < 100.0)
                {
                    var df = 100.0 / impulse;
                    fx = fx * df;
                    fy = fy * df;
                }

                // We apply the impact vector a second time
                Velocity.X -= fx * m1Inverse;
                Velocity.Y -= fy * m1Inverse;
                unit.Velocity.X += fx * m2Inverse;
                unit.Velocity.Y += fy * m2Inverse;

                // This is one of the rare places where a Vector class would have made the code more
                // readable. But this place is called so often that I can't pay a performance price to
            }

            public void Save()
            {
                cache[0] = X;
                cache[1] = Y;
                cache[2] = Velocity;
            }

            public void Load()
            {
                X = (double)cache[0];
                Y = (double)cache[1];
                Velocity = (Point)cache[2];
            }
        }

        public class Snaffle : Entity
        {
            public bool BeingCarried { get; set; }
            public Snaffle(int id, double x, double y, double vx, double vy, bool state) : base(id, x, y, vx, vy)
            {
                BeingCarried = state;
                Mass = 0.5;
                Friction = 0.75;
                Radius = 150;
            }

        }

        public class Wizard : Entity
        {
            public bool Carrying { get; set; }
            public Solution Solution { get; set; }

            public Wizard(int id, double x, double y, double vx, double vy, bool carrying) : base(id, x, y, vx, vy)
            {
                Carrying = carrying;
                Radius = 400;
                Mass = 1.0;
                Friction = 0.75;
            }

            //internal void Rotate(double a)
            //{
            //    // Can't turn by more than 18� in one turn
            //    a = Math.Max(-18, Math.Min(18, a));

            //    Angle += a;

            //    if (Angle >= 360)
            //        Angle = Angle - 360;
            //    else if (Angle < 0.0)
            //        Angle += 360;
            //}

            public void Solve(int timeLimit, bool hasSeed = false)
            {
                Solution best;
                if (hasSeed)
                {
                    best = Solution;
                }
                else
                {
                    best = new Solution();
                }
                GetScore(best);

                while (_stopwatch.ElapsedMilliseconds < timeLimit)
                {
                    var child = best.Clone();
                    child.Mutate();
                    if (GetScore(child) > GetScore(best))
                    {
                        best = child;
                    }
                }
                Solution = best;
            }

            public decimal GetScore(Solution sol)
            {
                if (sol.Score == -1)
                {
                    for (int i = 0; i < Chromosones; i++)
                    {
                        Thrust(sol.Moves[i].Power, sol.Moves[i].Destination);
                        Move(1.0);
                        //CheckCollision();
                        if (Carrying)
                        {
                            //Throw
                        }
                        End();
                    }


                }

                return sol.Score;
            }

        }

        public class Chromosone
        {
            public Point Destination { get; set; }
            public int Power { get; set; }
            public bool Carrying { get; set; }
        }

        public class Solution
        {
            public Chromosone[] Moves { get; set; }
            public int Score { get; set; }
            public Solution(bool init = true)
            {
                Moves = new Chromosone[Chromosones * 2];

                if (init)
                {
                    for (int i = 0; i < Chromosones * 2; i++)
                        Mutate(i, true);
                }
                Score = -1;
            }

            public void Mutate()
            {
                Mutate(rand.Next(0, Chromosones * 2));
                Score = -1;
            }

            public void Mutate(int index, bool all = false)
            {
                int r = rand.Next(1);
                if (r == 0 || all)
                {
                    int x = rand.Next(0, Width);
                    int y = rand.Next(0, Height);
                    Moves[index].Destination = new Point(x, y);
                }
                if (r == 1 || all)
                {
                    int power = rand.Next(0, MaxPower);
                    Moves[index].Power = power;
                }

                Score = -1;
            }

            public void Shift()
            {
                for (int i = 0; i < Chromosones * 2; i++)
                {
                    Moves[i] = Moves[i + 1];
                }

                Mutate(Chromosones, true);
                Mutate(Chromosones * 2, true);
            }
            public Solution Clone()
            {
                var s = new Solution();
                Moves.CopyTo(s.Moves, 0);
                return s;
            }


        }

        static void Main(string[] args)
        {
            string[] inputs;
            _myTeamId = int.Parse(Console.ReadLine()); // if 0 you need to score on the right of the map, if 1 you need to score on the left
            int round = -1;
            // game loop
            while (true)
            {
                round++;
                _stopwatch = Stopwatch.StartNew();

                inputs = Console.ReadLine().Split(' ');
                int myScore = int.Parse(inputs[0]);
                int myMagic = int.Parse(inputs[1]);
                inputs = Console.ReadLine().Split(' ');
                int opponentScore = int.Parse(inputs[0]);
                int opponentMagic = int.Parse(inputs[1]);
                int entities = int.Parse(Console.ReadLine()); // number of entities still in game
                for (int i = 0; i < entities; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int entityId = int.Parse(inputs[0]); // entity identifier
                    EntityType entityType = (EntityType)Enum.Parse(typeof(EntityType), inputs[1], true); // "WIZARD", "OPPONENT_WIZARD" or "SNAFFLE" or "BLUDGER" 
                    int x = int.Parse(inputs[2]); // position
                    int y = int.Parse(inputs[3]); // position
                    int vx = int.Parse(inputs[4]); // velocity
                    int vy = int.Parse(inputs[5]); // velocity
                    int state = int.Parse(inputs[6]); // 1 if the wizard is holding a Snaffle, 0 otherwise

                    switch (entityType)
                    {
                        case EntityType.Wizard:
                            var wiz = _myTeam.FirstOrDefault(w => w.Id == entityId);
                            if (wiz == null)
                                _myTeam.Add(new Wizard(entityId, x, y, vx, vy, Convert.ToBoolean(state)));
                            else
                            {
                                wiz.X = x;
                                wiz.Y = y;
                                wiz.Velocity.X = vx;
                                wiz.Velocity.Y = vy;
                                wiz.Carrying = Convert.ToBoolean(state);
                            }
                            wiz.Save();
                            break;
                        case EntityType.Opponent_Wizard:
                            var oppWiz = _oppTeam.FirstOrDefault(w => w.Id == entityId);
                            if (oppWiz == null)
                                _oppTeam.Add(new Wizard(entityId, x, y, vx, vy, Convert.ToBoolean(state)));
                            else
                            {
                                oppWiz.X = x;
                                oppWiz.Y = y;
                                oppWiz.Velocity.X = vx;
                                oppWiz.Velocity.Y = vy;
                                oppWiz.Carrying = Convert.ToBoolean(state);
                            }
                            oppWiz.Save();
                            break;
                        case EntityType.Snaffle:
                            var snaffle = _snaffles.FirstOrDefault(w => w.Id == entityId);
                            if (snaffle == null)
                                _snaffles.Add(new Snaffle(entityId, x, y, vx, vy, Convert.ToBoolean(state)));
                            else
                            {
                                snaffle.X = x;
                                snaffle.Y = y;
                                snaffle.BeingCarried = Convert.ToBoolean(state);
                            }
                            snaffle.Save();
                            break;
                    }
                }

                for (int i = 0; i < 2; i++)
                {

                    // Write an action using Console.WriteLine()
                    // To debug: Console.Error.WriteLine("Debug messages...");


                    // Edit this line to indicate the action for each wizard (0 ≤ thrust ≤ 150, 0 ≤ power ≤ 500)
                    // i.e.: "MOVE x y thrust" or "THROW x y power"
                    if (_myTeam[i].Carrying)
                        Console.WriteLine(string.Format("THROW {0} {1} {2}", _myTeamId == 0 ? Width : 0, GoalY, MaxPower));
                    else
                    {
                        var snaff = _snaffles.OrderBy(s => s.Distance2(_myTeam[i])).FirstOrDefault(s => !s.BeingCarried);
                        snaff.BeingCarried = true;
                        Console.WriteLine(string.Format("MOVE {0} {1} {2}", snaff.X, snaff.Y, MaxThrust));

                    }
                }
            }
        }
    }
}