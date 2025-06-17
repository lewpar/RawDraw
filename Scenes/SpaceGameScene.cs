using System.Collections.Generic;
using System.Drawing;
using Vec2 = System.Numerics.Vector2;

namespace RawDraw.Scenes;

public class SpaceGameScene : IScene
{
    private Vec2 playerPosition;
    private float playerRotation;
    private float playerSpeed = 0.5f; // Increased speed for larger screens
    private float rotationSpeed = 0.1f;
    private List<(Vec2 position, Vec2 velocity)> bullets;
    private List<(Vec2 position, float size, Vec2 velocity)> asteroids;
    private int score;
    private Random random;
    private float asteroidSpawnTimer;
    private const float ASTEROID_SPAWN_INTERVAL = 2000; // milliseconds

    // Game object sizes
    private readonly float playerSize;
    private readonly float bulletSize;
    private readonly float minAsteroidSize;
    private readonly float maxAsteroidSize;

    // Screen dimensions
    private readonly int screenWidth;
    private readonly int screenHeight;

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
        playerSize = Math.Min(screenWidth, screenHeight) * 0.02f; // 2% of screen size
        bulletSize = Math.Min(screenWidth, screenHeight) * 0.005f; // 0.5% of screen size
        minAsteroidSize = Math.Min(screenWidth, screenHeight) * 0.015f; // 1.5% of screen size
        maxAsteroidSize = Math.Min(screenWidth, screenHeight) * 0.04f; // 4% of screen size

        // Center the player
        playerPosition = new Vec2(screenWidth / 2, screenHeight / 2);
        playerRotation = 0;
        bullets = new List<(Vec2, Vec2)>();
        asteroids = new List<(Vec2, float, Vec2)>();
        random = new Random();
        score = 0;
        asteroidSpawnTimer = 0;
        SpawnAsteroid();
    }

    public void Render(FrameBuffer buffer, long deltaTime)
    {
        // Clear screen with dark blue background
        buffer.Clear(BACKGROUND_COLOR);

        // Update game state
        UpdateGame(deltaTime);

        // Draw player ship
        DrawPlayer(buffer);

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

        // Draw score with larger text and padding
        int textPadding = 20;
        buffer.DrawText(textPadding, textPadding, $"Score: {score}", TEXT_COLOR);

        // Handle input last to prepare for next frame
        HandleInput();
    }

    private void UpdateGame(long deltaTime)
    {
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

    private void HandleInput()
    {
        while (Console.KeyAvailable)
        {
            var key = Console.ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.W:
                    // Move forward in the direction we're facing
                    playerPosition += new Vec2(
                        (float)Math.Sin(playerRotation) * playerSpeed,
                        -(float)Math.Cos(playerRotation) * playerSpeed
                    );
                    // Wrap around screen
                    playerPosition = new Vec2(
                        (playerPosition.X + screenWidth) % screenWidth,
                        (playerPosition.Y + screenHeight) % screenHeight
                    );
                    break;
                case ConsoleKey.A:
                    playerRotation -= rotationSpeed;
                    break;
                case ConsoleKey.D:
                    playerRotation += rotationSpeed;
                    break;
                case ConsoleKey.Spacebar:
                    // Shoot bullet in the direction we're facing
                    Vec2 bulletVelocity = new Vec2(
                        (float)Math.Sin(playerRotation) * 1.5f, // Increased bullet speed
                        -(float)Math.Cos(playerRotation) * 1.5f
                    );
                    bullets.Add((playerPosition, bulletVelocity));
                    break;
            }
        }
    }
}
