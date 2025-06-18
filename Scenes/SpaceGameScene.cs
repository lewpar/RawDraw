using System.Collections.Generic;
using System.Drawing;
using Vec2 = System.Numerics.Vector2;

namespace RawDraw.Scenes;

public class SpaceGameScene : IScene
{
    // Screen dimensions
    private readonly int screenWidth;
    private readonly int screenHeight;

    // Player properties
    private Vec2 playerPosition;
    private float playerRotation;
    private float playerSpeed = 0.5f;
    private float rotationSpeed = 0.1f;
    private List<(Vec2 position, Vec2 velocity)> bullets;
    private List<(Vec2 position, float size, Vec2 velocity)> asteroids;
    private int score;
    private Random random;
    private float asteroidSpawnTimer;
    private const float ASTEROID_SPAWN_INTERVAL = 2000; // milliseconds

    // Input states
    private bool isMovingForward;
    private bool isRotatingLeft;
    private bool isRotatingRight;
    private bool isShooting;

    // Key codes
    private const int KEY_W = 17;
    private const int KEY_A = 30;
    private const int KEY_D = 32;
    private const int KEY_SPACE = 57;

    // Game object sizes
    private readonly float playerSize;
    private readonly float bulletSize;
    private readonly float minAsteroidSize;
    private readonly float maxAsteroidSize;

    // Colors
    private static readonly Color BACKGROUND_COLOR = Color.FromArgb(255, 0, 0, 20);
    private static readonly Color PLAYER_COLOR = Color.FromArgb(255, 0, 255, 0);
    private static readonly Color BULLET_COLOR = Color.FromArgb(255, 255, 255, 0);
    private static readonly Color ASTEROID_COLOR = Color.FromArgb(255, 139, 69, 19);
    private static readonly Color TEXT_COLOR = Color.FromArgb(255, 255, 255, 255);

    public SpaceGameScene(int screenWidth, int screenHeight)
    {
        this.screenWidth = screenWidth;
        this.screenHeight = screenHeight;

        // Scale sizes based on screen dimensions
        playerSize = Math.Min(screenWidth, screenHeight) * 0.02f;
        bulletSize = Math.Min(screenWidth, screenHeight) * 0.005f;
        minAsteroidSize = Math.Min(screenWidth, screenHeight) * 0.015f;
        maxAsteroidSize = Math.Min(screenWidth, screenHeight) * 0.04f;

        // Center the player
        playerPosition = new Vec2(screenWidth / 2, screenHeight / 2);
        playerRotation = 0;
        bullets = new List<(Vec2, Vec2)>();
        asteroids = new List<(Vec2, float, Vec2)>();
        random = new Random();
        score = 0;
        asteroidSpawnTimer = 0;
        SpawnAsteroid();

        // Initialize input states
        isMovingForward = false;
        isRotatingLeft = false;
        isRotatingRight = false;
        isShooting = false;
    }

    public void Input(int keyCode, bool state)
    {
        switch (keyCode)
        {
            case KEY_W:
                isMovingForward = state;
                break;
            case KEY_A:
                isRotatingLeft = state;
                break;
            case KEY_D:
                isRotatingRight = state;
                break;
            case KEY_SPACE:
                isShooting = state;
                if (state)
                {
                    // Shoot bullet in the direction we're facing
                    Vec2 bulletVelocity = new Vec2(
                        (float)Math.Sin(playerRotation) * 1.5f,
                        -(float)Math.Cos(playerRotation) * 1.5f
                    );
                    bullets.Add((playerPosition, bulletVelocity));
                }
                break;
        }
    }

    public void Render(FrameBuffer buffer, long deltaTime)
    {
        // Update game state
        UpdateGame(deltaTime);

        // Draw bullets
        foreach (var bullet in bullets)
        {
            int x = (int)bullet.position.X;
            int y = (int)bullet.position.Y;
            int size = (int)bulletSize;
            buffer.FillRect(x - size/2, y - size/2, size, size, BULLET_COLOR);
        }

        // Draw asteroids
        foreach (var asteroid in asteroids)
        {
            DrawAsteroid(buffer, asteroid);
        }

        // Draw player ship
        DrawPlayer(buffer);

        // Draw score with larger text and padding
        int textPadding = 20;
        buffer.DrawText(textPadding, textPadding, $"Score: {score}", TEXT_COLOR);
    }

    private void UpdateGame(long deltaTime)
    {
        // Handle rotation
        if (isRotatingLeft)
            playerRotation -= rotationSpeed;
        if (isRotatingRight)
            playerRotation += rotationSpeed;

        // Handle movement
        if (isMovingForward)
        {
            playerPosition += new Vec2(
                (float)Math.Sin(playerRotation) * playerSpeed,
                -(float)Math.Cos(playerRotation) * playerSpeed
            );
            // Wrap around screen
            playerPosition = new Vec2(
                (playerPosition.X + screenWidth) % screenWidth,
                (playerPosition.Y + screenHeight) % screenHeight
            );
        }

        // Update bullets
        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            var bullet = bullets[i];
            bullet.position += bullet.velocity * (deltaTime / 16.0f);
            
            // Remove bullets that are off screen
            if (bullet.position.X < 0 || bullet.position.X >= screenWidth || 
                bullet.position.Y < 0 || bullet.position.Y >= screenHeight)
            {
                bullets.RemoveAt(i);
                continue;
            }

            bullets[i] = bullet;
        }

        // Update asteroids
        for (int i = asteroids.Count - 1; i >= 0; i--)
        {
            var asteroid = asteroids[i];
            asteroid.position += asteroid.velocity * (deltaTime / 16.0f);

            // Wrap asteroids around screen
            asteroid.position = new Vec2(
                (asteroid.position.X + screenWidth) % screenWidth,
                (asteroid.position.Y + screenHeight) % screenHeight
            );

            asteroids[i] = asteroid;

            // Check collision with bullets
            for (int j = bullets.Count - 1; j >= 0; j--)
            {
                var bullet = bullets[j];
                if (Vec2.Distance(bullet.position, asteroid.position) < asteroid.size)
                {
                    // Hit! Remove both bullet and asteroid
                    bullets.RemoveAt(j);
                    asteroids.RemoveAt(i);
                    score += 100;
                    break;
                }
            }
        }

        // Spawn new asteroids
        asteroidSpawnTimer += deltaTime;
        if (asteroidSpawnTimer >= ASTEROID_SPAWN_INTERVAL)
        {
            SpawnAsteroid();
            asteroidSpawnTimer = 0;
        }
    }

    private void SpawnAsteroid()
    {
        // Randomly choose a side of the screen
        int side = random.Next(4);
        Vec2 position = new Vec2();
        Vec2 velocity = new Vec2();
        float baseSpeed = 0.25f; // Increased base speed for larger screens

        switch (side)
        {
            case 0: // Top
                position = new Vec2(random.Next(screenWidth), 0);
                velocity = new Vec2((random.Next(3) - 1) * baseSpeed, baseSpeed);
                break;
            case 1: // Right
                position = new Vec2(screenWidth - 1, random.Next(screenHeight));
                velocity = new Vec2(-baseSpeed, (random.Next(3) - 1) * baseSpeed);
                break;
            case 2: // Bottom
                position = new Vec2(random.Next(screenWidth), screenHeight - 1);
                velocity = new Vec2((random.Next(3) - 1) * baseSpeed, -baseSpeed);
                break;
            case 3: // Left
                position = new Vec2(0, random.Next(screenHeight));
                velocity = new Vec2(baseSpeed, (random.Next(3) - 1) * baseSpeed);
                break;
        }

        // Random size between min and max
        float size = minAsteroidSize + (float)random.NextDouble() * (maxAsteroidSize - minAsteroidSize);
        asteroids.Add((position, size, velocity));
    }

    private void DrawPlayer(FrameBuffer buffer)
    {
        int x = (int)playerPosition.X;
        int y = (int)playerPosition.Y;
        
        // Calculate triangle points for the ship based on rotation
        Point[] points = new Point[3];
        
        // Nose of the ship
        points[0] = new Point(
            x + (int)(Math.Sin(playerRotation) * playerSize),
            y - (int)(Math.Cos(playerRotation) * playerSize)
        );
        
        // Left wing
        points[1] = new Point(
            x + (int)(Math.Sin(playerRotation - 2.5f) * playerSize),
            y - (int)(Math.Cos(playerRotation - 2.5f) * playerSize)
        );
        
        // Right wing
        points[2] = new Point(
            x + (int)(Math.Sin(playerRotation + 2.5f) * playerSize),
            y - (int)(Math.Cos(playerRotation + 2.5f) * playerSize)
        );

        // Draw filled triangle for the ship
        buffer.FillTriangle(points[0], points[1], points[2], PLAYER_COLOR);
    }

    private void DrawAsteroid(FrameBuffer buffer, (Vec2 position, float size, Vec2 velocity) asteroid)
    {
        int x = (int)asteroid.position.X;
        int y = (int)asteroid.position.Y;
        int size = (int)asteroid.size;
        
        // Draw a filled circle-like shape for the asteroid
        for (int dy = -size; dy <= size; dy++)
        {
            for (int dx = -size; dx <= size; dx++)
            {
                if (dx * dx + dy * dy <= size * size)
                {
                    int drawX = x + dx;
                    int drawY = y + dy;
                    
                    // Handle screen wrapping for drawing
                    if (drawX < 0) drawX += screenWidth;
                    if (drawX >= screenWidth) drawX -= screenWidth;
                    if (drawY < 0) drawY += screenHeight;
                    if (drawY >= screenHeight) drawY -= screenHeight;
                    
                    buffer.DrawPixel(drawX, drawY, ASTEROID_COLOR);
                }
            }
        }
    }
}
