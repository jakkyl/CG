using System;

namespace Solution.HELPERS
{
    public class Point
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

        public double Dot(Point p)
        {
            return X * p.X + p.Y * Y;
        }

        public Point Subtract(Point p)
        {
            return new Point(p.X - X, p.Y - Y);
        }

        public override bool Equals(object obj)
        {
            if (obj is Point)
                return X == (obj as Point).X && Y == (obj as Point).Y;
            else
                return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash *= 23 + X.GetHashCode();
                hash *= 23 + Y.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return string.Format("[{0},{1}]", X, Y);
        }
    }
}