using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SpaceCourier.Controllers;
using SpaceCourier.Models;

namespace SpaceCourier.Views
{
    public class GameForm : Form, IGameView
    {
        private GameController controller;
        private readonly Timer gameTimer = new Timer { Interval = 16 };

        private Image heartImage;
        private Image decoyImage;
        private Image spaceshipImage;
        private Image planetImage;

        private readonly SolidBrush stationBrush = new SolidBrush(Color.FromArgb(100, 100, 150));
        private readonly SolidBrush energyBrush = new SolidBrush(Color.FromArgb(0, 255, 100));
        private readonly SolidBrush bgEnergyBrush = new SolidBrush(Color.FromArgb(0, 80, 30));

        private float planetRotation = 0f;
        private int damageFlashTimer = 0;
        private Button restartButton;

        public GameForm()
        {
            ClientSize = new Size(800, 600);
            Text = "Космический Курьер";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.Black;
            DoubleBuffered = true;
            KeyPreview = true;

            LoadImagesFromAssets();
            controller = new GameController(this);

            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();

            KeyDown += GameForm_KeyDown;
            KeyUp += GameForm_KeyUp;

            restartButton = new Button
            {
                Text = "НОВАЯ ИГРА",
                Font = new Font("Arial", 20, FontStyle.Bold),
                ForeColor = Color.Lime,
                BackColor = Color.FromArgb(100, 0, 0, 0),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(300, 80),
                Visible = false
            };
            restartButton.FlatAppearance.BorderSize = 3;
            restartButton.FlatAppearance.BorderColor = Color.Lime;
            restartButton.Location = new Point(250, 400);
            restartButton.Click += (s, e) => RestartGame();
            Controls.Add(restartButton);
        }

        private void GameTimer_Tick(object sender, EventArgs e) => controller.Update();
        private void GameForm_KeyDown(object sender, KeyEventArgs e) { controller.HandleKeyDown(e.KeyCode); e.Handled = true; }
        private void GameForm_KeyUp(object sender, KeyEventArgs e) => controller.HandleKeyUp(e.KeyCode);

        private void LoadImagesFromAssets()
        {
            string assetsPath = Path.Combine(Application.StartupPath, "Assets");
            heartImage = LoadImage(Path.Combine(assetsPath, "heart.png"));
            decoyImage = LoadImage(Path.Combine(assetsPath, "decoy.png"));
            spaceshipImage = LoadImage(Path.Combine(assetsPath, "spaceship.png"));
            planetImage = LoadImage(Path.Combine(assetsPath, "planet.png"));
        }

        private Image LoadImage(string path) => File.Exists(path) ? Image.FromFile(path) : null;

        public void InvalidateView() => Invalidate();
        public void ShowEndScreen(string message, Color color) => Invalidate();

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;

            var model = controller.Model;

            if (damageFlashTimer > 0)
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(120, 255, 0, 0)), 0, 0, Width, Height);
                damageFlashTimer--;
            }

            model.Starfield.Draw(g, ClientSize.Width, ClientSize.Height);

            DrawStation(g, model.StationPosition);
            DrawPlanet(g, model.PlanetPosition);

            var enemies = model.EnemiesReadOnly.ToList();
            for (int i = 0; i < enemies.Count; i++)
                DrawEnemy(g, enemies[i], i + 1);

            // === ПРИМАНКИ С БЕЛЫМ ПУЛЬСИРУЮЩИМ КРУЖКОМ ===
            foreach (var decoy in model.DecoysReadOnly)
                DrawDecoy(g, decoy);

            DrawPlayer(g, model.Player);

            if (enemies.Any(en => en.IsAlerted))
            {
                float px = (float)model.Player.Position.X;
                float py = (float)model.Player.Position.Y;
                g.DrawString("НАС ОБНАРУЖИЛИ!", new Font("Arial", 14, FontStyle.Bold), Brushes.Red, px - 70, py - 70);
            }

            DrawUI(g, model.Player);

            if (model.IsPlayerInvincible)
            {
                float px = (float)model.Player.Position.X;
                float py = (float)model.Player.Position.Y;
                int alpha = 150 + (int)(105 * Math.Sin(Environment.TickCount / 70.0));
                using (var ring = new SolidBrush(Color.FromArgb(alpha, 0, 255, 255)))
                    g.FillEllipse(ring, px - 55, py - 55, 110, 110);
            }

            if (controller.CurrentState != GameState.Playing)
            {
                DrawEndScreen(g, controller.StatusMessage, controller.StatusColor);
                restartButton.Visible = true;
            }
            else
            {
                restartButton.Visible = false;
            }
        }

        // === КРАСИВАЯ ПРИМАНКА С БЕЛЫМ КРУЖКОМ ===
        private void DrawDecoy(Graphics g, Decoy decoy)
        {
            float x = (float)decoy.Position.X;
            float y = (float)decoy.Position.Y;
            float baseSize = 36f * (float)decoy.Pulse;

            Color mainColor = Color.FromArgb((int)(255 * decoy.Alpha), 255, 255, 120);

            // Основная приманка
            if (decoyImage != null)
            {
                var matrix = new ColorMatrix { Matrix33 = (float)decoy.Alpha };
                var attrs = new ImageAttributes();
                attrs.SetColorMatrix(matrix);
                g.DrawImage(decoyImage,
                    new Rectangle((int)(x - baseSize / 2), (int)(y - baseSize / 2), (int)baseSize, (int)baseSize),
                    0, 0, decoyImage.Width, decoyImage.Height, GraphicsUnit.Pixel, attrs);
            }
            else
            {
                using (var brush = new SolidBrush(mainColor))
                {
                    PointF[] p = {
                        new PointF(x, y - baseSize * 0.35f),
                        new PointF(x - baseSize * 0.28f, y + baseSize * 0.28f),
                        new PointF(x + baseSize * 0.28f, y + baseSize * 0.28f)
                    };
                    g.FillPolygon(brush, p);
                    g.DrawPolygon(Pens.White, p);
                }
            }

            // БЕЛЫЙ ПУЛЬСИРУЮЩИЙ КРУЖОК
            float ringRadius = baseSize * 1.8f + (float)(Math.Sin(Environment.TickCount * 0.01) * 15);
            int ringAlpha = (int)(80 + 100 * decoy.Alpha);

            using (var ringBrush = new SolidBrush(Color.FromArgb(ringAlpha, 255, 255, 255)))
            {
                g.FillEllipse(ringBrush, x - ringRadius, y - ringRadius, ringRadius * 2, ringRadius * 2);
            }

            using (var pen = new Pen(Color.FromArgb((int)(200 * decoy.Alpha), 255, 255, 255), 3))
            {
                g.DrawEllipse(pen, x - ringRadius, y - ringRadius, ringRadius * 2, ringRadius * 2);
            }

            // Яркий блик в центре
            using (var glow = new SolidBrush(Color.FromArgb((int)(255 * decoy.Alpha), 255, 255, 255)))
            {
                g.FillEllipse(glow, x - 6, y - 6, 12, 12);
            }
        }

        private void DrawStation(Graphics g, Vector pos)
        {
            float x = (float)pos.X - 25;
            float y = (float)pos.Y - 25;
            g.FillRectangle(stationBrush, x, y, 50, 50);
            g.DrawString("Станция", new Font("Arial", 10), Brushes.White, x, y - 20);
        }

        private void DrawPlanet(Graphics g, Vector pos)
        {
            float x = (float)pos.X;
            float y = (float)pos.Y;
            const float size = 140f;
            planetRotation += 0.002f;
            if (planetRotation > Math.PI * 2) planetRotation -= (float)Math.PI * 2;

            if (planetImage != null)
            {
                var state = g.Save();
                g.TranslateTransform(x, y);
                g.RotateTransform(planetRotation * 180f / (float)Math.PI);
                g.TranslateTransform(-x, -y);
                g.DrawImage(planetImage, x - size / 2, y - size / 2, size, size);
                g.Restore(state);
            }
            else
            {
                using (var brush = new SolidBrush(Color.FromArgb(50, 200, 50)))
                    g.FillEllipse(brush, x - 70, y - 70, 140, 140);
            }
        }

        private void DrawEnemy(Graphics g, Enemy enemy, int number)
        {
            float ex = (float)enemy.Position.X;
            float ey = (float)enemy.Position.Y;
            const float size = 50f;

            var state = g.Save();
            g.TranslateTransform(ex, ey);
            g.RotateTransform((float)enemy.Rotation * 180f / (float)Math.PI + 180f);
            g.TranslateTransform(-ex, -ey);

            PointF[] points = { new PointF(ex, ey - 15), new PointF(ex - 12, ey + 12), new PointF(ex + 12, ey + 12) };
            g.FillPolygon(enemy.IsAlerted ? Brushes.DarkRed : Brushes.Red, points);
            g.Restore(state);

            if (enemy.SuspicionLevel > 0)
            {
                float dist = (float)enemy.ViewDistance;
                using (var cone = new SolidBrush(Color.FromArgb(70, enemy.IsAlerted ? Color.OrangeRed : Color.Orange)))
                    g.FillEllipse(cone, ex - dist, ey - dist, dist * 2, dist * 2);
            }

            g.DrawString($"Патруль {number}", new Font("Arial", 8, FontStyle.Bold), Brushes.Yellow, ex - 30, ey - 38);
        }

        private void DrawPlayer(Graphics g, Player player)
        {
            float px = (float)player.Position.X;
            float py = (float)player.Position.Y;
            const float size = 48f;

            if (spaceshipImage != null)
            {
                float x = px - size / 2;
                float y = py - size / 2;
                var state = g.Save();
                g.TranslateTransform(px, py);
                g.RotateTransform((float)player.Direction * 180f / (float)Math.PI + 90f);
                g.TranslateTransform(-px, -py);

                if (player.IsShieldActive)
                    using (var b = new SolidBrush(Color.FromArgb(100, 255, 255, 0)))
                        g.FillEllipse(b, px - size - 10, py - size - 10, size * 2 + 20, size * 2 + 20);

                if (player.IsCloaked)
                {
                    var matrix = new ColorMatrix { Matrix33 = 0.4f };
                    var attrs = new ImageAttributes();
                    attrs.SetColorMatrix(matrix);
                    g.DrawImage(spaceshipImage, new Rectangle((int)x, (int)y, (int)size, (int)size),
                        0, 0, spaceshipImage.Width, spaceshipImage.Height, GraphicsUnit.Pixel, attrs);
                }
                else
                {
                    g.DrawImage(spaceshipImage, x, y, size, size);
                }
                g.Restore(state);
            }
            else
            {
                var brush = player.IsCloaked ? Brushes.LightBlue : (player.IsShieldActive ? Brushes.Yellow : Brushes.Cyan);
                PointF[] p = { new PointF(px, py - 18), new PointF(px - 15, py + 15), new PointF(px + 15, py + 15) };
                g.FillPolygon(brush, p);
                g.DrawPolygon(Pens.White, p);
            }
        }

        private void DrawUI(Graphics g, Player player)
        {
            int baseX = Width - 210;
            int baseY = 20;

            g.FillRectangle(bgEnergyBrush, baseX, baseY, 200, 28);
            g.FillRectangle(energyBrush, baseX, baseY, 200 * (float)(player.Energy / player.MaxEnergy), 28);
            g.DrawString($"Энергия: {player.Energy:F0}%", new Font("Arial", 11, FontStyle.Bold), Brushes.White, baseX + 5, baseY + 4);

            for (int i = 0; i < player.Health; i++)
                DrawIcon(g, heartImage, baseX + i * 52, baseY + 50, 48);

            for (int i = 0; i < player.Decoys; i++)
                DrawIcon(g, decoyImage, baseX + i * 52, baseY + 110, 48);

            g.DrawString("WASD — движение | Shift — маскировка | Пробел — щит | E — приманка",
                new Font("Arial", 9), Brushes.LightGray, 10, Height - 35);
        }

        private void DrawIcon(Graphics g, Image img, float x, float y, float size)
        {
            if (img != null)
                g.DrawImage(img, x, y, size, size);
            else
                g.FillEllipse(Brushes.Gray, x + 8, y + 8, size - 16, size - 16);
        }

        private void DrawEndScreen(Graphics g, string message, Color color)
        {
            var font = new Font("Arial", 40, FontStyle.Bold);
            var sz = g.MeasureString(message, font);
            float cx = (Width - sz.Width) / 2f;
            float cy = (Height - sz.Height) / 2f;

            g.FillRectangle(new SolidBrush(Color.FromArgb(230, 0, 0, 0)), cx - 50, cy - 40, sz.Width + 100, sz.Height + 80);
            g.DrawRectangle(Pens.White, cx - 50, cy - 40, sz.Width + 100, sz.Height + 80);
            g.DrawString(message, font, new SolidBrush(color), cx, cy);
        }

        private void RestartGame()
        {
            controller = new GameController(this);
            restartButton.Visible = false;
            Invalidate();
        }
    }
}