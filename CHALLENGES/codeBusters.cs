using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Send your busters out into the fog to trap ghosts and bring them home!
 **/
namespace CodeBrusters
{

    class Player
    {
        private const int MapWidth = 16001;
        private const int MapHeight = 9001;
        private const int ReleaseArea = 1600;
        private const int MoveLength = 900;
        private const int BustRangeMin = 900;
        private const int BustRangeMax = 1760;

        private static MapState[,] graph = new MapState[MapWidth, MapHeight];

        enum BusterState
        {
            Idle = 0,
            Carrying = 1
        }

        enum MapState
        {
            Unexplored,
            Empty,
            Ghost,
            Buster,
            Base
        }

        class Point
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
        class Entity : Point
        {
            public int Id { get; set; }
            public int Value { get; set; }
            public int Radius { get; set; }

            public Entity(int id, double x, double y, int value)
                : base(x, y)
            {
                Id = id;
                Value = value;
            }

            public override string ToString()
            {
                return base.ToString() + Value;
            }
        }
        class Ghost : Entity
        {
            public Ghost(int id, double x, double y, int value) : base(id, x, y, value)
            {
            }
        }

        class Buster : Entity
        {
            public BusterState State { get; set; }
            public int Type { get; internal set; }

            public Buster(int id, int type, double x, double y, BusterState state, int value) : base(id, x, y, value)
            {
                State = state;
                Type = type;
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
                Console.Error.WriteLine("DTR: {0} {1}", Id, distanceToRelease);
                if (distanceToRelease <= ReleaseArea)
                    Console.WriteLine("RELEASE");
                else
                    Console.WriteLine(string.Format("MOVE {0} {1}", dX, dY)); // MOVE x y | BUST id | RELEASE
            }

            internal void Search()
            {
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
                if (Type == 0)
                {
                    var nextTiles = tiles.Where(t => t.Value == (int)MapState.Unexplored || t.Value != (int)MapState.Ghost).OrderBy(t => t.Distance(this));
                    Console.Error.WriteLine(string.Format("TILES: {0}", string.Join(";", nextTiles)));

                    if (nextTiles != null)
                    {
                        destX = nextTiles.First().X;
                        destY = nextTiles.First().Y;
                    }
                    else
                    {
                        destX = Math.Min(0, X + MoveLength);
                        destY = Math.Min(MapHeight, Y + MoveLength);
                    }
                }
                else
                {
                    destX = Math.Min(0, X - MoveLength);
                    destY = Math.Min(0, Y - MoveLength);
                }

                Console.WriteLine(string.Format("MOVE {0} {1}", destX, destY));
            }
        }

        private static IList<Entity> activeEntities = new List<Entity>();
        private static IList<Entity> tiles = new List<Entity>();

        static void Main(string[] args)
        {
            int bustersPerPlayer = int.Parse(Console.ReadLine()); // the amount of busters you control
            int ghostCount = int.Parse(Console.ReadLine()); // the amount of ghosts on the map
            int myTeamId = int.Parse(Console.ReadLine()); // if this is 0, your base is on the top left of the map, if it is one, on the bottom right

            for (int i = 0; i < MapWidth; i += MoveLength)
            {
                for (int j = 0; j < MapHeight; j += MoveLength)
                {
                    var state = MapState.Unexplored;
                    if ((i < ReleaseArea && j < ReleaseArea) || (i > MapWidth - ReleaseArea || j > MapWidth - ReleaseArea))
                        state = MapState.Base;
                    else
                        state = MapState.Unexplored;
                    var tile = new Entity(-1, i, j, (int)state);
                    tile.Radius = 600;
                    tiles.Add(tile);
                }
            }

            // game loop
            while (true)
            {
                //Console.Error.WriteLine(string.Format("TILES: {0}", string.Join(";",tiles.Select(t=> t.X+","+t.Y).ToArray())));

                activeEntities.Clear();

                int entities = int.Parse(Console.ReadLine()); // the number of busters and ghosts visible to you
                for (int i = 0; i < entities; i++)
                {
                    string[] inputs = Console.ReadLine().Split(' ');
                    int entityId = int.Parse(inputs[0]); // buster id or ghost id
                    int x = int.Parse(inputs[1]);
                    int y = int.Parse(inputs[2]); // position of this buster / ghost
                    int entityType = int.Parse(inputs[3]); // the team id if it is a buster, -1 if it is a ghost.
                    BusterState state = (BusterState)Enum.Parse(typeof(BusterState), inputs[4]); // For busters: 0=idle, 1=carrying a ghost.
                    int value = int.Parse(inputs[5]); // For busters: Ghost id being carried. For ghosts: number of busters attempting to trap this ghost.
                    if (entityType == -1)
                    {
                        var ghost = new Ghost(entityId, x, y, value);
                        activeEntities.Add(ghost);
                        foreach (var tile in tiles.Where(t => t.Distance(ghost) < t.Radius))
                        {
                            tile.Value = (int)MapState.Ghost;
                        }
                    }
                    else
                    {
                        var buster = new Buster(entityId, entityType, x, y, state, value);
                        activeEntities.Add(buster);
                        foreach(var tile in  tiles.Where(t => t.Distance(buster) < t.Radius))
                            tile.Value = (int)MapState.Buster;
                    }
                }

                foreach (var buster in activeEntities.OfType<Buster>().Where(e => e.Type == myTeamId))
                {
                    Console.Error.WriteLine(string.Format("BusterState: {0} {1}", buster.Id, buster.State));
                    switch (buster.State)
                    {
                        case BusterState.Carrying:
                            buster.ReturnToBase();
                            break;
                        case BusterState.Idle:
                            buster.Search();
                            break;
                    }
                }
            }
        }
    }
}