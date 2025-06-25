using RawDraw.Engine.Drawing;
using RawDraw.Engine.Input;
using RawDraw.Engine.Scene;
using RawDraw.Engine.Primitive;

namespace RawDraw.Scenes;

public class Platform
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public Platform(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public bool Collides(float px, float py, float width, float height)
    {
        return px < X + Width &&
               px + width > X &&
               py < Y + Height &&
               py + height > Y;
    }
}

public class PlatformerScene : Scene
{
    // Player properties
    private float _playerX = 100;
    private float _playerY = 100;
    private float _playerWidth = 20;
    private float _playerHeight = 30;
    private float _playerVelocityX;
    private float _playerVelocityY;
    private bool _isJumping;
    private int _score = 0;

    // Physics constants
    private const float MoveSpeed = 200f;
    private const float JumpForce = -400f;
    private const float Gravity = 800f;
    private const float Friction = 0.8f;

    // Platforms
    private List<Platform> _platforms;

    public PlatformerScene()
    {
        // Initialize platforms
        _platforms = new List<Platform>
        {
            new Platform(50, 400, 200, 20),    // Starting platform
            new Platform(300, 350, 150, 20),   // Platform to jump to
            new Platform(500, 300, 150, 20),   // Higher platform
            new Platform(200, 250, 150, 20),   // Even higher platform
            new Platform(0, 450, 800, 20),     // Ground
        };
    }

    public override void Update(float deltaTimeMs)
    {
        if (Input is null)
        {
            throw new InvalidOperationException("Input manager is not initialized in PlatformerScene");
        }

        float deltaTime = deltaTimeMs / 1000f; // Convert to seconds

        // Handle horizontal movement
        if (Input.IsKeyDown(KeyCodes.KEY_A))
        {
            _playerVelocityX = -MoveSpeed;
        }
        else if (Input.IsKeyDown(KeyCodes.KEY_D))
        {
            _playerVelocityX = MoveSpeed;
        }
        else
        {
            _playerVelocityX *= Friction;
        }

        // Apply gravity
        _playerVelocityY += Gravity * deltaTime;

        // Handle jumping
        if (Input.IsKeyDown(KeyCodes.KEY_SPACE) && !_isJumping)
        {
            _playerVelocityY = JumpForce;
            _isJumping = true;
        }

        // Update position
        float newPlayerX = _playerX + _playerVelocityX * deltaTime;
        float newPlayerY = _playerY + _playerVelocityY * deltaTime;

        // Check platform collisions
        bool onGround = false;
        foreach (var platform in _platforms)
        {
            if (platform.Collides(newPlayerX, newPlayerY, _playerWidth, _playerHeight))
            {
                // Collision resolution
                if (_playerVelocityY > 0 && _playerY + _playerHeight <= platform.Y + 5)
                {
                    // Landing on top of platform
                    newPlayerY = platform.Y - _playerHeight;
                    _playerVelocityY = 0;
                    _isJumping = false;
                    onGround = true;
                }
                else if (_playerVelocityY < 0 && _playerY >= platform.Y + platform.Height - 5)
                {
                    // Hitting platform from below
                    newPlayerY = platform.Y + platform.Height;
                    _playerVelocityY = 0;
                }
                
                if (_playerVelocityX > 0 && _playerX + _playerWidth <= platform.X + 5)
                {
                    // Hitting platform from left
                    newPlayerX = platform.X - _playerWidth;
                    _playerVelocityX = 0;
                }
                else if (_playerVelocityX < 0 && _playerX >= platform.X + platform.Width - 5)
                {
                    // Hitting platform from right
                    newPlayerX = platform.X + platform.Width;
                    _playerVelocityX = 0;
                }
            }
        }

        if (!onGround)
        {
            _isJumping = true;
        }

        // Update position
        _playerX = newPlayerX;
        _playerY = newPlayerY;

        // Keep player in bounds
        if (_playerX < 0) _playerX = 0;
        if (_playerX + _playerWidth > 800) _playerX = 800 - _playerWidth;
        if (_playerY < 0) _playerY = 0;
        if (_playerY + _playerHeight > 480) 
        {
            _playerY = 480 - _playerHeight;
            _playerVelocityY = 0;
            _isJumping = false;
        }
    }

    public override void Draw(FrameBuffer buffer)
    {
        // Draw platforms
        foreach (var platform in _platforms)
        {
            buffer.FillRect(platform.X, platform.Y, platform.Width, platform.Height, Color.Green);
        }

        // Draw player
        buffer.FillRect((int)_playerX, (int)_playerY, (int)_playerWidth, (int)_playerHeight, Color.Blue);

        // Draw score and instructions
        buffer.DrawText(10, 10, $"Score: {_score}", Color.White);
        buffer.DrawText(10, 30, "Use A/D to move, SPACE to jump", Color.White);
    }
} 