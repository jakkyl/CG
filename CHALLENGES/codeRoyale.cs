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
namespace CodeRoyale
{
    enum StructureType { TOWER = 1, BARRACKS }
    enum UnitType { KNIGHT, ARCHER }

    internal class Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point() { }
        public Point(int x, int y)
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
            return $"{X} {Y}";
        }
    }

    internal class Site : Point
    {
        public Site(string[] inputs)
        {
            siteId = int.Parse(inputs[0]);
            X = int.Parse(inputs[1]);
            Y = int.Parse(inputs[2]);
            radius = int.Parse(inputs[3]);
        }

        public Site(int x, int y) : base(x, y)
        {
        }

        public void Train(params int[] siteIds)
        {
            Console.WriteLine($"TRAIN {string.Join(" ", siteIds)}");
        }

        public override string ToString()
        {
            return $"{siteId}, {this}";
        }

        public int siteId { get; private set; }
        public int radius { get; private set; }
        public int ignore1 { get; internal set; }
        public StructureType structureType { get; internal set; }
        public int owner { get; internal set; }
        public int param1 { get; internal set; }
    }

    class Unit : Point
    {
        public Unit(string[] inputs)
        {
            X = int.Parse(inputs[0]);
            Y = int.Parse(inputs[1]);

            owner = int.Parse(inputs[2]);
            unitType = int.Parse(inputs[3]); // -1 = QUEEN, 0 = KNIGHT, 1 = ARCHER
            health = int.Parse(inputs[4]);
        }

        public Unit(int x, int y) : base(x, y)
        {
        }

        public override string ToString()
        {
            return $"Type: {unitType}, Owner: {owner}";
        }

        public int owner { get; private set; }
        public int unitType { get; private set; }
        public int health { get; private set; }
    }

    internal class Queen : Unit
    {
        private string _action = "WAIT";

        public Queen(string[] inputs) : base(inputs)
        {
        }

        public void Move(Point dest)
        {
            _action = $"MOVE {dest.X} {dest.Y}";
        }

        public void Build(int siteId, StructureType structureType, UnitType unitType)
        {
            if (structureType == StructureType.TOWER)
                _action = $"BUILD {siteId } {structureType}";
            else
                _action = $"BUILD {siteId } {structureType}-{unitType}";
        }

        internal void PerformAction()
        {
            Console.WriteLine(_action);
        }
    }


    internal class Player
    {
        static void Main(string[] args)
        {
            var sites = new List<Site>();

            string[] inputs;
            int numSites = int.Parse(Console.ReadLine());
            for (int i = 0; i < numSites; i++)
            {
                sites.Add(new Site(Console.ReadLine().Split(' ')));

            }

            // game loop
            while (true)
            {
                var units = new List<Unit>();

                inputs = Console.ReadLine().Split(' ');
                int gold = int.Parse(inputs[0]);
                int touchedSite = int.Parse(inputs[1]); // -1 if none
                for (int i = 0; i < numSites; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int siteId = int.Parse(inputs[0]);
                    var site = sites.Find(s => s.siteId == siteId);

                    site.ignore1 = int.Parse(inputs[1]); // used in future leagues
                    site.ignore1 = int.Parse(inputs[2]); // used in future leagues
                    site.structureType = (StructureType)int.Parse(inputs[3]); // -1 = No structure, 2 = Barracks
                    site.owner = int.Parse(inputs[4]); // -1 = No structure, 0 = Friendly, 1 = Enemy
                    site.param1 = int.Parse(inputs[5]);
                    site.param1 = int.Parse(inputs[6]);
                }
                int numUnits = int.Parse(Console.ReadLine());
                for (int i = 0; i < numUnits; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    if (int.Parse(inputs[3]) == -1)
                    {
                        units.Add(new Queen(inputs));
                    }
                    else
                    {
                        units.Add(new Unit(inputs));
                    }
                }
                Console.Error.WriteLine(string.Join(",", units));
                var me = units.OfType<Queen>().First(q => q.owner == 0);

                var availableSites = sites.Where(s => s.owner == 0).Select(s => s.siteId);
                int numberToTrain = gold % (80 + Math.Max(1, availableSites.Count()));
                Console.Error.WriteLine(numberToTrain / 4);
                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");
                var towers = sites.Where(s => s.structureType == StructureType.TOWER && s.owner == 0);
                var barracks = sites.Where(s => s.structureType == StructureType.BARRACKS && s.owner == 0);
                var enemyTowers = sites.Where(s => s.structureType == StructureType.TOWER && s.owner == 1);
                var enemyBarracks = sites.Where(s => s.structureType == StructureType.BARRACKS && s.owner == 1);
                var closestSite = sites.Where(s => s.owner == -1).OrderBy(s => s.Distance(me)).First();
                var enemyUnits = units.Where(u => u.owner == 1);
                var myUnits = units.Where(u => u.owner == 0);
                if (numberToTrain > availableSites.Count() * 4)
                {
                    me.Build(closestSite.siteId, StructureType.BARRACKS, UnitType.KNIGHT);
                }
                else if (barracks.Count() < enemyBarracks.Count())
                {
                    me.Build(closestSite.siteId, StructureType.BARRACKS, UnitType.KNIGHT);
                }
                else if (towers.Count() <= enemyTowers.Count() && closestSite != null)
                {
                    me.Build(closestSite.siteId, StructureType.TOWER, UnitType.KNIGHT);
                }
                else if (towers.Any())
                {
                    //if (me.Distance(closestSite) < closestSite.radius - 30)
                    //{
                    me.Move(towers.First());
                    //}
                    //else
                    //{
                    //    me.Move(closestSite);
                    //}
                }

                // First line: A valid queen action
                // Second line: A set of training instructions
                me.PerformAction();

                Console.WriteLine($"TRAIN {string.Join(" ", availableSites.Take(numberToTrain))}".Trim());
            }
        }
    }
}