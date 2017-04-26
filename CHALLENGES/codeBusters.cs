using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Send your busters out into the fog to trap ghosts and bring them home!
 **/

namespace CodeBuster
{
    internal class Player
    {
        private const int MapWidth = 16001;
        private const int MapHeight = 9001;
        private const int ReleaseArea = 1600;
        private const int MoveLength = 900;
        private const int BustRangeMin = 900;
        private const int BustRangeMax = 1760;
        private const int StunCooldown = 20;

        private static MapState[,] graph = new MapState[MapWidth, MapHeight];

        private enum BusterState
        {
            Idle = 0,
            Carrying,
            Stunned,
            Trapping
        }

        private enum MapState
        {
            Unexplored,
            Empty,
            Ghost,
            Buster,
            Base
        }

        private enum Job
        {
            Attack,
            Capture,
            Roam
        }

        private class Point
        {
            public double X { get; set; }
            public double Y { get; set; }

            public Point(double x, double y)
            {
                X = x;
                Y = y;
            }

            public double Distance(Point p)
            {
                return Math.Sqrt(Math.Pow(p.X - X, 2) + Math.Pow(p.Y - Y, 2));
            }

            public override string ToString()
            {
                return string.Format("[{0},{1}]", X, Y);
            }
        }

        private class Entity : Point
        {
            public int Id { get; set; }
            public int Value { get; set; }
            public int Radius { get; set; }
            public int Type { get; internal set; }
            public BusterState State { get; set; }

            public Entity(int id, double x, double y, int type, int value, BusterState state)
                : base(x, y)
            {
                Id = id;
                Value = value;
                Type = type;
                State = state;
            }

            public override string ToString()
            {
                return string.Format("[Id: {0}, P: {1}, V:{2}, T: {3}]", Id, base.ToString(), Value, Type);
            }
        }

        private class Ghost : Entity
        {
            public Ghost(int id, double x, double y, int type, int value, BusterState state)
                : base(id, x, y, type, value, state)
            { }
        }

        private class Buster : Entity
        {
            public Job Job { get; set; }
            public int StunTimer { get; set; }

            public Buster(int id, int type, double x, double y, BusterState state, int value)
                : base(id, x, y, type, value, state)
            {
                StunTimer = 0;
            }

            internal void ReturnToBase()
            {
                int dX, dY = 0;
                if (Type == 0)
                    dX = dY = 0;
                else
                {
                    dX = MapWidth;
                    dY = MapHeight;
                }

                var distanceToRelease = Distance(new Point(dX, dY));
                if (distanceToRelease <= ReleaseArea)
                {
                    Console.WriteLine("RELEASE");
                    var ghost = activeEntities.OfType<Ghost>().FirstOrDefault(g => g.Id == Value);
                    entitiesToRemove.Add(ghost);
                }
                else
                    Console.WriteLine(string.Format("MOVE {0} {1}", dX, dY)); // MOVE x y | BUST id | RELEASE
            }

            internal void Search()
            {
                if (StunTimer == 0)
                {
                    var busters = activeEntities.OfType<Buster>()
                                                .Where(b => b.Type != myTeamId && Distance(b) <= BustRangeMax);
                    var buster = busters.FirstOrDefault();
                    if (buster != null && buster.State != BusterState.Stunned)
                    {
                        StunTimer = StunCooldown;
                        Console.WriteLine(string.Format("STUN {0}", buster.Id));
                        return;
                    }
                }

                var closestGhosts = activeEntities.OfType<Ghost>().OrderBy(e => Distance(e));
                foreach (var ghost in closestGhosts)
                {
                    var distance = Distance(ghost);
                    if (distance < BustRangeMax && distance > BustRangeMin)
                    {
                        Console.WriteLine(string.Format("BUST {0}", ghost.Id));
                        return;
                    }
                    else
                    {
                        Console.WriteLine(string.Format("MOVE {0} {1}", ghost.X, ghost.Y));
                        return;
                    }
                }

                double destX = 0;
                double destY = 0;
                var nextTiles = tiles.Where(t => t.Value == (int)MapState.Unexplored || t.Value == (int)MapState.Ghost)
                                     .OrderBy(t => t.Distance(this)).FirstOrDefault();
                //Console.Error.WriteLine(string.Format("TILES: {0}", string.Join(";", nextTiles)));

                if (nextTiles != null)
                {
                    destX = nextTiles.X;
                    destY = nextTiles.Y;
                    //nextTiles.Value =  (int)MapState.Empty;
                }
                else
                {
                    if (Type == 0)
                    {
                        destX = Math.Max(0, X - MoveLength);
                        destY = Math.Max(0, Y - MoveLength);
                    }
                    else
                    {
                        destX = Math.Min(MapWidth, X + MoveLength);
                        destY = Math.Min(MapHeight, Y + MoveLength);
                    }
                }

                Console.WriteLine(string.Format("MOVE {0} {1}", destX, destY));
            }
        }

        private static List<Entity> activeEntities = new List<Entity>();
        private static List<Entity> tiles = new List<Entity>();
        private static List<Ghost> entitiesToRemove = new List<Ghost>();
        private static int myTeamId;

        private static void Main(string[] args)
        {
            int bustersPerPlayer = int.Parse(Console.ReadLine()); // the amount of busters you control
            int ghostCount = int.Parse(Console.ReadLine()); // the amount of ghosts on the map
            myTeamId = int.Parse(Console.ReadLine()); // if this is 0, your base is on the top left of the map, if it is one, on the bottom right

            for (int i = BustRangeMax; i < MapWidth; i += BustRangeMax)
            {
                for (int j = BustRangeMax; j < MapHeight; j += BustRangeMax)
                {
                    var state = MapState.Unexplored;
                    if ((i < ReleaseArea && j < ReleaseArea) || (i > MapWidth - ReleaseArea || j > MapWidth - ReleaseArea))
                        state = MapState.Base;
                    else
                        state = MapState.Unexplored;
                    var tile = new Entity(-1, i, j, -9, (int)state, BusterState.Idle);
                    tile.Radius = 900;
                    tiles.Add(tile);
                }
            }

            // game loop
            while (true)
            {
                int myCount = 0;
                int enemyCount = 0;
                //Console.Error.WriteLine(string.Format("TILES: {0}", string.Join(";",tiles.Select(t=> t.X+","+t.Y).ToArray())));

                int entities = int.Parse(Console.ReadLine()); // the number of busters and ghosts visible to you
                for (int i = 0; i < entities; i++)
                {
                    string[] inputs = Console.ReadLine().Split(' ');
                    int entityId = int.Parse(inputs[0]); // buster id or ghost id
                    int x = int.Parse(inputs[1]);
                    int y = int.Parse(inputs[2]); // position of this buster / ghost
                    int entityType = int.Parse(inputs[3]); // the team id if it is a buster, -1 if it is a ghost.
                    BusterState state = (BusterState)Enum.Parse(typeof(BusterState), inputs[4]); // For busters: 0=idle, 1=carrying a ghost.
                    int val = int.Parse(inputs[5]); // For busters: Ghost id being carried. For ghosts: number of busters attempting to trap this ghost.
                    if (entityType == -1)
                    {
                        var ghost = activeEntities.OfType<Ghost>().FirstOrDefault(e => e.Id == entityId);
                        if (ghost == null)
                        {
                            ghost = new Ghost(entityId, x, y, entityType, val, state);
                            activeEntities.Add(ghost);
                        }
                        else
                        {
                            ghost.X = x;
                            ghost.Y = y;
                            ghost.Value = val;
                            ghost.State = state;
                        }

                        foreach (var tile in tiles.Where(t => t.Distance(ghost) < t.Radius))
                        {
                            tile.Value = (int)MapState.Ghost;
                        }
                    }
                    else
                    {
                        var buster = activeEntities.OfType<Buster>().FirstOrDefault(e => e.Id == entityId);
                        if (buster == null)
                        {
                            buster = new Buster(entityId, entityType, x, y, state, val);
                            activeEntities.Add(buster);
                        }
                        else
                        {
                            if (buster.StunTimer > 0) buster.StunTimer--;
                            buster.X = x;
                            buster.Y = y;
                            buster.State = state;
                            buster.Value = val;
                        }

                        foreach (var tile in tiles.Where(t => t.Distance(buster) < t.Radius))
                            tile.Value = (int)MapState.Buster;
                    }
                }

                var myBusters = activeEntities.OfType<Buster>().Where(b => b.Type == myTeamId);
                foreach (var buster in myBusters)
                {
                    Console.Error.WriteLine(string.Format("BusterState: {0} {1}", buster.Id, buster.State));
                    switch (buster.State)
                    {
                        case BusterState.Carrying:
                            buster.ReturnToBase();
                            break;

                        case BusterState.Idle:
                        case BusterState.Stunned:
                            buster.Search();
                            break;

                        case BusterState.Trapping:
                            Console.WriteLine("BUST " + buster.Value);
                            break;
                    }
                }

                activeEntities.RemoveAll(e => e.Type == -1);
            }
        }
    }
}