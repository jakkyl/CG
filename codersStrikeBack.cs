using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
/* 4500/27000
 * Moves from List to Arry
 *   5500, 32700
 * 
 * */
internal class Player
{
    private const int Chromosones = 4;
    private const int MaxThrust = 100;
    private const double ShieldProbability = 10.0;

    private static int _laps = 0;
    private static int _checkpointCount = 0;
    private static int round = -1;
    private static int _solutionsTried = 0;

    private static bool is_p2 = false;
    private static int turn = 0;

    private static Stopwatch _stopwatch;
    private static Checkpoint[] checkpoints;

    private static Pod[] Pods = new Pod[] { new Pod(0, 0, 0), new Pod(1, 0, 0), new Pod(2, 0, 0), new Pod(3, 0, 0) };

    private static void Main(string[] args)
    {
        string[] inputs;
        _laps = int.Parse(Console.ReadLine());
        _checkpointCount = int.Parse(Console.ReadLine());
        checkpoints = new Checkpoint[_checkpointCount];
        for (int i = 0; i < _checkpointCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int checkpointX = int.Parse(inputs[0]);
            int checkpointY = int.Parse(inputs[1]);
            checkpoints[i] = new Checkpoint(i, checkpointX, checkpointY);
        }

        Pods[0].Partner = Pods[1];
        Pods[1].Partner = Pods[0];
        Pods[2].Partner = Pods[3];
        Pods[3].Partner = Pods[2];

        var meReflex = new ReflexBot();
        var oppReflex = new ReflexBot(2);

        var opp = new SearchBot(2);
        opp.Opponents.Add(meReflex);

        var me = new SearchBot();
        me.Opponents.Add(opp);
        me.Opponents.Add(oppReflex);

        while (true)
        {
            round++;
            for (int i = 0; i < 4; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int x = int.Parse(inputs[0]); // x position of your pod
                int y = int.Parse(inputs[1]); // y position of your pod
                int vx = int.Parse(inputs[2]); // x speed of your pod
                int vy = int.Parse(inputs[3]); // y speed of your pod
                int angle = int.Parse(inputs[4]); // angle of your pod
                int nextCheckPointId = int.Parse(inputs[5]); // next check point id of your pod
                if (round == 0 && i > 1 && angle > -1) is_p2 = true;
                Pods[i].Update(x, y, vx, vy, angle, nextCheckPointId);
            }

            //Console.Error.WriteLine("DTC: {0} {1}", Pods[0].Distance(checkpoints[Pods[0].CheckpointId]), Pods[0].CheckpointId);

            _stopwatch = Stopwatch.StartNew();
            int timeLimit = round == 0 ? 980 : 142;
            opp.Solve(timeLimit * 0.15);
            me.Solve(timeLimit, round > 0);
            Console.Error.WriteLine("Moving towards {0} at angle {1} : {2}\n{3}", me.Runner().CheckpointId, me.Runner().Angle, me.sol.Angle[0], string.Join(",", me.sol.Angle));
            if (round > 0) Console.Error.WriteLine("Avg iterations {0}; avg sims {1}", _solutionsTried / round, _solutionsTried * Chromosones / round);

            me.sol.Output(me.sol.Thrust[0], me.sol.Angle[0], Pods[0]);
            me.sol.Output(me.sol.Thrust[Chromosones], me.sol.Angle[Chromosones], Pods[1]);
        }
    }

    internal class Solution
    {
        public Solution()
        {
            Thrust = new int[Chromosones * 2];
            Angle = new double[Chromosones * 2];

            for (var i = 0; i < Chromosones * 2; i++)
            {
                Mutate(i, true);
            }
            _score = -1;
        }

        public int[] Thrust { get; set; }
        public double[] Angle { get; set; }
        //public Pod Pod { get; set; }

        internal double _score = -1;

        public Point EndPoint(int thrust, double angle, Pod pod)
        {
            var a = pod.Angle + angle;

            if (a >= 360.0)
            {
                a = a - 360.0;
            }
            else if (a < 0.0)
            {
                a += 360.0;
            }

            // Look for a point corresponding to the angle we want Multiply by 10000.0 to limit
            // rounding errors
            a = a * Math.PI / 180.0;

            var px = pod.X + Math.Cos(a) * 10000.0;
            var py = pod.Y + Math.Sin(a) * 10000.0;

            return new Point(Math.Round(px), Math.Round(py));
        }

        public void Output(int thrust, double angle, Pod pod)
        {
            var p = EndPoint(thrust, angle, pod);

            if (thrust == -1)
            {
                Console.WriteLine("{0} {1} {2}", p.X, p.Y, "SHIELD");
                pod.Shield = 4;
                //activateShield();
            }
            else if (thrust == 650)
            {
                pod.HasBoost = false;
                Console.WriteLine("{0} {1} {2}", (int)p.X, (int)p.Y, "BOOST");
            }
            else
            {
                Console.WriteLine("{0} {1} {2}", (int)p.X, (int)p.Y, thrust);
            }
        }

        public void Mutate(int index, bool all = false)
        {
            int rand = MyRandom.Rnd(2);

            if (all || rand == 0)
            {
                Angle[index] = Math.Max(-18, Math.Min(18, MyRandom.Rnd(-40, 40)));
            }

            if (all || rand == 1)
            {
                if (MyRandom.Rnd(100) < ShieldProbability)
                {
                    Thrust[index] = -1;
                }
                else
                {
                    Thrust[index] = Math.Max(0, Math.Min(MaxThrust, MyRandom.Rnd((int)-0.5 * MaxThrust, 2 * MaxThrust)));
                }
            }
            _score = -1;
            //if (!all)
            //    Console.Error.WriteLine("Inside: {0}, {1}:{2}", rand,Thrust[index], Angle[index]);
        }

        public void Shift()
        {
            for (int i = 1; i < Chromosones; i++)
            {
                Angle[i - 1] = Angle[i];
                Thrust[i - 1] = Thrust[i];
                Angle[i - 1 + Chromosones] = Angle[i + Chromosones];
                Thrust[i - 1 + Chromosones] = Thrust[i + Chromosones];
            }

            Mutate(Chromosones - 1, true);
            Mutate(2 * Chromosones - 1, true);
            _score = -1;
        }

        internal void Play()
        {
            double t = 0.0;
            while (t < 1.0)
            {
                Collision firstCol = null;

                for (int i = 0; i < 4; i++)
                {
                    //check other pods
                    for (int j = i + 1; j < 4; j++)
                    {
                        var col = Pods[i].Collision(Pods[j]);
                        if (col != null && col.T + t < 1.0 && (firstCol == null || col.T < firstCol.T))
                        {
                            firstCol = col;
                        }
                    }

                    var colCP = Pods[i].Collision(checkpoints[Pods[i].CheckpointId]);
                    if (colCP != null && colCP.T + t < 1.0 && (firstCol == null || colCP.T < firstCol.T))
                    {
                        firstCol = colCP;
                    }
                }

                if (firstCol == null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Pods[i].Move(1.0 - t);
                    }
                    t = 1.0;
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Pods[i].Move(firstCol.T);
                    }

                    firstCol.UnitA.Bounce(firstCol.UnitB);

                    t += firstCol.T;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                Pods[i].End();
            }
        }

        public Solution Clone()
        {
            var s = new Solution();
            Thrust.CopyTo(s.Thrust, 0);
            Angle.CopyTo(s.Angle, 0);
            s._score = -1;
            return s;
        }
    }

    //internal class Move
    //{
    //    public Move(double angle, int thrust)
    //    {
    //        Angle = angle;
    //        Thrust = thrust;
    //    }

    //    public Move()
    //    {
    //    }

    //    public int Thrust { get; set; }
    //    public double Angle { get; set; }
    //    public bool Shield { get; set; }

    //    public override string ToString()
    //    {
    //        return string.Format("[{0},{1}]", Angle, Thrust);
    //    }
    //}

    internal class Collision
    {
        public Collision(Unit unitA, Unit unitB, double t)
        {
            UnitA = unitA;
            UnitB = unitB;
            T = t;
        }

        public Unit UnitA { get; set; }
        public Unit UnitB { get; set; }
        public double T { get; set; }
    }

    internal class Pod : Unit
    {
        public Pod(double x, double y)
            : base(x, y)
        {
        }

        public Pod(int id, double x, double y)
            : base(x, y)
        {
            Id = id;
        }

        public Pod(int id, double x, double y, double vx, double vy, double angle, int checkpointId)
            : base(x, y, vx, vy)
        {
            Id = id;
            Angle = angle;
            CheckpointId = checkpointId;
            Checked = 0;
            Timeout = 100;
            Radius = 400;
            Shield = 0;
            HasBoost = true;
        }

        private double[] cache = new double[9];

        public int CheckpointId { get; set; }
        public int Checked { get; set; }
        public int Timeout { get; set; }
        public int Thrust { get; set; }
        //public Solution Solution { get; set; }
        //public List<Move> Moves { get; set; }
        public Pod Partner { get; set; }
        public bool HasBoost { get; internal set; }

        private int _nextAngle = -1;

        internal void Apply(int thrust, double angle)
        {
            Rotate(angle);
            Thrust = thrust;
            if (thrust == -1)
                Shield = 4;
            else
                Boost(thrust);
        }

        internal void Rotate(double a)
        {
            // Can't turn by more than 18� in one turn
            a = Math.Max(-18, Math.Min(18, a));

            Angle += a;

            if (Angle >= 360)
                Angle = Angle - 360;
            else if (Angle < 0.0)
                Angle += 360;
        }

        internal void Rotate(Unit unit)
        {
            var a = DiffAngle(unit);
            Rotate(a);
        }

        internal void Boost(int thrust)
        {
            if (Shield > 0) return;

            var ra = Angle * Math.PI / 180;

            VelocityX += Math.Cos(ra) * thrust;
            VelocityY += Math.Sin(ra) * thrust;
        }

        internal void Move(double t)
        {
            X += VelocityX * t;
            Y += VelocityY * t;
        }

        internal void End()
        {
            X = Math.Round(X);
            Y = Math.Round(Y);
            VelocityX = Math.Truncate(VelocityX * 0.85);
            VelocityY = Math.Truncate(VelocityY * 0.85);

            if (Checked >= _checkpointCount * _laps)
            {
                CheckpointId = 0;
                Checked = _checkpointCount * _laps;
            }

            Timeout--;
            if (Shield > 0) Shield--;
        }

        public override void Bounce(Unit unit)
        {
            //Console.Error.WriteLine("Bounce {0} {1}", Id, unit.Id);
            if (unit is Checkpoint)
            {
                Checked++;
                Timeout = Partner.Timeout = 100;
                CheckpointId = (CheckpointId + 1) % _checkpointCount;
                return;
            }

            // If a pod has its shield active its mass is 10 otherwise it's 1
            var m1 = Shield == 4 ? 10 : 1;
            var m2 = unit.Shield == 4 ? 10 : 1;
            var mcoeff = (m1 + m2) / (m1 * m2);

            var nx = X - unit.X;
            var ny = Y - unit.Y;

            // Square of the distance between the 2 pods. This value could be hardcoded because it is
            // always 800²
            var nxnysquare = nx * nx + ny * ny;

            var dvx = VelocityX - unit.VelocityX;
            var dvy = VelocityY - unit.VelocityY;

            // fx and fy are the components of the impact vector. product is just there for
            // optimisation purposes
            var product = (nx * dvx + ny * dvy) / (nxnysquare * mcoeff);
            var fx = nx * product;
            var fy = ny * product;
            var m1Inverse = 1.0 / m1;
            var m2Inverse = 1.0 / m2;

            // We apply the impact vector once
            VelocityX -= fx * m1Inverse;
            VelocityY -= fy * m1Inverse;
            unit.VelocityX += fx * m2Inverse;
            unit.VelocityY += fy * m2Inverse;

            // If the norm of the impact vector is less than 120, we normalize it to 120
            var impulse = Math.Sqrt(fx * fx + fy * fy);
            if (impulse < 120.0)
            {
                var df = 120.0 / impulse;
                fx = fx * df;
                fy = fy * df;
            }

            // We apply the impact vector a second time
            VelocityX -= fx * m1Inverse;
            VelocityY -= fy * m1Inverse;
            unit.VelocityX += fx * m2Inverse;
            unit.VelocityY += fy * m2Inverse;

            // This is one of the rare places where a Vector class would have made the code more
            // readable. But this place is called so often that I can't pay a performance price to
            // make it more readable.
        }

        internal double Score()
        {
            //Console.Error.WriteLine("{0} {1} {2} {3} {4} {5}", Checked, Distance(Checkpoint), Checkpoint.X, Checkpoint.Y, X, Y);
            return Checked * 50000 - Distance(checkpoints[CheckpointId]);
        }

        public override void Save()
        {
            base.Save();
            cache[0] = CheckpointId;
            cache[1] = Checked;
            cache[2] = Timeout;
            cache[3] = Shield;
            cache[4] = Angle;
            cache[5] = Convert.ToDouble(HasBoost);
        }

        public override void Load()
        {
            base.Load();
            CheckpointId = (int)cache[0];
            Checked = (int)cache[1];
            Timeout = (int)cache[2];
            Shield = (int)(cache[3]);
            Angle = cache[4];
            HasBoost = Convert.ToBoolean(cache[5]);
        }

        internal void Update(int x, int y, int vx, int vy, int angle, int nextCheckPointId)
        {
            if (Shield > 0) Shield--;
            if (CheckpointId != nextCheckPointId)
            {
                Timeout = Partner.Timeout = 100;
                Checked++;
            }
            else
            {
                Timeout--;
            }

            X = x;
            Y = y;
            VelocityX = vx;
            VelocityY = vy;
            CheckpointId = nextCheckPointId;

            if (is_p2 && Id > 1)
            {
                var temp = angle;
                angle = _nextAngle;
                _nextAngle = temp;
            }
            Angle = angle;
            if (round == 0) Angle = 1 + DiffAngle(checkpoints[1]);

            Save();
        }
    }

    class Bot
    {
        public int Id { get; set; }
        public Bot() { Id = 0; }
        public Bot(int id)
        {
            Id = id;
        }

        internal Pod Runner()
        {
            return Runner(Pods[Id], Pods[Id + 1]);
        }
        internal Pod Runner(Pod pod0, Pod pod1)
        {
            return pod0.Score() - pod1.Score() >= -1000 ? pod0 : pod1;
        }

        internal Pod Blocker()
        {
            return Blocker(Pods[Id], Pods[Id + 1]);
        }
        internal Pod Blocker(Pod pod0, Pod pod1)
        {
            return Runner(pod0, pod1).Partner;
        }

        internal virtual void Move() { }
    }

    class ReflexBot : Bot
    {
        public ReflexBot() : base() { }
        public ReflexBot(int id) : base(id) { }

        internal override void Move()
        {
            MoveRunner(true);
            MoveRunner(false);
        }

        internal void MoveRunner(bool isRunner)
        {
            var pod = isRunner ? Runner() : Blocker();
            var cp = checkpoints[pod.CheckpointId];
            var t = new Unit(cp.X - 3 * pod.VelocityX, cp.Y - 3 * pod.VelocityY);
            var rawAngle = pod.DiffAngle(t);

            int thrust = Math.Abs(rawAngle) < 90 ? MaxThrust : 0;
            var angle = Math.Max(-18, Math.Min(18, rawAngle));

            pod.Apply(thrust, angle);
        }

    }

    class SearchBot : Bot
    {
        public Solution sol { get; set; }
        public List<Bot> Opponents { get; set; }

        public SearchBot() : base() { Opponents = new List<Bot>(); }
        public SearchBot(int id) : base(id) { Opponents = new List<Bot>(); }

        internal void Move(Solution sol)
        {
            Pods[Id].Apply(sol.Thrust[turn], sol.Angle[turn]);
            Pods[Id + 1].Apply(sol.Thrust[turn + Chromosones], sol.Angle[turn + Chromosones]);
        }

        internal override void Move()
        {
            Move(sol);
        }

        internal void Solve(double timeLimit, bool hasSeed = false)
        {
            Solution bestSolution;
            if (hasSeed)
            {
                bestSolution = sol;
                bestSolution.Shift();
            }
            else
            {
                bestSolution = sol = new Solution();

                if (round == 0 && Pods[Id].Distance(checkpoints[1]) > 4000)
                {
                    bestSolution.Thrust[0] = 650;
                }
            }
            GetScore(bestSolution);

            Solution child;
            while (_stopwatch.ElapsedMilliseconds < timeLimit)
            {
                //Console.Error.WriteLine("0: {0}", _stopwatch.ElapsedMilliseconds);
                child = bestSolution.Clone();
                //Console.Error.WriteLine("1: {0}", _stopwatch.ElapsedMilliseconds);
                child.Mutate(MyRandom.Rnd(2 * Chromosones));
                //Console.Error.WriteLine("2: {0}", _stopwatch.ElapsedMilliseconds);

                if (GetScore(child) > GetScore(bestSolution))
                {
                    //Console.Error.WriteLine("2: {0}", _stopwatch.ElapsedMilliseconds);
                    bestSolution = child;
                }
            }
        }

        private double GetScore(Solution sol)
        {
            if (sol._score == -1)
            {
                var scores = new List<double>();
                foreach (var bot in Opponents)
                {
                    scores.Add(GetBotScore(sol, bot));
                }

                sol._score = scores.Min();
            }

            return sol._score;
        }

        private double GetBotScore(Solution sol, Bot opponent)
        {
            double score = 0.0;
            while (turn < Chromosones)
            {
                Move(sol);
                opponent.Move();
                sol.Play();
                if (turn == 0) score += 0.1 * Evaluation();
                turn++;
            }
            score += 0.9 * Evaluation();
            for (int i = 0; i < 4; i++) Pods[i].Load();
            turn = 0;

            if (round > 0) _solutionsTried++;

            return score;
        }

        private double Evaluation()
        {
            Pod my_runner = Runner(Pods[Id], Pods[Id + 1]);
            Pod my_blocker = Blocker(Pods[Id], Pods[Id + 1]);
            Pod opp_runner = Runner(Pods[(Id + 2) % 4], Pods[(Id + 3) % 4]);
            Pod opp_blocker = Blocker(Pods[(Id + 2) % 4], Pods[(Id + 3) % 4]);

            if (my_runner.Timeout <= 0) return -1e7;
            if (opp_runner.Timeout <= 0) return 1e7;
            if (opp_runner.Checked == _laps * _checkpointCount || opp_blocker.Checked == _laps * _checkpointCount) return -1e7;
            if (my_runner.Checked == _laps * _checkpointCount || my_blocker.Checked == _laps * _checkpointCount) return 1e7;

            var score = my_runner.Score() - opp_runner.Score();
            score -= my_blocker.Distance(checkpoints[opp_runner.CheckpointId]);
            score -= Math.Abs(my_blocker.DiffAngle(opp_runner));

            return score;
        }
    }
    internal class Checkpoint : Unit
    {
        public Checkpoint(int id, double x, double y)
            : base(x, y)
        {
            Id = id;
            Radius = 600;
        }
    }

    internal class Unit : Point
    {
        public Unit(double x, double y)
            : base(x, y)
        {
        }

        public Unit(double x, double y, double vx, double vy)
            : base(x, y)
        {
            VelocityX = vx;
            VelocityY = vy;
        }

        public int Id { get; set; }
        public double Radius { get; set; }
        public double VelocityX { get; set; }
        public double VelocityY { get; set; }
        public double Angle { get; set; }
        public int Shield { get; set; }

        private double[] cache = new double[4];

        public virtual void Bounce(Unit unit) { }

        public Collision Collision(Unit unit)
        {
            //same speed will never collide
            if (VelocityX == unit.VelocityX && VelocityY == unit.VelocityY) return null;

            var distance = Distance2(unit);
            var sumOfRadii = unit is Checkpoint ? 357604 : 640000;//Math.Pow(Radius + unit.Radius, 2);

            //already touching
            //if (distance < sumOfRadii) return new Collision(this, unit, 0.0);


            // We place ourselves in the reference frame of u. u is therefore stationary and is at (0,0)
            var x = X - unit.X;
            var y = Y - unit.Y;
            Point myp = new Point(x, y);
            var vx = VelocityX - unit.VelocityX;
            var vy = VelocityY - unit.VelocityY;
            var a = vx * vx + vy * vy;
            if (a < 0.00001) return null;

            var b = -2.0 * (x * vx + y * vy);



            var delta = b * b - 4.0 * a * (x * x + y * y - sumOfRadii);

            if (delta < 0.0) return null;

            var t = (b - Math.Sqrt(delta)) * (1.0 / 2.0 * a);
            if (t <= 0.0 || t > 1.0) return null;

            return new Collision(this, unit, t);


            /*
            Point up = new Point(0, 0);

            // We look for the closest point to u (which is in (0,0)) on the line described by our
            // speed vector
            Point p = up.Closest(myp, new Point(x + vx, y + vy));

            // Square of the distance between u and the closest point to u on the line described by
            // our speed vector
            var pdist = up.Distance2(p);

            // Square of the distance between us and that point
            var mypdist = myp.Distance2(p);

            // If the distance between u and this line is less than the sum of the radii, there might
            // be a collision
            if (pdist < sumOfRadii)
            {
                // Our speed on the line
                var length = Math.Sqrt(vx * vx + vy * vy);

                // We move along the line to find the point of impact
                var backdist = Math.Sqrt(sumOfRadii - pdist);
                p.X = p.X - backdist * (vx / length);
                p.Y = p.Y - backdist * (vy / length);

                // If the point is now further away it means we are not going the right way,
                // therefore the collision won't happen
                if (myp.Distance2(p) > mypdist)
                {
                    return null;
                }

                pdist = p.Distance(myp);

                // The point of impact is further than what we can travel in one turn
                if (pdist > length)
                {
                    return null;
                }

                // Time needed to reach the impact point
                var t = pdist / length;

                return new Collision(this, unit, t);
            }

            return null;*/
        }

        internal double DiffAngle(Unit unit)
        {
            var a = GetAngle(unit);

            // To know whether we should turn clockwise or not we look at the two ways and keep the smallest
            var right = Angle <= a ? a - Angle : 360 - Angle + a;
            var left = Angle >= a ? Angle - a : Angle + 360 - a;

            if (right < left)
                return right;

            return -left;
        }

        private double GetAngle(Unit unit)
        {
            var d = Distance(unit);
            var dx = (unit.X - X) / d;
            var dy = (unit.Y - Y) / d;

            // multiply by 180.0 / PI to convert radiants to degrees.
            var a = Math.Acos(dx) * 180 / Math.PI;

            // If the point I want is below me, I have to shift the angle for it to be correct
            if (dy < 0)
            {
                a = 360 - a;
            }

            return a;
        }

        public virtual void Save()
        {
            cache[0] = X;
            cache[1] = Y;
            cache[2] = VelocityX;
            cache[3] = VelocityY;
        }

        public virtual void Load()
        {
            X = cache[0];
            Y = cache[1];
            VelocityX = cache[2];
            VelocityY = cache[3];
        }
    }

    internal class Point
    {
        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; set; }

        public double Y { get; set; }

        public double Distance2(Point p)
        {
            return Math.Pow(X - p.X, 2) + Math.Pow(Y - p.Y, 2);
        }

        public double Distance(Point p)
        {
            return Math.Sqrt(Distance2(p));
        }

        public Point Closest(Point a, Point b)
        {
            var deltaA = b.Y - a.Y;
            var deltaB = a.X - b.X;
            var c1 = deltaA * a.X + deltaB * a.Y;
            var c2 = -deltaB * X + deltaA * Y;
            var det = deltaA * deltaA + deltaB * deltaB;
            double cY = 0;
            double cX = 0;

            if (det != 0)
            {
                cX = (deltaA * c1 - deltaB * c2) / det;
                cY = (deltaA * c2 + deltaB * c1) / det;
            }
            else
            {
                cX = X;
                cY = Y;
            }

            return new Point(cX, cY);
        }
    }

    static int g_seed = 42;

    internal class MyRandom
    {
        //private static Random ran = new Random();

        //public static double GetRandom(double min, double max)
        //{
        //    //return ran.NextDouble() * (max - min) + min;
        //    return Rnd((int)min, (int)max);
        //}

        public static int fastrand()
        {
            g_seed = (214013 * g_seed + 2531011);
            return (g_seed >> 16) & 0x7FFF;
        }

        public static int Rnd(int b)
        {
            return fastrand() % b;
        }

        public static int Rnd(int a, int b)
        {
            return a + Rnd(b - a + 1);
        }
    }
}