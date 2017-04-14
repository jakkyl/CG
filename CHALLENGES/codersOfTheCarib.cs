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

        public enum Direction
        {
            Right,
            UpperRight,
            UpperLeft,
            Left,
            LowerLeft,
            LowerRight
        }

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

            public Point Neighbor(Direction orientation)
            {
                double newY, newX;
                if (this.Y % 2 == 1)
                {
                    newY = this.Y + DIRECTIONS_ODD[(int)orientation, 1];
                    newX = this.X + DIRECTIONS_ODD[(int)orientation, 0];
                }
                else
                {
                    newY = this.Y + DIRECTIONS_EVEN[(int)orientation, 1];
                    newX = this.X + DIRECTIONS_EVEN[(int)orientation, 0];
                }

                return new Point(newX, newY);
            }

            public override string ToString()
            {
                return string.Format("[{0},{1}]", X, Y);
            }
        }

        public class Entity : Point
        {
            public int Id { get; set; }
            public Direction Direction { get; set; }

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

            public Ship(int id, double x, double y, Direction rotation, int speed, int rum, int team)
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

            public Point Move(Direction dir)
            {
                switch (dir)
                {
                    case Player.Direction.LowerLeft:
                        return new Point(-1, 1);
                }

                return new Point(0, 0);
            }

            internal void Update(int x, int y, Player.Direction direction, int speed, int rum)
            {
                X = x;
                Y = y;
                Direction = direction;
                Speed = speed;
                RumCarried = rum;
                if (CannonCooldown > 0) CannonCooldown--;
                if (MineCooldown > 0) MineCooldown--;
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
                                ent = new Ship(entityId, x, y, (Direction)arg1, arg2, arg3, arg4);
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
                                ((Ship)ent).Update(x, y, (Direction)arg1, arg2, arg3);
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

                foreach (var ship in _entities.OfType<Ship>().Where(s => !shipsAlive.Contains(s)))
                {
                    ship.IsAlive = false;
                }

                int timeLimit = _round == 0 ? 1000 : 50;

                var moves = new List<Entity>();
                foreach (var ship in myShip.Where(s => s.IsAlive))
                {
                    var closestRum = _entities.OfType<Barrel>().Where(b => !moves.Contains(b)).OrderBy(b => b.Distance(ship));
                    var enemy = enemyShip.Where(s => s.IsAlive).OrderBy(s => ship.Distance(s)).FirstOrDefault();
                    var enemyDistance = enemy.Distance(ship);
                    var barrel = closestRum.FirstOrDefault();
                    if ((ship.Speed > 0 || barrel == null) && enemyDistance < CannonRange && (barrel == null || enemyDistance < barrel.Distance(ship)) && ship.CannonCooldown <= 0)
                    {
                        var leadPos = enemy.GetNextPosition();
                        Console.WriteLine("FIRE {0} {1}", leadPos.X, leadPos.Y);
                        ship.CannonCooldown = CannonCooldown;
                    }
                    else if (barrel != null)
                    {
                        moves.Add(barrel);
                        Console.WriteLine("MOVE {0} {1}", barrel.X, barrel.Y);
                    }
                    else
                    {
                        Console.WriteLine("MOVE {0} {1}", _rand.Next(Width), _rand.Next(Height));
                    }
                    //Console.WriteLine("WAIT"); // Any valid action, such as "WAIT" or "MOVE x y"
                }
            }
        }
    }
}