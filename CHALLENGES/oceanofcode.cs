using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public enum Direction { N, E, S, W }

public class Position
{
    public int X { get; set; }
    public int Y { get; set; }

    public Position(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int Distance(Position end)
    {
        return Math.Abs(X - end.X) + Math.Abs(Y - end.Y);
    }

    public override bool Equals(object obj)
    {
        if (obj is Position pos)
        {
            return X == pos.X && Y == pos.Y;
        }

        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        unchecked // Overflow is fine, just wrap
        {
            int hash = 17;
            // Suitable nullity checks etc, of course :)
            hash = hash * 23 + X.GetHashCode();
            hash = hash * 23 + Y.GetHashCode();
            return hash;
        }
    }

    public override string ToString()
    {
        return $"{X} {Y}";
    }
}

public class Map
{
    private const int HeuristicValue = 1;
    private const double HeuristicFudgeFactor = .001;
    private const int BaseMovementCost = 1;
    private const int SurfaceMultiplier = 300;

    public int Width { get; internal set; }
    public int Height { get; internal set; }
    public Types[, ] Object { get; internal set; }

    public Dictionary<Position, Position> MappedPath { get; set; }

    public enum Types
    {
        Island,
        Empty,
        Visited
    }

    public Map(int width, int height)
    {
        Width = width;
        Height = height;
        Object = new Types[width, height];
    }

    private IEnumerable<Position> Neighbors(Position position)
    {
        if (IsValidMove(position.X + 1, position.Y, true))
        {
            yield return new Position(position.X + 1, position.Y);
        }
        if (IsValidMove(position.X, position.Y + 1, true))
        {
            yield return new Position(position.X, position.Y + 1);
        }
        if (IsValidMove(position.X - 1, position.Y, true))
        {
            yield return new Position(position.X - 1, position.Y);

        }
        if (IsValidMove(position.X, position.Y - 1, true))
        {
            yield return new Position(position.X, position.Y - 1);
        }
    }

    private Dictionary<Position, Position> ConstructPath(Position start, Position destination)
    {
        var frontier = new PriorityQueue<Position>();
        frontier.Enqueue(start, 0);
        var cameFrom = new Dictionary<Position, Position>();
        cameFrom.Add(start, start);
        var costSoFar = new Dictionary<Position, int>();
        costSoFar.Add(start, 0);

        while (frontier.Count > 0)
        {
            //Console.Error.WriteLine(string.Join(",", frontier.Select(c=> c)));

            var current = frontier.Dequeue();
            //Console.Error.WriteLine($"Current {current}");
            if (current.Equals(destination)) break;

            foreach (var next in Neighbors(current))
            {
                int newCost = costSoFar[current] + Cost(current, next);
                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    int priority = newCost + Heuristic(start, destination, next);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }
        //Console.Error.WriteLine($"CameFrom: {string.Join(";", cameFrom.Select((a) => $"{a.Key} {a.Value}"))}");

        MappedPath = cameFrom;
        return cameFrom;
    }

    public List<Position> Path(Position start, Position destination, Dictionary<Position, Position> cameFrom)
    {
        var current = destination;
        if (!cameFrom.ContainsKey(destination))
        {
            Console.Error.WriteLine($"Path missing destination!");
            return null;
        }

        var path = new List<Position>();
        while (current != start)
        {
            path.Add(current);
            current = cameFrom[current];
            //Console.Error.WriteLine($"CameFrom: {string.Join(";", cameFrom.Select((a) => $"{a.Key} {a.Value}"))}");

        }
        path.Reverse();
        //Console.Error.WriteLine($"Path: {string.Join(";", path)}");

        return path;
    }
    public List<Position> Path(Position start, Position destination)
    {
        var path = /*MappedPath ?? */ ConstructPath(start, destination);
        return Path(start, destination, path);
    }

    private int Cost(Position start, Position end)
    {
        int cost = BaseMovementCost;
        if (Object[end.X, end.Y] == Types.Visited)
        {
            cost += SurfaceMultiplier;
        }

        return cost;
    }

    private int Heuristic(Position current, Position start, Position end)
    {
        //prefer straight lines
        var dx1 = current.X - end.X;
        var dy1 = current.Y - end.Y;
        var dx2 = start.X - end.X;
        var dy2 = start.Y - end.Y;
        var cross = Math.Abs(dx1 * dy2 - dx2 * dy1);

        var distance = start.Distance(end);

        return (int) ((HeuristicValue * distance) + cross * HeuristicFudgeFactor);
    }

    public IEnumerable<Position> Find(Types search)
    {
        for (int h = 0; h < Height; h++)
        {
            for (int w = 0; w < Width; w++)
            {
                if (Object[w, h] == search)
                {
                    yield return new Position(w, h);
                }
            }
        }
    }

    public void ClearVisited()
    {
        for (int h = 0; h < Height; h++)
        {
            for (int w = 0; w < Width; w++)
            {
                if (Object[w, h] == Types.Visited)
                {
                    Object[w, h] = Types.Empty;
                }
            }
        }
    }

    public bool IsValidMove(int x, int y, bool allowVisited = false)
    {
        if (x == null ||
            y == null ||
            x < 0 ||
            y < 0 ||
            y > Height - 1 ||
            x > Width - 1)
        {
            return false;
        }

        switch (Object[x, y])
        {
            case Types.Empty:
                return true;
            case Types.Visited:
                return allowVisited;
            default:
                return false;
        }
    }
    public bool IsValidMove(Position position, bool allowVisited = false)
    {
        if (position?.X == null || position.Y == null)
            return false;
        return IsValidMove(position.X, position.Y, allowVisited);
    }

    public bool IsInQuadrant(int quadrant, Position position)
    {
        int sectorId = quadrant - 1;
        int xQuadrant = sectorId % 3;
        int yQuadrant = (int) Math.Floor(((double) (sectorId / 3)));

        return position.X >= xQuadrant * 5 &&
            position.X < (xQuadrant + 1) * 5 &&
            position.Y >= yQuadrant * 5 &&
            position.Y < (yQuadrant + 1) * 5;
    }

    public int GetSector(Position position)
    {
        if (position.X < 5)
        {
            if (position.Y < 5)
            {
                return 1;
            }
            else if (position.Y < 10)
            {
                return 4;
            }

            return 7;
        }
        else if (position.X < 10)
        {
            if (position.Y < 5)
            {
                return 2;
            }
            else if (position.Y < 10)
            {
                return 5;
            }

            return 8;
        }
        else
        {
            if (position.Y < 5)
            {
                return 3;
            }
            else if (position.Y < 10)
            {
                return 6;
            }

            return 9;
        }
    }

    public bool IsInRange(int range, Position source, Position destination)
    {
        return source.Distance(destination) <= range;
    }

    public Direction GetDirection(Position start, Position end)
    {
        var direction = Direction.N;
        if (end.X > start.X)
        {
            direction = Direction.E;
        }
        else if (end.Y > start.Y)
        {
            direction = Direction.S;
        }
        else if (end.X < start.X)
        {
            direction = Direction.W;
        }

        return direction;
    }

    public void Print()
    {
        for (int h = 0; h < Height; h++)
        {
            for (int w = 0; w < Width; w++)
            {
                Console.Error.Write(Object[w, h]);
            }
            Console.Error.WriteLine();
        }
    }
}

class Player
{
    private const int MaxSonarLocations = 5;
    private const int MapSize = 15;
    private const int Sectors = 9;
    private const int TooCloseDistance = 2;
    private const int TorpedoThreshold = 5;
    private const int TorpedoDistanceThreshold = 2;

    private static Map _map = new Map(MapSize, MapSize);
    private static Map _enemyMap = new Map(MapSize, MapSize);

    public enum Action
    {
        Move,
        Surface,
        Torpedo,
        Sonar,
        Silence,
        Mine,
        Trigger
    }

    public static void SendText(string value)
    {
        Console.Error.WriteLine(value);
    }

    internal class Submarine : Position
    {
        public Torpedo Torpedo { get; internal set; }
        public Silence Silence { get; internal set; }
        public Sonar Sonar { get; internal set; }
        private int LastLife;
        private int _life;
        public int Life
        {
            get => _life;

            set
            {
                LastLife = _life;
                _life = value;
            }
        }

        public int DamageTaken { get => LastLife - Life; }

        public Skill SkillToCharge { get; set; }

        public Submarine(Position location) : base(location.X, location.Y)
        {
            Torpedo = new Torpedo();
            Silence = new Silence();
            Sonar = new Sonar();
        }

        public Submarine(int x, int y) : base(x, y)
        {
            Torpedo = new Torpedo();
            Silence = new Silence();
            Sonar = new Sonar();
        }

        private Position GetPosition(Direction direction, Position current)
        {
            switch (direction)
            {
                case Direction.N:
                    current.Y--;
                    break;
                case Direction.E:
                    current.X++;
                    break;
                case Direction.S:
                    current.Y++;
                    break;
                case Direction.W:
                    current.X--;
                    break;
            }

            return current;
        }
        public string Move(Direction direction)
        {
            if (SkillToCharge != null)
            {
                SkillToCharge.Cooldown--;
            }
            var newPosition = GetPosition(direction, this as Position);
            X = newPosition.X;
            Y = newPosition.Y;

            return $"MOVE {direction} {SkillToCharge?.Name}";
        }
    }

    internal class Skill
    {
        public virtual string Name { get; }
        public virtual int Range { get; }
        public int Cooldown { get; set; }
        public bool FullyCharged { get { return Cooldown == 0; } }

        public override string ToString()
        {
            return $"Cooldown: {Cooldown}, Fully Charged? {FullyCharged}";
        }
    }

    internal class Torpedo : Skill
    {
        public override string Name { get { return "TORPEDO"; } }
        public override int Range => 4;
        public const int Damage = 2;
        public const int SplashDamage = 1;
        public Position LastFired { get; set; }
        public string Use(Position position)
        {
            Cooldown = 5;
            LastFired = position;
            return $"{Name} {position}";
        }
    }

    internal class Sonar : Skill
    {
        public int Sector { get; set; }
        public override string Name => "SONAR";
        public string Result { get; set; }

        public string Use(int sector)
        {
            Cooldown = 5;
            Sector = sector;
            return $"{Name} {sector}";
        }

        public override string ToString()
        {
            return $"{base.ToString()}, Result: {Result}";
        }
    }

    internal class Silence : Skill
    {
        public override string Name => "SILENCE";

        public string Use(Direction direction, int numberOfMoves)
        {
            Cooldown++;
            return $"{Name} {direction} {numberOfMoves}";
        }
    }
    internal class Mine : Skill
    {
        public override string Name => "MINE";

        public string Use(Direction direction)
        {
            return $"{Name} {direction}";
        }
    }

    internal class Trigger : Skill
    {
        public override string Name => "TRIGGER";
    }

    private static string Play(Action action, Direction direction, Position position, Skill charge)
    {
        switch (action)
        {
            case Action.Move:
                return $"{action} {direction} {charge.Name}";
            case Action.Surface:
                return "SURFACE";
            case Action.Torpedo:
                return $"TORPEDO {position}";
            case Action.Silence:
                return $"SILENCE {direction} 1";
            case Action.Mine:
                return $"MINE {direction}";
            case Action.Trigger:
                return $"TRIGGER {position}";
            default:
                return "";
        }

    }

    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int width = int.Parse(inputs[0]);
        int height = int.Parse(inputs[1]);
        int myId = int.Parse(inputs[2]);
        for (int i = 0; i < height; i++)
        {
            string line = Console.ReadLine();
            for (int j = 0; j < width; j++)
            {
                Map.Types cell;
                switch (line[j])
                {
                    case '.':
                        cell = Map.Types.Empty;
                        break;
                    case 'x':
                        cell = Map.Types.Island;
                        break;
                    default:
                        cell = Map.Types.Empty;
                        break;
                }
                _map.Object[j, i] = cell;
                _enemyMap.Object[j, i] = cell;
            }
        }

        var validStartingPositions = _map.Find(Map.Types.Empty);
        var pos = validStartingPositions.FirstOrDefault(c => _map.IsInQuadrant(5, c));
        Console.WriteLine(pos.ToString());

        Submarine me = new Submarine(pos);
        var enemy = new Submarine(pos);

        var possibleLocations = validStartingPositions.Where(p => _enemyMap.IsValidMove(p)).ToList();

        // game loop
        while (true)
        {
            _map.MappedPath = null;

            //_map.Print();
            inputs = Console.ReadLine().Split(' ');
            int x = int.Parse(inputs[0]);
            int y = int.Parse(inputs[1]);
            me.Life = int.Parse(inputs[2]);
            enemy.Life = int.Parse(inputs[3]);
            me.Torpedo.Cooldown = int.Parse(inputs[4]);
            me.Sonar.Cooldown = int.Parse(inputs[5]);
            me.Silence.Cooldown = int.Parse(inputs[6]);
            int mineCooldown = int.Parse(inputs[7]);
            me.Sonar.Result = Console.ReadLine();
            var opponentOrders = Console.ReadLine().Split('|');
            var opponentQuadrant = -1;

            me.X = x;
            me.Y = y;
            _map.Object[x, y] = Map.Types.Visited;
            if (!possibleLocations.Any())
            {
                //possibleLocations = _enemyMap.Find(Map.Types.Empty).ToList();
            }
            SendText($"Enemy Damage Taken: {enemy.DamageTaken}");
            if (me.Torpedo.LastFired != null)
            {
                if (opponentOrders.Any(o => o.Contains("SURFACE") || o.StartsWith("TORPEDO")))
                {
                    //do nothing
                }
                else if (enemy.DamageTaken == 2)
                {
                    SendText($"Enemy Found! {me.Torpedo.LastFired}");
                    possibleLocations = possibleLocations.Where(p => p == me.Torpedo.LastFired).ToList();
                }
                else if (enemy.DamageTaken == 1)
                {

                    SendText($"Enemy tagged! {me.Torpedo.LastFired}");
                    possibleLocations = possibleLocations.Where(p => p != me.Torpedo.LastFired && me.Torpedo.LastFired.Distance(p) <= 2).ToList();
                }

                me.Torpedo.LastFired = null;
            }

            switch (me.Sonar.Result)
            {
                case "Y":
                    possibleLocations = possibleLocations.Where(l => _map.IsInQuadrant(me.Sonar.Sector, l)).ToList();
                    me.Sonar.Sector = -1;
                    break;
                case "N":
                    possibleLocations = possibleLocations.Where(l => !_map.IsInQuadrant(me.Sonar.Sector, l)).ToList();
                    me.Sonar.Sector = -1;
                    break;
            }
            possibleLocations = possibleLocations.OrderBy(l => l.Distance(me)).ToList();

            foreach (var order in opponentOrders)
            {
                Console.Error.WriteLine($"OO: {order}");
                var parts = order.Split('|');
                foreach (var command in parts)
                {
                    var commandParts = order.Split(' ');
                    if (commandParts[0] == "SURFACE")
                    {
                        var quadrant = int.Parse(commandParts[1]);
                        possibleLocations = possibleLocations.Where(p => _enemyMap.IsInQuadrant(quadrant, p)).ToList();
                    }
                    else if (commandParts[0] == "TORPEDO")
                    {
                        possibleLocations = possibleLocations.Where(p => _enemyMap.IsInRange(me.Torpedo.Range, p, new Position(int.Parse(commandParts[1]), int.Parse(commandParts[2])))).ToList();
                    }
                    else if (commandParts[0] == "MOVE")
                    {
                        //Console.Error.WriteLine($"OO: {commandParts[1]}");
                        switch (commandParts[1])
                        {
                            case "N":
                                possibleLocations.ForEach(p => p.Y--);
                                break;
                            case "E":
                                possibleLocations.ForEach(p => p.X++);
                                break;
                            case "S":
                                possibleLocations.ForEach(p => p.Y++);
                                break;
                            case "W":
                                possibleLocations.ForEach(p => p.X--);
                                break;
                        }

                        possibleLocations = possibleLocations.Where(p => _enemyMap.IsValidMove(p.X, p.Y)).ToList();

                    }
                    else if (commandParts[0] == "SILENCE")
                    {
                        var newPossibles = new List<Position>();
                        possibleLocations.ForEach(l =>
                        {
                            for (int i = 1; i <= 4; i++)
                            {
                                var newPossible = new Position(l.X + i, l.Y);
                                if (_map.IsValidMove(newPossible, true) && !possibleLocations.Contains(newPossible) && !newPossibles.Contains(newPossible))
                                {
                                    newPossibles.Add(newPossible);
                                }

                                newPossible = new Position(l.X, l.Y + i);
                                if (_map.IsValidMove(newPossible, true) && !possibleLocations.Contains(newPossible) && !newPossibles.Contains(newPossible))
                                {
                                    newPossibles.Add(newPossible);
                                }

                                newPossible = new Position(l.X - i, l.Y);
                                if (_map.IsValidMove(newPossible, true) && !possibleLocations.Contains(newPossible) && !newPossibles.Contains(newPossible))
                                {
                                    newPossibles.Add(newPossible);
                                }

                                newPossible = new Position(l.X, l.Y - i);
                                if (_map.IsValidMove(newPossible, true) && !possibleLocations.Contains(newPossible) && !newPossibles.Contains(newPossible))
                                {
                                    newPossibles.Add(newPossible);
                                }
                            }
                        });

                        possibleLocations.AddRange(newPossibles);
                    }

                }

                if (possibleLocations.Count() < 10)
                {
                    Console.Error.WriteLine($"Possibles: {string.Join(";\n", possibleLocations)}");
                }
            }

            Console.Error.WriteLine($"Torpedo {me.Torpedo},\n Sonar {me.Sonar},\n Silence {me.Silence}\n Guess Count: {possibleLocations.Count()}");

            var direction = Direction.N;
            var newPosition = new Position(x, y);
            var moves = new List<string>();

            me.SkillToCharge = null;
            var possibleActions = Enum.GetValues(typeof(Action)).Cast<Action>().ToHashSet();
            possibleActions.Remove(Action.Mine);
            possibleActions.Remove(Action.Trigger);
            possibleActions.Remove(Action.Surface);

            var hasMoved = false;
            while (possibleActions.Any())
            {
                hasMoved = !possibleActions.Contains(Action.Move);
                SendText($"Remaining Actions: {string.Join(",", possibleActions)}, {hasMoved}");
                if (possibleActions.Contains(Action.Torpedo))
                {
                    if (possibleLocations.Count() <= TorpedoThreshold)
                    {
                        if (me.Torpedo.FullyCharged)
                        {
                            SendText("Looking for targets in range of torpedo...");
                            var closeTargets = possibleLocations.Select(l => new { Position = l, Distance = l.Distance(me) }).Where(l => l.Distance <= me.Torpedo.Range);
                            var target = closeTargets.FirstOrDefault(l => l.Distance > TorpedoDistanceThreshold || (me.Life > 1 && enemy.Life <= 2));

                            if (target != null)
                            {
                                Console.Error.WriteLine($"Within range [{target.Distance}].  Firing!");
                                moves.Add(me.Torpedo.Use(target.Position));
                                possibleActions.Remove(Action.Torpedo);
                            }
                        }
                        else if (!hasMoved)
                        {
                            SendText("Charging Torpedo...");
                            me.SkillToCharge = me.Torpedo;
                        }
                    }

                    if (hasMoved)
                    {
                        possibleActions.Remove(Action.Torpedo);
                    }
                }

                if (possibleActions.Contains(Action.Sonar))
                {
                    if (possibleLocations.Count() > MaxSonarLocations && me.Sonar.FullyCharged)
                    {
                        var mostCrowdedSector = possibleLocations.GroupBy(loc => _map.GetSector(loc))
                            .Select(g => new { Sector = g.Key, Count = g.Count() })
                            .OrderByDescending(g => g.Count);
                        if (mostCrowdedSector.Count() > 1) //don't use if all in 1 sector
                        {
                            Console.Error.WriteLine($"Sectors: {string.Join(";", mostCrowdedSector.Select(s => $"{s.Sector}, {s.Count}"))}");
                            moves.Add(me.Sonar.Use(mostCrowdedSector.First().Sector));
                            possibleActions.Remove(Action.Sonar);
                        }
                    }

                    if (hasMoved)
                    {
                        possibleActions.Remove(Action.Sonar);
                    }
                }

                if (possibleActions.Contains(Action.Silence))
                {
                    if (me.Silence.FullyCharged)
                    {
                        SendText($"Silent G... {direction}");
                        //moves.Add(Play(Action.Silence, direction, null, null));
                    }
                    possibleActions.Remove(Action.Silence);
                }

                if (me.SkillToCharge == null)
                {
                    if (!me.Sonar.FullyCharged && possibleLocations.Count() > MaxSonarLocations)
                    {
                        SendText("Charging Sonar...");
                        me.SkillToCharge = me.Sonar;
                    }
                    else if (!me.Silence.FullyCharged && me.Life < enemy.Life && me.Torpedo.FullyCharged)
                    {
                        SendText("Charging Silence...");
                        me.SkillToCharge = me.Silence;
                    }
                }

                if (possibleActions.Contains(Action.Move))
                {
                    if (possibleLocations.Count() > 0)
                    {
                        var possiblePaths = possibleLocations.Select(pl => _map.Path(me, pl));
                        var unblockedPaths = possiblePaths.Where(pp => _map.IsValidMove(pp?.FirstOrDefault(), false));

                        Console.Error.WriteLine($"Mapping Paths... {me}, {possiblePaths.Count()}");
                        if (possibleLocations.Count() == 1)
                        {
                            Console.Error.WriteLine($"Enemy spotted... {me}, {possibleLocations.First()}");
                            var route = unblockedPaths.FirstOrDefault();
                            if (route == null)
                            {
                                SendText("Movement blocked!");
                                var getAwayZone = _map.Find(Map.Types.Empty)
                                    .FirstOrDefault(c => c.Distance(me) >= me.Torpedo.Range && _map.IsValidMove(c));
                                if (getAwayZone != null)
                                {
                                    Console.Error.WriteLine("Too close for missiles, switching to guns");
                                    route = _map.Path(me, getAwayZone);
                                    if (route != null)
                                    {
                                        Console.Error.WriteLine($"Moving away: {string.Join(";", route)}");
                                        var next = route.FirstOrDefault();
                                        if (_map.IsValidMove(next))
                                        {
                                            _map.Object[next.X, next.Y] = Map.Types.Visited;
                                            moves.Add(me.Move(_map.GetDirection(me, next)));
                                        }
                                    }
                                }
                                possibleActions.Remove(Action.Move);
                            }
                            else
                            {
                                Console.Error.WriteLine($"Charting path... {string.Join(";", route)}");

                                var next = route.FirstOrDefault();
                                if (route.Count() <= TooCloseDistance)
                                {
                                    var getAwayZone = _map.Find(Map.Types.Empty).FirstOrDefault(c => c.Distance(me) >= me.Torpedo.Range);
                                    if (getAwayZone != null)
                                    {
                                        Console.Error.WriteLine("Too close for missiles, switching to guns");
                                        route = _map.Path(me, getAwayZone);
                                        if (route?.Count() > 0)
                                        {
                                            if (_map.IsValidMove(route.FirstOrDefault()))
                                            {
                                                next = route.FirstOrDefault();
                                                Console.Error.WriteLine($"Moving away: {string.Join(";", route)}");
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Console.Error.WriteLine($"Chasing enemy: {next}");
                                }

                                if (next == null) continue;
                                direction = _map.GetDirection(me, next);
                                if (me.Silence.FullyCharged)
                                {
                                    int numberOfMoves = 1;
                                    for (int i = 1; i < me.Silence.Range; i++)
                                    {
                                        if (_map.GetDirection(me, route[i]) == direction)
                                        {
                                            numberOfMoves++;
                                            _map.Object[route[i].X, route[i].Y] = Map.Types.Visited;
                                            me.Move(direction);
                                        }
                                    }
                                    SendText($"Silent G... {direction}");
                                    moves.Add(me.Silence.Use(direction, numberOfMoves));
                                }
                                else
                                {
                                    newPosition = next;
                                    _map.Object[newPosition.X, newPosition.Y] = Map.Types.Visited;
                                    moves.Add(me.Move(direction));
                                    possibleActions.Remove(Action.Move);
                                }
                            }
                        }
                        else
                        {
                            Console.Error.WriteLine($"Enemy missing... {me}, {possibleLocations.First()}");
                            var chosenPath = unblockedPaths.FirstOrDefault();
                            if (chosenPath != null)
                            {
                                Console.Error.WriteLine($"Chosen Path: {string.Join(";", chosenPath)}");
                                var next = chosenPath.First();
                                direction = _map.GetDirection(me, next);
                                if (me.Silence.FullyCharged)
                                {
                                    int numberOfMoves = 1;
                                    for (int i = 1; i < me.Silence.Range; i++)
                                    {
                                        if (_map.GetDirection(me, chosenPath[i]) == direction)
                                        {
                                            numberOfMoves++;
                                            _map.Object[chosenPath[i].X, chosenPath[i].Y] = Map.Types.Visited;
                                        }
                                    }
                                    SendText($"Silent G... {direction}");
                                    moves.Add(me.Silence.Use(direction, numberOfMoves));
                                }
                                else
                                {
                                    newPosition = next;
                                    _map.Object[newPosition.X, newPosition.Y] = Map.Types.Visited;
                                    moves.Add(me.Move(direction));
                                }
                            }
                            else if (me.Life == 1)
                            {
                                SendText("Last ditch effort");
                                var sos = _map.Find(Map.Types.Empty)
                                    .Where(p => _map.IsValidMove(_map.Path(me, p).First()))
                                    .FirstOrDefault();
                                if (sos != null)
                                {
                                    moves.Add(me.Move(_map.GetDirection(me, sos)));
                                }
                            }
                            possibleActions.Remove(Action.Move);
                            // else
                            // {
                            //     action = Action.Surface;
                            //     possibleActions.Remove(Action.Surface);
                            // }
                        }
                    }
                }
            }

            for (int i = 0; i < moves.Count; i++)
            {
                if (moves[i].StartsWith("MOVE") && moves[i].Length < 7)
                {
                    moves[i] += me.SkillToCharge?.Name ?? me.Torpedo.Name;
                }
            }

            if (!moves.Any())
            {
                moves.Add("SURFACE");
                _map.ClearVisited();
            }

            Console.WriteLine(string.Join("|", moves));
        }
    }
}

public class PriorityQueue<T>
{
    // I'm using an unsorted array for this example, but ideally this
    // would be a binary heap. There's an open issue for adding a binary
    // heap to the standard C# library: https://github.com/dotnet/corefx/issues/574
    //
    // Until then, find a binary heap class:
    // * https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp
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
}