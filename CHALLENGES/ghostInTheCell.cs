using System;
using System.Collections.Generic;
using System.Linq;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/

internal class Player
{
    public enum Quest
    {
        Upgrade,
        Attack,
        Defend,
        None
    }

    private static Dictionary<int, Factory> _factories;

    private static void Main(string[] args)
    {
        _factories = new Dictionary<int, Factory>();
        string[] inputs;
        int factoryCount = int.Parse(Console.ReadLine()); // the number of factories
        int linkCount = int.Parse(Console.ReadLine()); // the number of links between factories
        for (int i = 0; i < linkCount; i++)
        {
            string line = Console.ReadLine();
            inputs = line.Split(' ');
            int factory1 = int.Parse(inputs[0]);
            int factory2 = int.Parse(inputs[1]);
            int distance = int.Parse(inputs[2]);

            if (_factories.ContainsKey(factory1))
            {
                _factories[factory1].Distances.Add(factory2, distance);
            }
            else
            {
                _factories.Add(factory1, new Factory(factory1, factory2, distance));
            }
            if (_factories.ContainsKey(factory2))
            {
                _factories[factory2].Distances.Add(factory1, distance);
            }
            else
            {
                _factories.Add(factory2, new Factory(factory2, factory1, distance));
            }
        }
        Console.Error.WriteLine("Distances: {0}", string.Join(";", _factories.Select(f => string.Join(",", f.Value.Distances))));
        //sort the factories

        int bombDest = -1;
        int bombs = 2;
        // game loop
        while (true)
        {
            var troops = new Dictionary<int, Troop>();
            var moves = new List<string>();
            var actions = new List<int>();
            int entityCount = int.Parse(Console.ReadLine()); // the number of entities (e.g. factories and troops)
            for (int i = 0; i < entityCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int entityId = int.Parse(inputs[0]);
                string entityType = inputs[1];
                int arg1 = int.Parse(inputs[2]);
                int arg2 = int.Parse(inputs[3]);
                int arg3 = int.Parse(inputs[4]);
                int arg4 = int.Parse(inputs[5]);
                int arg5 = int.Parse(inputs[6]);

                switch (entityType)
                {
                    case "FACTORY":
                        if (!_factories.ContainsKey(entityId))
                        {
                            _factories.Add(entityId, new Factory(entityId, arg1, arg2, arg3));
                            moves.Add("MSG Did not find factory " + entityId);
                        }
                        else
                        {
                            _factories[entityId].Owner = arg1;
                            _factories[entityId].CurrentCyborgs = arg2;
                            _factories[entityId].Production = arg3;
                        }
                        _factories[entityId].Quest = Quest.None;
                        break;

                    case "TROOP":
                        if (!troops.ContainsKey(entityId))
                        {
                            troops.Add(entityId, new Troop(entityId, arg1, arg2, arg3, arg4, arg5));
                        }

                        break;

                    case "BOMB":
                        break;
                }
            }

            int myProduction = _factories.Where(f => f.Value.Owner == 1).Sum(f => f.Value.Production);
            int enemyProduction = _factories.Where(f => f.Value.Owner == -1).Sum(f => f.Value.Production);

            int highestEnemyCount = -1;
            int highestEnemyFactory = -1;
            var targets = new List<Factory>();
            var ownedFactories = _factories.Where(f => f.Value.Owner == 1).Select(f => f.Value);
            int ownedFactoriesCount = ownedFactories.Count();
            int enemyFactories = _factories.Where(f => f.Value.Owner == -1).Count();
            int availableCyborgs = ownedFactories.Sum(f => f.CurrentCyborgs);

            //find targets
            foreach (var factory in _factories.Values.Where(f => f.Owner != 1))
            {
                var sourceFactory = factory.ClosestFactory(ownedFactories, 0, true);
                if (sourceFactory != null)
                {
                    var enemyFactory = factory.ClosestFactory(_factories.Values.Where(f => f.Owner == -1), 0, false);

                    //only attack if I'm closer than the enemy
                    if (enemyFactory != null)
                    {
                        int deltaD = sourceFactory.Distances[factory.Id] - enemyFactory.Distances[factory.Id];
                        Console.Error.WriteLine("Delta {0} {1} {2} {3}", deltaD, sourceFactory, enemyFactory, factory);
                        if (deltaD <= 0 || sourceFactory.CurrentCyborgs > enemyFactory.CurrentCyborgs * 1.5)
                            targets.Add(factory);
                        //  moves.Add(string.Format("MOVE {0} {1} {2}", sourceFactory, dest, count));
                    }
                    else
                    {
                        targets.Add(factory);
                    }
                }

                //used for bombs
                if (factory.Production > 1 && factory.Owner < 1 && factory.CurrentCyborgs > highestEnemyCount)
                {
                    highestEnemyCount = factory.CurrentCyborgs;
                    highestEnemyFactory = factory.Id;
                }
            }

            var enemyAttackers = troops.Where(t => t.Value.Owner == -1 && ownedFactories.Any(f => f.Id == t.Value.Destination));
            Console.Error.WriteLine("Attackers {0}", string.Join(",", enemyAttackers));
            var attackPlan = new Dictionary<int, int>();
            foreach (var troop in enemyAttackers)
            {
                if (!attackPlan.ContainsKey(troop.Value.Destination)) attackPlan[troop.Value.Destination] = 0;

                attackPlan[troop.Value.Destination] += troop.Value.Cyborgs;
            }

            var defenders = ownedFactories.Where(f => attackPlan.ContainsKey(f.Id) && attackPlan[f.Id] > f.CurrentCyborgs * .5);

            Console.Error.WriteLine("Defenders {0}", string.Join(",", defenders));
            actions.AddRange(defenders.Select(f => f.Id));
            foreach (var defender in defenders.OrderByDescending(f => f.Production))
            {
                //attackPlan[f.Id] > (f.CurrentCyborgs + f.Production * enemyAttackers.Where(a => a.Value.Destination == f.Id).Min(a => a.Value.TimeToDestination))
                defender.Quest = Quest.Defend;

                var closestSupport = ownedFactories.Where(f => f.Id != defender.Id && !defenders.Contains(f) &&
                                                               f.Distances[defender.Id] < enemyAttackers.Where(a => a.Value.Destination == defender.Id).Min(a => a.Value.TimeToDestination));
                int numForSupport = closestSupport.Count();
                if (numForSupport == 0) continue;

                int borgsNeeded = attackPlan[defender.Id] / numForSupport;
                foreach (var support in closestSupport.Where(f => f.CurrentCyborgs >= borgsNeeded))
                {
                    moves.Add(string.Format("MOVE {0} {1} {2}", support.Id, defender.Id, borgsNeeded));
                    Console.Error.WriteLine("Supporting {0} to {1} with {2}", support.Id, defender.Id, borgsNeeded);
                    actions.Add(support.Id);
                    support.CurrentCyborgs -= borgsNeeded;
                }
            }
            int possibleOwned = troops.Where(f => f.Value.Owner == 1 && f.Value.TimeToDestination < 4).Count() + ownedFactoriesCount;
            //upgrade once i have half
            if (possibleOwned >= Math.Floor((double)(factoryCount / 2)) || ownedFactoriesCount > enemyFactories + 1)
            {
                foreach (var factory in ownedFactories.Where(f => f.Production < 3 && !defenders.Any(d => d.Id == f.Id) && !attackPlan.ContainsKey(f.Id)))
                {
                    factory.Quest = Quest.Upgrade;
                    int needed = 10 - factory.CurrentCyborgs;
                    if (needed <= 0)
                    {
                        moves.Add(string.Format("INC {0}", factory.Id));
                        factory.CurrentCyborgs -= 10;
                        continue;
                    }
                    else
                    {
                        if (factory.CurrentCyborgs + troops.Where(f => f.Value.Destination == factory.Id).Sum(f => f.Value.Cyborgs) > 10)
                        {
                            continue;
                        }
                    }

                    Factory closest;
                    var check = new List<int>();
                    check.AddRange(ownedFactories.Where(f => f.Production < 3).Select(f => f.Id));
                    do
                    {
                        closest = factory.ClosestFactory(ownedFactories.Where(f => !check.Contains(f.Id)), 1, true);
                        if (closest != null)
                        {
                            int sending = Math.Min(needed, closest.CurrentCyborgs);
                            moves.Add(string.Format("MOVE {0} {1} {2}", closest.Id, factory.Id, sending));
                            closest.CurrentCyborgs -= sending;
                            needed -= sending;
                            check.Add(closest.Id);
                        }
                    } while (needed > 0 && closest != null);
                }
            }

            //Order targets by distance
            targets = ClosestToAttack(targets, true).OrderBy(f => f.DistanceToClosestOwned)
                //.ThenBy(f => f.Owner)
                             .ThenByDescending(f => f.Production).ToList();
            Console.Error.WriteLine("Sorted by distance {0} ", string.Join(",", targets));

            //Determine whom to send to which factories
            int lastProduction = 0;
            foreach (var target in targets)
            {
                var tempMoves = new List<string>();
                int cyborgs = 0;
                var facs = _factories.Values.Where(f => f.Owner == 1 && !defenders.Any(d => d.Id == f.Id)).ToList();
                int facCount = 0;
                while (cyborgs <= target.CurrentCyborgs && facCount < facs.Count())
                {
                    facCount++;
                    var closest = target.ClosestFactory(facs, 1, true);
                    if (closest != null)
                    {
                        int c = Math.Min(target.CurrentCyborgs + 1, closest.CurrentCyborgs);
                        Console.Error.WriteLine("Sending {0} to {1} with {2}/{3} cyborgs", closest.Id, target.Id, c, closest.CurrentCyborgs);
                        closest.CurrentCyborgs -= c;
                        cyborgs += c;
                        tempMoves.Add(string.Format("MOVE {0} {1} {2}", closest.Id, target.Id, c));
                        facs.Remove(closest);
                    }
                }

                //Only send if we have acquired enough to take the factory
                if (cyborgs >= target.CurrentCyborgs)
                {
                    moves.AddRange(tempMoves);
                    actions.AddRange(tempMoves.Select(f => int.Parse(f.Split(' ')[1])));
                }
                else if (target.Production < 3 && cyborgs > 0) //hold on to them if the production was 3, we want to save up for it
                {
                    //add back the cyborgs we removed
                    foreach (var action in tempMoves.Select(f => f.Split(' ')))
                    {
                        _factories[int.Parse(action[1])].CurrentCyborgs += int.Parse(action[3]);
                    }
                }
            }

            if (bombs > 0 && highestEnemyFactory > -1 && highestEnemyFactory != bombDest &&
                highestEnemyCount * 2 > ownedFactories.Max(f => f.CurrentCyborgs))
            {
                var closest = _factories[highestEnemyFactory].ClosestFactory(_factories.Values.ToList(), 0, true);
                moves.Add(string.Format("BOMB {0} {1}", closest.Id, highestEnemyFactory));
                bombs--;
                bombDest = highestEnemyFactory;
            }

            //only increase production when necessary and we have the cyborgs
            Console.Write(string.Format("MSG {0}:{1};", myProduction, enemyProduction));
            if (myProduction + 2 < enemyProduction || myProduction > enemyProduction * 3)
            {
                var nonProducingOwnedFactories = _factories.Where(f => f.Value.Production < 3 && f.Value.Owner == 1 &&
                                                               f.Value.CurrentCyborgs > 10 && !defenders.Contains(f.Value));
                foreach (var factory in nonProducingOwnedFactories)
                {
                    moves.Add("INC " + factory.Value.Id);
                    actions.Add(factory.Value.Id);
                }
            }

            //find any factories not doing anything
            foreach (var factory in _factories.Where(f => f.Value.Owner == 1).Select(f => f.Value))
            {
                // reinforce
                if (factory.Production == 3 && factory.CurrentCyborgs > 10)
                {
                    //find the closest to me that is closer to an enemy
                    int minDistance = int.MaxValue;
                    Factory minTarget = null;
                    foreach (var t in targets.ToList())
                    {
                        if (minDistance == int.MaxValue || t.Distances[factory.Id] < minDistance)
                        {
                            minTarget = t;
                            minDistance = t.Distances[factory.Id];
                        }
                    }
                    if (minTarget != null)
                    {
                        var closest = minTarget.ClosestFactory(ownedFactories, 0, true);
                        if (closest != null && closest != factory)
                        {
                            moves.Add(string.Format("MOVE {0} {1} {2}", factory.Id, closest.Id, factory.CurrentCyborgs - 10));
                            Console.Error.WriteLine("Moving idlers {0} to {1} with {2}/{3} cyborgs", factory.Id, closest.Id, factory.CurrentCyborgs - 10, factory.CurrentCyborgs);
                        }
                    }
                    else
                    {
                        var closest = factory.ClosestFactory(ownedFactories.Where(f => f.Production < 3), 0, true);
                        if (closest != null)
                        {
                            moves.Add(string.Format("MOVE {0} {1} {2}", factory.Id, closest.Id, Math.Min(10, factory.CurrentCyborgs - 10)));
                            Console.Error.WriteLine("Moving idlers {0} to {1} with {2}/{3} cyborgs", factory.Id, closest.Id, Math.Min(10, factory.CurrentCyborgs - 10), factory.CurrentCyborgs);
                        }
                    }
                    //OrderedFactories(factory,true);

                    //_factories.Where(f=>f.Value)
                }
            }

            if (moves.Count() > 0)
            {
                Console.WriteLine(string.Join(";", moves));
            }
            else
            {
                //if there are no moves, see if we can take over a 0 production factory
                var emptyFactories = _factories.Where(f => f.Value.Production == 0 && f.Value.Owner == 0);

                foreach (var factory in emptyFactories.Select(f => f.Value))
                {
                    var closest = factory.ClosestFactory(_factories.Values.ToList(), factory.CurrentCyborgs + 1, true);
                    if (closest != null && !defenders.Any(f => f.Id == closest.Id))
                        moves.Add(string.Format("MOVE {0} {1} {2}", closest.Id, factory.Id, factory.CurrentCyborgs + 1));
                }

                if (moves.Count() > 0)
                {
                    Console.WriteLine(string.Join(";", moves));
                }
                else
                {
                    Console.WriteLine("WAIT");
                }
            }
        }
    }

    private static List<Factory> ClosestToAttack(List<Factory> targets, bool isOwned)
    {
        var sortedList = new List<Factory>();
        foreach (var factory in targets)
        {
            var closest = factory.ClosestFactory(_factories.Values.ToList(), 0, isOwned);
            if (closest == null) continue;

            factory.DistanceToClosestOwned = closest.Distances[factory.Id];
            //Console.Error.WriteLine("Closest distance to {0} is {1}", factory.Id, factory.DistanceToClosestOwned);
        }

        return targets;
    }

    private static IEnumerable<Factory> OrderedFactories(Factory goal, bool isOwned)
    {
        var orderedFactories = new List<Factory>();
        var facs = _factories.Values.Where(f => f.Owner == 1).ToList();
        while (facs.Count() > 0)
        {
            var closest = goal.ClosestFactory(facs, 0, isOwned);
            if (closest != null)
            {
                orderedFactories.Add(closest);
                facs.Remove(closest);
            }
        }

        return orderedFactories;
    }
}

internal class Factory
{
    public Factory(int id, int firstFactory, int distance)
    {
        Id = id;
        Distances = new Dictionary<int, int>();
        Distances.Add(id, 0);
        Distances.Add(firstFactory, distance);
    }

    public Factory(int id, int owner, int currentCyborgs, int production)
    {
        Id = id;
        Owner = owner;
        CurrentCyborgs = currentCyborgs;
        Production = production;
        Distances = new Dictionary<int, int>();
        Distances.Add(id, 0);
    }

    public int Id { get; set; }
    public int Owner { get; set; }
    public int CurrentCyborgs { get; set; }
    public int Production { get; set; }
    public Dictionary<int, int> Distances { get; set; }
    public int DistanceToClosestOwned { get; set; }
    public Player.Quest Quest { get; set; }

    public Factory ClosestFactory(IEnumerable<Factory> factories, int minCyborgs, bool isOwned)
    {
        int closestDistance = -1;
        Factory closestFactory = null;
        foreach (var factory in factories.Where(f => !isOwned || f.Owner == 1))
        {
            if (factory.Distances.ContainsKey(Id))
            {
                if ((closestDistance == -1 || factory.Distances[Id] < closestDistance) &&
                    factory.CurrentCyborgs >= minCyborgs)
                {
                    closestDistance = factory.Distances[Id];
                    closestFactory = factory;
                }
            }
        }
        if (closestFactory == null) return null;

        var frontier = new PriorityQueue<Factory>(); //openSet
        frontier.Enqueue(this, 0);

        var cameFrom = new Dictionary<Factory, Factory>();
        cameFrom[this] = this;

        var gScore = new Dictionary<Factory, double>();
        gScore[this] = 0;

        //Console.Error.WriteLine("Closest factory to {0}. Owned: {1}", Id, isOwned);
        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();

            if (current == closestFactory) break;

            foreach (var next in factories.Where(f => /*(!isOwned || f.Owner == 1) &&*/ f.Id != current.Id))
            {
                var newCost = gScore[current] + Cost(current, next);
                if (!gScore.ContainsKey(next) || newCost < gScore[next])
                {
                    gScore[next] = newCost;
                    cameFrom[next] = current;
                    //var priority = newCost + Heuristic(next, this);
                    frontier.Enqueue(next, 1);
                }
            }
        }

        if (cameFrom.Count() == 1) return null;
        //Console.Error.WriteLine("Closest factory to {0}. Owned: {1} - Result: {2}", Id, isOwned, string.Join(",", cameFrom));
        //if (cameFrom.Count() > 1) return cameFrom.ElementAt(2).Key;
        var path = GetPath(cameFrom, this, closestFactory);
        //Console.Error.WriteLine("Path from {3} to {0}. Owned: {1} - Result: {2}", Id, isOwned, string.Join(",", path), closestFactory);
        return path.FirstOrDefault();
        //return closestFactory;
    }

    private int Cost(Factory source, Factory dest)
    {
        //Console.Error.WriteLine("Distance from " + source.Id + " to " + dest.Id + " " + source.Distances[dest.Id]);
        return source.Distances[dest.Id];// +(50 / Math.Max(1, dest.CurrentCyborgs));
    }

    private double Heuristic(Factory current, Factory goal)
    {
        double h = current.Distances[goal.Id] + 1D;

        return h;
    }

    private List<Factory> GetPath(Dictionary<Factory, Factory> cameFrom, Factory start, Factory goal)
    {
        var result = new List<Factory>();
        var current = goal;
        result.Add(current);

        while (current != start)
        {
            //Console.Error.WriteLine("Current {0} Came From {1}", current, string.Join(",", cameFrom));
            current = cameFrom[current];
            result.Add(current);
        }
        //result.Reverse();
        return result;
    }

    public override string ToString()
    {
        return string.Format("[Id: {0}]", Id);
    }
}

internal class Troop
{
    public Troop(int entityId, int arg1, int arg2, int arg3, int arg4, int arg5)
    {
        // TODO: Complete member initialization
        Id = entityId;
        Owner = arg1;
        Source = arg2;
        Destination = arg3;
        Cyborgs = arg4;
        TimeToDestination = arg5;
    }

    public int Id { get; set; }
    public int Owner { get; set; }
    public int TimeToDestination { get; set; }
    public int Cyborgs { get; set; }
    public int Destination { get; set; }
    public int Source { get; set; }

    public override string ToString()
    {
        return string.Format("[Id: {0}, Dest: {1}, Owner: {2}]", Id, Destination, Owner);
    }
}

public class PriorityQueue<T>
{
    // I'm using an unsorted array for this example, but ideally this
    // would be a binary heap. There's an open issue for adding a binary
    // heap to the standard C# library: https://github.com/dotnet/corefx/issues/574
    //
    // Until then, find a binary heap class:
    // * https://bitbucket.org/BlueRaja/high-speed-priority-queue-for-c/wiki/Home
    // * http://visualstudiomagazine.com/articles/2012/11/01/priority-queues-with-c.aspx
    // * http://xfleury.github.io/graphsearch.html
    // * http://stackoverflow.com/questions/102398/priority-queue-in-net

    private List<Tuple<T, double>> elements = new List<Tuple<T, double>>();

    public int Count
    {
        get { return elements.Count; }
    }

    public void Enqueue(T item, double priority)
    {
        elements.Add(Tuple.Create(item, priority));
    }

    public T Dequeue()
    {
        int bestIndex = 0;

        for (int i = 0; i < elements.Count; i++)
        {
            if (elements[i].Item2 < elements[bestIndex].Item2)
            {
                bestIndex = i;
            }
        }

        T bestItem = elements[bestIndex].Item1;
        elements.RemoveAt(bestIndex);
        return bestItem;
    }

    public List<T> ToList()
    {
        return elements.Select(e => e.Item1).ToList();
    }
}