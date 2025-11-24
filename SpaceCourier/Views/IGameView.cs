using System.Drawing;

namespace SpaceCourier.Views
{
    public interface IGameView
    {
        void InvalidateView();
        void ShowEndScreen(string message, Color color);
    }
}