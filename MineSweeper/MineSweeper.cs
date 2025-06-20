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
        private Grid _board = new Grid();
        private int _mines;
        private int _sweeps;
        private int _flags;
        private int _rows;
        private int _cols;
        private TextBox _timer;
        private CancellationTokenSource _cancellation;

        public MineSweeperBoard()
        {
            InitializeComponent();
            _board.RowDefinitions.Add(new RowDefinition());
            _timer = new TextBox();
            _timer.FontSize += 10;

            _rows = 8;
            _cols = 8;
            _mines = 7;
            for (int i = 0; i < _rows; i++)
                _board.RowDefinitions.Add(new RowDefinition());

            for (int j = 0; j < _cols; j++)
                _board.ColumnDefinitions.Add(new ColumnDefinition());
            
            Grid.SetRow(_timer, 0);
            Grid.SetColumn(_timer, 0);
            _board.Children.Add(_timer);

            CreateBoard();

            Content = _board;
        }

        private void Sweep(Tile t)
        {
            if (t.Swept || t.Content?.ToString() == "f") return;
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

        public void CreateBoard()
        {
            _timer.Text = "0";
            _cancellation?.Cancel();
            _tiles = new Tile[_rows][];
            TextBox mines = new TextBox();
            mines.FontSize += 10;
            mines.Text = (_mines - _flags).ToString();
            Grid.SetRow(mines, 0);
            Grid.SetColumn(mines, _cols - 1);
            _board.Children.Add(mines);
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
                                t.Foreground = Brushes.Red;
                                t.Content = "f";
                                _flags++;
                            }
                            mines.Text = (_mines - _flags).ToString();
                        }
                        else
                        {
                            ForEachNeighbour(t.X, t.Y, Sweep, _cancellation.Token);
                        }
                    };

                    Grid.SetRow(button, i + 1);
                    Grid.SetColumn(button, j);
                    _board.Children.Add(button);
                    _tiles[i][j] = button;
                }
            }
            SetupBoard();
            _cancellation = new CancellationTokenSource();
            CancellationToken token = _cancellation.Token;
            Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    Thread.Sleep(1000);
                    this.Dispatcher.Invoke(() =>
                    {
                        _timer.Text = (int.Parse(_timer.Text) + 1).ToString();
                    });
                    
                }
            }, token);
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

        private void ForEachNeighbour(int x, int y, Action<Tile> act, CancellationToken? token = null)
        {
            for (int i = x - 1; i <= x + 1; i++)
            {
                if (i < 0 || i >= _rows) continue;
                if (token != null && ((CancellationToken)token).IsCancellationRequested) break;
                for (int j = y - 1; j <= y + 1; j++)
                {
                    if (j < 0 || j >= _cols) continue;
                    if (token != null && ((CancellationToken)token).IsCancellationRequested) break;
                    act(_tiles[i][j]);
                }
            }
        }
    }
}