using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SpaceCourier.Models;

namespace SpaceCourier.Views
{
    public class StartForm : Form
    {
        private float opacity = 0f;
        private readonly Starfield starfield = new Starfield();
        private GameForm gameForm;

        private readonly Rectangle btnStart = new Rectangle(250, 340, 300, 70);
        private readonly Rectangle btnExit = new Rectangle(250, 430, 300, 70);

        public StartForm()
        {
            Text = "Космический Курьер";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(800, 600);
            DoubleBuffered = true;
            MaximizeBox = false;
            BackColor = Color.Black;

            // Предзагрузка игры
            gameForm = new GameForm();
            gameForm.Visible = false;

            // Анимация появления
            var timer = new Timer { Interval = 20 };
            timer.Tick += (s, e) =>
            {
                opacity += 0.05f;
                if (opacity >= 1f) { opacity = 1f; timer.Stop(); }
                Invalidate();
            };
            timer.Start();

            MouseMove += (s, e) => Invalidate();
            Click += (s, e) =>
            {
                Point p = PointToClient(MousePosition);
                if (btnStart.Contains(p)) StartGame();
                if (btnExit.Contains(p)) Application.Exit();
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Фон
            starfield.Draw(g, Width, Height);

            // Оверлей
            using (var brush = new SolidBrush(Color.FromArgb((int)(100 * opacity), 0, 0, 60)))
                g.FillRectangle(brush, 0, 0, Width, Height);

            // Заголовок — ПОЛНОСТЬЮ ВЛЕЗАЕТ (шрифт 44)
            string title = "КОСМИЧЕСКИЙ КУРЬЕР";
            using (var font = new Font("Arial", 44, FontStyle.Bold)) // ← УМЕНЬШИЛ ДО 44
            {
                var sz = g.MeasureString(title, font);
                float x = (Width - sz.Width) / 2;
                float y = 100; // ← Подвинул выше

                // Тень
                using (var shadow = new SolidBrush(Color.FromArgb((int)(255 * opacity), 0, 100, 200)))
                    g.DrawString(title, font, shadow, x + 3, y + 3); // Тоньше тень

                // Текст
                using (var main = new SolidBrush(Color.FromArgb((int)(255 * opacity), 100, 255, 255)))
                    g.DrawString(title, font, main, x, y);
            }

            // Подзаголовок
            string sub = "Стелс-миссия в глубинах космоса";
            using (var font = new Font("Arial", 20))
            {
                var sz = g.MeasureString(sub, font);
                float x = (Width - sz.Width) / 2;
                using (var brush = new SolidBrush(Color.FromArgb((int)(200 * opacity), 200, 255, 255)))
                    g.DrawString(sub, font, brush, x, 210);
            }

            // Кнопки
            DrawButton(g, "НАЧАТЬ ИГРУ", btnStart, btnStart.Contains(PointToClient(MousePosition)));
            DrawButton(g, "ВЫХОД", btnExit, btnExit.Contains(PointToClient(MousePosition)));
        }

        private void DrawButton(Graphics g, string text, Rectangle rect, bool hovered)
        {
            int alpha = (int)(255 * opacity);

            // Фон
            Color bg = hovered
                ? Color.FromArgb(alpha, 0, 180, 255)
                : Color.FromArgb(alpha, 0, 120, 220);
            using (var path = RoundedRect(rect, 20))
            using (var brush = new SolidBrush(bg))
                g.FillPath(brush, path);

            // Обводка
            using (var path = RoundedRect(rect, 20))
            using (var pen = new Pen(Color.FromArgb(alpha, 100, 255, 255), 4))
                g.DrawPath(pen, path);

            // Текст
            using (var font = new Font("Arial", 28, FontStyle.Bold))
            {
                var sz = g.MeasureString(text, font);
                float tx = rect.X + (rect.Width - sz.Width) / 2;
                float ty = rect.Y + (rect.Height - sz.Height) / 2;
                g.DrawString(text, font, Brushes.White, tx, ty);
            }
        }

        private GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void StartGame()
        {
            Hide();
            gameForm.Show();
            gameForm.FormClosed += (s, e) => Close();
        }
    }
}