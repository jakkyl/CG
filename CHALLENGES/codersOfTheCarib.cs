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

        public class Point
        {
            private int[,] DIRECTIONS_EVEN = new int[,] { { 1, 0 }, { 0, -1 }, { -1, -1 }, { -1, 0 }, { -1, 1 }, { 0, 1 } };
            private int[,] DIRECTIONS_ODD = new int[,] { { 1, 0 }, { 1, -1 }, { 0, -1 }, { -1, 0 }, { 0, 1 }, { 1, 1 } };

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

            public bool IsInsideMap()
            {
                return X >= 0 && X < Width && Y >= 0 && Y < Height;
            }

            public Point Neighbor(int orientation)
            {
                double newY, newX;
                if (this.Y % 2 == 1)
                {
                    newY = this.Y + DIRECTIONS_ODD[orientation, 1];
                    newX = this.X + DIRECTIONS_ODD[orientation, 0];
                }
                else
                {
                    newY = this.Y + DIRECTIONS_EVEN[orientation, 1];
                    newX = this.X + DIRECTIONS_EVEN[orientation, 0];
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
                        var ent = _entities.FirstOrDefault(e => e as Point == n);
                        if (ent != null) ns.Add(ent);
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
        }

        public class Entity : Point
        {
            public int Id { get; set; }
            public int Direction { get; set; }

            public Entity(int id, double x, double y)
                : base(x, y)
            {
                Id = id;
            }

            public double Angle(Point targetPosition)
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
            public int TurnsToImpact { get; set; }

            public Cannonball(int id, double x, double y, int turns)
                : base(id, x, y)
            {
                TurnsToImpact = turns;
            }
        }

        public class Mine : Entity
        {
            public Mine(int id, double x, double y)
                : base(id, x, y)
            {
            }
        }

        public class Ship : Entity
        {
            public int Speed { get; set; }
            public int RumCarried { get; set; }
            public int Team { get; set; }
            public int MineCooldown { get; set; }
            public int CannonCooldown { get; set; }
            public bool IsAlive { get; set; }

            public Ship(int id, double x, double y, int rotation, int speed, int rum, int team)
                : base(id, x, y)
            {
                Direction = rotation;
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

            public Point GetNextPosition()
            {
                Point nextPos = this as Point;
                for (int i = 1; i < Speed; i++)
                {
                    var temp = nextPos.Neighbor(Direction);
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
                        Direction = Direction
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
                    Direction = Direction
                };
                return s.Stern();
            }

            public Point Bow()
            {
                return Neighbor(Direction);
            }

            public Point Stern()
            {
                return Neighbor(((int)Direction + 3) % 6);
            }

            internal void Update(int x, int y, int direction, int speed, int rum)
            {
                X = x;
                Y = y;
                Direction = direction;
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

                    foreach (var next in current.Neighbors())
                    {
                        //Console.Error.WriteLine("{0} - N: {1}", current, next);
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
                while (current != this)
                {
                    current = fullPath[current];
                    bestPath.Add(current);
                }
                bestPath.Reverse();
                bestPath.Remove(this);

                return bestPath;
            }
        }

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

                int timeLimit = _round == 0 ? 1000 : 50;

                var moves = new List<Entity>();
                foreach (var ship in myShip.Where(s => s.IsAlive))
                {
                    var closestRum = _entities.OfType<Barrel>().Where(b => !moves.Contains(b)).OrderBy(b => b.Distance(ship));
                    var enemy = enemyShip.Where(s => s.IsAlive).OrderBy(s => ship.Distance(s)).FirstOrDefault();
                    var enemyDistance = enemy.Distance(ship.GetNextBow());
                    var barrel = closestRum.FirstOrDefault();
                    var nextPos = ship.GetNextPosition();

                    if ((ship.Speed > 0 || barrel == null) &&
                        enemyDistance < CannonRange * 0.5 &&
                        (barrel == null || enemyDistance < barrel.Distance(ship)) &&
                        ship.CannonCooldown <= 0)
                    {
                        var eta = 1 + Math.Round(enemyDistance / 3);
                        var leadPos = enemy.GetNextPosition();
                        var delta = leadPos.Subtract(enemy);
                        var newPos = new Point(enemy.X + delta.X * eta, enemy.Y + delta.Y * eta);
                        Console.WriteLine("FIRE {0} {1}", newPos.X, newPos.Y);
                        ship.CannonCooldown = CannonCooldown;
                    }
                    else if (barrel != null)
                    {
                        moves.Add(barrel);
                        var path = ship.FindBestPath(barrel);
                        Console.Error.WriteLine("DEST {1} PATH {0}", string.Join(",", path), barrel);
                        if (path.Count > 0)
                            Console.WriteLine("MOVE {0} {1}", path[0].X, path[0].Y);
                        else
                            Console.WriteLine("MOVE {0} {1}", barrel.X, barrel.Y);
                    }
                    else
                    {
                        var path = ship.FindBestPath(new Entity(-1, _rand.Next(Width), _rand.Next(Height)));
                        if (path.Count > 0)
                            Console.WriteLine("MOVE {0} {1}", path[0].X, path[0].Y);
                        else
                            Console.WriteLine("MOVE {0} {1}", _rand.Next(Width), _rand.Next(Height));
                    }
                    //Console.WriteLine("WAIT"); // Any valid action, such as "WAIT" or "MOVE x y"
                }
            }
        }
    }
}