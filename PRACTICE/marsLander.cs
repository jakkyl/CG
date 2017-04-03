using System;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/

namespace MarsLander
{
    internal class Player
    {
        private const int ZoneWidth = 7000;
        private const int ZoneHeight = 3000;
        private const int MinFlatSurface = 1000;
        private const double Gravity = 3.711;
        private const double MaxVerticalSpeed = 40.0;
        private const double MaxHorizontalSpeed = 20.0;

        private static IList<Point> map = new List<Point>();

        private class Point
        {
            public double X { get; set; }
            public double Y { get; set; }

            public Point(double x, double y)
            {
                X = x;
                Y = y;
            }
        }

        private static void Main(string[] args)
        {
            string[] inputs;
            int surfaceN = int.Parse(Console.ReadLine()); // the number of points used to draw the surface of Mars.
            map.Add(new Point(0, 0));
            for (int i = 0; i < surfaceN; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int landX = int.Parse(inputs[0]); // X coordinate of a surface point. (0 to 6999)
                int landY = int.Parse(inputs[1]); // Y coordinate of a surface point. By linking all the points together in a sequential fashion, you form the surface of Mars.
                map.Add(new Point(landX, landY));
            }
            map.Add(new Point(ZoneWidth - 1, 0));

            // game loop
            while (true)
            {
                inputs = Console.ReadLine().Split(' ');
                int X = int.Parse(inputs[0]);
                int Y = int.Parse(inputs[1]);
                int hSpeed = int.Parse(inputs[2]); // the horizontal speed (in m/s), can be negative.
                int vSpeed = int.Parse(inputs[3]); // the vertical speed (in m/s), can be negative.
                int fuel = int.Parse(inputs[4]); // the quantity of remaining fuel in liters.
                int rotate = int.Parse(inputs[5]); // the rotation angle in degrees (-90 to 90).
                int power = int.Parse(inputs[6]); // the thrust power (0 to 4).

                // Write an action using Console.WriteLine() To debug: Console.Error.WriteLine("Debug messages...");

                if (vSpeed < -40 && power < 4)
                {
                    power += 1;
                }

                // 2 integers: rotate power. rotate is the desired rotation angle, power is the
                // desired thrust power (0 to 4).
                Console.WriteLine(rotate + " " + power);
            }
        }
    }
}