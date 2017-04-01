using System;
using System.Collections.Generic;
using System.Linq;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/

internal class Player
{
    private static List<int> gateways = new List<int>();

    private static void Main(string[] args)
    {
        var graph = new Graph<int>();
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int N = int.Parse(inputs[0]); // the total number of nodes in the level, including the gateways
        int L = int.Parse(inputs[1]); // the number of links
        int E = int.Parse(inputs[2]); // the number of exit gateways
        for (int i = 0; i < L; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int N1 = int.Parse(inputs[0]); // N1 and N2 defines a link between these nodes
            int N2 = int.Parse(inputs[1]);
            if (!graph.Edges.ContainsKey(N1))
            {
                graph.Edges[N1] = new List<int>();
            }
            graph.Edges[N1].Add(N2);
            if (!graph.Edges.ContainsKey(N2))
            {
                graph.Edges[N2] = new List<int>();
            }
            graph.Edges[N2].Add(N1);
        }
        for (int i = 0; i < E; i++)
        {
            int EI = int.Parse(Console.ReadLine()); // the index of a gateway node
            gateways.Add(EI);
        }
        Console.Error.WriteLine("Gateways... {0}", string.Join(", ", gateways));

        // game loop
        while (true)
        {
            foreach (var edge in graph.Edges)
            {
                //Console.Error.WriteLine("Graph... {0}: {1}", edge.Key, string.Join(", ", edge.Value));
            }

            int SI = int.Parse(Console.ReadLine()); // The index of the node on which the Skynet agent is positioned this turn

            var path = new List<int>();
            int closestGateway = -1;
            int priority = 4;
            foreach (var gateway in gateways.Where(g => graph.Neighbors(g) != null).OrderByDescending(g => graph.Neighbors(g).Count()))
            {
                var g = Search(graph, SI, gateway);
                if (!graph.Edges.ContainsKey(gateway)) continue;

                var newPath = GetPath(g, SI, gateway);
                int newCost = CostOfPath(newPath);
                int cost = CostOfPath(path);
                int newPathLength = newPath.Count();
                int closestPathLength = closestGateway == -1 ? 100 : path.Count();
                int gatewayNodes = graph.Neighbors(newPath[newPathLength - 2]).Cast<int>().Count(n => gateways.Contains(n));
                int closestGatewayNodes = closestGateway > -1 ? graph.Neighbors(path[closestPathLength - 2]).Cast<int>().Count(n => gateways.Contains(n)) : 0;
                Console.Error.WriteLine("Checking Path... {0} with {1} neighors and cost of {2}", string.Join(" ", newPath), graph.Neighbors(gateway).Count(), newCost);

                //p1 - adjacent to gateway
                if (newPathLength == 2)
                {
                    Console.Error.WriteLine("Changing priority {0}->{1}", priority, 1);
                    priority = 1;
                    path = newPath;
                    closestGateway = gateway;
                }
                // p3 closer
                else if (priority > 2 && (closestGateway == -1 || newPathLength <= closestPathLength))
                {
                    Console.Error.WriteLine("Changing priority {0}->{1}", priority, 3);
                    priority = 3;
                    if (newPathLength == closestPathLength)
                    {
                        int edges = graph.Edges[path[closestPathLength - 2]].Count();
                        if (graph.Edges[newPath[newPathLength - 2]].Count() <= edges) continue;
                    }
                    path = newPath;
                    closestGateway = gateway;
                }
                //p2 more paths to gateway, within certain distance
                else if (priority > 1 && closestGateway >= -1 && gatewayNodes >= closestGatewayNodes && (priority > 2 || newCost < cost))
                {
                    Console.Error.WriteLine("Changing priority {0}->{1}", priority, 2);
                    priority = 2;
                    path = newPath;
                    closestGateway = gateway;
                }
            }

            int first = path.Last();
            int second = path[path.Count() - 2];
            Console.Error.WriteLine("Chosen Path... {0}", string.Join(" ", path));
            Console.WriteLine("{0} {1}", first, second);
            graph.Edges[first].Remove(second);
            graph.Edges[second].Remove(first);
            if (graph.Edges[first].Count() == 0) graph.Edges.Remove(first);
            if (graph.Edges[second].Count() == 0) graph.Edges.Remove(second);
            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            // Example: 0 1 are the indices of the nodes you wish to sever the link between
            foreach (var edge in graph.Edges)
            {
                //Console.Error.WriteLine("Graph... {0}: {1}", edge.Key, string.Join(", ", edge.Value));
            }
        }
    }

    private static Dictionary<int, int> _costSoFar;

    private static Dictionary<int, int> Search(Graph<int> graph, int start, int goal)
    {
        var frontier = new PriorityQueue<int>();
        frontier.Enqueue(start, 0);

        var cameFrom = new Dictionary<int, int>();
        cameFrom[start] = start;
        _costSoFar = new Dictionary<int, int>();
        _costSoFar[start] = 0;

        Console.Error.WriteLine("Start {0}, Goal {1}", start, goal);
        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();

            //Console.Error.WriteLine("Popped... {0}", current);
            if (current == goal)
            {
                break;
            }

            foreach (var next in graph.Neighbors(current))
            {
                if (current == start && next == goal)
                {
                    cameFrom[next] = current;
                    _costSoFar[next] = 0;
                    return cameFrom;
                }

                var newCost = _costSoFar[current] + graph.Cost(current, next);
                if (!_costSoFar.ContainsKey(next) || newCost < _costSoFar[next])
                {
                    _costSoFar[next] = newCost;
                    int priority = newCost + graph.Heuristic(goal, next);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                    //Console.Error.WriteLine("Priority... {0} {1} {2}", priority, goal, next);
                    //Console.Error.WriteLine("Cost... {0} {1} {2}", newCost, current, next);
                }
            }
        }

        return cameFrom;
    }

    private static List<int> GetPath(Dictionary<int, int> cameFrom, int start, int goal)
    {
        var result = new List<int>();
        int current = goal;
        result.Add(current);

        while (current != start)
        {
            current = cameFrom[current];
            result.Add(current);
        }
        result.Reverse();
        return result;
    }

    private static int CostOfPath(List<int> path)
    {
        int cost = 0;
        foreach (var line in path)
        {
            cost += _costSoFar.ContainsKey(line) ? _costSoFar[line] : 0;
            //Console.Error.WriteLine("Cost... {0}", cost);
        }
        return cost;
    }

    private class Graph<T>
    {
        public Graph()
        {
            Edges = new Dictionary<T, List<T>>();
        }

        public Dictionary<T, List<T>> Edges { get; set; }

        public List<T> Neighbors(T id)
        {
            if (Edges.ContainsKey(id))
                return Edges[id];
            else
                return null;
        }

        public int Heuristic(T goal, T source)
        {
            //int accessPoints = Neighbors(goal).Count();
            //bool adjacentToGoal = Neighbors(source).Contains(goal);
            int gatewaysAdjacent = Neighbors(source).Cast<int>().Count(n => gateways.Contains(n));

            //return 1 / (int)Math.Pow(accessPoints, adjacentToGoal ? 4 : 2);
            int h = (50 / Math.Max(1, (int)Math.Pow(gatewaysAdjacent, 2)));
            return h;
        }

        internal int Cost(T current, T next)
        {
            int gatewaysAdjacent = Neighbors(next).Cast<int>().Count(n => gateways.Contains(n));
            int g = Neighbors(current).Cast<int>().Count(n => gateways.Contains(n));
            int cost = (350 / Math.Max(1, (int)Math.Pow(gatewaysAdjacent, 3)));
            if (gatewaysAdjacent >= 2)
            {
                return cost;
            }
            else if (g == 1)
            {
                return cost + 100;
            }
            else
            {
                return cost + 1;
            }

            //return 1 / (int)Math.Pow(accessPoints, adjacentToGoal ? 4 : 2);
            //Console.Error.WriteLine("H... {0} {1} {2}", h, goal, source);
            //return cost;
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
}