using System;
using System.Collections.Generic;

namespace SpaceCourier.Models
{
    public class Enemy
    {
        public Vector Position { get; set; }
        public double Speed { get; set; } = 1.8; // быстрее для погони
        public double Rotation { get; set; } = 0;
        public double RotationSpeed { get; set; } = 0.08; // быстрее поворот
        public double PatrolRadius { get; set; } = 120;
        public double ViewDistance { get; set; } = 150; // радиус обнаружения
        public double ViewAngle { get; set; } = 360; // ← 360° — ВИДЯТ ВСЕ СТОРОНЫ!
        public double SuspicionLevel { get; private set; } = 0;
        public bool IsAlerted => SuspicionLevel >= 70;
        public bool IsChasing => SuspicionLevel >= 30;

        private Vector patrolCenter;
        private double patrolPhase = 0;
        private Vector lastKnownPlayer = new Vector(0, 0); // память о цели
        private readonly Random random = new Random();
        private double chaseTimer = 0;

        public Enemy(Vector position)
        {
            Position = position;
            patrolCenter = new Vector(position.X, position.Y);
            patrolPhase = random.NextDouble() * Math.PI * 2;
            lastKnownPlayer = position;
        }

        // ← ОСНОВНОЙ ИИ: видит кругом + погоня
        public void UpdateAI(Vector playerPos, bool playerCloaked, List<Decoy> decoys)
        {
            // 1. Поиск цели (игрок или приманка)
            Vector targetPos = playerPos;
            bool seesTarget = false;
            double closestDist = double.MaxValue;

            // Игрок (если не cloaked)
            if (!playerCloaked)
            {
                double d = Position.DistanceTo(playerPos);
                if (CanSeePlayer(playerPos) && d < closestDist)
                {
                    closestDist = d;
                    targetPos = playerPos;
                    seesTarget = true;
                }
            }

            // Приманки (всегда видимы)
            foreach (var decoy in decoys)
            {
                double d = Position.DistanceTo(decoy.Position);
                if (CanSeePlayer(decoy.Position) && d < closestDist)
                {
                    closestDist = d;
                    targetPos = decoy.Position;
                    seesTarget = true;
                }
            }

            // 2. Реакция на обнаружение
            if (seesTarget)
            {
                SuspicionLevel = Math.Min(100, SuspicionLevel + 3.0); // быстро растёт
                lastKnownPlayer = targetPos; // запоминаем позицию
                chaseTimer = 420; // 7 сек погони
            }
            else
            {
                SuspicionLevel = Math.Max(0, SuspicionLevel - 0.7); // медленно падает
                if (SuspicionLevel < 20) chaseTimer = 0;
            }

            // 3. Поворот к цели
            Vector toTarget = new Vector(lastKnownPlayer.X - Position.X, lastKnownPlayer.Y - Position.Y);
            double targetAngle = Math.Atan2(toTarget.Y, toTarget.X);
            double diff = targetAngle - Rotation;
            while (diff > Math.PI) diff -= 2 * Math.PI;
            while (diff < -Math.PI) diff += 2 * Math.PI;
            Rotation += diff * RotationSpeed * (IsChasing ? 2.0 : 0.5); // быстрее в погоне

            // 4. Движение
            if (IsChasing && chaseTimer > 0)
            {
                chaseTimer--;
                // ПРЕСЛЕДОВАНИЕ lastKnownPlayer
                double dx = lastKnownPlayer.X - Position.X;
                double dy = lastKnownPlayer.Y - Position.Y;
                double dist = Math.Sqrt(dx * dx + dy * dy);
                if (dist > 3)
                {
                    Position.X += (dx / dist) * Speed * 1.2; // агрессивно
                    Position.Y += (dy / dist) * Speed * 1.2;
                }
            }
            else
            {
                // Патруль (круг)
                patrolPhase += 0.018;
                Position.X = patrolCenter.X + Math.Cos(patrolPhase) * PatrolRadius * 0.8;
                Position.Y = patrolCenter.Y + Math.Sin(patrolPhase) * PatrolRadius * 0.7;
            }

            // Границы экрана
            Position.X = Math.Max(25, Math.Min(775, Position.X));
            Position.Y = Math.Max(25, Math.Min(575, Position.Y));
        }

        // Проверка видимости (теперь 360° — всегда true, если в радиусе)
        public bool CanSeePlayer(Vector pos)
        {
            double dist = Position.DistanceTo(pos);
            if (dist > ViewDistance) return false;

            // Для 360° угол всегда в обзоре
            if (ViewAngle >= 360) return true;

            Vector toPos = new Vector(pos.X - Position.X, pos.Y - Position.Y);
            double posAngle = Math.Atan2(toPos.Y, toPos.X);
            double diff = posAngle - Rotation;
            while (diff > Math.PI) diff -= 2 * Math.PI;
            while (diff < -Math.PI) diff += 2 * Math.PI;
            return Math.Abs(diff) < (ViewAngle / 2 * Math.PI / 180);
        }

        // Старые методы для совместимости (пустые)
        public void UpdatePatrol() { }
        public void UpdateRotation(Vector playerPosition, bool canSeePlayer) { }
        public void UpdateSuspicion(bool canSeePlayer) { }
    }
}