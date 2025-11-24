using System;
using System.Collections.Generic;

namespace SpaceCourier.Models
{
    public class Player
    {
        public Vector Position { get; set; }
        public double Speed { get; set; } = 5;
        public int Health { get; set; } = 3;
        public double Energy { get; set; } = 100;
        public double MaxEnergy { get; set; } = 100;
        public bool IsCloaked { get; private set; }
        public bool IsShieldActive { get; private set; }
        public int Decoys { get; set; } = 3;

        // ←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←
        // НОВОЕ СВОЙСТВО: направление корабля в радианах (0 = вправо)
        public double Direction { get; private set; } = 0;
        // ←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←

        public Player(Vector startPosition)
        {
            Position = startPosition;
        }

        // === Движение ===
        public void Move(Vector direction)
        {
            if (direction.X != 0 || direction.Y != 0)
            {
                // ←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←
                // Обновляем направление (угол в радианах)
                Direction = Math.Atan2(direction.Y, direction.X);
                // ←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←

                double length = Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);
                Vector normalized = new Vector(direction.X / length, direction.Y / length);
                Position = new Vector(
                    Position.X + normalized.X * Speed,
                    Position.Y + normalized.Y * Speed
                );
            }
        }

        // Перегрузка для совместимости
        public void Move(double dx, double dy) => Move(new Vector(dx, dy));

        // === Маскировка ===
        public void ActivateCloak()
        {
            if (Energy >= 10 && !IsCloaked) IsCloaked = true;
        }

        public void DeactivateCloak() => IsCloaked = false;
        public void TryActivateCloak() => ActivateCloak();

        // === Щит ===
        public void ActivateShield()
        {
            if (Energy >= 30 && !IsShieldActive) IsShieldActive = true;
        }

        public void DeactivateShield() => IsShieldActive = false;
        public void TryActivateShield() => ActivateShield();

        // === Энергия ===
        public void UpdateEnergy()
        {
            if (Energy < MaxEnergy)
            {
                Energy += 0.5;
                if (Energy > MaxEnergy) Energy = MaxEnergy;
            }
            if (IsCloaked) Energy -= 2;
            if (IsShieldActive) Energy -= 5;
            if (Energy <= 0)
            {
                Energy = 0;
                IsCloaked = false;
                IsShieldActive = false;
            }
        }

        // Обновление для GameModel
        public void Update() => UpdateEnergy();

        // === Урон ===
        public void TakeDamage(int amount)
        {
            if (IsShieldActive && Energy > 0)
            {
                Energy -= 1.5;
                if (Energy <= 0) IsShieldActive = false;
            }
            else
            {
                Health = Math.Max(0, Health - amount);
            }
        }

        // === Приманки ===
        public bool UseDecoy()
        {
            if (Decoys > 0)
            {
                Decoys--;
                return true;
            }
            return false;
        }

        public bool TryUseDecoy(IEnumerable<Enemy> enemies) => UseDecoy();
    }
}