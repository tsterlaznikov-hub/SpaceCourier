using System;

namespace SpaceCourier.Models
{
    public class Decoy
    {
        public Vector Position { get; private set; }
        public double Lifetime { get; private set; } = 360; // 6 секунд при 60 FPS
        public double Alpha { get; private set; } = 1.0;
        public double Pulse { get; private set; } = 1.0;

        private static readonly Random rnd = new Random();

        public Decoy(Vector playerPos)
        {
            double angle = rnd.NextDouble() * Math.PI * 2;
            double dist = 70 + rnd.NextDouble() * 70;
            Position = new Vector(
                playerPos.X + Math.Cos(angle) * dist,
                playerPos.Y + Math.Sin(angle) * dist
            );
        }

        public void Update()
        {
            Lifetime--;
            Alpha = 0.3 + 0.7 * (Math.Sin(Lifetime * 0.15) + 1) / 2;
            Pulse = 0.8 + 0.4 * Math.Sin(Lifetime * 0.2);
        }

        public bool IsExpired => Lifetime <= 0;
    }
}