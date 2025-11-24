using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceCourier.Models
{
    public class GameModel
    {
        public Player Player { get; private set; }
        public List<Enemy> Enemies { get; private set; } = new List<Enemy>();
        public List<Decoy> Decoys { get; private set; } = new List<Decoy>();

        public Vector PlanetPosition { get; private set; }
        public Vector StationPosition { get; private set; }

        public int LevelWidth => 800;
        public int LevelHeight => 600;

        public Starfield Starfield { get; private set; } = new Starfield();

        public IReadOnlyList<Enemy> EnemiesReadOnly => Enemies;
        public IReadOnlyList<Decoy> DecoysReadOnly => Decoys;

        private int invincibilityTimer = 180;
        public bool IsPlayerInvincible => invincibilityTimer > 0;

        public GameModel()
        {
            InitializeLevel();
        }

        private void InitializeLevel()
        {
            StationPosition = new Vector(50, 50);
            Player = new Player(StationPosition);
            PlanetPosition = new Vector(700, 500);

            Enemies.Clear();
            Decoys.Clear();

            Enemies.Add(new Enemy(new Vector(300, 200)));
            Enemies.Add(new Enemy(new Vector(400, 400)));
            Enemies.Add(new Enemy(new Vector(200, 350)));

            invincibilityTimer = 180;
        }

        public void Update()
        {
            Player.Update();
            Starfield.Update(Player.Position.X, Player.Position.Y);

            if (invincibilityTimer > 0)
                invincibilityTimer--;

            // === Обновление приманок ===
            for (int i = Decoys.Count - 1; i >= 0; i--)
            {
                Decoys[i].Update();
                if (Decoys[i].IsExpired)
                    Decoys.RemoveAt(i);
            }

            bool lifeLost = false;

            // === Обновление всех врагов ===
            foreach (var enemy in Enemies)
            {
                // ← ВСЁ ДВИЖЕНИЕ И ИИ ВРАГА ЗДЕСЬ
                enemy.UpdateAI(Player.Position, Player.IsCloaked, Decoys);

                // Урон при столкновении с игроком
                if (enemy.IsAlerted &&
                    enemy.Position.DistanceTo(Player.Position) < 35 &&
                    !IsPlayerInvincible)
                {
                    lifeLost = true;
                }
            }

            // === Смерть игрока ===
            if (lifeLost)
            {
                Player.TakeDamage(1);
                Player.Position = new Vector(StationPosition.X, StationPosition.Y);
                invincibilityTimer = 180; // 3 секунды неуязвимости
            }

            // Границы экрана для игрока
            Player.Position.X = Math.Max(0, Math.Min(LevelWidth, Player.Position.X));
            Player.Position.Y = Math.Max(0, Math.Min(LevelHeight, Player.Position.Y));
        }

        public void MovePlayer(double dx, double dy)
        {
            if (dx != 0 || dy != 0)
                Player.Move(dx, dy);
        }

        public void ActivateCloak() => Player.TryActivateCloak();
        public void ActivateShield() => Player.TryActivateShield();

        public void UseDecoy()
        {
            if (Player.Decoys > 0)
            {
                Player.Decoys--;
                Decoys.Add(new Decoy(Player.Position));
            }
        }
    }
}