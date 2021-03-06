﻿using System;
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
        private const int PopulationSize = 1;

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

        private static List<Entity> _entities = new List<Entity>();
        private static int _round = -1;


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
                double dy = (targetPosition.y - this.y) * Math.Sqrt(3) * 0.5;
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

            public virtual Coord[] neighbors()
            {
                var n = new Coord[6];
                for (int i = 0; i < 6; i++)
                {
                    n[i] = neighbor(i);
                }
                return n;
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
                return (int)((Math.Abs(x - dst.x) + Math.Abs(y - dst.y) + Math.Abs(z - dst.z)) * 0.5);
            }

            public override string ToString()
            {
                return string.Format("{0} {1} {2}", x, y, z);
            }
        }

        public enum EntityType
        {
            SHIP, BARREL, MINE, CANNONBALL,
            EMPTY
        }

        public class Entity
        {
            public int UNIQUE_ENTITY_ID = 0;

            public int id;
            public EntityType type;
            public Coord position;
            public bool IsAlive { get; set; }

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

            public override string ToString()
            {
                return string.Format("{0} {1} {2}", id, position.y, position.x);
            }

            public string toPlayerstring(int arg1, int arg2, int arg3, int arg4)
            {
                return string.Format("{0} {1} {2} {3} {4} {5} {6} {7}", id, type.ToString(), position.x, position.y, arg1, arg2, arg3, arg4);
            }

            private object[] cache = new object[2];

            public virtual void Save()
            {
                cache[0] = position;
                cache[1] = IsAlive;
            }

            public virtual void Load()
            {
                position = (Coord)cache[0];
                IsAlive = (bool)cache[1];
            }

            public List<Entity> neighbors()
            {
                var ns = new List<Entity>();
                for (int i = 0; i < 6; i++)
                {
                    var neighbor = position.neighbor(i);
                    var ent = _entities.FirstOrDefault(e => e.position.equals(neighbor));
                    if (ent == null)
                    {
                        ent = new Entity(EntityType.EMPTY, neighbor.x, neighbor.y);
                    }
                    ns.Add(ent);
                }

                return ns;
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

            public override string ToString()
            {
                return string.Format("{0} {1} {2} {3} {4} {5} {6} {7}", id, position.y, position.x, srcY, srcX, initialRemainingTurns, remainingTurns, ownerEntityId);
            }

            public string toPlayerstring(int playerIdx)
            {
                return toPlayerstring(ownerEntityId, remainingTurns, 0, 0);
            }

            private int[] cache = new int[2];

            public override void Save()
            {
                cache[0] = initialRemainingTurns;
                cache[1] = remainingTurns;
            }

            public override void Load()
            {
                initialRemainingTurns = cache[0];
                remainingTurns = cache[1];
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

            public override string ToString()
            {
                return string.Format("{0} {1} {2} {3}", id, position.y, position.x, health);
            }

            public string toPlayerstring(int playerIdx)
            {
                return toPlayerstring(health, 0, 0, 0);
            }

            private int[] cache = new int[1];

            public override void Save()
            {
                cache[0] = health;
            }

            public override void Load()
            {
                health = cache[0];
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
            public DNA Dna;
            public Coord[] Targets;

            public Ship(int x, int y, int orientation, int owner)
                : base(EntityType.SHIP, x, y)
            {
                //super(EntityType.SHIP, x, y);
                this.orientation = orientation;
                this.speed = 0;
                this.health = INITIAL_SHIP_HEALTH;
                this.owner = owner;
            }

            public void ApplyMove(int index)
            {
                action = Dna.Genes[index];
                target = Targets[index];
            }

            public void ApplyMove(Action action, Coord target)
            {
                this.action = action;
                this.target = target;
            }



            private int[] cache = new int[4];

            public override void Save()
            {
                cache[0] = speed;
                cache[1] = health;
                cache[2] = mineCooldown;
                cache[3] = cannonCooldown;
            }

            public override void Load()
            {
                speed = cache[0];
                health = cache[1];
                mineCooldown = cache[2];
                cannonCooldown = cache[3];
            }

            public Ship Clone()
            {
                var ship = new Ship(position.x, position.y, orientation, owner)
                {
                    cannonCooldown = cannonCooldown,
                    mineCooldown = mineCooldown,
                    orientation = orientation,
                    health = health,
                    speed = speed
                };

                return ship;
            }

            public override string ToString()
            {
                return string.Format("{0} {1} {2}", id, position.y, position.x);
                //return string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11}", id, position.y, position.x, orientation, health, speed, action, bow().y, bow().x, stern().y,
                //        stern().x, " ;" + (message != null ? message : ""));
            }

            public string toPlayerstring(int playerIdx)
            {
                return toPlayerstring(orientation, speed, health, owner == playerIdx ? 1 : 0);
            }

            public void moveTo(int x, int y)
            {
                Coord currentPosition = this.position;
                Coord targetPosition = new Coord(x, y);

                if (currentPosition.equals(targetPosition)) { this.action = Action.SLOWER; return; }

                double targetAngle, angleStraight, anglePort, angleStarboard, centerAngle,
                anglePortCenter, angleStarboardCenter;

                switch (speed)
                {
                    case 2:
                        this.action = Action.SLOWER;
                        break;

                    case 1: // Suppose we've moved first
                        currentPosition = currentPosition.neighbor(orientation);
                        if (!currentPosition.isInsideMap())
                        {
                            this.action = Action.SLOWER;
                            break;
                        }

                        // Target reached at next turn
                        if (at(targetPosition))
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
                            if (distanceMin == null || distance < distanceMin || (distance == distanceMin && angleStarboard < anglePort - 0.5 && this.action == Action.PORT) ||
                                (distance == distanceMin && angleStarboard < angleStraight - 0.5 && this.action == Action.WAIT) ||
                                (distance == distanceMin && this.action == Action.PORT && angleStarboard == anglePort && angleStarboardCenter < anglePortCenter) ||
                                (distance == distanceMin && this.action == Action.PORT && angleStarboard == anglePort && angleStarboardCenter == anglePortCenter &&
                                (orientation == 1 || orientation == 4)))
                            {
                                distanceMin = distance;
                                this.action = Action.STARBOARD;
                            }
                        }
                        break;

                    case 0: // Rotate ship towards target
                        targetAngle = currentPosition.angle(targetPosition);
                        angleStraight = Math.Min(Math.Abs(orientation - targetAngle), 6 - Math.Abs(orientation - targetAngle));
                        anglePort = Math.Min(Math.Abs((orientation + 1) - targetAngle), Math.Abs((orientation - 5) - targetAngle));
                        angleStarboard = Math.Min(Math.Abs((orientation + 5) - targetAngle), Math.Abs((orientation - 1) - targetAngle));

                        centerAngle = currentPosition.angle(new Coord(MAP_WIDTH / 2, MAP_HEIGHT / 2));
                        anglePortCenter = Math.Min(Math.Abs((orientation + 1) - centerAngle),
                        Math.Abs((orientation - 5) - centerAngle)); angleStarboardCenter =
                        Math.Min(Math.Abs((orientation + 5) - centerAngle), Math.Abs((orientation - 1) - centerAngle));

                        Coord forwardPosition = currentPosition.neighbor(orientation);

                        this.action = Action.WAIT;

                        if (anglePort <= angleStarboard) { this.action = Action.PORT; }

                        if (angleStarboard < anglePort || angleStarboard == anglePort && angleStarboardCenter < anglePortCenter ||
                            angleStarboard == anglePort && angleStarboardCenter == anglePortCenter && (orientation == 1 || orientation == 4))
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
                return ships.Where(s => s != this).Any(s => newBowIntersect(s));
                //foreach (Ship other in ships)
                //{
                //    if (this != other && newBowIntersect(other))
                //    {
                //        return true;
                //    }
                //}
                //return false;
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
                return ships.Where(s => s != this).Any(s => newPositionsIntersect(s));
                //foreach (Ship other in ships)
                //{
                //    if (this != other && newPositionsIntersect(other))
                //    {
                //        return true;
                //    }
                //}
                //return false;
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

            public void PrintAction()
            {
                switch (this.action)
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

                    case Action.FIRE:
                        Console.WriteLine("FIRE {0} {1}", target.x, target.y);
                        break;

                    default:
                        Console.WriteLine("WAIT");
                        break;
                }
            }
        }

        public class Bot
        {
            public int id;
            public List<Ship> ships;
            public List<Ship> shipsAlive;

            public Bot(int id)
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
                    data.Add(ship.ToString());
                }

                return data;
            }
        }

        public static List<Cannonball> cannonballs = new List<Cannonball>();
        public static List<Mine> mines = new List<Mine>();
        public static List<Rum> barrels = new List<Rum>();
        public static List<Bot> players = new List<Bot>();
        public static List<Ship> ships = new List<Ship>();
        public static List<Damage> damage = new List<Damage>();
        public static List<Coord> cannonBallExplosions = new List<Coord>();
        public static int shipsPerPlayer;
        public static int mineCount;
        public static int barrelCount;

        public static void decrementRum()
        {
            foreach (Ship ship in ships)
            {
                ship.damage(1);
            }
        }

        public static void updateInitialRum()
        {
            foreach (Ship ship in ships)
            {
                ship.initialHealth = ship.health;
            }
        }

        public static void moveCannonballs()
        {
            cannonballs = cannonballs.Where(b => b.IsAlive).ToList();

            foreach (Cannonball ball in cannonballs)
            {
                //Cannonball ball = it.next();
                if (ball.remainingTurns == 0)
                {
                    cannonBallExplosions.Add(ball.position);
                }
                else if (ball.remainingTurns > 0)
                {
                    ball.remainingTurns--;
                }
            }
        }

        public static void applyActions()
        {
            foreach (Bot player in players)
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

        public static void checkCollisions(Ship ship)
        {
            Coord bow = ship.bow();
            Coord stern = ship.stern();
            Coord center = ship.position;

            // Collision with the barrels
            var barrelsAlive = barrels.Where(b => b.IsAlive).ToList();
            foreach (Rum barrel in barrelsAlive)
            {
                if (barrel.position.equals(bow) || barrel.position.equals(stern) || barrel.position.equals(center))
                {
                    ship.heal(barrel.health);
                    barrel.IsAlive = false;
                }
            }

            // Collision with the mines
            var minesAlive = mines.Where(b => b.IsAlive).ToList();
            foreach (Mine mine in minesAlive)
            {
                List<Damage> mineDamage = mine.explode(ships, false);

                if (mineDamage.Count() > 0)
                {
                    damage.AddRange(mineDamage);
                    mine.IsAlive = false;
                }
            }
        }

        public static void moveShips()
        {
            // --- Go forward ---
            for (int i = 1; i <= MAX_SHIP_SPEED; i++)
            {
                foreach (Bot player in players)
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

                foreach (Bot player in players)
                {
                    foreach (Ship ship in player.shipsAlive)
                    {
                        ship.position = ship.newPosition;
                        checkCollisions(ship);
                    }
                }
            }
        }

        public static void rotateShips()
        {
            // Rotate
            foreach (Bot player in players)
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
            foreach (Bot player in players)
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
            foreach (Bot player in players)
            {
                if (player.shipsAlive.Count() == 0)
                {
                    return true;
                }
            }
            return barrels.Count() == 0;
        }

        public static void explodeShips()
        {
            List<Coord> removedPositions = new List<Coord>();
            bool hit = false;
            foreach (Coord position in cannonBallExplosions)
            {
                foreach (Ship ship in ships)
                {
                    if (position.equals(ship.bow()) || position.equals(ship.stern()))
                    {
                        damage.Add(new Damage(position, LOW_DAMAGE, true));
                        ship.damage(LOW_DAMAGE);
                        removedPositions.Add(position);
                        hit = true;
                        break;
                    }
                    else if (position.equals(ship.position))
                    {
                        damage.Add(new Damage(position, HIGH_DAMAGE, true));
                        ship.damage(HIGH_DAMAGE);
                        removedPositions.Add(position);
                        hit = true;
                        break;
                    }
                }
                if (hit) continue;
                var minesAlive = mines.Where(m => m.IsAlive).ToList();
                foreach (Mine mine in minesAlive)
                {
                    if (mine.position.equals(position))
                    {
                        damage.AddRange(mine.explode(ships, true));
                        removedPositions.Add(position);

                        hit = true;
                        mine.IsAlive = false;
                        break;
                    }
                }
                if (hit) continue;

                var barrelsAlive = barrels.Where(b => b.IsAlive).ToList();
                foreach (Rum barrel in barrelsAlive)
                {
                    if (barrel.position.equals(position))
                    {
                        damage.Add(new Damage(position, 0, true));
                        removedPositions.Add(position);

                        hit = true;
                        barrel.IsAlive = false;
                        break;
                    }
                }
            }

            foreach (var position in removedPositions)
            {
                cannonBallExplosions.Remove(position);
            }
        }

        public static bool updateGame(int round)
        {
            moveCannonballs();
            decrementRum();
            //updateInitialRum();

            applyActions();
            moveShips();
            rotateShips();

            explodeShips();

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

                    //players.First(a => a.id == ship.owner).shipsAlive.Remove(ship);
                    ship.IsAlive = false;
                    Console.Error.WriteLine("SHIP DEAD {0}", ship.id);
                }
            }

            foreach (Coord position in cannonBallExplosions)
            {
                damage.Add(new Damage(position, 0, false));
            }

            return false;
        }




        public class DNA
        {
            private const int ActionLength = 5;
            public Action[] Genes { get; set; }
            public Coord[] Targets { get; set; }

            public DNA()
            {
                Genes = new Action[Lifetime];
                Targets = new Coord[Lifetime];
                Mutate(1.1f);
            }

            public DNA(Action[] genes, Coord[] targets)
            {
                Genes = genes;
                Targets = targets;
            }

            //public DNA Crossover(DNA partner)
            //{
            //    var child = new Action[Lifetime];
            //    int crossoverPoint = _rand.Next(Lifetime);
            //    for (int i = 0; i < Lifetime; i++)
            //    {
            //        if (i > crossoverPoint)
            //            child[i] = Genes[i];
            //        else
            //            child[i] = partner.Genes[i];
            //    }

            //    return new DNA(child);
            //}

            public void Mutate(float rate)
            {
                for (int i = 0; i < Lifetime; i++)
                {
                    if (MyRandom.Next(1) < rate)
                    {
                        var action = (Action)MyRandom.Next(ActionLength);
                        if (action == Action.FIRE)
                        {
                            action = Action.WAIT;
                            //Targets[i] = new Coord(_rand.Next(Width), _rand.Next(Height));
                        }
                        Genes[i] = action;
                    }
                }
            }

            internal void Shift()
            {
                for (int i = 1; i < Lifetime; i++)
                {
                    Genes[i - 1] = Genes[i];
                }
                //action[Lifetime-1]
            }
        }

        private static int g_seed = 42;

        internal class MyRandom
        {
            public static int fastrand()
            {
                g_seed = (214013 * g_seed + 2531011);
                return (g_seed >> 16) & 0x7FFF;
            }

            public static int Next(int b)
            {
                return fastrand() % b;
            }

            public static int Next(int a, int b)
            {
                return a + Next(b - a + 1);
            }
        }


        public static Dictionary<Entity, Entity> GetPossiblePath(Entity source, Entity target)
        {
            var openSet = new Queue<Entity>();
            openSet.Enqueue(source);

            var closed = new List<Entity>();
            var path = new Dictionary<Entity, Entity>
            {
                [source] = null
            };
            while (openSet.Count > 0)
            {
                var current = openSet.Dequeue();

                if (current == target) break;

                closed.Add(current);

                var neighbors = current.neighbors();
                foreach (var next in neighbors)
                {
                    if (path.ContainsKey(next) || next is Mine || next is Ship) continue;

                    openSet.Enqueue(next);
                    path[next] = current;
                }
            }

            Console.Error.WriteLine("PATH FULL {0}", string.Join(",", path));
            return path;
        }

        public static List<Coord> FindBestPath(Entity source, Entity target)
        {
            Console.Error.WriteLine("S: {0}, T: {1}", source, target);
            var fullPath = GetPossiblePath(source, target);
            var bestPath = new List<Coord>();
            var current = target;
            bestPath.Add(target.position);
            int max = 5;
            while (current != source && max > 0)
            {
                current = fullPath[current];
                bestPath.Add(current.position);
                max--;
            }
            Console.Error.WriteLine("BEST {0}", string.Join(",", bestPath));
            bestPath.Reverse();
            bestPath.Remove(source.position);

            return bestPath;
        }

        /// <summary>
        /// GA
        /// </summary>

        public static int generations = 0;

        public class Population
        {
            private float mutationRate = 1.0f;
            private int popSize = 0;
            public Ship[] Pop { get; set; }

            public Population(float mutationRate, int populationSize, Ship[] ships)
            {
                this.mutationRate = mutationRate;
                Pop = new Ship[populationSize];
                for (int i = 0; i < populationSize; i++)
                {
                    Pop = ships;
                    var d = new DNA();
                    Pop[i].Dna = d;
                    Pop[i].Targets = d.Targets;
                }
                popSize = populationSize;
            }

            internal double Fitness()
            {
                double fitness = 0;
                var myRum = players[0].shipsAlive.Sum(s => s.health);
                var enemyRum = players[1].shipsAlive.Sum(s => s.health);
                fitness = myRum - enemyRum;

                var distance = _entities.Sum(e => e.position.distanceTo(players[0].shipsAlive[0].position));
                fitness -= distance;

                var edgePenalty = players[0].shipsAlive.Any(s => s.position.x < 2 || s.position.x > Width - 2 || s.position.y < 2 || s.position.y > Height - 2);
                if (edgePenalty)
                    fitness -= 1000;
                //Console.Error.WriteLine("FITNESS: {0} {1} {2}", fitness, myRum, enemyRum);
                return fitness;
            }

            public double Live(DNA[] dna)
            {
                //for (int index = 0; index < PopulationSize; index++)
                //{
                //Pop[index].Run();
                for (int i = 0; i < Lifetime; i++)
                {
                    for (int j = 0; j < dna.Length; j++)
                    {
                        Pop[j].ApplyMove(dna[j].Genes[i], dna[j].Targets[i]);
                    }
                    var enemies = players[0].ships.Where(s => s.IsAlive).ToList();
                    var rum = _entities.OfType<Rum>().Where(b => b.IsAlive).ToList();
                    foreach (var enemy in enemies)
                    {
                        var closestRum = rum.OrderBy(b => b.position.distanceTo(enemy.position)).FirstOrDefault();
                        if (closestRum != null) enemy.moveTo(closestRum.position.x, closestRum.position.y);
                    }
                    updateGame(_round);
                }
                if (_round > 0) generations++;
                var fitness = Fitness();
                _entities.ForEach(e => e.Load());

                return fitness;
                //}
            }
        }

        private static Stopwatch stopwatch;

        private static void Main(string[] args)
        {
            players = new List<Bot>();
            var me = new Bot(1);
            var enemyPlayer = new Bot(0);
            players.Add(me);
            players.Add(enemyPlayer);
            Population pop = null;
            var maxFitness = double.MinValue;//pop.Live(new DNA(pop.Pop[0].Moves, pop.Pop[0].Targets));

            // game loop
            while (true)
            {
                _round++;
                _entities.Clear();
                me.shipsAlive.Clear();
                enemyPlayer.shipsAlive.Clear();
                barrels.Clear();
                mines.Clear();
                cannonballs.Clear();

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
                            Ship ship;
                            if (_round == 0)
                            {
                                ship = new Ship(x, y, arg1, arg4);
                                ship.id = entityId;
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
                                ships.Add(ship);
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
                            if (ship.cannonCooldown > 0) ship.cannonCooldown--;
                            if (ship.mineCooldown > 0) ship.mineCooldown--;
                            break;

                        case "BARREL":
                            ent = new Rum(x, y, arg1);
                            barrels.Add((Rum)ent);
                            break;

                        case "CANNONBALL":
                            var source = me.ships.Concat(enemyPlayer.ships).FirstOrDefault(s => s.id == arg1);
                            ent = new Cannonball(x, y, entityId, source.position.x, source.position.y, arg2);
                            cannonballs.Add((Cannonball)ent);
                            break;

                        case "MINE":
                            ent = new Mine(x, y);
                            mines.Add((Mine)ent);
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
                _entities.ForEach(e => e.Save());

                int timeLimit = _round == 0 ? 900 : 40;
                stopwatch = Stopwatch.StartNew();

                var moves = new List<Entity>();
                foreach (var ship in me.shipsAlive)
                {
                    var closestRum = barrels.Where(b => !moves.Contains(b)).OrderBy(b => b.position.distanceTo(ship.position));
                    var enemy = enemyPlayer.shipsAlive.OrderBy(s => Math.Min(ship.bow().distanceTo(s.position), ship.position.distanceTo(s.position))).FirstOrDefault();
                    var enemyDistance = enemy.position.distanceTo(ship.newBow());
                    var barrel = closestRum.FirstOrDefault();
                    var nextPos = ship.newPosition;
                    var closestCannon = _entities.OfType<Cannonball>().OrderBy(c => c.position.distanceTo(ship.position)).FirstOrDefault();
                    var closestMine = mines.OrderBy(m => m.position.distanceTo(ship.position)).FirstOrDefault();
                    var mineDistance = closestMine != null ? closestMine.position.distanceTo(ship.position) : int.MaxValue;

                    Console.Error.WriteLine("ENEMY DISTANCE {0}\nMINE DISTANCE {1}", enemyDistance, mineDistance);
                    if ((ship.speed > 0 || enemyDistance <= 1) && enemyDistance < 5 && ship.cannonCooldown > 0)
                    {
                        var eta = 1 + (enemyDistance / 3);
                        Coord enemyPos = enemy.position;
                        if (eta > 0)
                        {
                            enemyPos = enemy.newBow();
                        }
                        ship.fire(enemyPos.x, enemyPos.y);
                        ship.cannonCooldown = CannonCooldown;
                    }
                    else if (ship.speed > 0 && mineDistance < 5)
                    {
                        var eta = 1 + (mineDistance / 3);
                        ship.fire(closestMine.position.x, closestMine.position.y);
                    }
                    else if (barrel != null && ship.health < 90)
                    {
                        //var path = FindBestPath(ship, barrel);
                        //Console.WriteLine("MOVE {0} {1}", path[0].x, path[0].y);
                        ship.moveTo(barrel.position.x, barrel.position.y);

                        moves.Add(barrel);
                    }
                    else if (ship.speed > 0 && enemyDistance < 6 && enemy.orientation == ship.orientation)
                    {
                        ship.placeMine();
                    }
                    else
                    {
                        ship.moveTo(MyRandom.Next(Width - 2), MyRandom.Next(Height - 2));
                    }
                    ship.PrintAction();
                }
            }
        }
    }



}