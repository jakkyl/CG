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

namespace MeanMax
{
    internal class Player
    {
        internal enum UnitType
        {
            Reaper = 0,
            Destroyer = 1,
            Doof,
            Tanker = 3,
            Wreck = 4,
            Tar,
            Oil
        }

        internal struct Point
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

        internal class Unit
        {
            public int Id { get; set; }
            public UnitType UnitType { get; set; }
            public Point Position { get; set; }
            public int Radius { get; internal set; }
            public float Mass { get; internal set; }
            public Point Velocity { get; internal set; }

            public double Angle
            {
                get
                {
                    // multiply by 180.0 / PI to convert radiants to degrees.
                    var a = Math.Acos(Velocity.X) * 180 / Math.PI;

                    // If the point I want is below me, I have to shift the angle for it to be correct
                    if (Velocity.Y < 0)
                    {
                        a = 360 - a;
                    }
                    return a;
                }
            }

            public int Player { get; internal set; }

            public override string ToString()
            {
                return $"{Enum.GetName(typeof(UnitType), UnitType)} ({Id})";
            }

            internal double DiffAngle(Unit unit)
            {
                var a = GetAngle(unit);

                // To know whether we should turn clockwise or not we look at the two ways and keep
                // the smallest
                var right = Angle <= a ? a - Angle : 360 - Angle + a;
                var left = Angle >= a ? Angle - a : Angle + 360 - a;

                if (right < left)
                    return right;

                return -left;
            }

            private double GetAngle(Unit unit)
            {
                var d = Position.Distance(unit.Position);
                var dx = (unit.Position.X - Position.X) / d;
                var dy = (unit.Position.Y - Position.Y) / d;

                // multiply by 180.0 / PI to convert radiants to degrees.
                var a = Math.Acos(dx) * 180 / Math.PI;

                // If the point I want is below me, I have to shift the angle for it to be correct
                if (dy < 0)
                {
                    a = 360 - a;
                }
                Console.Error.WriteLine("DIFFANGLE: " + a);
                return a;
            }

            public bool Collision(Unit unit)
            {
                //same speed will never collide
                if (Velocity.X == unit.Velocity.X && Velocity.Y == unit.Velocity.Y) return false;

                var distance = Position.Distance2(unit.Position);
                var sumOfRadii = Math.Pow(Radius + unit.Radius, 2);

                //already touching
                //if (distance < sumOfRadii) return new Collision(this, unit, 0.0);

                // We place ourselves in the reference frame of u. u is therefore stationary and is
                // at (0,0)
                var dx = Position.X - unit.Position.X;
                var dy = Position.Y - unit.Position.Y;
                Point myp = new Point(dx, dy);
                var vx = Velocity.X - unit.Velocity.X;
                var vy = Velocity.Y - unit.Velocity.Y;
                var a = vx * vx + vy * vy;
                if (a < 0.00001) return false;

                var b = -2.0 * (dx * vx + dy * vy);

                var delta = b * b - 4.0 * a * (dx * dx + dy * dy - sumOfRadii);

                if (delta < 0.0) return false;

                var t = (b - Math.Sqrt(delta)) * (1.0 / (2.0 * a));
                //Console.Error.WriteLine("Bounce {0} {1} - {2}", this, unit, t);
                if (t <= 0.0 || t > 1.0) return false;

                return true;// new Collision(this, unit, t);
            }
        }

        internal class Wreck : Unit
        {
            public int Water { get; set; }

            public override string ToString()
            {
                return $"{base.ToString()} ({Id})";
            }
        }

        internal class Tanker : Unit
        {
            public int Water { get; set; }

            public override string ToString()
            {
                return $"{base.ToString()} ({Id})";
            }
        }

        internal static List<Unit> _units = new List<Unit>();
        internal static Unit _myReaper = null;
        internal static Unit _myDestroyer = null;
        internal static Unit _myDoof = null;

        private static void Main(string[] args)
        {
            // game loop
            while (true)
            {
                _units.Clear();
                _myReaper = null;
                _myDestroyer = null;
                _myDoof = null;
                int myScore = int.Parse(Console.ReadLine());
                int enemyScore1 = int.Parse(Console.ReadLine());
                int enemyScore2 = int.Parse(Console.ReadLine());
                int myRage = int.Parse(Console.ReadLine());
                int enemyRage1 = int.Parse(Console.ReadLine());
                int enemyRage2 = int.Parse(Console.ReadLine());
                int unitCount = int.Parse(Console.ReadLine());
                for (int i = 0; i < unitCount; i++)
                {
                    string[] inputs = Console.ReadLine().Split(' ');
                    int unitId = int.Parse(inputs[0]);
                    int unitType = int.Parse(inputs[1]);
                    int player = int.Parse(inputs[2]);
                    float mass = float.Parse(inputs[3]);
                    int radius = int.Parse(inputs[4]);
                    int x = int.Parse(inputs[5]);
                    int y = int.Parse(inputs[6]);
                    int vx = int.Parse(inputs[7]);
                    int vy = int.Parse(inputs[8]);
                    int extra = int.Parse(inputs[9]);
                    int extra2 = int.Parse(inputs[10]);

                    var unit = _units.FirstOrDefault(u => u.Id == unitId);
                    if (unit == null)
                    {
                        switch (unitType)
                        {
                            case (int)UnitType.Wreck:
                                unit = new Wreck
                                {
                                    Water = extra
                                };
                                break;

                            case (int)UnitType.Tanker:
                                unit = new Tanker
                                {
                                    Water = extra2
                                };
                                break;

                            default:
                                unit = new Unit();
                                break;
                        }

                        unit.Id = unitId;
                        unit.UnitType = (UnitType)unitType;
                        unit.Radius = radius;
                        unit.Mass = mass;
                        unit.Player = player;

                        _units.Add(unit);
                        if (_myReaper == null && player == 0 && unitType == (int)UnitType.Reaper) _myReaper = unit;
                        if (_myDestroyer == null && player == 0 && unitType == (int)UnitType.Destroyer) _myDestroyer = unit;
                        if (_myDoof == null && player == 0 && unitType == (int)UnitType.Doof) _myDoof = unit;
                    }
                    unit.Velocity = new Point(vx, vy);
                    unit.Position = new Point(x, y);
                    if (unit is Wreck) ((Wreck)unit).Water = extra;
                    if (unit is Tanker) ((Tanker)unit).Water = extra2;
                }

                int thrust = 300;
                string message = ".";
                var wreck = _units.OfType<Wreck>()
                                  //.OrderByDescending(w => w.Water)
                                  .OrderBy(w => w.Position.Distance2(_myReaper.Position) / w.Water)
                                  .FirstOrDefault();
                var tanker = _units.OfType<Tanker>()
                                   .OrderByDescending(t => t.Position.Distance2(_myDestroyer.Position) / t.Water)
                                   .FirstOrDefault();
                bool foundWreck = false;
                if (wreck != null)
                {
                    Console.WriteLine($"{wreck.Position.X - _myReaper.Velocity.X} {wreck.Position.Y - _myReaper.Velocity.Y} {thrust} {message}");
                    foundWreck = true;
                }

                string tankerMessage = "SMASH";

                var oilPools = _units.Where(o => o.UnitType == UnitType.Oil);
                var enemyTarget = _units.FirstOrDefault(u => u.Player == (enemyScore1 > enemyScore2 ? 1 : 2));
                var enemyWreck = _units.OfType<Wreck>()
                                       .FirstOrDefault(w => w.Position.Distance(enemyTarget.Position) < w.Radius);
                if (enemyWreck != null && oilPools.Any(o => o.Position.Distance(enemyWreck.Position) < o.Radius)) enemyWreck = null;

                //no wreck, go after same tanker as destroyer
                double v = 1d;
                if (!foundWreck)
                {
                    Console.WriteLine($"{enemyTarget.Position.X - _myReaper.Velocity.X * v} {enemyTarget.Position.Y - _myReaper.Velocity.Y * v} {thrust} troll");
                }

                if (myRage > 250 && enemyTarget.Position.Distance(_myDoof.Position) < 1000)
                {
                    Console.WriteLine($"SKILL {enemyTarget.Position.X + enemyTarget.Velocity.X} {enemyTarget.Position.Y + enemyTarget.Velocity.Y} FIRE");
                }
                else
                {
                    Console.WriteLine($"{tanker.Position.X - _myDestroyer.Velocity.X} {tanker.Position.Y - _myDestroyer.Velocity.Y} 300 {tankerMessage}");
                }

                if (enemyWreck == null || enemyWreck.Position.Distance(_myDoof.Position) > 2000)
                {
                    Console.WriteLine($"{enemyTarget.Position.X - _myDoof.Velocity.X} {enemyTarget.Position.Y - _myDoof.Velocity.Y} 300");
                }
                else
                {
                    Console.WriteLine($"SKILL {enemyTarget.Position.X} {enemyTarget.Position.Y} OIL");
                }
            }
        }
    }
}