using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MineSweeper
{
    class Tile : Button
    {
        public int Value { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool Swept { get; set; }
    }

    public partial class MineSweeperBoard : Window
    {
        private Tile[][] _tiles;
        private Grid _board;
        private int _mines;
        private int _sweeps;
        private int _flags;
        private int _rows;
        private int _cols;

        public MineSweeperBoard()
        {
            InitializeComponent();
            _board = new Grid();

            _rows = 8;
            _cols = 8;
            for (int i = 0; i < _rows; i++)
                _board.RowDefinitions.Add(new RowDefinition());

            for (int j = 0; j < _cols; j++)
                _board.ColumnDefinitions.Add(new ColumnDefinition());

            _mines = 7;

            CreateBoard();

            Content = _board;
        }

        private void Sweep(Tile t)
        {
            if (t.Swept) return;
            t.Background = Brushes.LightGray;
            t.Content = t.Value switch
            {
                -1 => "m",
                0 => "",
                _ => t.Value.ToString()
            };
            if (t.Value == -1)
            {
                t.Background = Brushes.Red;
            }
            t.Swept = true;
            _sweeps++;
            if (t.Value == -1)
            {
                GameOver("You lose!");
                return;
            }
            else if (t.Value == 0)
            {
                ForEachNeighbour(t.X, t.Y, Sweep);
            }
            if (_sweeps >= _rows * _cols - _mines)
            {
                GameOver("You win!");
                CreateBoard();
            }
        }

        private void GameOver(string message)
        {
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _cols; j++)
                {
                    if (_tiles[i][j].Value == -1)
                    {
                        _tiles[i][j].Foreground = Brushes.Black;
                        _tiles[i][j].Content = "m";
                    }
                }
            }
            MessageBox.Show(message, "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
            _sweeps = 0;
            _flags = 0;
            CreateBoard();
        }

        private void Click(object sender, RoutedEventArgs e)
        {
            Sweep((Tile)sender);
        }

        public void CreateBoard()
        {
            _tiles = new Tile[_rows][];
            for (int i = 0; i < _rows; i++)
            {
                _tiles[i] = new Tile[_cols];
                for (int j = 0; j < _cols; j++)
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

                    button.Click += (sender, e) => Sweep((Tile)sender);
                    button.MouseRightButtonDown += (sender, e) =>
                    {
                        Tile t = (Tile)sender;
                        if (!t.Swept)
                        {
                            if (t.Content?.ToString() == "f")
                            {
                                t.Foreground = Brushes.Black;
                                t.Content = "";
                                _flags--;
                            }
                            else if (_flags < _mines)
                            {
                                t.Foreground =Brushes.Red;
                                t.Content = "f";
                                _flags++;
                            }
                        }    
                    };

                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);
                    _board.Children.Add(button);
                    _tiles[i][j] = button;
                }
            }
            SetupBoard();
        }

        private void SetupBoard()
        {
            Random rand = new Random();
            for (int i = 0; i < _mines;)
            {
                (int x, int y) = (rand.Next(_tiles.Length), rand.Next(_tiles[0].Length));
                if (_tiles[x][y].Value == -1)
                {
                    continue;
                }
                _tiles[x][y].Value = -1;
                ForEachNeighbour(x, y, n =>
                {
                    if (n.Value != -1)
                    {
                        n.Value++;
                    }
                });
                i++;
            }
        }

        private void ForEachNeighbour(int x, int y, Action<Tile> act)
        {
            for (int i = x - 1; i <= x + 1; i++)
            {
                if (i < 0 || i >= _rows) continue;
                for (int j = y - 1; j <= y + 1; j++)
                {
                    if (j < 0 || j >= _cols) continue;
                    act(_tiles[i][j]);
                }
            }
        }
    }
}