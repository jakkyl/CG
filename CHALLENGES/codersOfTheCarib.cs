using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/

namespace CodersOfTheCari
{
    internal class Player
    {
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
        private const int CannonCooldown = 1;
        private const int MaxSpeed = 2;

        private static List<Entity> _entities = new List<Entity>();
        private static int _round = -1;

        private static Random _rand = new Random();

        private enum Action
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

            public List<Entity> Neighbors()
            {
                var ns = new List<Entity>();
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
                        else ns.Add(new Entity(-1, n.X, n.Y));
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
            public int Id { get; set; }
            public int Orientation { get; set; }
            public bool IsAlive { get; set; }

            public Entity(int id, double x, double y)
                : base(x, y)
            {
                Id = id;
            }

            public override string ToString()
            {
                return string.Format("{0} {1},{2}", Id, X, Y);
            }
        }

        public class Barrel : Entity
        {
            public int Rum { get; set; }

            public Barrel(int id, double x, double y, int rum)
                : base(id, x, y)
            {
                Rum = rum;
            }
        }

        public class Cannonball : Entity
        {
            public Point Target { get; set; }
            public int SourceId { get; set; }
            public Point SourcePosition { get; set; }

            private int travelTime;

            public int TurnsToImpact { get; set; }

            public Cannonball(int id, double x, double y, int turns)
                : base(id, x, y)
            {
                TurnsToImpact = turns;
            }

            public Cannonball(int id, Point target, Ship source, int remainingTurns)
                : base(id, source.X, source.Y)
            {
                Target = target;
                SourceId = source.Id;
                SourcePosition = source.Bow();

                this.travelTime = remainingTurns;
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

            public Dictionary<Entity, Entity> GetPossiblePath(Entity target)
            {
                var openSet = new Queue<Entity>();
                openSet.Enqueue(this);

                var closed = new List<Entity>();
                var path = new Dictionary<Entity, Entity>();
                path[this] = null;

                while (openSet.Count > 0)
                {
                    var current = openSet.Dequeue();

                    if (current == target) break;

                    var neighbors = current is Ship ? ((Ship)current).Neighbors() : current.Neighbors();
                    foreach (var next in neighbors)
                    {
                        if (path.ContainsKey(next) || next is Mine || next is Ship) continue;

                        openSet.Enqueue(next);
                        path[next] = current;
                    }
                }

                //Console.Error.WriteLine("PATH FULL {0}", string.Join(",", path));
                return path;
            }

            public List<Point> FindBestPath(Entity target)
            {
                var fullPath = GetPossiblePath(target);
                var bestPath = new List<Point>();
                var current = target;
                bestPath.Add(target);
                while (current != this)
                {
                    current = fullPath[current];
                    bestPath.Add(current);
                }
                bestPath.Reverse();
                bestPath.Remove(this);

                return bestPath;
            }

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
        }

        /***********/

        private void applyActions()
        {
            foreach (var player in _entities.OfType<Ship>())
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

                    if (ship.Action != null)
                    {
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
            }
        }

        private bool checkCollisions(Ship ship)
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

        private void moveShips()
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

        private void rotateShips()
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

        /************/
        private List<Ship> ships = new List<Ship>();
        private List<Barrel> barrels = new List<Barrel>();
        private List<Mine> mines = new List<Mine>();
        private List<Cannonball> cannonballs = new List<Cannonball>();

        private static void Main(string[] args)
        {
            var myShip = new List<Ship>();
            var enemyShip = new List<Ship>();
            var shipsAlive = new List<Ship>();
            // game loop
            while (true)
            {
                _round++;
                _entities.Clear();
                shipsAlive.Clear();
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
                    Entity ent = null;
                    switch (entityType)
                    {
                        case "SHIP":
                            if (_round == 0)
                            {
                                Console.Error.WriteLine("New Ship: {0}", entityId);
                                ent = new Ship(entityId, x, y, arg1, arg2, arg3, arg4);
                                if (arg4 == 1)
                                    myShip.Add((Ship)ent);
                                else
                                    enemyShip.Add((Ship)ent);
                            }
                            else
                            {
                                if (arg4 == 1)
                                    ent = myShip.FirstOrDefault(s => s.Id == entityId);
                                else
                                    ent = enemyShip.FirstOrDefault(s => s.Id == entityId);
                                ((Ship)ent).Update(x, y, arg1, arg2, arg3);
                            }
                            shipsAlive.Add((Ship)ent);
                            break;

                        case "BARREL":
                            ent = new Barrel(entityId, x, y, arg1);
                            break;

                        case "CANNONBALL":
                            ent = new Cannonball(entityId, x, y, arg1);
                            break;

                        case "MINE":
                            ent = new Mine(entityId, x, y);
                            break;
                    }
                    _entities.Add(ent);
                }
                var allShips = myShip.Concat(enemyShip);
                foreach (var ship in allShips.Where(s => !shipsAlive.Contains(s)))
                {
                    ship.IsAlive = false;
                }
                //Console.Error.WriteLine("Ents {0}", string.Join(",", _entities));
                int timeLimit = _round == 0 ? 1000 : 50;

                var moves = new List<Entity>();
                foreach (var ship in myShip.Where(s => s.IsAlive))
                {
                    var closestRum = _entities.OfType<Barrel>().Where(b => !moves.Contains(b)).OrderBy(b => b.Distance(ship));
                    var enemy = enemyShip.Where(s => s.IsAlive).OrderBy(s => ship.Distance(s)).FirstOrDefault();
                    var enemyDistance = enemy.Distance(ship.GetNextBow());
                    var barrel = closestRum.FirstOrDefault();
                    var nextPos = ship.GetNextPosition();
                    var closestCannon = _entities.OfType<Cannonball>().OrderBy(c => c.Distance(ship)).FirstOrDefault();

                    if ((ship.Speed > 0 || barrel == null || enemyDistance < 2) &&
                        enemyDistance < 6 && ship.CannonCooldown <= 0)
                    {
                        var newPos = ship.PredictedLocation(enemy);
                        //new Point(enemy.X + delta.X * eta, enemy.Y + delta.Y * eta);
                        Console.WriteLine("FIRE {0} {1}", newPos.X, newPos.Y);
                        ship.CannonCooldown = CannonCooldown;
                    }
                    else if (closestCannon != null && closestCannon.Distance(ship) < 2)
                    {
                        Console.WriteLine("FASTER");
                    }
                    else if (enemyDistance > CannonRange * 0.5)
                    {
                        //var path = ship.FindBestPath(enemy);
                        //if (path.Count > 0)
                        //    Console.WriteLine("MOVE {0} {1}", path[0].X, path[0].Y);
                        //elseif (
                        Console.Error.WriteLine("Chasing enemy {0}", enemy.Id);
                        if (_entities.OfType<Mine>().Any(m => m.Equals(nextPos) || m.Equals(ship.GetNextBow()) || m.Equals(ship.GetNextStern()) ||
                                                              m.Equals(ship) || m.Equals(ship.Bow()) || m.Equals(ship.Stern())))
                            Console.WriteLine("STARBOARD");
                        else
                            Console.WriteLine("MOVE {0} {1}", enemy.X, enemy.Y);
                    }
                    else if (barrel != null)
                    {
                        moves.Add(barrel);
                        //var path = ship.FindBestPath(barrel);
                        //Console.Error.WriteLine("CUR: {2} DEST {1} PATH {0}", string.Join(",", path), barrel, ship);
                        //if (path.Count > 0)
                        //{
                        int Direction = ship.Orientation;
                        var targetAngle = nextPos.Angle(barrel);
                        var angleStraight = Math.Min(Math.Abs(Direction - targetAngle), 6 - Math.Abs(Direction - targetAngle));
                        var anglePort = Math.Min(Math.Abs((Direction + 1) - targetAngle), Math.Abs((Direction - 5) - targetAngle));
                        var angleStarboard = Math.Min(Math.Abs((Direction + 5) - targetAngle), Math.Abs((Direction - 1) - targetAngle));

                        var centerAngle = nextPos.Angle(new Point(Width / 2, Height / 2));
                        var anglePortCenter = Math.Min(Math.Abs((Direction + 1) - centerAngle), Math.Abs((Direction - 5) - centerAngle));
                        var angleStarboardCenter = Math.Min(Math.Abs((Direction + 5) - centerAngle), Math.Abs((Direction - 1) - centerAngle));

                        if (nextPos.Distance(barrel) == 1 && angleStraight > 1.5)
                        {
                            Console.WriteLine("SLOWER");
                            continue;
                        }
                        var dir = ship.MoveTo(barrel.X, barrel.Y);
                        Console.Error.WriteLine(dir);
                        if (string.IsNullOrEmpty(dir))
                        {
                            if (_entities.OfType<Mine>().Any(m => m.Equals(nextPos) || m.Equals(ship.GetNextBow()) || m.Equals(ship.GetNextStern())))
                                Console.WriteLine("STARBOARD DODGING!");
                            else
                                Console.WriteLine("MOVE {0} {1}", barrel.X, barrel.Y);
                        }
                        else if (dir == "STARBOARD")
                        {
                            var n = ship.Neighbor((ship.Orientation + 5) % 6);
                            if (_entities.OfType<Mine>().Any(m => m.Equals(n)))//|| m.Equals(ship.GetNextBow()) || m.Equals(ship.GetNextStern())))
                                Console.WriteLine("MINE DODGING!");
                            else
                                Console.WriteLine("MOVE {0} {1}", barrel.X, barrel.Y);
                        }
                        else if (dir == "PORT")
                        {
                            var n = ship.Neighbor((ship.Orientation + 1) % 6);
                            if (_entities.OfType<Mine>().Any(m => m.Equals(n)))//|| m.Equals(ship.GetNextBow()) || m.Equals(ship.GetNextStern())))
                                Console.WriteLine("MINE DODGING!");
                            else
                                Console.WriteLine("MOVE {0} {1}", barrel.X, barrel.Y);
                        }
                        else
                            Console.WriteLine("MOVE {0} {1}", barrel.X, barrel.Y);
                        //    else
                        //    {
                        //        Console.WriteLine("MOVE {0} {1}", path[0].X, path[0].Y);
                        //    }
                        //}
                        //else
                        Console.Error.WriteLine("Chasing barrel {0}", barrel.Id);
                    }
                    else
                    {
                        //var path = ship.FindBestPath(new Entity(-1, _rand.Next(Width - 1), _rand.Next(Height - 1)));
                        //if (path.Count > 0)
                        //    Console.WriteLine("MOVE {0} {1}", path[0].X, path[0].Y);
                        //else
                        Console.WriteLine("MOVE {0} {1}", _rand.Next(Width - 1), _rand.Next(Height - 1));
                    }
                    //Console.WriteLine("WAIT"); // Any valid action, such as "WAIT" or "MOVE x y"
                }
            }
        }
    }
}