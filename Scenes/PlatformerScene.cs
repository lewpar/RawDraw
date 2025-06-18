using System.Collections.Generic;
using System.Drawing;
using Vec2 = System.Numerics.Vector2;

namespace RawDraw.Scenes;

public class PlatformerScene : IScene
{
    // Screen dimensions
    private readonly int screenWidth;
    private readonly int screenHeight;

    // Player properties
    private Vec2 playerPosition;
    private Vec2 playerVelocity;
    private readonly float playerWidth;
    private readonly float playerHeight;
    private bool isJumping;
    private bool isGrounded;
    private float gravity;
    private float jumpForce;
    private float moveSpeed;
    private const float BASE_GRAVITY = 0.5f;
    private const float BASE_JUMP_FORCE = -120f;
    private const float BASE_MOVE_SPEED = 75f;

    // Input states
    private bool isMovingLeft;
    private bool isMovingRight;
    private bool isJumpPressed;

    // Camera properties
    private Vec2 cameraPosition;
    private const float CAMERA_SMOOTHING = 0.1f;

    // Level properties
    private readonly List<Rectangle> platforms;
    private readonly float platformHeight;
    private readonly float minPlatformWidth;
    private readonly float maxPlatformWidth;
    private readonly float platformSpacing;
    private readonly int numPlatforms;

    // Colors
    private static readonly Color BACKGROUND_COLOR = Color.FromArgb(255, 135, 206, 235); // Sky blue
    private static readonly Color PLAYER_COLOR = Color.FromArgb(255, 255, 0, 0); // Red
    private static readonly Color PLATFORM_COLOR = Color.FromArgb(255, 34, 139, 34); // Forest green
    private static readonly Color TEXT_COLOR = Color.FromArgb(255, 255, 255, 255); // White

    // Key codes
    private const int KEY_A = 30;
    private const int KEY_D = 32;
    private const int KEY_SPACE = 57;

    public PlatformerScene(int screenWidth, int screenHeight)
    {
        this.screenWidth = screenWidth;
        this.screenHeight = screenHeight;

        // Scale physics values based on screen size
        float screenScale = Math.Min(screenWidth, screenHeight) / 1080f; // Scale relative to 1080p
        gravity = BASE_GRAVITY * screenScale * 2f; // Double the base gravity
        jumpForce = BASE_JUMP_FORCE * screenScale * 2f; // Double the base jump force
        moveSpeed = BASE_MOVE_SPEED * screenScale * 3f; // Triple the base move speed

        // Initialize player
        playerWidth = screenWidth * 0.02f;
        playerHeight = screenHeight * 0.04f;
        playerVelocity = Vec2.Zero;
        isJumping = false;
        isGrounded = false;

        // Initialize input states
        isMovingLeft = false;
        isMovingRight = false;
        isJumpPressed = false;

        // Initialize camera
        cameraPosition = Vec2.Zero;

        // Initialize level
        platformHeight = screenHeight * 0.02f;
        minPlatformWidth = screenWidth * 0.1f;
        maxPlatformWidth = screenWidth * 0.2f;
        platformSpacing = screenHeight * 0.15f;
        numPlatforms = 20;

        platforms = new List<Rectangle>();
        GenerateLevel();

        // Position player on the first platform
        if (platforms.Count > 0)
        {
            var firstPlatform = platforms[0];
            playerPosition = new Vec2(
                firstPlatform.X + firstPlatform.Width / 2, // Center on platform
                firstPlatform.Y - playerHeight - 5 // 5 pixels above platform
            );
        }
        else
        {
            // Fallback position if no platforms were generated
            playerPosition = new Vec2(screenWidth * 0.2f, screenHeight * 0.5f);
        }
    }

    public void Input(int keyCode, bool state)
    {
        switch (keyCode)
        {
            case KEY_A:
                isMovingLeft = state;
                break;
            case KEY_D:
                isMovingRight = state;
                break;
            case KEY_SPACE:
                isJumpPressed = state;
                if (state && isGrounded && !isJumping)
                {
                    playerVelocity.Y = jumpForce;
                    isJumping = true;
                    isGrounded = false;
                }
                break;
        }
    }

    public void Render(FrameBuffer buffer, long deltaTime)
    {
        // Update game state
        UpdateGame(deltaTime);

        // Draw platforms
        foreach (var platform in platforms)
        {
            // Convert platform position to screen space
            int screenX = (int)(platform.X - cameraPosition.X);
            int screenY = (int)(platform.Y - cameraPosition.Y);

            // Only draw if on screen
            if (screenX + platform.Width > 0 && screenX < screenWidth &&
                screenY + platform.Height > 0 && screenY < screenHeight)
            {
                buffer.FillRect(screenX, screenY, platform.Width, platform.Height, PLATFORM_COLOR);
            }
        }

        // Draw player
        int playerScreenX = (int)(playerPosition.X - cameraPosition.X);
        int playerScreenY = (int)(playerPosition.Y - cameraPosition.Y);
        buffer.FillRect(playerScreenX, playerScreenY, (int)playerWidth, (int)playerHeight, PLAYER_COLOR);

        // Draw debug info
        buffer.DrawText(10, 10, $"Position: ({playerPosition.X:F1}, {playerPosition.Y:F1})", TEXT_COLOR);
        buffer.DrawText(10, 30, $"Velocity: ({playerVelocity.X:F1}, {playerVelocity.Y:F1})", TEXT_COLOR);
        buffer.DrawText(10, 50, $"Grounded: {isGrounded}", TEXT_COLOR);
    }

    private void UpdateGame(long deltaTime)
    {
        float deltaTimeSeconds = deltaTime / 1000f;

        // Handle movement input
        if (isMovingLeft)
            playerVelocity.X = -moveSpeed;
        else if (isMovingRight)
            playerVelocity.X = moveSpeed;
        else
            playerVelocity.X *= 0.9f; // Apply friction when no keys are pressed

        // Apply gravity
        playerVelocity.Y += gravity;

        // Update position
        playerPosition += playerVelocity * deltaTimeSeconds;

        // Check platform collisions
        isGrounded = false;
        foreach (var platform in platforms)
        {
            if (CheckCollision(playerPosition, playerWidth, playerHeight, 
                             new Vec2(platform.X, platform.Y), platform.Width, platform.Height))
            {
                // Collision from above (landing on platform)
                if (playerVelocity.Y > 0 && playerPosition.Y + playerHeight - playerVelocity.Y * deltaTimeSeconds <= platform.Y)
                {
                    playerPosition.Y = platform.Y - playerHeight;
                    playerVelocity.Y = 0;
                    isGrounded = true;
                    isJumping = false;
                }
                // Collision from below (hitting ceiling)
                else if (playerVelocity.Y < 0 && playerPosition.Y - playerVelocity.Y * deltaTimeSeconds >= platform.Y + platform.Height)
                {
                    playerPosition.Y = platform.Y + platform.Height;
                    playerVelocity.Y = 0;
                }
                // Collision from sides
                else
                {
                    if (playerVelocity.X > 0)
                        playerPosition.X = platform.X - playerWidth;
                    else if (playerVelocity.X < 0)
                        playerPosition.X = platform.X + platform.Width;
                    playerVelocity.X = 0;
                }
            }
        }

        // Update camera to follow player
        Vec2 targetCameraPos = new Vec2(
            playerPosition.X - screenWidth * 0.3f, // Keep player 30% from left edge
            playerPosition.Y - screenHeight * 0.5f // Keep player centered vertically
        );

        // Smooth camera movement
        cameraPosition += (targetCameraPos - cameraPosition) * CAMERA_SMOOTHING;

        // Prevent camera from going below ground level
        cameraPosition.Y = Math.Max(0, cameraPosition.Y);
    }

    private void GenerateLevel()
    {
        float currentY = screenHeight * 0.7f;
        float currentX = 0;

        // Generate initial platforms
        for (int i = 0; i < numPlatforms; i++)
        {
            float platformWidth = minPlatformWidth + (float)Random.Shared.NextDouble() * (maxPlatformWidth - minPlatformWidth);
            
            // Make first platform wider and more stable
            if (i == 0)
            {
                platformWidth = maxPlatformWidth * 1.5f;
                currentY = screenHeight * 0.7f; // Fixed height for first platform
            }
            
            platforms.Add(new Rectangle((int)currentX, (int)currentY, (int)platformWidth, (int)platformHeight));
            
            currentX += platformWidth + (screenWidth * 0.1f);
            
            // Only vary Y position after first platform
            if (i > 0)
            {
                currentY += (float)(Random.Shared.NextDouble() - 0.5) * platformSpacing;
                // Keep platforms within reasonable bounds
                currentY = Math.Clamp(currentY, screenHeight * 0.3f, screenHeight * 0.8f);
            }
        }
    }

    private bool CheckCollision(Vec2 pos1, float width1, float height1, Vec2 pos2, float width2, float height2)
    {
        return pos1.X < pos2.X + width2 &&
               pos1.X + width1 > pos2.X &&
               pos1.Y < pos2.Y + height2 &&
               pos1.Y + height1 > pos2.Y;
    }
} 