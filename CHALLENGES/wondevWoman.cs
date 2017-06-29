using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace WondevWoman
{
    internal class Player
    {
        private static int _boardSize = 0;
        private static int[,] _board;

        private enum Dir
        { N, NE, E, SE, S, SW, W, NW }

        private struct Point
        {
            public int x;
            public int y;

            public Point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public int Distance(Point p)
            {
                return (int)Math.Sqrt(Math.Pow(p.x - x, 2) + Math.Pow(p.y - y, 2));
            }

            public override string ToString()
            {
                return string.Format("[{0},{1}]", x, y);
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Point)) return false;
                var o = (Point)obj;

                return x == o.x && y == o.y;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash *= 23 + x.GetHashCode();
                    hash *= 23 + y.GetHashCode();
                    return hash;
                }
            }
        }

        private struct Action
        {
            public int id;
            public Dir moveDirection;
            public Dir buildDirection;
            public string atype;
            public Point position;
            public Point[] enemyPos;
            public Eval evaluation;

            public override string ToString()
            {
                bool debug = false;
                string s = "{3} {0} {1} {2}";
                if (debug)
                {
                    s += " - {4}";
                }
                return string.Format(s, id, moveDirection, buildDirection, atype, evaluation);
            }
        }

        private struct Eval : IComparable<Eval>
        {
            public int blocksEnemy;
            public int helpsEnemy;
            public int moveHeight;
            public int overBuilt;
            public int buildHeight;

            public int CompareTo(Eval other)
            {
                if (!helpsEnemy.Equals(other.helpsEnemy)) return helpsEnemy.CompareTo(other.helpsEnemy);
                if (!blocksEnemy.Equals(other.blocksEnemy)) return blocksEnemy.CompareTo(other.blocksEnemy);
                if (!moveHeight.Equals(other.moveHeight)) return moveHeight.CompareTo(other.moveHeight);
                if (!overBuilt.Equals(other.overBuilt)) return overBuilt.CompareTo(other.overBuilt);

                return buildHeight.CompareTo(other.buildHeight);
            }

            public override string ToString()
            {
                return string.Format("Block: {0}, Help: {1}, Move: {2}, Over: {3}, Build: {4}", blocksEnemy, helpsEnemy, moveHeight, overBuilt, buildHeight);
            }
        }

        private static List<Point> Neighbors(Point pos)
        {
            var result = new List<Point>(4);
            //Console.Error.WriteLine("{0} {1}", Position.X, Position.Y);

            if (pos.x > 0) result.Add(new Point(pos.x - 1, pos.y));
            if (pos.x + 1 < _boardSize - 1) result.Add(new Point(pos.x + 1, pos.y));
            if (pos.y > 0) result.Add(new Point(pos.x, pos.y - 1));
            if (pos.y + 1 < _boardSize) result.Add(new Point(pos.x, pos.y + 1));

            return result;
        }

        private static Dir GetDirection(Point src, Point dest)
        {
            string dir = "";

            if (dest.y > src.y) dir += "S";
            else if (dest.y < src.y) dir += "N";

            if (dest.x > src.x) dir += "E";
            else if (dest.x < src.x) dir += "W";

            return (Dir)Enum.Parse(typeof(Dir), dir);
        }

        private static Point GetMove(Point myLocation, Dir moveDirection)
        {
            var move = new Point(myLocation.x, myLocation.y);
            if (moveDirection == Dir.N || moveDirection == Dir.NE || moveDirection == Dir.NW)
                move.y = myLocation.y - 1;
            else if (moveDirection == Dir.S || moveDirection == Dir.SE || moveDirection == Dir.SW)
                move.y = myLocation.y + 1;

            if (moveDirection == Dir.W || moveDirection == Dir.NW || moveDirection == Dir.SW)
                move.x = myLocation.x - 1;
            else if (moveDirection == Dir.E || moveDirection == Dir.SE || moveDirection == Dir.NE)
                move.x = myLocation.x + 1;

            return move;
        }

        private static bool IsValidLocation(Point pos)
        {
            return pos.x >= 0 && pos.x < _boardSize && pos.y >= 0 && pos.y < _boardSize;
        }

        private static int GetHeight(Point pos)
        {
            if (pos.x < 0 || pos.y < 0) return -1;

            return _board[pos.x, pos.y];
        }

        private static Eval evaluate(Action action, Point[] enemyPos)
        {
            if (action.atype == "PUSH&BUILD")
            {
                return evaluatePush(action, enemyPos);
            }
            else
            {
                return evaluateMove(action, enemyPos);
            }
        }

        private static Eval evaluatePush(Action action, Point[] enemyPos)
        {
            var pos = enemyPos[action.id];
            var newEnemyPosition = GetMove(pos, action.moveDirection);
            if (!IsValidLocation(newEnemyPosition)) return new Eval { blocksEnemy = -1, helpsEnemy = -1, moveHeight = -1, overBuilt = -1, buildHeight = -1 };

            //if (action.position.Equals(newEnemyPosition)) return new Eval { blocksEnemy = -1, helpsEnemy = -1, moveHeight = -1, overBuilt = -1, buildHeight = -1 };
            //Console.Error.WriteLine("na: " + action);
            //Console.Error.WriteLine("np: " + newEnemyPosition);
            var build = GetMove(newEnemyPosition, action.buildDirection);
            var initHeight = GetHeight(pos);
            var moveHeight = GetHeight(newEnemyPosition);
            var buildHeight = 0;// GetHeight(build);

            int netHeight = moveHeight - initHeight;
            var myHeight = GetHeight(action.position);

            int overBuilt = buildHeight >= 3 ? 1 : 0;
            int helpedEnemy = 0;
            int blocksEnemy = 0;
            for (int i = 0; i < 2; i++)
            {
                int enemyHeight = GetHeight(enemyPos[i]);
                int distance = build.Distance(enemyPos[i]);
                if (netHeight > 0 /*|| !Neighbors(action.position).Contains(build)*/)
                {
                    helpedEnemy = 1;
                }
                if (netHeight < 0)
                {
                    blocksEnemy = -netHeight;
                }
            }

            var result = new Eval { blocksEnemy = blocksEnemy, helpsEnemy = -helpedEnemy, moveHeight = myHeight - netHeight, overBuilt = -overBuilt, buildHeight = buildHeight };
            Console.Error.WriteLine("PUSHING: " + result);
            return result;
        }

        private static Eval evaluateMove(Action action, Point[] enemyPos)
        {
            var move = GetMove(action.position, action.moveDirection);
            if (enemyPos[0].Equals(move) || enemyPos[1].Equals(move)) return new Eval { blocksEnemy = -1, helpsEnemy = -1, moveHeight = -1, overBuilt = -1, buildHeight = -1 };

            var build = GetMove(move, action.buildDirection);
            var moveHeight = GetHeight(move);
            var buildHeight = GetHeight(build);

            int overBuilt = buildHeight >= 3 ? 1 : 0;
            int helpedEnemy = 0;
            int blocksEnemy = 0;
            for (int i = 0; i < 2; i++)
            {
                int enemyHeight = GetHeight(enemyPos[i]);
                int distance = build.Distance(enemyPos[i]);
                if (enemyHeight >= 2 && distance == 1 && buildHeight == 2)
                {
                    helpedEnemy = 1;
                }
                if (enemyHeight >= 2 && distance == 1 && buildHeight == 3)
                {
                    blocksEnemy = 1;
                }
            }

            //ADDS
            //does this put me in a position to get pushed down > 1 levels
            return new Eval { blocksEnemy = blocksEnemy, helpsEnemy = -helpedEnemy, moveHeight = moveHeight, overBuilt = -overBuilt, buildHeight = buildHeight };
        }

        private static void Main(string[] args)
        {
            string[] inputs;
            _boardSize = int.Parse(Console.ReadLine());
            _board = new int[_boardSize, _boardSize];
            int unitsPerPlayer = int.Parse(Console.ReadLine());

            // game loop
            while (true)
            {
                var myLocation = new Point[2];
                var enemyLocation = new Point[2];
                var actions = new List<Action>();
                for (int i = 0; i < _boardSize; i++)
                {
                    var row = Console.ReadLine();
                    //Console.Error.WriteLine(row);
                    for (int j = 0; j < _boardSize; j++)
                    {
                        _board[j, i] = row[j] == '.' ? -1 : int.Parse(row[j].ToString());
                    }
                }
                for (int i = 0; i < unitsPerPlayer; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    myLocation[i].x = int.Parse(inputs[0]);
                    myLocation[i].y = int.Parse(inputs[1]);
                }
                for (int i = 0; i < unitsPerPlayer; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    enemyLocation[i].x = int.Parse(inputs[0]);
                    enemyLocation[i].y = int.Parse(inputs[1]);
                }

                int legalActions = int.Parse(Console.ReadLine());

                for (int i = 0; i < legalActions; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    string atype = inputs[0];
                    int index = int.Parse(inputs[1]);
                    string dir1 = inputs[2];
                    string dir2 = inputs[3];
                    actions.Add(new Action()
                    {
                        atype = atype,
                        id = index,
                        moveDirection = (Dir)Enum.Parse(typeof(Dir), dir1),
                        buildDirection = (Dir)Enum.Parse(typeof(Dir), dir2),
                        position = myLocation[index]
                    });
                }

                //Console.Error.WriteLine(string.Join(",", actions));
                Console.Error.WriteLine("current: " + string.Join(",", myLocation));

                var bestMove = new Action();
                int maxHeight = -50;
                //actions.ForEach(a => a.evaluation = evaluate(a, enemyLocation));
                for (int i = 0; i < legalActions; i++)
                {
                    var action = actions[i];
                    action.evaluation = evaluate(actions[i], enemyLocation);
                    actions[i] = action;
                }
                actions = actions.OrderByDescending(a => a.evaluation).ToList();
                //Console.Error.WriteLine(string.Join(",\n", actions));
                bestMove = actions.FirstOrDefault();
                //for (int i = 0; i < 2; i++)
                //{
                //    foreach (var action in actions.Where(a => a.id == i))
                //    {
                //        int eval = evaluate(action, enemyLocation);

                // if (eval > maxHeight) { Console.Error.WriteLine(eval); bestMove = action;
                // maxHeight = eval; } }

                // /* if (maxHeight < 3) { bool newMove = false; var neighbors =
                // Neighbors(myLocation[i]); foreach (var neighbor in neighbors) { if
                // (neighbor.Equals(enemyLocation)) { newMove = true; bestMoves[i] = new Action {
                // atype = "PUSH&BUILD", id = 0, moveDirection = GetDirection(myLocation[i],
                // neighbor) }; } }

                //         if (newMove)
                //         {
                //             var newLocation = GetMove(enemyLocation[i], bestMoves[i].moveDirection);
                //             var newNeighbors = Neighbors(newLocation);
                //             int minHeight = 4;
                //             foreach (var neighbor in newNeighbors)
                //             {
                //                 var pos = _board[neighbor.x, neighbor.y];
                //                 if (pos < minHeight)
                //                 {
                //                     minHeight = pos;
                //                     bestMoves[i].buildDirection = GetDirection(newLocation, neighbor);
                //                 }
                //             }
                //         }
                //     }*/
                //}
                Console.Error.WriteLine(bestMove.atype + ": " + bestMove + " " + evaluate(bestMove, enemyLocation));

                /*if (bestMoves[0].atype == default(string))
                {
                    Console.WriteLine(bestMoves[1]);
                }
                else
                {*/
                Console.WriteLine(bestMove);
                //}
            }
        }
    }
}