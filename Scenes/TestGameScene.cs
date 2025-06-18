using System.Drawing;

namespace RawDraw.Scenes;

public class TestGameScene : IScene
{
    private char[,] _map;
    private Point _playerPos;
    private int _health = 3;
    private int _treasureCount = 0;
    private bool _gameOver = false;

    private int _mapCols;
    private int _mapRows;
    private int _tileSize = 12; // Pixels per tile
    private int _screenWidth;
    private int _screenHeight;

    private Random _rng = new();

    public TestGameScene(int width, int height)
    {
        _screenWidth = width;
        _screenHeight = height;

        _mapCols = width / _tileSize;
        _mapRows = (height - 30) / _tileSize; // leave HUD space

        _map = new char[_mapCols, _mapRows];
        GenerateMap();

        _playerPos = new Point(1, 1);
    }

    public void Render(FrameBuffer buffer, long deltaTime)
    {
        if (_gameOver)
        {
            buffer.FillRect(0, 0, _screenWidth, _screenHeight, Color.Black);
            buffer.DrawText(_screenWidth / 2 - 30, _screenHeight / 2, "GAME OVER - Press any key to exit", Color.Red);
            return;
        }

        HandleInput();

        // Draw map tiles
        for (int y = 0; y < _mapRows; y++)
        {
            for (int x = 0; x < _mapCols; x++)
            {
                char tile = _map[x, y];

                int px = x * _tileSize;
                int py = y * _tileSize;

                if (tile == '=')
                {
                    // Base square
                    buffer.FillRect(px, py, _tileSize, _tileSize, Color.SandyBrown);

                    // Road dots
                    buffer.DrawPixel(px + _tileSize / 3, py + _tileSize / 3, Color.Black);
                    buffer.DrawPixel(px + 2 * _tileSize / 3, py + 2 * _tileSize / 3, Color.Black);
                }
                else
                {
                    Color tileColor = tile switch
                    {
                        '#' => Color.DarkSlateGray,
                        '.' => Color.ForestGreen,
                        '~' => Color.CornflowerBlue,
                        'T' => Color.Gold,
                        'E' => Color.IndianRed,
                        _ => Color.Magenta
                    };

                    buffer.FillRect(px, py, _tileSize, _tileSize, tileColor);
                }
            }
        }

        // Draw player
        int playerPx = _playerPos.X * _tileSize;
        int playerPy = _playerPos.Y * _tileSize;
        buffer.FillRect(playerPx, playerPy, _tileSize, _tileSize, Color.White);
        buffer.DrawChar(playerPx + 2, playerPy + 2, '@', Color.Black);

        // HUD
        buffer.DrawText(5, _screenHeight - 25, $"Health: {_health}", Color.White);
        buffer.DrawText(120, _screenHeight - 25, $"Treasure: {_treasureCount}", Color.White);
    }

    private void HandleInput()
    {
        while (Console.KeyAvailable)
        {
            var key = Console.ReadKey(true).Key;

            int dx = 0, dy = 0;

            switch (key)
            {
                case ConsoleKey.LeftArrow: dx = -1; break;
                case ConsoleKey.RightArrow: dx = 1; break;
                case ConsoleKey.UpArrow: dy = -1; break;
                case ConsoleKey.DownArrow: dy = 1; break;
                default: return;
            }

            int newX = _playerPos.X + dx;
            int newY = _playerPos.Y + dy;

            if (newX < 0 || newY < 0 || newX >= _mapCols || newY >= _mapRows)
                return;

            char targetTile = _map[newX, newY];
            if (targetTile == '#') return; // Blocked by wall

            if (targetTile == 'T')
            {
                _treasureCount++;
                _map[newX, newY] = '.';
            }
            else if (targetTile == 'E')
            {
                _health--;
                _map[newX, newY] = '.';
                if (_health <= 0)
                {
                    _gameOver = true;
                    return;
                }
            }

            _playerPos = new Point(newX, newY);
        }
    }

    private void GenerateMap()
    {
        var tileTypes = new[] { '.', '#', '~', '=' };

        Dictionary<char, char[]> rules = new()
        {
            ['.'] = new[] { '.', '#', '=', '~' },
            ['#'] = new[] { '.', '#', '=' },
            ['~'] = new[] { '~', '.' },
            ['='] = new[] { '=', '.', '#' },
        };

        // Initialize all to uncollapsed
        for (int y = 0; y < _mapRows; y++)
        {
            for (int x = 0; x < _mapCols; x++)
            {
                _map[x, y] = '?';
            }
        }

        // Set borders as walls
        for (int x = 0; x < _mapCols; x++) { _map[x, 0] = '#'; _map[x, _mapRows - 1] = '#'; }
        for (int y = 0; y < _mapRows; y++) { _map[0, y] = '#'; _map[_mapCols - 1, y] = '#'; }

        // Seed center with grass
        _map[1, 1] = '.';

        // Simplified WFC row-by-row
        for (int y = 1; y < _mapRows - 1; y++)
        {
            for (int x = 1; x < _mapCols - 1; x++)
            {
                if (_map[x, y] != '?') continue;

                List<char> neighbors = new();

                if (_map[x - 1, y] != '?') neighbors.Add(_map[x - 1, y]);
                if (_map[x, y - 1] != '?') neighbors.Add(_map[x, y - 1]);

                HashSet<char> options = new(tileTypes);

                foreach (var neighbor in neighbors)
                {
                    options.IntersectWith(rules[neighbor]);
                }

                if (options.Count == 0)
                    _map[x, y] = '.'; // fallback
                else
                    _map[x, y] = Pick(options);
            }
        }

        // Add treasures
        for (int i = 0; i < 10; i++)
        {
            int x = _rng.Next(1, _mapCols - 1);
            int y = _rng.Next(1, _mapRows - 1);
            if (_map[x, y] == '.') _map[x, y] = 'T';
        }

        // Add enemies
        for (int i = 0; i < 5; i++)
        {
            int x = _rng.Next(1, _mapCols - 1);
            int y = _rng.Next(1, _mapRows - 1);
            if (_map[x, y] == '.') _map[x, y] = 'E';
        }
    }

    private char Pick(IEnumerable<char> options)
    {
        var list = new List<char>();

        foreach (var c in options)
        {
            int weight = c switch
            {
                '=' => 5,  // Heavily weighted for roads
                '.' => 3,
                '#' => 2,
                '~' => 1,
                _ => 1
            };

            for (int i = 0; i < weight; i++)
                list.Add(c);
        }

        return list[_rng.Next(list.Count)];
    }

    public void Input(int keyCode, bool state)
    {
        // Test game scene doesn't need input handling
    }
}
