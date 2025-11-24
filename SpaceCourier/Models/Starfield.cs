using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SpaceCourier
{
    public class Starfield
    {
        private readonly List<Star> stars = new List<Star>();
        private readonly Random random = new Random(42);

        private double offsetX = 0, offsetY = 0;

        private class Star
        {
            public double X, Y;
            public double Brightness;
            public double Size;
            public double Speed;
            public double TwinklePhase;
        }

        public Starfield()
        {
            GenerateStars(480);
        }

        private void GenerateStars(int count)
        {
            stars.Clear();
            for (int i = 0; i < count; i++)
            {
                stars.Add(new Star
                {
                    X = random.NextDouble() * 2000,
                    Y = random.NextDouble() * 1200,
                    Size = random.NextDouble() * 2.2 + 0.5,
                    Speed = random.NextDouble() * 1.5 + 0.3,
                    Brightness = random.NextDouble() * 0.5 + 0.5,
                    TwinklePhase = random.NextDouble() * Math.PI * 2
                });
            }
        }

        public void Update(double playerX, double playerY)
        {
            offsetX += (playerX - 400) * 0.02;
            offsetY += (playerY - 300) * 0.02;

            if (offsetX > 800) offsetX -= 800;
            if (offsetY > 600) offsetY -= 600;
            if (offsetX < -800) offsetX += 800;
            if (offsetY < -600) offsetY += 600;

            foreach (var star in stars)
            {
                star.TwinklePhase += 0.05;
                double twinkle = Math.Sin(star.TwinklePhase) * 0.3 + 0.7;
                star.Brightness = Math.Max(0.3, Math.Min(1.0, twinkle));
            }
        }

        public void Draw(Graphics g, int width, int height)
        {
            // ГЛУБОКИЙ КОСМОС
            using (var bg = new LinearGradientBrush(
                new Rectangle(0, 0, width, height),
                Color.FromArgb(5, 10, 30),
                Color.FromArgb(0, 0, 15),
                45))
            {
                g.FillRectangle(bg, 0, 0, width, height);
            }

            // ТУМАННОСТЬ
            DrawNebula(g, width, height);

            // ЗВЁЗДЫ
            foreach (var star in stars)
            {
                double screenX = (star.X - offsetX) % width;
                double screenY = (star.Y - offsetY) % height;

                if (screenX < 0) screenX += width;
                if (screenY < 0) screenY += height;

                float alpha = (float)star.Brightness;
                float size = (float)star.Size;

                int r = (int)(170 * alpha);
                int gg = (int)(230 * alpha);
                int b = (int)(255 * alpha);

                // МЯГКИЙ ОРЕОЛ
                using (var glow = new SolidBrush(Color.FromArgb((int)(alpha * 40), r, gg, b)))
                {
                    g.FillEllipse(glow,
                        (float)screenX - size * 1.5f,
                        (float)screenY - size * 1.5f,
                        size * 3,
                        size * 3);
                }

                // ЯРКОЕ ЯДРО
                using (var core = new SolidBrush(Color.FromArgb((int)(alpha * 220), r + 30, gg + 25, b)))
                {
                    g.FillEllipse(core,
                        (float)screenX,
                        (float)screenY,
                        size,
                        size);
                }
            }
        }

        private void DrawNebula(Graphics g, int width, int height)
        {
            float cx = width * 0.7f + (float)(Math.Sin(offsetX * 0.001) * 100);
            float cy = height * 0.6f + (float)(Math.Cos(offsetY * 0.001) * 80);

            using (var path = new GraphicsPath())
            {
                path.AddEllipse(cx - 300, cy - 200, 600, 400);
                using (var pgb = new PathGradientBrush(path))
                {
                    pgb.CenterColor = Color.FromArgb(80, 60, 120, 200);
                    pgb.SurroundColors = new[] { Color.FromArgb(0, 20, 40, 100) };
                    g.FillPath(pgb, path);
                }
            }

            // Вторая туманность
            cx = width * 0.3f - (float)(Math.Sin(offsetX * 0.0015) * 80);
            cy = height * 0.4f - (float)(Math.Cos(offsetY * 0.0012) * 60);

            using (var path = new GraphicsPath())
            {
                path.AddEllipse(cx - 200, cy - 150, 400, 300);
                using (var pgb = new PathGradientBrush(path))
                {
                    pgb.CenterColor = Color.FromArgb(60, 40, 100, 180);
                    pgb.SurroundColors = new[] { Color.FromArgb(0, 10, 30, 80) };
                    g.FillPath(pgb, path);
                }
            }
        }
    }
}
