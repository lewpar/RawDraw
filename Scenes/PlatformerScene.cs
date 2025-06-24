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
    private float playerX = 100;
    private float playerY = 100;
    private float playerWidth = 20;
    private float playerHeight = 30;
    private float playerVelocityX = 0;
    private float playerVelocityY = 0;
    private bool isJumping = false;
    private int score = 0;

    // Physics constants
    private const float MOVE_SPEED = 200f;
    private const float JUMP_FORCE = -400f;
    private const float GRAVITY = 800f;
    private const float FRICTION = 0.8f;

    // Platforms
    private List<Platform> platforms;

    public PlatformerScene()
    {
        // Initialize platforms
        platforms = new List<Platform>
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
            playerVelocityX = -MOVE_SPEED;
        }
        else if (Input.IsKeyDown(KeyCodes.KEY_D))
        {
            playerVelocityX = MOVE_SPEED;
        }
        else
        {
            playerVelocityX *= FRICTION;
        }

        // Apply gravity
        playerVelocityY += GRAVITY * deltaTime;

        // Handle jumping
        if (Input.IsKeyDown(KeyCodes.KEY_SPACE) && !isJumping)
        {
            playerVelocityY = JUMP_FORCE;
            isJumping = true;
        }

        // Update position
        float newPlayerX = playerX + playerVelocityX * deltaTime;
        float newPlayerY = playerY + playerVelocityY * deltaTime;

        // Check platform collisions
        bool onGround = false;
        foreach (var platform in platforms)
        {
            if (platform.Collides(newPlayerX, newPlayerY, playerWidth, playerHeight))
            {
                // Collision resolution
                if (playerVelocityY > 0 && playerY + playerHeight <= platform.Y + 5)
                {
                    // Landing on top of platform
                    newPlayerY = platform.Y - playerHeight;
                    playerVelocityY = 0;
                    isJumping = false;
                    onGround = true;
                }
                else if (playerVelocityY < 0 && playerY >= platform.Y + platform.Height - 5)
                {
                    // Hitting platform from below
                    newPlayerY = platform.Y + platform.Height;
                    playerVelocityY = 0;
                }
                
                if (playerVelocityX > 0 && playerX + playerWidth <= platform.X + 5)
                {
                    // Hitting platform from left
                    newPlayerX = platform.X - playerWidth;
                    playerVelocityX = 0;
                }
                else if (playerVelocityX < 0 && playerX >= platform.X + platform.Width - 5)
                {
                    // Hitting platform from right
                    newPlayerX = platform.X + platform.Width;
                    playerVelocityX = 0;
                }
            }
        }

        if (!onGround)
        {
            isJumping = true;
        }

        // Update position
        playerX = newPlayerX;
        playerY = newPlayerY;

        // Keep player in bounds
        if (playerX < 0) playerX = 0;
        if (playerX + playerWidth > 800) playerX = 800 - playerWidth;
        if (playerY < 0) playerY = 0;
        if (playerY + playerHeight > 480) 
        {
            playerY = 480 - playerHeight;
            playerVelocityY = 0;
            isJumping = false;
        }
    }

    public override void Draw(FrameBuffer buffer)
    {
        // Draw platforms
        foreach (var platform in platforms)
        {
            buffer.FillRect(platform.X, platform.Y, platform.Width, platform.Height, Color.Green);
        }

        // Draw player
        buffer.FillRect((int)playerX, (int)playerY, (int)playerWidth, (int)playerHeight, Color.Blue);

        // Draw score and instructions
        buffer.DrawText(10, 10, $"Score: {score}", Color.White);
        buffer.DrawText(10, 30, "Use A/D to move, SPACE to jump", Color.White);
    }
} 