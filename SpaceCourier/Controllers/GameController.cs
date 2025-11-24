using System.Drawing;
using System.Windows.Forms;
using SpaceCourier.Models;
using SpaceCourier.Views;

namespace SpaceCourier.Controllers
{
    public enum GameState { Playing, Victory, GameOver }

    public class GameController
    {
        private readonly GameModel model;
        private readonly IGameView view;

        private readonly bool[] keyPressed = new bool[256];
        private readonly bool[] keyJustPressed = new bool[256];

        public GameState CurrentState { get; private set; } = GameState.Playing;
        public string StatusMessage { get; private set; } = "";
        public Color StatusColor { get; private set; } = Color.White;

        public GameModel Model => model;

        public GameController(IGameView view)
        {
            this.view = view;
            model = new GameModel();
        }

        public void Update()
        {
            if (CurrentState != GameState.Playing)
            {
                view.InvalidateView();
                return;
            }

            // === Ввод ===
            double dx = 0, dy = 0;
            if (IsKeyPressed(Keys.W)) dy -= 1;
            if (IsKeyPressed(Keys.S)) dy += 1;
            if (IsKeyPressed(Keys.A)) dx -= 1;
            if (IsKeyPressed(Keys.D)) dx += 1;
            if (dx != 0 || dy != 0) model.MovePlayer(dx, dy);

            if (IsKeyJustPressed(Keys.ShiftKey)) model.ActivateCloak();
            if (IsKeyJustPressed(Keys.Space)) model.ActivateShield();
            if (IsKeyJustPressed(Keys.E)) model.UseDecoy();

            // === Обновление модели ===
            model.Update();

            // === Проверка конца игры ===
            if (model.Player.Health <= 0)
            {
                CurrentState = GameState.GameOver;
                StatusMessage = "ИГРА ОКОНЧЕНА!";
                StatusColor = Color.Red;
            }
            else if (model.Player.Position.DistanceTo(model.PlanetPosition) < 30)
            {
                CurrentState = GameState.Victory;
                StatusMessage = "УРОВЕНЬ ПРОЙДЕН!";
                StatusColor = Color.Lime;
            }

            view.InvalidateView();
        }

        public void HandleKeyDown(Keys key)
        {
            if ((int)key < 256)
            {
                if (!keyPressed[(int)key])
                    keyJustPressed[(int)key] = true;
                keyPressed[(int)key] = true;
            }
        }

        public void HandleKeyUp(Keys key)
        {
            if ((int)key < 256)
                keyPressed[(int)key] = false;
        }

        private bool IsKeyPressed(Keys key) => (int)key < 256 && keyPressed[(int)key];

        private bool IsKeyJustPressed(Keys key)
        {
            if ((int)key >= 256) return false;
            bool pressed = keyJustPressed[(int)key];
            keyJustPressed[(int)key] = false;
            return pressed;
        }
    }
}