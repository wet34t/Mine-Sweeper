using System.Windows;

namespace MineSweeper
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            var app = new Application();
            var window = new MineSweeperBoard();
            app.Run(window);
        }
    }
}