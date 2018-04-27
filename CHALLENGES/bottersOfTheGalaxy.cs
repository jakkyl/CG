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



    internal static class Player
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
            public int MovementSpeed { get; internal set; }
            public int AttackDamage { get; internal set; }
        }

        internal class Hero : Unit
        {
            public Hero(double x, double y) : base(x, y)
            {
            }

            public int Gold { get; internal set; }
            public int LastHp { get; internal set; }
            public int ItemsOwned { get; internal set; }
            public string Type { get; internal set; }
            public int CountDown1 { get; internal set; }
            public int CountDown2 { get; internal set; }
            public int CountDown3 { get; internal set; }
            public int Mana { get; internal set; }

            internal void GoHome()
            {
                Console.WriteLine($"MOVE {_myTower}");
            }

            internal void Attack(Unit unit)
            {
                Console.WriteLine($"ATTACK {unit.Id}");
            }

            internal bool WithinRange(Unit u)
            {
                return u.Distance(this) <= AttackRange;
            }
        }

        internal class Groot : Unit
        {
            public Groot(double x, double y) : base(x, y)
            {
            }
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

        internal static Unit _myTower = new Unit(0, 0);

        private static void Main(string[] args)
        {
            var myUnits = new List<Unit>();
            var enemyUnits = new List<Unit>();
            var myHeroes = new List<Hero>();
            var villains = new List<Hero>();
            var enemyTower = new Unit(0, 0);
            var bushes = new List<Unit>();
            var groots = new List<Groot>();
            var items = new List<Item>();

            string[] inputs;
            int myTeam = int.Parse(Console.ReadLine());
            int enemyTeam = myTeam == 0 ? 1 : 0;
            int bushAndSpawnPointCount = int.Parse(Console.ReadLine()); // usefrul from wood1, represents the number of bushes and the number of places where neutral units can spawn
            for (int i = 0; i < bushAndSpawnPointCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                string entityType = inputs[0]; // BUSH, from wood1 it can also be SPAWN
                int x = int.Parse(inputs[1]);
                int y = int.Parse(inputs[2]);
                int radius = int.Parse(inputs[3]);
                bushes.Add(new Unit(x, y));
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
                groots.Clear();

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
                            AttackDamage = attackDamage,
                            MaxHP = maxHealth
                        };

                        if (team == myTeam)
                        {
                            _myTower = unit;
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
                            AttackDamage = attackDamage,
                            MaxHP = maxHealth,
                            MovementSpeed = movementSpeed
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
                    else if (unitType == "GROOT")
                    {
                        var unit = new Groot(x, y)
                        {
                            Id = unitId,
                            HP = health,
                            AttackRange = attackRange,
                            AttackDamage = attackDamage,
                            MaxHP = maxHealth
                        };

                        groots.Add(unit);
                    }
                    else if (unitType == "HERO")
                    {
                        Hero hero = myHeroes.Find(h => h.Id == unitId);
                        if (hero == null) villains.Find(h => h.Id == unitId);
                        if (hero == null) hero = new Hero(x, y);
                        hero.Type = heroType;
                        hero.X = x;
                        hero.Y = y;
                        hero.Id = unitId;
                        hero.HP = health;
                        hero.Mana = mana;
                        hero.AttackRange = attackRange;
                        hero.AttackDamage = attackDamage;
                        hero.MaxHP = maxHealth;
                        hero.Gold = gold;
                        hero.ItemsOwned = itemsOwned;
                        hero.TakingDamage = hero.HP < hero.LastHp;
                        hero.LastHp = hero.HP;
                        hero.CountDown1 = countDown1;
                        hero.CountDown2 = countDown2;
                        hero.CountDown3 = countDown3;
                        hero.MovementSpeed = movementSpeed;

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


                // If roundType has a negative value then you need to output a Hero name, such as "DEADPOOL" or "VALKYRIE".
                // Else you need to output roundType number of any valid action, such as "WAIT" or "ATTACK unitId"
                if (roundType == -2)
                {
                    Console.WriteLine("IRONMAN");
                }
                else if (roundType == -1)
                {
                    Console.WriteLine("HULK");
                }
                else
                {
                    foreach (var myHero in myHeroes)
                    {
                        if (myUnits.Count == 0)
                        {
                            Console.WriteLine($"MOVE {_myTower}");
                            continue;
                        }

                        int hpMissing = myHero.MaxHP - myHero.HP;
                        bool heroNeeds = myHeroes.OrderBy(h => h.HP / (double)(h.MaxHP)).First().Id == myHero.Id;
                        Console.Error.WriteLine($"{myHero.Id} {heroNeeds} {string.Join(",", myHeroes.Select(h => new { h.Id, h.HP, h.MaxHP }).OrderBy(h => (double)(h.HP / h.MaxHP)))}");

                        Console.Error.WriteLine($"{myHero.Id} {heroNeeds} {string.Join(",", myHeroes.Select(h => h.HP / (double)(h.MaxHP)))}");
                        var availablePotions = items.Where(i => i.itemCost <= myHero.Gold && i.health > 0 && i.isPotion == 1);
                        if (heroNeeds && hpMissing > 100 && availablePotions.Any())
                        {
                            Item item;
                            if (hpMissing >= 500 && availablePotions.Any(i => i.health == 500))
                            {
                                item = availablePotions.First(i => i.health == 500);
                            }
                            else
                            {
                                item = availablePotions.FirstOrDefault(i => i.itemCost < myHero.Gold);
                            }

                            if (item != null)
                            {
                                Console.Error.WriteLine("NEED A POT!");
                                Console.WriteLine($"BUY {item.itemName}; CHACHING");
                                myHeroes.ForEach(h => h.Gold -= item.itemCost);
                                continue;
                            }
                        }
                        else if (myHero.HP < myHero.MaxHP * .2)
                        {
                            Console.Error.WriteLine("RUN AWAY!");
                            var enemiesInRange = enemyUnits.Where(u => u.Distance(myHero) <= myHero.AttackRange);
                            var closestBush = bushes.OrderBy(b => b.Distance(_myTower)).FirstOrDefault();
                            if (myHero as Point == _myTower as Point && enemiesInRange.Any())
                            {
                                myHero.Attack(enemiesInRange.First());
                            }
                            else if (myHero.TakingDamage)
                            {
                                var closestEnemy = enemiesInRange.OrderBy(u => u.Distance(myHero)).FirstOrDefault();
                                if (closestEnemy != null)
                                {
                                    myHero.Attack(closestEnemy);
                                }
                                else
                                {
                                    var groot = groots.Where(g => myHero.WithinRange(g)).FirstOrDefault();
                                    if (groot != null)
                                    {
                                        myHero.Attack(groot);
                                    }
                                    else
                                    {
                                        myHero.GoHome();
                                    }
                                }
                            }
                            else if (closestBush.Distance(myHero) < _myTower.Distance(myHero))
                            {
                                Console.WriteLine($"MOVE {closestBush}");
                            }
                            else
                            {
                                myHero.GoHome();
                            }
                            continue;
                        }

                        var allEnemies = enemyUnits.Union(villains);
                        var backLine = myTeam == 0 ? myUnits.Min(u => u.X) : myUnits.Max(u => u.X);
                        var frontLine = myTeam == 0 ? myUnits.Max(u => u.X) : myUnits.Min(u => u.X);
                        var lastUnit = myUnits.FirstOrDefault(u => u.X == backLine);
                        var unitsInRange = myUnits.Any(u => u.Distance(myHero) < 100);
                        var bestItem = items.Where(i => i.itemCost < myHero.Gold && i.itemCost > 200 && i.isPotion != 1).OrderByDescending(i => i.damage * 50 + i.health).FirstOrDefault();
                        var unitsEngaged = enemyUnits.Any(enemy => myUnits.Any(me => enemy.Distance(me) <= enemy.AttackRange) || _myTower.Distance(enemy) <= enemy.AttackRange);
                        var farmableGroots = groots.Where(g => (!enemyUnits.Any(u => OnMySide(u, myTeam)) && OnMySide(g, myTeam)) || myUnits.Any(u => OnMySide(u, enemyTeam)))
                                                   .OrderBy(g => g.Distance(_myTower));
                        /*if (myHero.TakingDamage)
                        {
                            if (myHero as Point == myTower as Point)
                            {
                                Console.Error.WriteLine($"BACKED TO CORNER");
                                Console.WriteLine($"ATTACK_NEAREST ENEMY");
                            }
                            else
                            {
                                Console.Error.WriteLine($"OUCH - TOWER {myTower}");
                                Console.WriteLine($"MOVE {myTower}");
                            }
                        }
                        else */
                        if (myHero.Type == "IRONMAN")
                        {
                            if (myHero.CountDown2 == 0 && myHero.Mana >= 60 && enemyUnits.Any(u => u.Distance(myHero) <= 900))
                            {

                                Console.WriteLine($"FIREBALL {enemyUnits[0].X - enemyUnits[0].MovementSpeed} {enemyUnits[0].Y}");
                                continue;
                            }
                        }
                        if (myHero.ItemsOwned < 3 && bestItem != null)
                        {
                            Console.WriteLine($"BUY {bestItem.itemName}");
                            myHeroes.ForEach(h => h.Gold -= bestItem.itemCost);
                        }
                        else if (enemyTower.Distance(myHero) <= enemyTower.AttackRange + myHero.MovementSpeed && myUnits.Any(u => u.Distance(enemyTower) < enemyTower.AttackRange))
                        {
                            Console.Error.WriteLine("MOVING AAWAY FROM TOWER");
                            myHero.GoHome();
                        }
                        //else if (farmableGroots.Any(g => OnMySide(g, myTeam)/* InFrontOf(new Point(frontLine, 1), g, myTeam)*/) && //myHero.HP > myHero.MaxHP * .30 &&
                        //        (!enemyUnits.Any(u => OnMySide(u, myTeam)) || myUnits.Any(u => OnMySide(u, enemyTeam))))
                        //{
                        //    var closestGroot = farmableGroots.First();
                        //    if (closestGroot.Distance(myHero) <= myHero.AttackRange + myHero.MovementSpeed)
                        //    {
                        //        Console.Error.WriteLine($"ATTACKING GROOT  {closestGroot}");
                        //        myHero.Attack(closestGroot);
                        //    }
                        //    else
                        //    {
                        //        Console.Error.WriteLine($"MOVING TO GROOT  {closestGroot}");
                        //        Console.WriteLine($"MOVE {closestGroot}");
                        //    }
                        //}
                        else if (!unitsEngaged)
                        {
                            if (myHero.X == _myTower.X)
                            {
                                Console.WriteLine($"ATTACK_NEAREST ENEMY; cornered");
                            }
                            else
                            {
                                Console.Error.WriteLine("Grouping Up");
                                Console.WriteLine($"MOVE {backLine + (20 * (myTeam == 0 ? -1 : 1))} {myHero.Y}");
                            }
                        }
                        else if (unitsEngaged)
                        {

                            if (myHero.WithinRange(enemyTower))
                            {
                                Console.Error.WriteLine("ATTACK THE TOWER!");
                                Console.WriteLine("ATTACK_NEAREST TOWER");
                            }
                            else if (enemyUnits.Count > 0)
                            {
                                var enemiesInRange = allEnemies.Where(u => myHero.WithinRange(u)).OrderBy(u => u.HP);
                                var lastHit = enemiesInRange.FirstOrDefault(u => u.HP <= myHero.AttackDamage);

                                if (enemiesInRange.Any())
                                {
                                    var unitsToDeny = myUnits.FirstOrDefault(m => allEnemies.Any(e => m.HP < e.AttackDamage && m.Distance(e) < e.AttackRange &&
                                                                                  m.Distance(myHero) <= myHero.AttackRange && m.HP < myHero.AttackDamage));

                                    var closest = enemiesInRange.First();
                                    if (lastHit != null)
                                    {
                                        Console.Error.WriteLine("LAST HITTING");
                                        myHero.Attack(lastHit);
                                        continue;
                                    }
                                    else if (unitsToDeny != null)
                                    {
                                        Console.Error.WriteLine("DENYING");
                                        myHero.Attack(unitsToDeny);
                                        continue;
                                    }

                                    if (myHero.Type == "HULK")
                                    {
                                        if (myHero.Mana >= 30 && myHero.CountDown2 == 0 && (enemyUnits.Any(u => u.Distance(myHero) <= 100) || villains.Any(v => v.Distance(myHero) <= 100)))
                                        {
                                            Console.WriteLine("EXPLOSIVESHIELD; BOOM!");
                                            continue;
                                        }
                                        else if (myHero.Mana >= 40 && myHero.CountDown3 == 0)
                                        {

                                            Console.WriteLine($"BASH {enemiesInRange.First().Id}; HULK SMASH!");
                                            continue;

                                        }
                                    }
                                    else if (myHero.Type == "VALKYRIE")
                                    {
                                        if (myHero.Mana >= 20 && myHero.CountDown1 == 0)
                                        {
                                            Console.WriteLine($"SPEARFLIP {enemiesInRange.First().Id}; FLIPPY");
                                            continue;
                                        }

                                    }

                                    if (closest.Distance(myHero) < myHero.AttackRange - 100 && myHero.AttackRange > 150 && enemiesInRange.Count() > 3)
                                    {
                                        Console.Error.WriteLine("EXtending range");
                                        myHero.GoHome();
                                    }
                                    else
                                    {
                                        Console.Error.WriteLine("HITTING ClOSEST");
                                        myHero.Attack(closest);
                                    }
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
                        else if (frontLine > 0)
                        {
                            Console.Error.WriteLine("MOVING UP");
                            Console.WriteLine($"MOVE {frontLine} {myHero.Y}");
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

        internal static bool OnMySide(Point point, int team)
        {
            if (team == 0)
            {
                return point.X < 960;
            }

            return point.X > 960;
        }
        internal static bool InFrontOf(Point point, Point target, int team)
        {
            if (team == 0)
            {
                return point.X > target.X;
            }

            return point.X < target.X;
        }
    }

}