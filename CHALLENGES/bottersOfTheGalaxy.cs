using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Made with love by AntiSquid, Illedan and Wildum.
 * You can help children learn to code while you participate by donating to CoderDojo.
 **/
namespace BottersOfTheGalaxy
{
    internal class Point
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
            return string.Format("{0} {1}", X, Y);
        }
    }

    internal class Unit : Point
    {
        public Unit(double x, double y) : base(x, y)
        {
        }

        public int Id { get; internal set; }
        public int HP { get; internal set; }
        public bool TakingDamage { get; internal set; }
        public int AttackRange { get; internal set; }
        public int MaxHP { get; internal set; }
    }

    internal class Hero : Unit
    {
        public Hero(double x, double y) : base(x, y)
        {
        }

        public int Gold { get; internal set; }
        public int LastHp { get; internal set; }
    }

    internal class Item
    {
        public string itemName;// contains keywords such as BRONZE, SILVER and BLADE, BOOTS connected by "_" to help you sort easier
        public int itemCost; // BRONZE items have lowest cost, the most expensive items are LEGENDARY
        public int damage; // keyword BLADE is present if the most important item stat is damage
        public int health;
        public int maxHealth;
        public int mana;
        public int maxMana;
        public int moveSpeed; // keyword BOOTS is present if the most important item stat is moveSpeed

        public int isPotion; // 0 if it's not instantly consumed

        public override string ToString()
        {
            return $"{itemName}: {itemCost}, {health}";
        }
    }
    internal class Player
    {
        private static void Main(string[] args)
        {
            var myUnits = new List<Unit>();
            var enemyUnits = new List<Unit>();
            var myHeroes = new List<Hero>();
            var villains = new List<Hero>();
            var myTower = new Unit(0, 0);
            var enemyTower = new Unit(0, 0);


            var items = new List<Item>();

            string[] inputs;
            int myTeam = int.Parse(Console.ReadLine());
            int bushAndSpawnPointCount = int.Parse(Console.ReadLine()); // usefrul from wood1, represents the number of bushes and the number of places where neutral units can spawn
            for (int i = 0; i < bushAndSpawnPointCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                string entityType = inputs[0]; // BUSH, from wood1 it can also be SPAWN
                int x = int.Parse(inputs[1]);
                int y = int.Parse(inputs[2]);
                int radius = int.Parse(inputs[3]);
            }
            int itemCount = int.Parse(Console.ReadLine()); // useful from wood2
            for (int i = 0; i < itemCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                string itemName = inputs[0]; // contains keywords such as BRONZE, SILVER and BLADE, BOOTS connected by "_" to help you sort easier
                int itemCost = int.Parse(inputs[1]); // BRONZE items have lowest cost, the most expensive items are LEGENDARY
                int damage = int.Parse(inputs[2]); // keyword BLADE is present if the most important item stat is damage
                int health = int.Parse(inputs[3]);
                int maxHealth = int.Parse(inputs[4]);
                int mana = int.Parse(inputs[5]);
                int maxMana = int.Parse(inputs[6]);
                int moveSpeed = int.Parse(inputs[7]); // keyword BOOTS is present if the most important item stat is moveSpeed
                int manaRegeneration = int.Parse(inputs[8]);
                int isPotion = int.Parse(inputs[9]); // 0 if it's not instantly consumed

                items.Add(new Item
                {
                    itemName = itemName,
                    itemCost = itemCost,
                    damage = damage,
                    health = health,
                    maxHealth = maxHealth,
                    mana = mana,
                    maxMana = maxMana,
                    moveSpeed = moveSpeed,
                    isPotion = isPotion
                });
            }

            Console.Error.WriteLine(string.Join("\n", items));

            // game loop
            while (true)
            {
                myUnits.Clear();
                enemyUnits.Clear();

                int gold = int.Parse(Console.ReadLine());
                int enemyGold = int.Parse(Console.ReadLine());
                int roundType = int.Parse(Console.ReadLine()); // a positive value will show the number of heroes that await a command
                int entityCount = int.Parse(Console.ReadLine());
                for (int i = 0; i < entityCount; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int unitId = int.Parse(inputs[0]);
                    int team = int.Parse(inputs[1]);
                    string unitType = inputs[2]; // UNIT, HERO, TOWER, can also be GROOT from wood1
                    int x = int.Parse(inputs[3]);
                    int y = int.Parse(inputs[4]);
                    int attackRange = int.Parse(inputs[5]);
                    int health = int.Parse(inputs[6]);
                    int maxHealth = int.Parse(inputs[7]);
                    int shield = int.Parse(inputs[8]); // useful in bronze
                    int attackDamage = int.Parse(inputs[9]);
                    int movementSpeed = int.Parse(inputs[10]);
                    int stunDuration = int.Parse(inputs[11]); // useful in bronze
                    int goldValue = int.Parse(inputs[12]);
                    int countDown1 = int.Parse(inputs[13]); // all countDown and mana variables are useful starting in bronze
                    int countDown2 = int.Parse(inputs[14]);
                    int countDown3 = int.Parse(inputs[15]);
                    int mana = int.Parse(inputs[16]);
                    int maxMana = int.Parse(inputs[17]);
                    int manaRegeneration = int.Parse(inputs[18]);
                    string heroType = inputs[19]; // DEADPOOL, VALKYRIE, DOCTOR_STRANGE, HULK, IRONMAN
                    int isVisible = int.Parse(inputs[20]); // 0 if it isn't
                    int itemsOwned = int.Parse(inputs[21]); // useful from wood1

                    if (unitType == "TOWER")
                    {
                        var unit = new Unit(x, y)
                        {
                            Id = unitId,
                            HP = health,
                            AttackRange = attackRange,
                            MaxHP = maxHealth
                        };

                        if (team == myTeam)
                        {
                            myTower = unit;
                        }
                        else
                        {
                            enemyTower = unit;
                        }
                    }
                    if (unitType == "UNIT")
                    {
                        var unit = new Unit(x, y)
                        {
                            Id = unitId,
                            HP = health,
                            AttackRange = attackRange,
                            MaxHP = maxHealth
                        };

                        if (team == myTeam)
                        {
                            myUnits.Add(unit);
                        }
                        else
                        {
                            enemyUnits.Add(unit);
                        }
                    }
                    else if (unitType == "HERO")
                    {
                        Hero hero = myHeroes.FirstOrDefault(h => h.Id == unitId);
                        if (hero == null) villains.FirstOrDefault(h => h.Id == unitId);
                        if (hero == null) hero = new Hero(x, y);
                        hero.X = x;
                        hero.Y = y;
                        hero.Id = unitId;
                        hero.HP = health;
                        hero.AttackRange = attackRange;
                        hero.MaxHP = maxHealth;
                        hero.Gold = gold;

                        hero.TakingDamage = hero.HP < hero.LastHp;
                        hero.LastHp = hero.HP;

                        if (team == myTeam)
                        {
                            if (!myHeroes.Contains(hero))
                            {
                                myHeroes.Add(hero);
                            }
                        }
                        else
                        {
                            if (!villains.Contains(hero))
                            {
                                villains.Add(hero);
                            }
                        }
                    }
                }

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");


                // If roundType has a negative value then you need to output a Hero name, such as "DEADPOOL" or "VALKYRIE".
                // Else you need to output roundType number of any valid action, such as "WAIT" or "ATTACK unitId"
                if (roundType == -2)
                {
                    Console.WriteLine("IRONMAN");
                }
                else if (roundType == -1)
                {
                    Console.WriteLine("VALKYRIE");
                }
                else
                {
                    foreach (var myHero in myHeroes)
                    {
                        if (myUnits.Count == 0)
                        {
                            Console.WriteLine($"MOVE {myTower}");
                            continue;
                        }

                        if (myHero.HP < myHero.MaxHP * .5 && items.Any(i => i.itemCost < myHero.Gold && i.health > 0 && i.itemName.Contains("potion")))
                        {
                            var item = items.FirstOrDefault(i => i.itemCost < myHero.Gold && i.health > 0 && i.itemName.Contains("potion"));
                            Console.Error.WriteLine("NEED A POT!");
                            Console.WriteLine($"BUY {item.itemName}");
                            continue;
                        }
                        else if (myHero.HP < myHero.MaxHP * .2)
                        {
                            Console.Error.WriteLine("RUN AWAY!");
                            Console.WriteLine($"MOVE {myTower}");
                            continue;
                        }

                        var backLine = myTeam == 0 ? myUnits.Min(u => u.X) : myUnits.Max(u => u.X);
                        var frontLine = myTeam == 0 ? myUnits.Max(u => u.X) : myUnits.Min(u => u.X);
                        var lastUnit = myUnits.FirstOrDefault(u => u.X == backLine);
                        //Console.Error.WriteLine($"MINX: {minX}\n" +
                        //                        $"HEROX: {myHero.X}");
                        var unitsInRange = myUnits.Any(u => u.Distance(myHero) < 100);
                        var enemiesInRange = enemyUnits.Where(u => u.Distance(myHero) <= myHero.AttackRange).OrderBy(u => u.HP);
                        if (myHero.TakingDamage)
                        {
                            Console.Error.WriteLine($"OUCH - TOWER {myTower}");
                            Console.WriteLine($"MOVE {myTower}");
                        }
                        else if (!unitsInRange)
                        {
                            Console.Error.WriteLine("Grouping Up");
                            Console.WriteLine($"MOVE {backLine} {myHero.Y}");
                        }
                        else if (enemyUnits.Any(u => myUnits.Any(m => u.Distance(m) < m.AttackRange)))
                        {
                            if (enemyTower.Distance(myHero) <= myHero.AttackRange)
                            {
                                Console.Error.WriteLine("ATTACK THE TOWER!");
                                Console.WriteLine("ATTACK_NEAREST TOWER");
                            }
                            else if (enemyUnits.Count > 0)
                            {
                                if (enemiesInRange.Any())
                                {
                                    var closest = enemiesInRange.First();
                                    Console.Error.WriteLine("LAST HITTING");
                                    Console.WriteLine($"ATTACK {closest.Id}");
                                }
                                else
                                {
                                    Console.Error.WriteLine("Attack Nearest");
                                    Console.WriteLine("ATTACK_NEAREST UNIT");
                                }
                            }
                            else
                            {
                                Console.Error.WriteLine("NO enemies");
                                Console.WriteLine("WAIT");
                            }
                        }
                        else
                        {
                            Console.Error.WriteLine("Fail Safe");
                            Console.WriteLine("WAIT");
                        }
                    }
                }
            }
        }
    }
}