using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/

namespace CodersOfTheCari
{
    internal class Player
    {
        private const int Lifetime = 4;
        private const int PopulationSize = 4;

        private const int Width = 23;
        private const int Height = 21;
        private const int ShipLength = 3;
        private const int ShipWidth = 1;
        private const int MaxRum = 100;
        private const int MineCooldown = 4;
        private const int MineDamage = 25;
        private const int MineSplash = 10;
        private const int MineSightDistance = 5;
        private const int CannonRange = 10;
        private const int CannonDirectDamage = 50;
        private const int CannonIndirectDamage = 25;
        private const int CannonCooldown = 1;
        private const int MaxSpeed = 2;

        private static List<Referee.Entity> _entities = new List<Referee.Entity>();
        private static int _round = -1;

        private static Random _rand = new Random();

        public enum Action
        {
            FASTER, SLOWER, PORT, STARBOARD, FIRE, MINE
        }

        public class Point
        {
            internal int[,] DIRECTIONS_EVEN = new int[,] { { 1, 0 }, { 0, -1 }, { -1, -1 }, { -1, 0 }, { -1, 1 }, { 0, 1 } };
            internal int[,] DIRECTIONS_ODD = new int[,] { { 1, 0 }, { 1, -1 }, { 0, -1 }, { -1, 0 }, { 0, 1 }, { 1, 1 } };

            public Point(double x, double y)
            {
                X = x;
                Y = y;
            }

            public double X { get; set; }

            public double Y { get; set; }

            public double Distance(Point dst)
            {
                return this.toCubeCoordinate().distanceTo(dst.toCubeCoordinate());
            }

            private CubeCoordinate toCubeCoordinate()
            {
                var xp = X - (Y - ((int)Y & 1)) / 2;
                var zp = Y;
                var yp = -(xp + zp);
                return new CubeCoordinate(xp, yp, zp);
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

            public bool IsInsideMap()
            {
                return X >= 0 && X < Width && Y >= 0 && Y < Height;
            }

            public Point Neighbor(int Direction)
            {
                double newY, newX;
                if (this.Y % 2 == 1)
                {
                    newY = this.Y + DIRECTIONS_ODD[Direction, 1];
                    newX = this.X + DIRECTIONS_ODD[Direction, 0];
                }
                else
                {
                    newY = this.Y + DIRECTIONS_EVEN[Direction, 1];
                    newX = this.X + DIRECTIONS_EVEN[Direction, 0];
                }

                return new Point(newX, newY);
            }

            public List<Referee.Entity> Neighbors()
            {
                var ns = new List<Referee.Entity>();
                for (int i = 0; i < 6; i++)
                {
                    var n = Neighbor(i);
                    if (n.IsInsideMap())
                    {
                        var ent = _entities.FirstOrDefault(e => e.Equals(n));

                        if (ent != null)
                        {
                            ns.Add(ent);
                        }
                        //else ns.Add(new Referee.Entity(-1, n.X, n.Y));
                    }
                }

                return ns;
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

            public override string ToString()
            {
                return string.Format("[{0},{1}]", X, Y);
            }

            internal double Angle(Point targetPosition)
            {
                double dy = (targetPosition.Y - this.Y) * Math.Sqrt(3) / 2;
                double dx = targetPosition.X - this.X + ((int)(this.Y - targetPosition.Y) & 1) * 0.5;
                double angle = -Math.Atan2(dy, dx) * 3 / Math.PI;
                if (angle < 0)
                {
                    angle += 6;
                }
                else if (angle >= 6)
                {
                    angle -= 6;
                }
                return angle;
            }
        }

        public class CubeCoordinate
        {
            private static int[,] directions = new int[,] { { 1, -1, 0 }, { +1, 0, -1 }, { 0, +1, -1 }, { -1, +1, 0 }, { -1, 0, +1 }, { 0, -1, +1 } };
            private double x, y, z;

            public CubeCoordinate(double x, double y, double z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            private Point toOffsetPointinate()
            {
                var newX = x + (z - ((int)z & 1)) / 2;
                var newY = z;
                return new Point(newX, newY);
            }

            private CubeCoordinate neighbor(int orientation)
            {
                var nx = this.x + directions[orientation, 0];
                var ny = this.y + directions[orientation, 1];
                var nz = this.z + directions[orientation, 2];

                return new CubeCoordinate(nx, ny, nz);
            }

            public double distanceTo(CubeCoordinate dst)
            {
                return (Math.Abs(x - dst.x) + Math.Abs(y - dst.y) + Math.Abs(z - dst.z)) / 2;
            }
        }

        public class Damage : Point
        {
            private int health;
            private bool hit;

            public Damage(Point position, int health, bool hit)
                : base(position.X, position.Y)
            {
                this.health = health;
                this.hit = hit;
            }
        }

        public class Entity : Point
        {
            private object[] cache;
            public int Id { get; set; }
            public int Orientation { get; set; }
            public bool IsAlive { get; set; }

            public Entity(int id, double x, double y)
                : base(x, y)
            {
                Id = id;
                cache = new object[2];
            }

            public virtual void Save()
            {
                cache[0] = Orientation;
                cache[1] = IsAlive;
            }

            public virtual void Load()
            {
                Orientation = (int)cache[0];
                IsAlive = (bool)cache[1];
            }

            public override string ToString()
            {
                return string.Format("{0} {1},{2}", Id, X, Y);
            }
        }

        public class Barrel : Entity
        {
            private object[] cache;

            public int Rum { get; set; }

            public Barrel(int id, double x, double y, int rum)
                : base(id, x, y)
            {
                Rum = rum;
                cache = new object[1];
            }

            public override void Save()
            {
                cache[0] = Rum;
            }

            public override void Load()
            {
                Rum = (int)cache[0];
            }
        }

        public class Cannonball : Entity
        {
            private object[] cache;

            public Point Target { get; set; }
            public int SourceId { get; set; }
            public Point SourcePosition { get; set; }
            public int TurnsToImpact { get; set; }

            public Cannonball(int id, double x, double y, int turns)
                : base(id, x, y)
            {
                TurnsToImpact = turns;
                cache = new object[2];
            }

            public Cannonball(int id, Point target, Ship source, int remainingTurns)
                : this(id, source.X, source.Y, remainingTurns)
            {
                Target = target;
                SourceId = source.Id;
                SourcePosition = source.Bow();
            }

            public override void Save()
            {
                cache[0] = Target;
                cache[1] = TurnsToImpact;
            }

            public override void Load()
            {
                Target = (Point)cache[0];
                TurnsToImpact = (int)cache[1];
            }
        }

        public class Mine : Entity
        {
            public Mine(int id, double x, double y)
                : base(id, x, y)
            {
            }

            internal List<Damage> Explode(List<Ship> ships, bool force)
            {
                List<Damage> damage = new List<Damage>();
                Ship victim = null;

                foreach (Ship ship in ships)
                {
                    if (this.Equals(ship.Bow()) || this.Equals(ship.Stern()) || this.Equals(ship))
                    {
                        damage.Add(new Damage(this, MineDamage, true));
                        ship.Damage(MineDamage);
                        victim = ship;
                    }
                }

                if (force || victim != null)
                {
                    if (victim == null)
                    {
                        damage.Add(new Damage(this, MineDamage, true));
                    }

                    foreach (Ship ship in ships)
                    {
                        if (ship != victim)
                        {
                            Point impactPosition = null;
                            if (ship.Stern().Distance(this) <= 1)
                            {
                                impactPosition = ship.Stern();
                            }
                            if (ship.Bow().Distance(this) <= 1)
                            {
                                impactPosition = ship.Bow();
                            }
                            if (ship.Distance(this) <= 1)
                            {
                                impactPosition = ship as Point;
                            }

                            if (impactPosition != null)
                            {
                                ship.Damage(MineSplash);
                                damage.Add(new Damage(impactPosition, MineSplash, true));
                            }
                        }
                    }
                }

                return damage;
            }
        }

        public class Ship : Entity
        {
            private int[] cache = new int[4];

            public int Speed { get; set; }
            public int RumCarried { get; set; }
            public int Team { get; set; }
            public int MineCooldown { get; set; }
            public int CannonCooldown { get; set; }
            public Point Target { get; set; }

            public Ship(int id, double x, double y, int rotation, int speed, int rum, int team)
                : base(id, x, y)
            {
                Orientation = rotation;
                Speed = speed;
                RumCarried = rum;
                Team = team;
                MineCooldown = 0;
                IsAlive = true;
            }

            public Ship(int id)
                : base(id, 0, 0)
            {
                IsAlive = true;
            }

            public override void Save()
            {
                cache[0] = Speed;
                cache[1] = RumCarried;
                cache[2] = MineCooldown;
                cache[3] = CannonCooldown;
            }

            public override void Load()
            {
                Speed = cache[0];
                RumCarried = cache[1];
                MineCooldown = cache[2];
                CannonCooldown = cache[3];
            }

            public Point PredictedLocation(Ship target)
            {
                // predicted point
                if (target.Speed == 0) return target as Point;

                var eta = 1 + Math.Round(Distance(target) / 3);

                if (target.Y % 2 == 0)
                {
                    target.X += DIRECTIONS_EVEN[target.Orientation, 0] * target.Speed * eta;
                    target.Y += DIRECTIONS_EVEN[target.Orientation, 1] * target.Speed * eta;
                }
                else
                {
                    target.X += DIRECTIONS_ODD[target.Orientation, 0] * target.Speed * eta;
                    target.Y += DIRECTIONS_ODD[target.Orientation, 1] * target.Speed * eta;
                }

                target.X = Math.Max(0, Math.Min(target.X, Width - 1));
                target.Y = Math.Max(0, Math.Min(target.Y, Height - 1));

                return target as Point;
            }

            public Point GetNextPosition()
            {
                Point nextPos = this as Point;
                for (int i = 1; i < 1; i++)
                {
                    var temp = nextPos.Neighbor(Orientation);
                    if (!temp.IsInsideMap()) break;
                    nextPos = temp;
                }

                return nextPos;
            }

            public Point GetNextBow()
            {
                var next = GetNextPosition();
                var p = new Point(next.X, next.Y);
                var s = new Ship(-1)
                    {
                        X = p.X,
                        Y = p.Y,
                        Orientation = Orientation
                    };
                return s.Bow();
            }

            public Point GetNextStern()
            {
                var next = GetNextPosition();
                var p = new Point(next.X, next.Y);
                var s = new Ship(-1)
                {
                    X = p.X,
                    Y = p.Y,
                    Orientation = Orientation
                };
                return s.Stern();
            }

            public Point Bow()
            {
                return Neighbor(Orientation);
            }

            public Point Stern()
            {
                return Neighbor(((int)Orientation + 3) % 6);
            }

            internal void Update(int x, int y, int direction, int speed, int rum)
            {
                X = x;
                Y = y;
                Orientation = direction;
                Speed = speed;
                RumCarried = rum;
                if (CannonCooldown > 0) CannonCooldown--;
                if (MineCooldown > 0) MineCooldown--;
            }

            /*
            internal bool CheckCollisions()
            {
                var bow = GetNextBow();
                var stern = GetNextStern();

                Console.Error.WriteLine("BOW: {0}, STERN: {1}, C: {2}", bow, stern, this);
                foreach (var entity in _entities.Where(s => s.Id != Id))
                {
                    if (entity as Point == bow as Point || entity as Point == stern as Point || entity as Point == this as Point)
                    {
                        if (entity is Barrel)
                        {
                            RumCarried += ((Barrel)entity).Rum;
                        }
                        else if (entity is Mine)
                        {
                            Console.Error.WriteLine("HIT A MINE! {0}", entity.Id);
                            return true;
                        }
                        else if (entity is Ship)
                        {
                            Console.Error.WriteLine("HIT A SHIP! {0}", entity.Id);
                            return true;
                        }
                    }
                }

                return false;
            }
            */
            //public Dictionary<Entity, Entity> GetPossiblePath(Entity target)
            //{
            //    var openSet = new Queue<Entity>();
            //    openSet.Enqueue(this);

            // var closed = new List<Entity>(); var path = new Dictionary<Entity, Entity>();
            // path[this] = null;

            // while (openSet.Count > 0) { var current = openSet.Dequeue();

            // if (current == target) break;

            // var neighbors = current is Ship ? ((Ship)current).Neighbors() : current.Neighbors();
            // foreach (var next in neighbors) { if (path.ContainsKey(next) || next is Mine || next
            // is Ship) continue;

            // openSet.Enqueue(next); path[next] = current; } }

            //    //Console.Error.WriteLine("PATH FULL {0}", string.Join(",", path));
            //    return path;
            //}

            //public List<Point> FindBestPath(Entity target)
            //{
            //    var fullPath = GetPossiblePath(target);
            //    var bestPath = new List<Point>();
            //    var current = target;
            //    bestPath.Add(target);
            //    while (current != this)
            //    {
            //        current = fullPath[current];
            //        bestPath.Add(current);
            //    }
            //    bestPath.Reverse();
            //    bestPath.Remove(this);

            //    return bestPath;
            //}

            public string MoveTo(double x, double y)
            {
                string action = "";

                Point currentPosition = this as Point;
                Point targetPosition = new Point(x, y);

                if (currentPosition.Equals(targetPosition))
                {
                    return "SLOWER";
                }

                double targetAngle, angleStraight, anglePort, angleStarboard, centerAngle, anglePortCenter, angleStarboardCenter;

                switch (Speed)
                {
                    case 2:
                        return "SLOWER";

                    case 1:
                        // Suppose we've moved first
                        currentPosition = currentPosition.Neighbor(Orientation);
                        if (!currentPosition.IsInsideMap())
                        {
                            return "SLOWER";
                        }

                        // Target reached at next turn
                        if (currentPosition.Equals(targetPosition))
                        {
                            return "WAIT";
                        }

                        // For each neighbor cell, find the closest to target
                        targetAngle = currentPosition.Angle(targetPosition);
                        angleStraight = Math.Min(Math.Abs(Orientation - targetAngle), 6 - Math.Abs(Orientation - targetAngle));
                        anglePort = Math.Min(Math.Abs((Orientation + 1) - targetAngle), Math.Abs((Orientation - 5) - targetAngle));
                        angleStarboard = Math.Min(Math.Abs((Orientation + 5) - targetAngle), Math.Abs((Orientation - 1) - targetAngle));

                        centerAngle = currentPosition.Angle(new Point(Width / 2, Height / 2));
                        anglePortCenter = Math.Min(Math.Abs((Orientation + 1) - centerAngle), Math.Abs((Orientation - 5) - centerAngle));
                        angleStarboardCenter = Math.Min(Math.Abs((Orientation + 5) - centerAngle), Math.Abs((Orientation - 1) - centerAngle));

                        // Next to target with bad angle, slow down then rotate (avoid to turn around
                        // the target!)
                        if (currentPosition.Distance(targetPosition) == 1 && angleStraight > 1.5)
                        {
                            return "SLOWER";
                        }

                        double distanceMin = int.MaxValue;

                        // Test forward
                        Point nextPosition = currentPosition.Neighbor(Orientation);
                        if (nextPosition.IsInsideMap())
                        {
                            distanceMin = nextPosition.Distance(targetPosition);
                            action = "";
                        }

                        // Test port
                        nextPosition = currentPosition.Neighbor((Orientation + 1) % 6);
                        if (nextPosition.IsInsideMap())
                        {
                            var distance = nextPosition.Distance(targetPosition);
                            if (distanceMin == int.MaxValue || distance < distanceMin || distance == distanceMin && anglePort < angleStraight - 0.5)
                            {
                                distanceMin = distance;
                                action = "PORT";
                            }
                        }

                        // Test starboard
                        nextPosition = currentPosition.Neighbor((Orientation + 5) % 6);
                        if (nextPosition.IsInsideMap())
                        {
                            var distance = nextPosition.Distance(targetPosition);
                            if (distanceMin == int.MaxValue || distance < distanceMin
                                    || (distance == distanceMin && angleStarboard < anglePort - 0.5 && action == "PORT")
                                    || (distance == distanceMin && angleStarboard < angleStraight - 0.5 && action == null)
                                    || (distance == distanceMin && action == "PORT" && angleStarboard == anglePort
                                            && angleStarboardCenter < anglePortCenter)
                                    || (distance == distanceMin && action == "PORT" && angleStarboard == anglePort
                                            && angleStarboardCenter == anglePortCenter && (Orientation == 1 || Orientation == 4)))
                            {
                                distanceMin = distance;
                                action = "STARBOARD";
                            }
                        }
                        break;

                    case 0:
                        // Rotate ship towards target
                        targetAngle = currentPosition.Angle(targetPosition);
                        angleStraight = Math.Min(Math.Abs(Orientation - targetAngle), 6 - Math.Abs(Orientation - targetAngle));
                        anglePort = Math.Min(Math.Abs((Orientation + 1) - targetAngle), Math.Abs((Orientation - 5) - targetAngle));
                        angleStarboard = Math.Min(Math.Abs((Orientation + 5) - targetAngle), Math.Abs((Orientation - 1) - targetAngle));

                        centerAngle = currentPosition.Angle(new Point(Width / 2, Height / 2));
                        anglePortCenter = Math.Min(Math.Abs((Orientation + 1) - centerAngle), Math.Abs((Orientation - 5) - centerAngle));
                        angleStarboardCenter = Math.Min(Math.Abs((Orientation + 5) - centerAngle), Math.Abs((Orientation - 1) - centerAngle));

                        Point forwardPosition = currentPosition.Neighbor(Orientation);

                        action = null;

                        if (anglePort <= angleStarboard)
                        {
                            action = "PORT";
                        }

                        if (angleStarboard < anglePort || angleStarboard == anglePort && angleStarboardCenter < anglePortCenter
                                || angleStarboard == anglePort && angleStarboardCenter == anglePortCenter && (Orientation == 1 || Orientation == 4))
                        {
                            action = "STARBOARD";
                        }

                        if (forwardPosition.IsInsideMap() && angleStraight <= anglePort && angleStraight <= angleStarboard)
                        {
                            action = "FASTER";
                        }
                        break;
                }
                return action;
            }

            public int NewOrientation { get; set; }

            public Action Action { get; set; }

            internal bool At(Point target)
            {
                Point stern = Stern();
                Point bow = Bow();
                return stern != null && stern.Equals(target) || bow != null && bow.Equals(target) || this.Equals(target);
            }

            internal void Heal(int health)
            {
                this.RumCarried += health;
                if (this.RumCarried > MaxRum)
                {
                    this.RumCarried = MaxRum;
                }
            }

            internal void Damage(int health)
            {
                this.RumCarried -= health;
                if (this.RumCarried <= 0)
                {
                    this.RumCarried = 0;
                }
            }

            public Point NewPosition { get; set; }

            public Point NewBowCoordinate { get; set; }

            public Point NewSternCoordinate { get; set; }

            public Point NewStern()
            {
                return Neighbor((NewOrientation + 3) % 6);
            }

            public Point NewBow()
            {
                return Neighbor(NewOrientation);
            }

            public bool newBowIntersect(Ship other)
            {
                return NewBowCoordinate != null && (NewBowCoordinate.Equals(other.NewBowCoordinate) || NewBowCoordinate.Equals(other.NewPosition)
                        || NewBowCoordinate.Equals(other.NewSternCoordinate));
            }

            public bool NewBowIntersect(List<Ship> ships)
            {
                foreach (Ship other in ships)
                {
                    if (this != other && newBowIntersect(other))
                    {
                        return true;
                    }
                }
                return false;
            }

            public bool newPositionsIntersect(Ship other)
            {
                bool sternCollision = NewSternCoordinate != null && (NewSternCoordinate.Equals(other.NewBowCoordinate)
                        || NewSternCoordinate.Equals(other.NewPosition) || NewSternCoordinate.Equals(other.NewSternCoordinate));
                bool centerCollision = NewPosition != null && (NewPosition.Equals(other.NewBowCoordinate) || NewPosition.Equals(other.NewPosition)
                        || NewPosition.Equals(other.NewSternCoordinate));
                return newBowIntersect(other) || sternCollision || centerCollision;
            }

            public bool NewPositionsIntersect(List<Ship> ships)
            {
                foreach (Ship other in ships)
                {
                    if (this != other && newPositionsIntersect(other))
                    {
                        return true;
                    }
                }
                return false;
            }

            internal Ship Clone(DNA dna)
            {
                var ship = new Ship(Id)
                {
                    CannonCooldown = CannonCooldown,
                    IsAlive = IsAlive,
                    MineCooldown = MineCooldown,
                    Orientation = Orientation,
                    RumCarried = RumCarried,
                    Speed = Speed,
                    Team = Team,
                    X = X,
                    Y = Y,
                    DNA = dna
                };

                return ship;
            }

            public DNA DNA { get; set; }

            internal double Fitness()
            {
                double fitness = 0;
                var myRum = Referee.players[0].shipsAlive.Sum(s => s.health);
                var enemyRum = Referee.players[1].shipsAlive.Sum(s => s.health);
                fitness = myRum - enemyRum;

                //Console.Error.WriteLine("FITNESS: {0} {1} {2} {3}", fitness, myRum, enemyRum, livingShips.Count());
                return fitness;
            }

            internal void Run()
            {
                //for (int i = 0; i < Lifetime; i++)
                //{
                //    Action = DNA.Genes[i];
                //    referee.updateGame(_round);
                //}
            }
        }

        /***********/
        /*
        private static void decrementRum()
        {
            foreach (Ship ship in ships)
            {
                ship.Damage(1);
            }
        }

        private static void updateInitialRum()
        {
            //foreach (Ship ship in ships) {
            //    ship.initialHealth = ship.health;
            //}
        }

        private static void moveCannonballs()
        {
            foreach (var ball in cannonballs)
            {
                if (ball.TurnsToImpact == 0)
                {
                    ball.IsAlive = false;
                    continue;
                }
                else if (ball.TurnsToImpact > 0)
                {
                    ball.TurnsToImpact--;
                }

                if (ball.TurnsToImpact == 0)
                {
                    cannonBallExplosions.Add(ball as Point);
                }
            }
        }

        private static List<Point> cannonBallExplosions = new List<Point>();
        private static List<Damage> damage = new List<Damage>();

        private static void explodeThings()
        {
            bool alive = true;
            for (int i = 0; i < cannonBallExplosions.Count; i++)
            {
                var position = cannonBallExplosions[i];
                foreach (Ship ship in ships.Where(s => s.IsAlive))
                {
                    if (position.Equals(ship.Bow()) || position.Equals(ship.Stern()))
                    {
                        damage.Add(new Damage(position, CannonIndirectDamage, true));
                        ship.Damage(CannonIndirectDamage);
                        cannonBallExplosions.Remove(position);
                        alive = false;
                        break;
                    }
                    else if (position.Equals(ship))
                    {
                        damage.Add(new Damage(position, CannonDirectDamage, true));
                        ship.Damage(CannonDirectDamage);
                        cannonBallExplosions.Remove(position);
                        alive = false;
                        break;
                    }
                }
                if (!alive) continue;

                for (int j = 0; j < mines.Count; j++)
                {
                    var mine = mines[j];
                    if (mine.IsAlive && mine.Equals(position))
                    {
                        damage.AddRange(mine.Explode(ships, true));
                        mine.IsAlive = false;
                        cannonBallExplosions.Remove(position);
                        alive = false;
                        break;
                    }
                }

                if (!alive) continue;
                for (int j = 0; j < barrels.Count; j++)
                {
                    var barrel = barrels[j];
                    if (barrel.IsAlive && barrel.Equals(position))
                    {
                        damage.Add(new Damage(position, 0, true));
                        barrel.IsAlive = false;
                        cannonBallExplosions.Remove(position);
                        break;
                    }
                }
            }
        }

        internal static void Play()
        {
            moveCannonballs();
            decrementRum();
            updateInitialRum();

            applyActions();
            moveShips();
            rotateShips();

            explodeThings();
            // For each sunk ship, create a new rum barrel with the amount of rum the ship had at the begin of the turn (up to 30).
            //for (Ship ship : ships) {
            //    if (ship.health <= 0) {
            //        int reward = Math.min(REWARD_RUM_BARREL_VALUE, ship.initialHealth);
            //        if (reward > 0) {
            //            barrels.add(new RumBarrel(ship.position.x, ship.position.y, reward));
            //        }
            //    }
            //}

            //for (Coord position : cannonBallExplosions) {
            //    damage.add(new Damage(position, 0, false));
            //}

            foreach (var ship in ships)
            {
                if (ship.RumCarried <= 0)
                {
                    ship.IsAlive = false;
                }
            }
        }

        internal static void applyActions()
        {
            foreach (Ship ship in _entities.OfType<Ship>().Where(s => s.IsAlive))
            {
                if (ship.MineCooldown > 0)
                {
                    ship.MineCooldown--;
                }
                if (ship.CannonCooldown > 0)
                {
                    ship.CannonCooldown--;
                }

                ship.NewOrientation = ship.Orientation;

                switch (ship.Action)
                {
                    case Action.FASTER:
                        if (ship.Speed < MaxSpeed)
                        {
                            ship.Speed++;
                        }
                        break;

                    case Action.SLOWER:
                        if (ship.Speed > 0)
                        {
                            ship.Speed--;
                        }
                        break;

                    case Action.PORT:
                        ship.NewOrientation = (ship.Orientation + 1) % 6;
                        break;

                    case Action.STARBOARD:
                        ship.NewOrientation = (ship.Orientation + 5) % 6;
                        break;

                    case Action.MINE:
                        if (ship.MineCooldown == 0)
                        {
                            Point target = ship.Stern().Neighbor((ship.Orientation + 3) % 6);

                            if (target.IsInsideMap())
                            {
                                bool cellIsFreeOfBarrels = !barrels.Any(barrel => barrel.Equals(target));
                                bool cellIsFreeOfShips = !ships.Where(b => b != ship).Any(b => b.At(target));

                                if (cellIsFreeOfBarrels && cellIsFreeOfShips)
                                {
                                    ship.MineCooldown = MineCooldown;
                                    Mine mine = new Mine(-1, target.X, target.Y);
                                    mines.Add(mine);
                                }
                            }
                        }
                        break;

                    case Action.FIRE:
                        var distance = ship.Bow().Distance(ship.Target);
                        if (ship.Target.IsInsideMap() && distance <= CannonRange && ship.CannonCooldown == 0)
                        {
                            int travelTime = (int)(1 + Math.Round(ship.Bow().Distance(ship.Target) / 3.0));
                            cannonballs.Add(new Cannonball(-1, ship.Target, ship, travelTime));
                            ship.CannonCooldown = CannonCooldown;
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        private static bool checkCollisions(Ship ship)
        {
            Point bow = ship.Bow();
            Point stern = ship.Stern();
            Point center = ship as Point;

            // Collision with the barrels
            foreach (var barrel in barrels.Where(b => b.IsAlive))
            {
                if (barrel.Equals(bow) || barrel.Equals(stern) || barrel.Equals(center))
                {
                    ship.Heal(barrel.Rum);
                    barrel.IsAlive = false;
                }
            }

            // Collision with the mines
            foreach (var mine in mines.Where(m => m.IsAlive))
            {
                List<Damage> mineDamage = mine.Explode(ships, false);

                if (mineDamage.Count > 0)
                {
                    //damage.addAll(mineDamage);
                    mine.IsAlive = false;
                }
            }

            return ship.RumCarried <= 0;
        }

        private static void moveShips()
        {
            // --- Go forward ---
            for (int i = 1; i <= MaxSpeed; i++)
            {
                foreach (Ship ship in ships.Where(s => s.IsAlive))
                {
                    ship.NewPosition = ship as Point;
                    ship.NewBowCoordinate = ship.Bow();
                    ship.NewSternCoordinate = ship.Stern();

                    if (i > ship.Speed)
                    {
                        continue;
                    }

                    Point newCoordinate = ship.Neighbor(ship.Orientation);

                    if (newCoordinate.IsInsideMap())
                    {
                        // Set new coordinate.
                        ship.NewPosition = newCoordinate;
                        ship.NewBowCoordinate = newCoordinate.Neighbor(ship.Orientation);
                        ship.NewSternCoordinate = newCoordinate.Neighbor((ship.Orientation + 3) % 6);
                    }
                    else
                    {
                        // Stop ship!
                        ship.Speed = 0;
                    }
                }

                // Check ship and obstacles collisions
                List<Ship> collisions = new List<Ship>();
                bool collisionDetected = true;
                while (collisionDetected)
                {
                    collisionDetected = false;

                    foreach (Ship ship in ships)
                    {
                        if (ship.NewBowIntersect(ships))
                        {
                            collisions.Add(ship);
                        }
                    }

                    foreach (Ship ship in collisions)
                    {
                        // Revert last move
                        ship.NewPosition = ship as Point;
                        ship.NewBowCoordinate = ship.Bow();
                        ship.NewSternCoordinate = ship.Stern();

                        // Stop ships
                        ship.Speed = 0;

                        collisionDetected = true;
                    }
                    collisions.Clear();
                }

                foreach (Ship ship in ships.Where(s => s.IsAlive))
                {
                    ship.X = ship.NewPosition.X;
                    ship.Y = ship.NewPosition.Y;
                    if (checkCollisions(ship))
                    {
                        ship.IsAlive = false;  //LOST ? ?
                    }
                }
            }
        }

        private static void rotateShips()
        {
            // Rotate
            foreach (Ship ship in ships.Where(s => s.IsAlive))
            {
                ship.NewPosition = ship as Point;
                ship.NewBowCoordinate = ship.NewBow();
                ship.NewSternCoordinate = ship.NewStern();
            }

            // Check collisions
            bool collisionDetected = true;
            List<Ship> collisions = new List<Ship>();
            while (collisionDetected)
            {
                collisionDetected = false;

                foreach (Ship ship in ships)
                {
                    if (ship.NewPositionsIntersect(ships))
                    {
                        collisions.Add(ship);
                    }
                }

                foreach (Ship ship in collisions)
                {
                    ship.NewOrientation = ship.Orientation;
                    ship.NewBowCoordinate = ship.NewBow();
                    ship.NewSternCoordinate = ship.NewStern();
                    ship.Speed = 0;
                    collisionDetected = true;
                }

                collisions.Clear();
            }

            // Apply rotation
            foreach (Ship ship in ships.Where(s => s.IsAlive))
            {
                if (ship.RumCarried == 0)
                {
                    continue;
                }

                ship.Orientation = ship.NewOrientation;
                if (checkCollisions(ship))
                {
                    ship.IsAlive = false;  /// ???
                    //shipLosts.add(ship);
                }
            }
        }
        */
        /************/

        /// <summary>
        /// GA
        /// </summary>
        public class DNA
        {
            private const int ActionLength = 5;
            public Referee.Action[] Genes { get; set; }

            public DNA()
            {
                Genes = new Referee.Action[Lifetime];
                for (int i = 0; i < Lifetime; i++)
                {
                    var move = (Referee.Action)_rand.Next(ActionLength);
                    Genes[i] = move;
                }
            }

            public DNA(Referee.Action[] genes)
            {
                Genes = genes;
            }

            public DNA Crossover(DNA partner)
            {
                var child = new Referee.Action[Lifetime];
                int crossoverPoint = _rand.Next(Lifetime);
                for (int i = 0; i < Lifetime; i++)
                {
                    if (i > crossoverPoint)
                        child[i] = Genes[i];
                    else
                        child[i] = partner.Genes[i];
                }

                return new DNA(child);
            }

            public void Mutate(float rate)
            {
                for (int i = 0; i < Lifetime; i++)
                {
                    if (_rand.NextDouble() < rate)
                    {
                        var action = (Referee.Action)_rand.Next(ActionLength);
                        Genes[i] = action;
                    }
                }
            }
        }

        public static int generations = 0;

        public class Population
        {
            private float mutationRate = 1.0f;

            public Referee.Ship[] Pop { get; set; }
            public List<Ship> MatingPool { get; set; }

            public Population(float mutationRate, int populationSize, Referee.Ship ship)
            {
                this.mutationRate = mutationRate;
                Pop = new Referee.Ship[populationSize];
                MatingPool = new List<Ship>();
                for (int i = 0; i < populationSize; i++)
                {
                    Pop[i] = ship;//.Clone(new DNA());
                }
            }

            internal void Run()
            {
            }

            internal double Fitness()
            {
                double fitness = 0;
                var myRum = Referee.players[0].shipsAlive.Sum(s => s.health);
                var enemyRum = Referee.players[1].shipsAlive.Sum(s => s.health);
                fitness = myRum - enemyRum;

                //Console.Error.WriteLine("FITNESS: {0} {1} {2} {3}", fitness, myRum, enemyRum, livingShips.Count());
                return fitness;
            }

            public void Live()
            {
                for (int index = 0; index < PopulationSize; index++)
                {
                    //Pop[index].Run();
                    for (int i = 0; i < Lifetime; i++)
                    {
                        Pop[index].action = DNA.Genes[i];
                        referee.updateGame(_round);
                    }
                }
            }

            public void Fitness()
            {
                for (int i = 0; i < PopulationSize; i++)
                {
                    Pop[i].Fitness();
                }
            }

            public DNA Selection()
            {
                MatingPool.Clear();

                var s = GetMaxFitness();
                double maxFitness = s.Fitness();

                for (int i = 0; i < PopulationSize; i++)
                {
                    MatingPool.Add(Pop[i]);
                }

                return s.DNA;
            }

            public void Reproduction()
            {
                //_entities.ForEach(e => e.Load());
                for (int i = 0; i < PopulationSize; i++)
                {
                    int m = _rand.Next(MatingPool.Count);
                    int f = _rand.Next(MatingPool.Count);

                    var mom = MatingPool[m];
                    var dad = MatingPool[f];

                    var momGenes = mom.DNA;
                    var dadGenes = dad.DNA;

                    var child = momGenes.Crossover(dadGenes);
                    child.Mutate(mutationRate);

                    Pop[i] = mom.Clone(child);
                }
                generations++;
            }

            public Ship GetMaxFitness()
            {
                var maxFitness = double.MinValue;
                Ship max = Pop[0];
                for (int i = 0; i < PopulationSize; i++)
                {
                    if (Pop[i].Fitness() > maxFitness)
                    {
                        maxFitness = Pop[i].Fitness();
                        max = Pop[i];
                    }
                }

                return max;
            }
        }

        private static List<Barrel> barrels = new List<Barrel>();
        private static List<Mine> mines = new List<Mine>();
        private static List<Cannonball> cannonballs = new List<Cannonball>();
        private static Stopwatch stopwatch;
        private static Referee referee = new Referee();

        private static void Main(string[] args)
        {
            var me = new Referee.Player(1);
            var enemyPlayer = new Referee.Player(0);
            Referee.players.Add(me);
            Referee.players.Add(enemyPlayer);

            // game loop
            while (true)
            {
                _round++;
                _entities.Clear();
                me.shipsAlive.Clear();
                enemyPlayer.shipsAlive.Clear();
                barrels.Clear();
                cannonballs.Clear();
                mines.Clear();

                int myShipCount = int.Parse(Console.ReadLine()); // the number of remaining ships
                int entityCount = int.Parse(Console.ReadLine()); // the number of entities (e.g. ships, mines or cannonballs)
                for (int i = 0; i < entityCount; i++)
                {
                    string[] inputs = Console.ReadLine().Split(' ');
                    int entityId = int.Parse(inputs[0]);
                    string entityType = inputs[1];
                    int x = int.Parse(inputs[2]);
                    int y = int.Parse(inputs[3]);
                    int arg1 = int.Parse(inputs[4]);
                    int arg2 = int.Parse(inputs[5]);
                    int arg3 = int.Parse(inputs[6]);
                    int arg4 = int.Parse(inputs[7]);
                    Referee.Entity ent = null;
                    switch (entityType)
                    {
                        case "SHIP":
                            Referee.Ship ship;
                            if (_round == 0)
                            {
                                ship = new Referee.Ship(x, y, arg1, arg4);
                                if (arg4 == 1)
                                {
                                    me.ships.Add(ship);
                                    me.shipsAlive.Add(ship);
                                }
                                else
                                {
                                    enemyPlayer.ships.Add(ship);
                                    enemyPlayer.shipsAlive.Add(ship);
                                }
                            }
                            else
                            {
                                if (arg4 == 1)
                                {
                                    ship = me.ships.FirstOrDefault(s => s.id == entityId);
                                    me.shipsAlive.Add(ship);
                                }
                                else
                                {
                                    ship = enemyPlayer.ships.FirstOrDefault(s => s.id == entityId);
                                    enemyPlayer.shipsAlive.Add(ship);
                                }
                                ship.position.x = x;
                                ship.position.y = y;
                                ship.orientation = arg1;
                                ship.health = arg3;
                            }
                            ent = ship;
                            break;

                        case "BARREL":
                            ent = new Referee.Rum(x, y, arg1);
                            Referee.barrels.Add((Referee.Rum)ent);
                            break;

                        case "CANNONBALL":
                            var source = me.ships.Concat(enemyPlayer.ships).FirstOrDefault(s => s.id == entityId);
                            ent = new Referee.Cannonball(x, y, entityId, source.position.x, source.position.y, arg1);
                            Referee.cannonballs.Add((Referee.Cannonball)ent);
                            break;

                        case "MINE":
                            ent = new Referee.Mine(x, y);
                            Referee.mines.Add((Referee.Mine)ent);
                            break;
                    }
                    _entities.Add(ent);
                }

                foreach (var ship in me.ships.Where(s => !me.shipsAlive.Contains(s)))
                {
                    me.shipsAlive.Remove(ship);
                }
                foreach (var ship in enemyPlayer.ships.Where(s => !enemyPlayer.shipsAlive.Contains(s)))
                {
                    enemyPlayer.shipsAlive.Remove(ship);
                }
                //_entities.ForEach(e => e.Save());

                //Console.Error.WriteLine("Ents {0}", string.Join(",", _entities));
                int timeLimit = 30;// _round == 0 ? 1000 : 50;
                stopwatch = Stopwatch.StartNew();
                var pop = new Population(1.0f, PopulationSize, me.shipsAlive[0]);
                DNA best = null;
                while (stopwatch.ElapsedMilliseconds < timeLimit)
                {
                    pop.Live();
                    pop.Fitness();
                    best = pop.Selection();

                    pop.Reproduction();
                }

                if (_round > 0) Console.Error.WriteLine("AVG: {0}", generations / _round);
                for (int i = 0; i < myShipCount; i++)
                {
                    switch (best.Genes[0])
                    {
                        case Action.FASTER:
                            Console.WriteLine("FASTER");
                            break;

                        case Action.SLOWER:
                            Console.WriteLine("SLOWER");
                            break;

                        case Action.PORT:
                            Console.WriteLine("PORT");
                            break;

                        case Action.STARBOARD:
                            Console.WriteLine("STARBOARD");
                            break;

                        case Action.MINE:
                            Console.WriteLine("MINE");
                            break;

                        default:
                            Console.WriteLine("WAIT");
                            break;
                    }
                }
                //continue;
                //var moves = new List<Entity>();
                //foreach (var ship in myShip.Where(s => s.IsAlive))
                //{
                //    var closestRum = _entities.OfType<Barrel>().Where(b => !moves.Contains(b)).OrderBy(b => b.Distance(ship));
                //    var enemy = enemyShip.Where(s => s.IsAlive).OrderBy(s => ship.Distance(s)).FirstOrDefault();
                //    var enemyDistance = enemy.Distance(ship.GetNextBow());
                //    var barrel = closestRum.FirstOrDefault();
                //    var nextPos = ship.GetNextPosition();
                //    var closestCannon = _entities.OfType<Cannonball>().OrderBy(c => c.Distance(ship)).FirstOrDefault();

                // if ((ship.Speed > 0 || barrel == null || enemyDistance < 2) && enemyDistance < 6
                // && ship.CannonCooldown <= 0) { var newPos = ship.PredictedLocation(enemy); //new
                // Point(enemy.X + delta.X * eta, enemy.Y + delta.Y * eta); Console.WriteLine("FIRE
                // {0} {1}", newPos.X, newPos.Y); ship.CannonCooldown = CannonCooldown; } else if
                // (closestCannon != null && closestCannon.Distance(ship) < 2) {
                // Console.WriteLine("FASTER"); } else if (enemyDistance > CannonRange * 0.5) { //var
                // path = ship.FindBestPath(enemy); //if (path.Count > 0) // Console.WriteLine("MOVE
                // {0} {1}", path[0].X, path[0].Y); //elseif ( Console.Error.WriteLine("Chasing enemy
                // {0}", enemy.Id); if (_entities.OfType<Mine>().Any(m => m.Equals(nextPos) ||
                // m.Equals(ship.GetNextBow()) || m.Equals(ship.GetNextStern()) || m.Equals(ship) ||
                // m.Equals(ship.Bow()) || m.Equals(ship.Stern()))) Console.WriteLine("STARBOARD");
                // else Console.WriteLine("MOVE {0} {1}", enemy.X, enemy.Y); } else if (barrel !=
                // null) { moves.Add(barrel); //var path = ship.FindBestPath(barrel);
                // //Console.Error.WriteLine("CUR: {2} DEST {1} PATH {0}", string.Join(",", path),
                // barrel, ship); //if (path.Count > 0) //{ int Direction = ship.Orientation; var
                // targetAngle = nextPos.Angle(barrel); var angleStraight =
                // Math.Min(Math.Abs(Direction - targetAngle), 6 - Math.Abs(Direction -
                // targetAngle)); var anglePort = Math.Min(Math.Abs((Direction + 1) - targetAngle),
                // Math.Abs((Direction - 5) - targetAngle)); var angleStarboard =
                // Math.Min(Math.Abs((Direction + 5) - targetAngle), Math.Abs((Direction - 1) - targetAngle));

                // var centerAngle = nextPos.Angle(new Point(Width / 2, Height / 2)); var
                // anglePortCenter = Math.Min(Math.Abs((Direction + 1) - centerAngle),
                // Math.Abs((Direction - 5) - centerAngle)); var angleStarboardCenter =
                // Math.Min(Math.Abs((Direction + 5) - centerAngle), Math.Abs((Direction - 1) - centerAngle));

                //        if (nextPos.Distance(barrel) == 1 && angleStraight > 1.5)
                //        {
                //            Console.WriteLine("SLOWER");
                //            continue;
                //        }
                //        var dir = ship.MoveTo(barrel.X, barrel.Y);
                //        Console.Error.WriteLine(dir);
                //        if (string.IsNullOrEmpty(dir))
                //        {
                //            if (_entities.OfType<Mine>().Any(m => m.Equals(nextPos) || m.Equals(ship.GetNextBow()) || m.Equals(ship.GetNextStern())))
                //                Console.WriteLine("STARBOARD DODGING!");
                //            else
                //                Console.WriteLine("MOVE {0} {1}", barrel.X, barrel.Y);
                //        }
                //        else if (dir == "STARBOARD")
                //        {
                //            var n = ship.Neighbor((ship.Orientation + 5) % 6);
                //            if (_entities.OfType<Mine>().Any(m => m.Equals(n)))//|| m.Equals(ship.GetNextBow()) || m.Equals(ship.GetNextStern())))
                //                Console.WriteLine("MINE DODGING!");
                //            else
                //                Console.WriteLine("MOVE {0} {1}", barrel.X, barrel.Y);
                //        }
                //        else if (dir == "PORT")
                //        {
                //            var n = ship.Neighbor((ship.Orientation + 1) % 6);
                //            if (_entities.OfType<Mine>().Any(m => m.Equals(n)))//|| m.Equals(ship.GetNextBow()) || m.Equals(ship.GetNextStern())))
                //                Console.WriteLine("MINE DODGING!");
                //            else
                //                Console.WriteLine("MOVE {0} {1}", barrel.X, barrel.Y);
                //        }
                //        else
                //            Console.WriteLine("MOVE {0} {1}", barrel.X, barrel.Y);
                //        //    else
                //        //    {
                //        //        Console.WriteLine("MOVE {0} {1}", path[0].X, path[0].Y);
                //        //    }
                //        //}
                //        //else
                //        Console.Error.WriteLine("Chasing barrel {0}", barrel.Id);
                //    }
                //    else
                //    {
                //        //var path = ship.FindBestPath(new Entity(-1, _rand.Next(Width - 1), _rand.Next(Height - 1)));
                //        //if (path.Count > 0)
                //        //    Console.WriteLine("MOVE {0} {1}", path[0].X, path[0].Y);
                //        //else
                //        Console.WriteLine("MOVE {0} {1}", _rand.Next(Width - 1), _rand.Next(Height - 1));
                //    }
                //    //Console.WriteLine("WAIT"); // Any valid action, such as "WAIT" or "MOVE x y"
                //}
            }
        }
    }

    internal class Referee
    {
        public const int MAP_WIDTH = 23;
        public const int MAP_HEIGHT = 21;
        public const int COOLDOWN_CANNON = 2;
        public const int COOLDOWN_MINE = 5;
        public const int INITIAL_SHIP_HEALTH = 100;
        public const int MAX_SHIP_HEALTH = 100;
        public const int MAX_SHIP_SPEED = 2;
        public const int MIN_SHIPS = 1;
        public const int MAX_SHIPS = 3;
        public const int MIN_MINES = 5;
        public const int MAX_MINES = 10;
        public const int MIN_RUM_BARRELS = 10;
        public const int MAX_RUM_BARRELS = 26;
        public const int MIN_RUM_BARREL_VALUE = 10;
        public const int MAX_RUM_BARREL_VALUE = 20;
        public const int REWARD_RUM_BARREL_VALUE = 30;
        public const int MINE_VISIBILITY_RANGE = 5;
        public const int FIRE_DISTANCE_MAX = 10;
        public const int LOW_DAMAGE = 25;
        public const int HIGH_DAMAGE = 50;
        public const int MINE_DAMAGE = 25;
        public const int NEAR_MINE_DAMAGE = 10;

        public static int clamp(int val, int min, int max)
        {
            return Math.Max(min, Math.Min(max, val));
        }

        public Referee()
        {
            players = new List<Player>();
            mines = new List<Mine>();
            barrels = new List<Rum>();
            cannonballs = new List<Cannonball>();
            damage = new List<Damage>();
            cannonBallExplosions = new List<Coord>();
        }

        public class Coord
        {
            public static int[,] DIRECTIONS_EVEN = new int[,] { { 1, 0 }, { 0, -1 }, { -1, -1 }, { -1, 0 }, { -1, 1 }, { 0, 1 } };
            public static int[,] DIRECTIONS_ODD = new int[,] { { 1, 0 }, { 1, -1 }, { 0, -1 }, { -1, 0 }, { 0, 1 }, { 1, 1 } };
            public int x;
            public int y;

            public Coord(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public Coord(Coord other)
            {
                this.x = other.x;
                this.y = other.y;
            }

            public double angle(Coord targetPosition)
            {
                double dy = (targetPosition.y - this.y) * Math.Sqrt(3) / 2;
                double dx = targetPosition.x - this.x + ((this.y - targetPosition.y) & 1) * 0.5;
                double angle = -Math.Atan2(dy, dx) * 3 / Math.PI;
                if (angle < 0)
                {
                    angle += 6;
                }
                else if (angle >= 6)
                {
                    angle -= 6;
                }
                return angle;
            }

            public CubeCoordinate toCubeCoordinate()
            {
                int xp = x - (y - (y & 1)) / 2;
                int zp = y;
                int yp = -(xp + zp);
                return new CubeCoordinate(xp, yp, zp);
            }

            public Coord neighbor(int orientation)
            {
                int newY, newX;
                if (this.y % 2 == 1)
                {
                    newY = this.y + DIRECTIONS_ODD[orientation, 1];
                    newX = this.x + DIRECTIONS_ODD[orientation, 0];
                }
                else
                {
                    newY = this.y + DIRECTIONS_EVEN[orientation, 1];
                    newX = this.x + DIRECTIONS_EVEN[orientation, 0];
                }

                return new Coord(newX, newY);
            }

            public bool isInsideMap()
            {
                return x >= 0 && x < MAP_WIDTH && y >= 0 && y < MAP_HEIGHT;
            }

            public int distanceTo(Coord dst)
            {
                return this.toCubeCoordinate().distanceTo(dst.toCubeCoordinate());
            }

            public bool equals(Object obj)
            {
                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }
                Coord other = (Coord)obj;
                return y == other.y && x == other.x;
            }

            public override string ToString()
            {
                return string.Format("{0} {1}", x, y);
            }
        }

        public class CubeCoordinate
        {
            private static int[,] directions = new int[,] { { 1, -1, 0 }, { +1, 0, -1 }, { 0, +1, -1 }, { -1, +1, 0 }, { -1, 0, +1 }, { 0, -1, +1 } };
            private int x, y, z;

            public CubeCoordinate(int x, int y, int z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            public Coord toOffsetCoordinate()
            {
                int newX = x + (z - (z & 1)) / 2;
                int newY = z;
                return new Coord(newX, newY);
            }

            public CubeCoordinate neighbor(int orientation)
            {
                int nx = this.x + directions[orientation, 0];
                int ny = this.y + directions[orientation, 1];
                int nz = this.z + directions[orientation, 2];

                return new CubeCoordinate(nx, ny, nz);
            }

            public int distanceTo(CubeCoordinate dst)
            {
                return (Math.Abs(x - dst.x) + Math.Abs(y - dst.y) + Math.Abs(z - dst.z)) / 2;
            }

            public override string ToString()
            {
                return string.Format("{0} {1} {2}", x, y, z);
            }
        }

        public enum EntityType
        {
            SHIP, BARREL, MINE, CANNONBALL
        }

        public class Entity
        {
            public int UNIQUE_ENTITY_ID = 0;

            public int id;
            public EntityType type;
            public Coord position;

            public Entity()
            {
            }

            public Entity(EntityType type, int x, int y)
            {
                this.id = UNIQUE_ENTITY_ID++;
                this.type = type;
                this.position = new Coord(x, y);
            }

            public Entity(int Id, EntityType type, int x, int y)
            {
                this.id = Id;
                this.type = type;
                this.position = new Coord(x, y);
            }

            public virtual string toViewstring()
            {
                return string.Format("{0} {1} {2}", id, position.y, position.x);
            }

            public string toPlayerstring(int arg1, int arg2, int arg3, int arg4)
            {
                return string.Format("{0} {1} {2} {3} {4} {5} {6} {7}", id, type.ToString(), position.x, position.y, arg1, arg2, arg3, arg4);
            }
        }

        public class Mine : Entity
        {
            public Mine(int x, int y)
                : base(EntityType.MINE, x, y)
            {
                //base(EntityType.MINE, x, y);
            }

            public Mine(int Id, int x, int y)
                : base(Id, EntityType.MINE, x, y)
            {
                //base(Id, EntityType.MINE, x, y);
            }

            public string toPlayerstring(int playerIdx)
            {
                return toPlayerstring(0, 0, 0, 0);
            }

            public List<Damage> explode(List<Ship> ships, bool force)
            {
                List<Damage> damage = new List<Damage>();
                Ship victim = null;

                foreach (Ship ship in ships)
                {
                    if (position.equals(ship.bow()) || position.equals(ship.stern()) || position.equals(ship.position))
                    {
                        damage.Add(new Damage(this.position, MINE_DAMAGE, true));
                        ship.damage(MINE_DAMAGE);
                        victim = ship;
                    }
                }

                if (force || victim != null)
                {
                    if (victim == null)
                    {
                        damage.Add(new Damage(this.position, MINE_DAMAGE, true));
                    }

                    foreach (Ship ship in ships)
                    {
                        if (ship != victim)
                        {
                            Coord impactPosition = null;
                            if (ship.stern().distanceTo(position) <= 1)
                            {
                                impactPosition = ship.stern();
                            }
                            if (ship.bow().distanceTo(position) <= 1)
                            {
                                impactPosition = ship.bow();
                            }
                            if (ship.position.distanceTo(position) <= 1)
                            {
                                impactPosition = ship.position;
                            }

                            if (impactPosition != null)
                            {
                                ship.damage(NEAR_MINE_DAMAGE);
                                damage.Add(new Damage(impactPosition, NEAR_MINE_DAMAGE, true));
                            }
                        }
                    }
                }

                return damage;
            }
        }

        public class Cannonball : Entity
        {
            public int ownerEntityId;
            public int srcX;
            public int srcY;
            public int initialRemainingTurns;
            public int remainingTurns;

            public Cannonball(int row, int col, int ownerEntityId, int srcX, int srcY, int remainingTurns)
                : base(EntityType.CANNONBALL, row, col)
            {
                //super(EntityType.CANNONBALL, row, col);
                this.ownerEntityId = ownerEntityId;
                this.srcX = srcX;
                this.srcY = srcY;
                this.initialRemainingTurns = this.remainingTurns = remainingTurns;
            }

            public override string toViewstring()
            {
                return string.Format("{0} {1} {2} {3} {4} {5} {6} {7}", id, position.y, position.x, srcY, srcX, initialRemainingTurns, remainingTurns, ownerEntityId);
            }

            public string toPlayerstring(int playerIdx)
            {
                return toPlayerstring(ownerEntityId, remainingTurns, 0, 0);
            }
        }

        public class Rum : Entity
        {
            public int health;

            public Rum(int x, int y, int health)
                : base(EntityType.BARREL, x, y)
            {
                //super(EntityType.BARREL, x, y);
                this.health = health;
            }

            public override string toViewstring()
            {
                return string.Format("{0} {1} {2} {3}", id, position.y, position.x, health);
            }

            public string toPlayerstring(int playerIdx)
            {
                return toPlayerstring(health, 0, 0, 0);
            }
        }

        public class Damage
        {
            public Coord position;
            public int health;
            public bool hit;

            public Damage(Coord position, int health, bool hit)
            {
                this.position = position;
                this.health = health;
                this.hit = hit;
            }

            public string toViewstring()
            {
                return string.Format("{0} {1} {2} {3}", position.y, position.x, health, (hit ? 1 : 0));
            }
        }

        public enum Action
        {
            FASTER, SLOWER, PORT, STARBOARD, FIRE, MINE, WAIT
        }

        public class Ship : Entity
        {
            public int orientation;
            public int speed;
            public int health;
            public int initialHealth;
            public int owner;
            public string message;
            public Action action;
            public int mineCooldown;
            public int cannonCooldown;
            public Coord target;
            public int newOrientation;
            public Coord newPosition;
            public Coord newBowCoordinate;
            public Coord newSternCoordinate;

            public Ship(int x, int y, int orientation, int owner)
                : base(EntityType.SHIP, x, y)
            {
                //super(EntityType.SHIP, x, y);
                this.orientation = orientation;
                this.speed = 0;
                this.health = INITIAL_SHIP_HEALTH;
                this.owner = owner;
            }

            public override string toViewstring()
            {
                return string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11}", id, position.y, position.x, orientation, health, speed, action, bow().y, bow().x, stern().y,
                        stern().x, " ;" + (message != null ? message : ""));
            }

            public string toPlayerstring(int playerIdx)
            {
                return toPlayerstring(orientation, speed, health, owner == playerIdx ? 1 : 0);
            }

            public void setMessage(string message)
            {
                if (message != null && message.Length > 50)
                {
                    message = message.Substring(0, 50) + "...";
                }
                this.message = message;
            }

            public void moveTo(int x, int y)
            {
                Coord currentPosition = this.position;
                Coord targetPosition = new Coord(x, y);

                if (currentPosition.equals(targetPosition))
                {
                    this.action = Action.SLOWER;
                    return;
                }

                double targetAngle, angleStraight, anglePort, angleStarboard, centerAngle, anglePortCenter, angleStarboardCenter;

                switch (speed)
                {
                    case 2:
                        this.action = Action.SLOWER;
                        break;

                    case 1:
                        // Suppose we've moved first
                        currentPosition = currentPosition.neighbor(orientation);
                        if (!currentPosition.isInsideMap())
                        {
                            this.action = Action.SLOWER;
                            break;
                        }

                        // Target reached at next turn
                        if (currentPosition.equals(targetPosition))
                        {
                            this.action = Action.WAIT;
                            break;
                        }

                        // For each neighbor cell, find the closest to target
                        targetAngle = currentPosition.angle(targetPosition);
                        angleStraight = Math.Min(Math.Abs(orientation - targetAngle), 6 - Math.Abs(orientation - targetAngle));
                        anglePort = Math.Min(Math.Abs((orientation + 1) - targetAngle), Math.Abs((orientation - 5) - targetAngle));
                        angleStarboard = Math.Min(Math.Abs((orientation + 5) - targetAngle), Math.Abs((orientation - 1) - targetAngle));

                        centerAngle = currentPosition.angle(new Coord(MAP_WIDTH / 2, MAP_HEIGHT / 2));
                        anglePortCenter = Math.Min(Math.Abs((orientation + 1) - centerAngle), Math.Abs((orientation - 5) - centerAngle));
                        angleStarboardCenter = Math.Min(Math.Abs((orientation + 5) - centerAngle), Math.Abs((orientation - 1) - centerAngle));

                        // Next to target with bad angle, slow down then rotate (avoid to turn around
                        // the target!)
                        if (currentPosition.distanceTo(targetPosition) == 1 && angleStraight > 1.5)
                        {
                            this.action = Action.SLOWER;
                            break;
                        }

                        int? distanceMin = null;

                        // Test forward
                        Coord nextPosition = currentPosition.neighbor(orientation);
                        if (nextPosition.isInsideMap())
                        {
                            distanceMin = nextPosition.distanceTo(targetPosition);
                            this.action = Action.WAIT;
                        }

                        // Test port
                        nextPosition = currentPosition.neighbor((orientation + 1) % 6);
                        if (nextPosition.isInsideMap())
                        {
                            int distance = nextPosition.distanceTo(targetPosition);
                            if (distanceMin == null || distance < distanceMin || distance == distanceMin && anglePort < angleStraight - 0.5)
                            {
                                distanceMin = distance;
                                this.action = Action.PORT;
                            }
                        }

                        // Test starboard
                        nextPosition = currentPosition.neighbor((orientation + 5) % 6);
                        if (nextPosition.isInsideMap())
                        {
                            int distance = nextPosition.distanceTo(targetPosition);
                            if (distanceMin == null || distance < distanceMin
                                    || (distance == distanceMin && angleStarboard < anglePort - 0.5 && this.action == Action.PORT)
                                    || (distance == distanceMin && angleStarboard < angleStraight - 0.5 && this.action == Action.WAIT)
                                    || (distance == distanceMin && this.action == Action.PORT && angleStarboard == anglePort
                                            && angleStarboardCenter < anglePortCenter)
                                    || (distance == distanceMin && this.action == Action.PORT && angleStarboard == anglePort
                                            && angleStarboardCenter == anglePortCenter && (orientation == 1 || orientation == 4)))
                            {
                                distanceMin = distance;
                                this.action = Action.STARBOARD;
                            }
                        }
                        break;

                    case 0:
                        // Rotate ship towards target
                        targetAngle = currentPosition.angle(targetPosition);
                        angleStraight = Math.Min(Math.Abs(orientation - targetAngle), 6 - Math.Abs(orientation - targetAngle));
                        anglePort = Math.Min(Math.Abs((orientation + 1) - targetAngle), Math.Abs((orientation - 5) - targetAngle));
                        angleStarboard = Math.Min(Math.Abs((orientation + 5) - targetAngle), Math.Abs((orientation - 1) - targetAngle));

                        centerAngle = currentPosition.angle(new Coord(MAP_WIDTH / 2, MAP_HEIGHT / 2));
                        anglePortCenter = Math.Min(Math.Abs((orientation + 1) - centerAngle), Math.Abs((orientation - 5) - centerAngle));
                        angleStarboardCenter = Math.Min(Math.Abs((orientation + 5) - centerAngle), Math.Abs((orientation - 1) - centerAngle));

                        Coord forwardPosition = currentPosition.neighbor(orientation);

                        this.action = Action.WAIT;

                        if (anglePort <= angleStarboard)
                        {
                            this.action = Action.PORT;
                        }

                        if (angleStarboard < anglePort || angleStarboard == anglePort && angleStarboardCenter < anglePortCenter
                                || angleStarboard == anglePort && angleStarboardCenter == anglePortCenter && (orientation == 1 || orientation == 4))
                        {
                            this.action = Action.STARBOARD;
                        }

                        if (forwardPosition.isInsideMap() && angleStraight <= anglePort && angleStraight <= angleStarboard)
                        {
                            this.action = Action.FASTER;
                        }
                        break;
                }
            }

            public void faster()
            {
                this.action = Action.FASTER;
            }

            public void slower()
            {
                this.action = Action.SLOWER;
            }

            public void port()
            {
                this.action = Action.PORT;
            }

            public void starboard()
            {
                this.action = Action.STARBOARD;
            }

            public void placeMine()
            {
                this.action = Action.MINE;
            }

            public Coord stern()
            {
                return position.neighbor((orientation + 3) % 6);
            }

            public Coord bow()
            {
                return position.neighbor(orientation);
            }

            public Coord newStern()
            {
                return position.neighbor((newOrientation + 3) % 6);
            }

            public Coord newBow()
            {
                return position.neighbor(newOrientation);
            }

            public bool at(Coord coord)
            {
                Coord sternCoord = stern();
                Coord bowCoord = bow();
                return sternCoord != null && sternCoord.equals(coord) || bowCoord != null && bowCoord.equals(coord) || position.equals(coord);
            }

            public bool newBowIntersect(Ship other)
            {
                return newBowCoordinate != null && (newBowCoordinate.equals(other.newBowCoordinate) || newBowCoordinate.equals(other.newPosition)
                        || newBowCoordinate.equals(other.newSternCoordinate));
            }

            public bool newBowIntersect(List<Ship> ships)
            {
                foreach (Ship other in ships)
                {
                    if (this != other && newBowIntersect(other))
                    {
                        return true;
                    }
                }
                return false;
            }

            public bool newPositionsIntersect(Ship other)
            {
                bool sternCollision = newSternCoordinate != null && (newSternCoordinate.equals(other.newBowCoordinate)
                        || newSternCoordinate.equals(other.newPosition) || newSternCoordinate.equals(other.newSternCoordinate));
                bool centerCollision = newPosition != null && (newPosition.equals(other.newBowCoordinate) || newPosition.equals(other.newPosition)
                        || newPosition.equals(other.newSternCoordinate));
                return newBowIntersect(other) || sternCollision || centerCollision;
            }

            public bool newPositionsIntersect(List<Ship> ships)
            {
                foreach (Ship other in ships)
                {
                    if (this != other && newPositionsIntersect(other))
                    {
                        return true;
                    }
                }
                return false;
            }

            public void damage(int health)
            {
                this.health -= health;
                if (this.health <= 0)
                {
                    this.health = 0;
                }
            }

            public void heal(int health)
            {
                this.health += health;
                if (this.health > MAX_SHIP_HEALTH)
                {
                    this.health = MAX_SHIP_HEALTH;
                }
            }

            public void fire(int x, int y)
            {
                Coord target = new Coord(x, y);
                this.target = target;
                this.action = Action.FIRE;
            }
        }

        public class Player
        {
            public int id;
            public List<Ship> ships;
            public List<Ship> shipsAlive;

            public Player(int id)
            {
                this.id = id;
                this.ships = new List<Ship>();
                this.shipsAlive = new List<Ship>();
            }

            public void setDead()
            {
                foreach (Ship ship in ships)
                {
                    ship.health = 0;
                }
            }

            public int getScore()
            {
                int score = 0;
                foreach (Ship ship in ships)
                {
                    score += ship.health;
                }
                return score;
            }

            public List<string> toViewstring()
            {
                List<string> data = new List<string>();

                data.Add(this.id.ToString());
                foreach (Ship ship in ships)
                {
                    data.Add(ship.toViewstring());
                }

                return data;
            }
        }

        public static long seed;
        public static List<Cannonball> cannonballs;
        public static List<Mine> mines;
        public static List<Rum> barrels;
        public static List<Player> players;
        public static List<Ship> ships;
        public static List<Damage> damage;
        public static List<Coord> cannonBallExplosions;
        public static int shipsPerPlayer;
        public static int mineCount;
        public static int barrelCount;
        public static Random random;

        public void prepare(int round)
        {
            foreach (Player player in players)
            {
                foreach (Ship ship in player.ships)
                {
                    ship.action = Action.WAIT;
                    ship.message = null;
                }
            }
            cannonBallExplosions.Clear();
            damage.Clear();
        }

        public int getExpectedOutputLineCountForPlayer(int playerIdx)
        {
            return players.First(a => a.id == playerIdx).shipsAlive.Count();
        }

        public void decrementRum()
        {
            foreach (Ship ship in ships)
            {
                ship.damage(1);
            }
        }

        public void updateInitialRum()
        {
            foreach (Ship ship in ships)
            {
                ship.initialHealth = ship.health;
            }
        }

        public void moveCannonballs()
        {
            cannonballs = cannonballs.Where(b => b.remainingTurns != 0).ToList();

            foreach (Cannonball ball in cannonballs)
            {
                //Cannonball ball = it.next();
                if (ball.remainingTurns == 0)
                {
                    //should never reach
                    continue;
                }
                else if (ball.remainingTurns > 0)
                {
                    ball.remainingTurns--;
                }

                if (ball.remainingTurns == 0)
                {
                    cannonBallExplosions.Add(ball.position);
                }
            }
        }

        public void applyActions()
        {
            foreach (Player player in players)
            {
                foreach (Ship ship in player.shipsAlive)
                {
                    if (ship.mineCooldown > 0)
                    {
                        ship.mineCooldown--;
                    }
                    if (ship.cannonCooldown > 0)
                    {
                        ship.cannonCooldown--;
                    }

                    ship.newOrientation = ship.orientation;

                    if (ship.action != Action.WAIT)
                    {
                        switch (ship.action)
                        {
                            case Action.FASTER:
                                if (ship.speed < MAX_SHIP_SPEED)
                                {
                                    ship.speed++;
                                }
                                break;

                            case Action.SLOWER:
                                if (ship.speed > 0)
                                {
                                    ship.speed--;
                                }
                                break;

                            case Action.PORT:
                                ship.newOrientation = (ship.orientation + 1) % 6;
                                break;

                            case Action.STARBOARD:
                                ship.newOrientation = (ship.orientation + 5) % 6;
                                break;

                            case Action.MINE:
                                if (ship.mineCooldown == 0)
                                {
                                    Coord target = ship.stern().neighbor((ship.orientation + 3) % 6);

                                    if (target.isInsideMap())
                                    {
                                        bool cellIsFreeOfBarrels = !barrels.Any(barrel => barrel.position.equals(target));
                                        bool cellIsFreeOfMines = !mines.Any(mine => mine.position.equals(target));
                                        bool cellIsFreeOfShips = !ships.Where(b => b.id != ship.id).Any(b => b.at(target));

                                        if (cellIsFreeOfBarrels && cellIsFreeOfShips && cellIsFreeOfMines)
                                        {
                                            ship.mineCooldown = COOLDOWN_MINE;
                                            Mine mine = new Mine(target.x, target.y);
                                            mines.Add(mine);
                                        }
                                    }
                                }
                                break;

                            case Action.FIRE:
                                int distance = ship.bow().distanceTo(ship.target);
                                if (ship.target.isInsideMap() && distance <= FIRE_DISTANCE_MAX && ship.cannonCooldown == 0)
                                {
                                    int travelTime = (int)(1 + Math.Round(ship.bow().distanceTo(ship.target) / 3.0));
                                    cannonballs.Add(new Cannonball(ship.target.x, ship.target.y, ship.id, ship.bow().x, ship.bow().y, travelTime));
                                    ship.cannonCooldown = COOLDOWN_CANNON;
                                }
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
        }

        public void checkCollisions(Ship ship)
        {
            Coord bow = ship.bow();
            Coord stern = ship.stern();
            Coord center = ship.position;

            List<Rum> removedBarrels = new List<Rum>();
            // Collision with the barrels
            foreach (Rum barrel in barrels)
            {
                if (barrel.position.equals(bow) || barrel.position.equals(stern) || barrel.position.equals(center))
                {
                    ship.heal(barrel.health);
                    removedBarrels.Add(barrel);
                }
            }

            List<Mine> removedMines = new List<Mine>();
            // Collision with the mines
            foreach (Mine mine in mines)
            {
                List<Damage> mineDamage = mine.explode(ships, false);

                if (mineDamage.Count() > 0)
                {
                    damage.AddRange(mineDamage);
                    removedMines.Add(mine);
                }
            }

            foreach (var barrel in removedBarrels)
            {
                barrels.Remove(barrel);
            }

            foreach (var mine in removedMines)
            {
                mines.Remove(mine);
            }
        }

        public void moveShips()
        {
            // --- Go forward ---
            for (int i = 1; i <= MAX_SHIP_SPEED; i++)
            {
                foreach (Player player in players)
                {
                    foreach (Ship ship in player.shipsAlive)
                    {
                        ship.newPosition = ship.position;
                        ship.newBowCoordinate = ship.bow();
                        ship.newSternCoordinate = ship.stern();

                        if (i > ship.speed)
                        {
                            continue;
                        }

                        Coord newCoordinate = ship.position.neighbor(ship.orientation);

                        if (newCoordinate.isInsideMap())
                        {
                            // Set new coordinate.
                            ship.newPosition = newCoordinate;
                            ship.newBowCoordinate = newCoordinate.neighbor(ship.orientation);
                            ship.newSternCoordinate = newCoordinate.neighbor((ship.orientation + 3) % 6);
                        }
                        else
                        {
                            // Stop ship!
                            ship.speed = 0;
                        }
                    }
                }

                // Check ship and obstacles collisions
                List<Ship> collisions = new List<Ship>();
                bool collisionDetected = true;
                while (collisionDetected)
                {
                    collisionDetected = false;

                    foreach (Ship ship in ships)
                    {
                        if (ship.newBowIntersect(ships))
                        {
                            collisions.Add(ship);
                        }
                    }

                    foreach (Ship ship in collisions)
                    {
                        // Revert last move
                        ship.newPosition = ship.position;
                        ship.newBowCoordinate = ship.bow();
                        ship.newSternCoordinate = ship.stern();

                        // Stop ships
                        ship.speed = 0;

                        collisionDetected = true;
                    }
                    collisions.Clear();
                }

                foreach (Player player in players)
                {
                    foreach (Ship ship in player.shipsAlive)
                    {
                        ship.position = ship.newPosition;
                        checkCollisions(ship);
                    }
                }
            }
        }

        public void rotateShips()
        {
            // Rotate
            foreach (Player player in players)
            {
                foreach (Ship ship in player.shipsAlive)
                {
                    ship.newPosition = ship.position;
                    ship.newBowCoordinate = ship.newBow();
                    ship.newSternCoordinate = ship.newStern();
                }
            }

            // Check collisions
            bool collisionDetected = true;
            List<Ship> collisions = new List<Ship>();
            while (collisionDetected)
            {
                collisionDetected = false;

                foreach (Ship ship in ships)
                {
                    if (ship.newPositionsIntersect(ships))
                    {
                        collisions.Add(ship);
                    }
                }

                foreach (Ship ship in collisions)
                {
                    ship.newOrientation = ship.orientation;
                    ship.newBowCoordinate = ship.newBow();
                    ship.newSternCoordinate = ship.newStern();
                    ship.speed = 0;
                    collisionDetected = true;
                }

                collisions.Clear();
            }

            // Apply rotation
            foreach (Player player in players)
            {
                foreach (Ship ship in player.shipsAlive)
                {
                    ship.orientation = ship.newOrientation;
                    checkCollisions(ship);
                }
            }
        }

        public bool gameIsOver()
        {
            foreach (Player player in players)
            {
                if (player.shipsAlive.Count() == 0)
                {
                    return true;
                }
            }
            return barrels.Count() == 0;
        }

        public void explodeShips()
        {
            List<Coord> removedPositions = new List<Coord>();

            foreach (Coord position in cannonBallExplosions)
            {
                foreach (Ship ship in ships)
                {
                    if (position.equals(ship.bow()) || position.equals(ship.stern()))
                    {
                        damage.Add(new Damage(position, LOW_DAMAGE, true));
                        ship.damage(LOW_DAMAGE);
                        removedPositions.Add(position);
                        break;
                    }
                    else if (position.equals(ship.position))
                    {
                        damage.Add(new Damage(position, HIGH_DAMAGE, true));
                        ship.damage(HIGH_DAMAGE);
                        removedPositions.Add(position);
                        break;
                    }
                }
            }

            foreach (var position in removedPositions)
            {
                cannonBallExplosions.Remove(position);
            }
        }

        public void explodeMines()
        {
            List<Coord> removedPositions = new List<Coord>();
            List<Mine> removedMines = new List<Mine>();
            foreach (Coord position in cannonBallExplosions)
            {
                foreach (Mine mine in mines)
                {
                    if (mine.position.equals(position))
                    {
                        damage.AddRange(mine.explode(ships, true));
                        removedPositions.Add(position);
                        removedMines.Add(mine);

                        break;
                    }
                }
            }
            foreach (var position in removedPositions)
            {
                cannonBallExplosions.Remove(position);
            }
            foreach (var mine in removedMines)
            {
                mines.Remove(mine);
            }
        }

        public void explodeBarrels()
        {
            List<Coord> removedPositions = new List<Coord>();
            List<Rum> removedRum = new List<Rum>();

            foreach (Coord position in cannonBallExplosions)
            {
                foreach (Rum barrel in barrels)
                {
                    if (barrel.position.equals(position))
                    {
                        damage.Add(new Damage(position, 0, true));
                        removedPositions.Add(position);
                        removedRum.Add(barrel);
                        break;
                    }
                }
            }

            foreach (var position in removedPositions)
            {
                cannonBallExplosions.Remove(position);
            }
            foreach (var rum in removedRum)
            {
                barrels.Remove(rum);
            }
        }

        public bool updateGame(int round)
        {
            moveCannonballs();
            decrementRum();
            updateInitialRum();

            applyActions();
            moveShips();
            rotateShips();

            explodeShips();
            explodeMines();
            explodeBarrels();

            // For each sunk ship, create a new rum barrel with the amount of rum the ship had at the
            // begin of the turn (up to 30).
            foreach (Ship ship in ships)
            {
                if (ship.health <= 0)
                {
                    int reward = Math.Min(REWARD_RUM_BARREL_VALUE, ship.initialHealth);
                    if (reward > 0)
                    {
                        barrels.Add(new Rum(ship.position.x, ship.position.y, reward));
                    }
                }
            }

            foreach (Coord position in cannonBallExplosions)
            {
                damage.Add(new Damage(position, 0, false));
            }

            List<Ship> removedShips = new List<Ship>();

            foreach (Ship ship in ships)
            {
                if (ship.health <= 0)
                {
                    players.First(a => a.id == ship.owner).shipsAlive.Remove(ship);
                    removedShips.Add(ship);
                }
            }

            foreach (Ship ship in removedShips)
            {
                ships.Remove(ship);
            }

            if (gameIsOver())
            {
                return true;
            }
            return false;
        }

        public string[] getPlayerActions(int playerIdx, int round)
        {
            return new string[0];
        }

        public bool isPlayerDead(int playerIdx)
        {
            return false;
        }

        public string getDeathReason(int playerIdx)
        {
            return "$" + playerIdx + ": Eliminated!";
        }

        public int getScore(int playerIdx)
        {
            return players.First(a => a.id == playerIdx).getScore();
        }
    }
}