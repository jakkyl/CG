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
    internal class Player
    {
        private const int Population = 1;
        private const int Chromosones = 6;

        private const int Width = 16001;
        private const int Height = 7501;
        private const int GoalY = 3750;
        private const int GoalTop = 1900;
        private const int GoalBottom = 5600;
        private const int MaxRounds = 200;

        private const int PoleRadius = 300;
        private const int SnaffleRadius = 150;
        private const int MaxThrust = 150;
        private const int MaxPower = 500;

        private static int round = -1;
        private static int _myTeamId = 0;
        private static int _myScore = 0;
        private static int _oppScore = 0;

        private static List<Wizard> _wizards = new List<Wizard>();
        private static List<Snaffle> _snaffles = new List<Snaffle>();
        private static List<Bludger> _bludgers = new List<Bludger>();

        private static Random rand = new Random();
        private static Stopwatch _stopwatch;
        private static int _simulationTurn = 0;
        private static int _simulations = 0;

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

            public double Distance2(double x, double y)
            {
                return Math.Pow(X - x, 2) + Math.Pow(Y - y, 2);
            }

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

            public override bool Equals(object obj)
            {
                if (obj is Point)
                    return X == (obj as Point).X && Y == (obj as Point).Y;
                else
                    return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash *= 23 + X.GetHashCode();
                    hash *= 23 + Y.GetHashCode();
                    return hash;
                }
            }

            public Point Normalized()
            {
                var mag = Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2));
                return new Point(X / mag, Y / mag);
            }
        }

        #region Spells

        public class Spell
        {
            public static int Cost { get; set; }
            public int Duration { get; set; }
            public string Name { get; set; }
            public Wizard Source { get; set; }
            public Entity Target { get; set; }
            public double Power { get; set; }

            public virtual void GetPower()
            {
            }

            public void Cast(Entity target)
            {
                Console.WriteLine(string.Format("{0} {1}", Name, target.Id));
            }
        }

        public class Obliviate : Spell
        {
            public static new int Cost = 5;

            public Obliviate()
            {
                Duration = 4;
                Name = "OBLIVIATE";
            }
        }

        public class Petrificus : Spell
        {
            public static new int Cost = 10;

            public Petrificus()
            {
                Duration = 1;
                Name = "PETRIFICUS";
            }
        }

        public class Accio : Spell
        {
            public static new int Cost = 15;

            public Accio()
            {
                Duration = 6;
                Name = "ACCIO";
            }

            public override void GetPower()
            {
                Power = Math.Min(3000 / Math.Pow(Source.Distance(Target), 2), 1000);
            }
        }

        public class Flipendo : Spell
        {
            public static new int Cost = 20;

            public Flipendo()
            {
                Duration = 3;
                Name = "FLIPENDO";
            }

            public override void GetPower()
            {
                Power = Math.Min(6000 / Math.Pow(Source.Distance(Target), 2), 1000);
            }
        }

        #endregion Spells

        public class Entity : Point
        {
            public int Id { get; private set; }
            public int Radius { get; set; }
            public double Mass { get; set; }
            public double Friction { get; set; }
            public Point Velocity { get; set; }
            private object[] cache = new object[7];

            public Entity(int id, double x, double y, double vx, double vy)
                : base(x, y)
            {
                Id = id;
                Velocity = new Point(vx, vy);
            }

            public void Thrust(int power, Point dest)
            {
                var vec = Subtract(dest);
                var normal = vec.Normalized();
                var invM = 1 / Mass;
                //if (this is Wizard)
                //    Console.Error.WriteLine("Thrust {0} {1}", thrust, Id);
                Velocity.X += vec.X * (power * invM);
                Velocity.Y += vec.Y * (power * invM);
            }

            public void Move(double t)
            {
                //var oldP = new Point(this.X, this.Y);
                X += Velocity.X * t;
                Y += Velocity.Y * t;
                //if (this is Wizard)
                //    Console.Error.WriteLine("MOVING {3} FROM {0} TO {1} AT {2}", oldP, this as Point, Velocity, Id);
            }

            public virtual void End()
            {
                X = Math.Round(X);
                Y = Math.Round(Y);
                Velocity.X = Math.Truncate(Velocity.X * Friction);
                Velocity.Y = Math.Truncate(Velocity.Y * Friction);
            }

            public void Bounce(Entity unit)
            {
                if (unit is Snaffle && this is Wizard)
                {
                    var wiz = (this as Wizard);
                    if (wiz.Snaffle != unit && (wiz.Carrying || wiz.SnaffleCooldown > 0)) return;

                    wiz.Carrying = true;
                    wiz.Snaffle = unit as Snaffle;
                    unit.Velocity = wiz.Velocity;
                    unit.X = wiz.X;
                    unit.Y = wiz.Y;
                    wiz.SnaffleCooldown = 3;
                    //Console.Error.WriteLine("GOT A SNAFFLE! {0}", this.Id);
                    return;
                }
                if (unit is Wall)
                {
                    if (X < 0 || X > Width)
                    {
                        if (this is Snaffle && Y > GoalTop && Y < GoalBottom)
                        {
                            (this as Snaffle).InPlay = false;
                            if ((X < 0 && _myTeamId == 1) || (X > Width && _myTeamId == 0)) _myScore++;
                            else _oppScore++;
                            return;
                        }
                        Velocity.X *= -1;
                    }
                    if (Y < 0 || Y > Height) Velocity.Y *= -1;

                    return;
                }
                var mcoeff = (Mass + unit.Mass) / (Mass * unit.Mass);

                var nx = X - unit.X;
                var ny = Y - unit.Y;

                // Square of the distance between the 2 pods. This value could be hardcoded because
                // it is always 800²
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

            public virtual void Save()
            {
                cache[0] = X;
                cache[1] = Y;
                cache[2] = Velocity;
                cache[3] = _myScore;
                cache[4] = _oppScore;
            }

            public virtual void Load()
            {
                X = (double)cache[0];
                Y = (double)cache[1];
                Velocity = (Point)cache[2];
                _myScore = (int)cache[3];
                _oppScore = (int)cache[4];
            }

            public Collision CheckCollisions(Entity unit)
            {
                if (X < 0 || X > Width || Y < 0 || Y > Height)
                {
                    //Console.Error.WriteLine("HIT A WALL {0}", Id);
                    return new Collision(this, new Wall(), 1.0);
                }

                //same speed will never collide
                if (Velocity == unit.Velocity) return null;

                var distance = Distance2(unit);
                var sumOfRadii = Math.Pow(Radius + (unit is Snaffle ? -1 : unit.Radius), 2);

                //already touching
                //if (distance < sumOfRadii) return new Collision(this, unit, 0.0);

                // We place ourselves in the reference frame of u. u is therefore stationary and is
                // at (0,0)
                var dx = X - unit.X;
                var dy = Y - unit.Y;
                Point myp = new Point(dx, dy);
                var vx = Velocity.X - unit.Velocity.X;
                var vy = Velocity.Y - unit.Velocity.Y;
                var a = vx * vx + vy * vy;
                if (a < 0.00001) return null;

                var b = -2.0 * (dx * vx + dy * vy);

                var delta = b * b - 4.0 * a * (dx * dx + dy * dy - sumOfRadii);

                if (delta < 0.0) return null;

                var t = Math.Round((b - Math.Sqrt(delta)) * (1.0 / (2.0 * a)), 4);
                //Console.Error.WriteLine("Bounce {0} {1} - {2}", this, unit, t);
                if (t <= 0.0 || t >= 1.0) return null;

                //Console.Error.WriteLine("HIT SOMETING! Me: {1} IT:{0}", unit.Id, Id);
                return new Collision(this, unit, t);
            }
        }

        public class Collision
        {
            public Entity EntityA { get; set; }
            public Entity EntityB { get; set; }
            public double Time { get; set; }

            public Collision(Entity a, Entity b, double t)
            {
                EntityA = a;
                EntityB = b;
                Time = t;
            }
        }

        public class Wall : Entity
        {
            public Wall()
                : base(-1, 0, 0, 0, 0)
            {
            }
        }

        public class Bludger : Entity
        {
            public Wizard Target { get; set; }
            public Wizard LastTarget { get; set; }

            public Bludger(int id, double x, double y, double vx, double vy)
                : base(id, x, y, vx, vy)
            {
                Mass = 0.5;
                Friction = 0.75;
                Radius = 200;
            }
        }

        public class Snaffle : Entity
        {
            public bool BeingCarried { get; set; }
            public bool InPlay { get; set; }

            public Snaffle(int id, double x, double y, double vx, double vy, bool state)
                : base(id, x, y, vx, vy)
            {
                BeingCarried = state;
                Mass = 0.5;
                Friction = 0.75;
                Radius = 150;
                InPlay = true;
            }
        }

        public class Wizard : Entity
        {
            public Solution Solution { get; set; }
            public int TeamId { get; internal set; }

            public Spell ActiveSpell { get; set; }
            public int MP { get; internal set; }

            public Snaffle Snaffle { get; set; }
            public int SnaffleCooldown { get; set; }
            public bool Carrying { get; set; }

            public Wizard(int id, double x, double y, double vx, double vy, bool carrying)
                : base(id, x, y, vx, vy)
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

            // Angle += a;

            //    if (Angle >= 360)
            //        Angle = Angle - 360;
            //    else if (Angle < 0.0)
            //        Angle += 360;
            //}
            private object[] cache = new object[7];

            public override void Save()
            {
                base.Save();
                cache[0] = Carrying;
                cache[1] = Snaffle;
            }

            public override void Load()
            {
                base.Load();
                Carrying = (bool)cache[0];
                Snaffle = (Snaffle)cache[1];
            }

            public override void End()
            {
                base.End();
                MP++;
            }
        }

        #region Bots

        public class Bot
        {
            public int Id { get; set; }
            public Wizard Wizard { get; set; }

            public Bot()
                : this(0)
            {
            }

            public Bot(int id)
            {
                Id = id;
            }

            internal virtual void Move()
            {
            }
        }

        public class ReflexBot : Bot
        {
            public ReflexBot()
                : base()
            {
            }

            public ReflexBot(int id)
                : base(id)
            {
            }

            internal override void Move()
            {
                if (Wizard.Carrying)
                {
                    var x = _myTeamId == 0 ? Width : 0 - Wizard.Velocity.X;
                    var y = GoalY - Wizard.Velocity.Y;
                    Wizard.Snaffle.Thrust(MaxPower, new Point(x, y));
                    Wizard.Carrying = false;
                }
                else
                {
                    var snaff = _snaffles[0];//.OrderBy(s => s.Distance2(Wizard)).FirstOrDefault(s => !s.BeingCarried);
                    if (snaff == null) snaff = _snaffles.FirstOrDefault();
                    Wizard.Thrust(MaxThrust, snaff);
                }
            }
        }

        public class SearchBot : Bot
        {
            public Solution Solution { get; set; }
            public List<Bot> Opponents { get; set; }

            public SearchBot()
                : base()
            {
            }

            public SearchBot(int id)
                : base(id)
            {
            }

            internal override void Move()
            {
                Move(Solution);
            }

            private void Move(Player.Solution Solution)
            {
                if (Wizard.Carrying)
                {
                    Wizard.Snaffle.Thrust(MaxPower, Solution.Moves[_simulationTurn].Destination);
                    Wizard.Carrying = false;
                }
                else
                {
                    Wizard.Thrust(MaxPower, Solution.Moves[_simulationTurn].Destination);
                }
            }

            public void Solve(int timeLimit, bool hasSeed = false)
            {
                Solution best;

                if (hasSeed)
                {
                    best = Solution;
                    best.Shift();
                }
                else
                {
                    best = new Solution();
                    Solution = best;
                    Console.Error.WriteLine("New Sol {0}", string.Join(",", (object[])best.Moves));
                }
                GetScore(best);

                while (_stopwatch.ElapsedMilliseconds < timeLimit)
                {
                    var child = best.Clone();
                    child.Mutate();
                        //Console.Error.WriteLine("B: {0} C: {1}",GetScore(best), GetScore(child));
                    if (GetScore(child) > GetScore(best))
                    {
                        best = child;
                        //Console.Error.WriteLine("B: {0} C: {1}", best.Score, child.Score);
                    }
                }
                Solution = best;
            }

            public double GetScore(Solution sol)
            {
                if (sol.Score == -1)
                {
                    for (int i = 0; i < Chromosones; i++)
                    {
                        Move();
                        // for (int j = 0; j < Opponents.Count; j++)
                        //     Opponents[j].Move();
                        MoveEntities();
                        sol.Play();
                        if (round > 0) _simulationTurn++;
                    }

                    sol.Score = Evaluate();

                    _simulationTurn = 0;
                    for (int i = 0; i < 4; i++) _wizards[i].Load();
                    for (int i = 0; i < 2; i++) _bludgers[i].Load();
                    foreach (var s in _snaffles) s.Load();

                    _simulations++;
                }

                return sol.Score;
            }

            private double Evaluate()
            {
                int myGoal = Wizard.TeamId == 1 ? 0 : Width;
                //double myDist = _snaffles.Where(s => s.InPlay).Sum(s=>s.Distance2(myGoal, GoalY));
                double score = 100*(_myScore - _oppScore);// + _snaffles.Count(s => s.X < 8000);
                for (int i = 0; i < _snaffles.Count; i++)
                {
                    if (_snaffles[i].InPlay)
                    {
                        score -= _snaffles[i].Distance2(myGoal, GoalY);
                        score -= _snaffles[i].Distance2(Wizard);
                    }
                }


                return score;
            }

            private void MoveEntities()
            {
                for (int i = 0; i < 2; i++)
                {
                    var closestWiz = _wizards.Where(w => w != _bludgers[i].LastTarget).OrderBy(w => w.Distance(_bludgers[i])).First();
                    _bludgers[i].Thrust(1000, closestWiz);
                    _bludgers[i].Move(1.0);
                }
                foreach (var s in _snaffles)
                {
                    if (!s.BeingCarried) s.Move(1.0);
                }
            }
        }

        #endregion Bots

        public class Chromosone
        {
            public Point Destination { get; set; }
            public int Power { get; set; }
            public bool Carrying { get; set; }
            public string Action { get; set; }

            public override string ToString()
            {
                return string.Format("[{0} {1}]", Destination, Power);
            }
        }

        public class Solution
        {
            public Chromosone[] Moves { get; set; }
            public double Score { get; set; }

            public Solution(bool init = true)
            {
                Moves = new Chromosone[Chromosones * 2];

                if (init)
                {
                    for (int i = 0; i < Chromosones * 2; i++)
                    {
                        Moves[i] = new Chromosone();
                        Mutate(i, true);
                    }
                }
                Score = -1;
            }

            public void Mutate()
            {
                Mutate(rand.Next(0, Chromosones * 2));
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
                // if (r == 1 || all)
                // {
                //     int power = rand.Next(0, MaxPower);
                //     Moves[index].Power = power;
                // }
                Moves[index].Power = MaxPower;
                Score = -1;
            }

            public void Shift()
            {
                for (int i = 1; i < Chromosones; i++)
                {
                    Moves[i - 1] = Moves[i];
                    Moves[i - 1 + Chromosones] = Moves[i + Chromosones];
                }

                Mutate(Chromosones - 1, true);
                Mutate(Chromosones * 2 - 1, true);
            }

            public Solution Clone()
            {
                var s = new Solution();
                Moves.CopyTo(s.Moves, 0);
                return s;
            }

            internal void Play()
            {
                int snaffleCount = _snaffles.Count;

                double t = 0.0;
                while (t < 1.0)
                {
                    Collision firstCol = null;

                    for (int i = 0; i < 4; i++)
                    {
                        //check other pods
                        for (int j = i + 1; j < 4; j++)
                        {
                            var col = _wizards[i].CheckCollisions(_wizards[j]);
                            //if (col != null)
                            //    Console.Error.WriteLine("Bounce {0} {1}", col.T, t);
                            if (col != null && col.Time + t < 1.0 && (firstCol == null || col.Time < firstCol.Time))
                            {
                                firstCol = col;
                            }
                        }
                        // for (int j = 0; j < 2; j++)
                        // {
                        //     var col = _wizards[i].CheckCollisions(_bludgers[j]);
                        //     //if (col != null)
                        //     //    Console.Error.WriteLine("Bounce {0} {1}", col.T, t);
                        //     if (col != null && col.Time + t < 1.0 && (firstCol == null || col.Time < firstCol.Time))
                        //     {
                        //         firstCol = col;
                        //     }
                        // }
                        for (int j = 0; j < snaffleCount; j++)
                        {
                            if (_snaffles[j].X < 0 || _snaffles[j].X > Width)
                            {
                                _snaffles[j].InPlay = false;
                            }
                            if (!_snaffles[j].InPlay) continue;

                            var col = _wizards[i].CheckCollisions(_snaffles[j]);
                            //if (col != null)
                            //    Console.Error.WriteLine("Bounce {0} {1}", col.T, t);
                            if (col != null && col.Time + t < 1.0 && (firstCol == null || col.Time < firstCol.Time))
                            {
                                firstCol = col;
                            }
                        }
                    }

                    if (firstCol == null)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            _wizards[i].Move(1.0 - t);
                        }
                        t = 1.0;
                    }
                    else
                    {
                        //Console.Error.WriteLine("Bounce {0} {1} {2}", firstCol.EntityA.GetType(), firstCol.EntityB.GetType(), firstCol.Time);
                        for (int i = 0; i < 4; i++)
                        {
                            _wizards[i].Move(firstCol.Time);
                        }

                        firstCol.EntityA.Bounce(firstCol.EntityB);

                        //t += firstCol.Time;
                        t=1.0;
                    }
                }

                for (int i = 0; i < 4; i++)
                    _wizards[i].End();
                for (int j = 0; j < 2; j++)
                    _bludgers[j].End();
                for (int j = 0; j < snaffleCount; j++)
                    _snaffles[j].End();
            }

            internal void Output(Chromosone chromosone, Wizard wizard)
            {
                if (wizard.Carrying)
                {
                    chromosone.Action = "THROW";
                    chromosone.Power = Math.Min(chromosone.Power, MaxPower);
                }
                else
                {
                    chromosone.Action = "MOVE";
                    chromosone.Power = Math.Min(chromosone.Power, MaxThrust);
                }

                Console.WriteLine(string.Format("{0} {1} {2} {3}", chromosone.Action, chromosone.Destination.X, chromosone.Destination.Y, chromosone.Power));
            }
        }

        private static void Main(string[] args)
        {
            string[] inputs;
            _myTeamId = int.Parse(Console.ReadLine()); // if 0 you need to score on the right of the map, if 1 you need to score on the left


            var oppReflex = new ReflexBot(_myTeamId == 0 ? 3 : 1);
            var meReflex = new ReflexBot(_myTeamId == 0 ? 1 : 3);

            var oppSearch = new ReflexBot(oppReflex.Id - 1);
            var meSearch = new SearchBot(meReflex.Id - 1);
            meSearch.Opponents = new List<Bot>() { oppReflex, meReflex, oppSearch };

            // game loop
            while (true)
            {
                round++;
                _snaffles.Clear();

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
                        case EntityType.Opponent_Wizard:
                            var wiz = _wizards.FirstOrDefault(w => w.Id == entityId);
                            if (wiz == null)
                            {
                                wiz = new Wizard(entityId, x, y, vx, vy, Convert.ToBoolean(state));
                                wiz.TeamId = entityType == EntityType.Wizard ? _myTeamId : Math.Abs(_myTeamId - 1);
                                _wizards.Add(wiz);
                                if (entityType == EntityType.Wizard)
                                {
                                    if (meSearch.Wizard == null)
                                        meSearch.Wizard = wiz;
                                    else
                                        meReflex.Wizard = wiz;
                                }
                                else
                                {
                                    if (oppSearch.Wizard == null)
                                        oppSearch.Wizard = wiz;
                                    else
                                        oppReflex.Wizard = wiz;
                                }
                            }
                            else
                            {
                                wiz.X = x;
                                wiz.Y = y;
                                wiz.Velocity.X = vx;
                                wiz.Velocity.Y = vy;
                                wiz.Carrying = Convert.ToBoolean(state);
                            }
                            wiz.MP = myMagic;
                            if (wiz.ActiveSpell != null) wiz.ActiveSpell.Duration--;
                            wiz.Save();
                            break;

                        case EntityType.Snaffle:

                            var snaffle = new Snaffle(entityId, x, y, vx, vy, Convert.ToBoolean(state));
                            _snaffles.Add(snaffle);

                            snaffle.Save();
                            break;

                        case EntityType.Bludger:

                            var bludger = _bludgers.FirstOrDefault(w => w.Id == entityId);

                            if (bludger == null)
                            {
                                bludger = new Bludger(entityId, x, y, vx, vy);
                                _bludgers.Add(bludger);
                            }
                            else
                            {
                                bludger.X = x;
                                bludger.Y = y;
                            }
                            bludger.LastTarget = _wizards.FirstOrDefault(w => w.Id == state);
                            bludger.Save();
                            break;
                    }
                }

                for (int i = 0; i < 4; i++)
                {
                    _wizards[i].Snaffle = _snaffles.FirstOrDefault(s => s.Distance(_wizards[i]) < _wizards[i].Radius);
                    _wizards[i].Save();
                }
                _stopwatch = Stopwatch.StartNew();
                int time = round == 0 ? 500 : 30;
                meSearch.Solve(time, round > 0);
                meSearch.Solution.Output(meSearch.Solution.Moves[0], _wizards.First(w => w.Id == meSearch.Id));
                meSearch.Solution.Output(meSearch.Solution.Moves[Chromosones], _wizards.First(w => w.Id == meSearch.Id + 1));
                if (round > 0)
                    Console.Error.WriteLine(string.Format("Sims Per Round: {0}, Avg: {1}", _simulations / round, _simulations * Chromosones / round));
                continue;

                foreach (var wizard in _wizards.Where(w => w.TeamId == _myTeamId))
                {
                    // Write an action using Console.WriteLine() To debug:
                    // Console.Error.WriteLine("Debug messages...");

                    // Edit this line to indicate the action for each wizard (0 ≤ thrust ≤ 150, 0 ≤
                    // power ≤ 500) i.e.: "MOVE x y thrust" or "THROW x y power"
                    if (wizard.Carrying)
                    {
                        var x = _myTeamId == 0 ? Width : 0 - wizard.Velocity.X;
                        var y = GoalY - wizard.Velocity.Y;
                        Console.WriteLine(string.Format("THROW {0} {1} {2}", x, y, MaxPower));
                    }
                    else
                    {
                        //check bludger
                        if (myMagic >= Flipendo.Cost)
                        {
                            int myGoal = _myTeamId == 0 ? 0 : Width;
                            bool cast = false;
                            foreach (var s in _snaffles.OrderByDescending(s => s.Distance(wizard)))
                            {
                                var spell = new Flipendo()
                                {
                                    Source = wizard,
                                    Target = s
                                };
                                Console.Error.WriteLine("Old {0}", wizard);
                                spell.GetPower();
                                s.Thrust((int)spell.Power, new Point(myGoal, GoalY));
                                s.Move(1.0);
                                Console.Error.WriteLine("New {0}", wizard);
                                if (s.X > Width || s.X < 0)
                                {
                                    wizard.ActiveSpell.Cast(s);
                                    myMagic -= Flipendo.Cost;
                                    cast = true;
                                    break;
                                }
                                s.Load();
                                //if ((_myTeamId == 0 && s.X < 3000 && wizard.X > s.X) || (_myTeamId == 1 && s.X > Width - 3000 && wizard.X < s.X))
                                //{
                                //    wizard.ActiveSpell = new Flipendo()
                                //    {
                                //        Target = s
                                //    };
                                //    wizard.ActiveSpell.Cast(s);
                                //    cast = true;
                                //    myMagic -= Flipendo.Cost;
                                //    break;
                                //}
                            }
                            if (cast) continue;
                        }
                        /*
                        if (myMagic > Obliviate.Cost)
                        {
                            var minBludgerDistance = 10000.0;
                            Bludger minBludger = null;
                            for (int i = 0; i < 2; i++)
                            {
                                var d = _bludgers[i].Distance(wizard);
                                if (d < minBludgerDistance)
                                {
                                    minBludgerDistance = d;
                                    minBludger = _bludgers[i];
                                }
                            }
                            if (minBludgerDistance < 1000 && minBludger.LastTarget != wizard)
                            {
                                wizard.ActiveSpell = new Obliviate();
                                wizard.ActiveSpell.Cast(minBludger);
                                Console.Error.WriteLine("Oblivia D: {0}", minBludgerDistance);
                                myMagic -= Obliviate.Cost;
                                continue;
                            }
                        }*/
                        if (myMagic >= Accio.Cost)
                        {
                            bool cast = false;
                            foreach (var s in _snaffles)
                            {
                                if ((_myTeamId == 0 && s.X < 2000) || (_myTeamId == 1 && s.X > Width - 2000))
                                {
                                    wizard.ActiveSpell = new Accio();
                                    wizard.ActiveSpell.Cast(s);
                                    cast = true;
                                    myMagic -= Accio.Cost;
                                    break;
                                }
                            }
                            if (cast) continue;
                        }/*
                        if (myMagic >= Petrificus.Cost)
                        {
                            bool cast = false;
                            foreach (var enemy in _wizards.Where(w => w.TeamId != _myTeamId && !_wizards.Any(w1 => w1.ActiveSpell?.Target == w)))
                            {
                                if ((_myTeamId == 0 && enemy.X < 3000) || (_myTeamId == 1 && enemy.X > Width - 3000))
                                {
                                    wizard.ActiveSpell = new Petrificus();
                                    wizard.ActiveSpell.Target = enemy;
                                    wizard.ActiveSpell.Cast(enemy);
                                    myMagic -= Petrificus.Cost;
                                    cast = true;
                                    break;
                                }
                            }
                            if (cast) continue;
                        }
                        */

                        var snaff = _snaffles.OrderBy(s => s.Distance2(wizard)).FirstOrDefault(s => !s.BeingCarried);
                        if (snaff == null) snaff = _snaffles.FirstOrDefault();
                        snaff.BeingCarried = true;
                        Console.WriteLine(string.Format("MOVE {0} {1} {2}", snaff.X, snaff.Y, MaxThrust));
                    }
                }
            }
        }
    }
}