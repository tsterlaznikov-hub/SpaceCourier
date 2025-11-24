using System;

namespace SpaceCourier
{
    public class Vector
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Vector(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double DistanceTo(Vector other)
        {
            return Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
        }

        public static Vector operator +(Vector a, Vector b) =>
            new Vector(a.X + b.X, a.Y + b.Y);

        public static Vector operator -(Vector a, Vector b) =>
            new Vector(a.X - b.X, a.Y - b.Y);
    }
}
