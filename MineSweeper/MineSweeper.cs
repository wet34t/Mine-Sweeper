using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MineSweeper
{
    partial class Tile : Button
    {
        public int Value { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool Swept { get; set; }
    }

    public partial class MineSweeperBoard : Window
    {
        private Tile[][] _tiles;
        public Grid Board { get; set; }

        public MineSweeperBoard()
        {
            InitializeComponent();
            Board = new Grid();

            int rows = 8;
            int cols = 8;
            for (int i = 0; i < rows; i++)
                Board.RowDefinitions.Add(new RowDefinition());

            for (int j = 0; j < cols; j++)
                Board.ColumnDefinitions.Add(new ColumnDefinition());

            CreateBoard(rows, cols, 7);

            Content = Board;
        }

        private void Sweep(Tile t)
        {
            if (t.Swept) return;
            t.Content = t.Value.ToString();
            t.Background = Brushes.LightGray;
            t.Swept = true;
            if (t.Value == 0)
            {
                ForEachNeighbour(t.X, t.Y, n =>
                {
                    Sweep(n);
                });
            }
        }

        private void Click(object sender, RoutedEventArgs e)
        {
            Tile t = (Tile)sender;

            Sweep(t);
        }

        public void CreateBoard(int n, int m, int mines)
        {
            _tiles = new Tile[n][];
            for (int i = 0; i < n; i++)
            {
                _tiles[i] = new Tile[m];
                for (int j = 0; j < m; j++)
                {
                    Tile button = new Tile
                    {
                        Margin = new Thickness(5),
                        Value = 0,
                        Background = Brushes.Gray,
                        X = i,
                        Y = j,
                        Swept = false
                    };

                    button.Click += Click;

                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);
                    Board.Children.Add(button);
                    _tiles[i][j] = button;

                }
            }
            SetupBoard(mines);
        }

        private void SetupBoard(int mines)
        {
            Random rand = new Random();
            for (int i = 0; i < mines;)
            {
                (int x, int y) = (rand.Next(_tiles.Length), rand.Next(_tiles[0].Length));
                if (_tiles[x][y].Value == -1)
                {
                    continue;
                }
                SetupMine(x, y);
                i++;
            }
        }

        private void SetupMine(int x, int y)
        {
            _tiles[x][y].Value = -1;
            ForEachNeighbour(x, y, n =>
            {
                if (n.Value != -1)
                {
                    n.Value++;
                }
            });
        }

        private void ForEachNeighbour(int x, int y, Action<Tile> func)
        {
            for (int i = x - 1; i <= x + 1; i++)
            {
                if (i < 0 || i >= _tiles.Length) continue;
                for (int j = y - 1; j <= y + 1; j++)
                {
                    if (j < 0 || j >= _tiles[i].Length) continue;
                    func(_tiles[i][j]);
                }
            }
        }
    }
}