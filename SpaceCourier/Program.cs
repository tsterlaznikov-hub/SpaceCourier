using System;
using System.Windows.Forms;
using SpaceCourier.Views;   // ← ЭТО ОБЯЗАТЕЛЬНО!

namespace SpaceCourier
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GameForm());   // теперь видит GameForm
        }
    }
}