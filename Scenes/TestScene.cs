using RawDraw.Engine.Drawing;
using RawDraw.Engine.Input;
using RawDraw.Engine.Primitive;
using RawDraw.Engine.Scene;

namespace RawDraw.Scenes;

public class TestScene : RenderScene
{
    private float posX = 80;
    private float posY = 80;

    private float moveSpeed = 1f;

    public override void OnDraw(FrameBuffer buffer)
    {
        buffer.Clear(Color.Black);

        buffer.DrawText(30, 30, "Hello, World!", Color.White);

        buffer.FillRect((int)posX, (int)posY, 10, 10, Color.White);
    }

    public override void OnUpdate(float deltaTimeMs)
    {
        if (Input is null)
        {
            throw new Exception("Input manager not initialized.");
        }

        if (Input.IsKeyDown(KeyCodes.KEY_W))
        {
            Console.WriteLine("Key W down.");
            posY -= moveSpeed * deltaTimeMs;
        }

        if (Input.IsKeyDown(KeyCodes.KEY_A))
        {
            posX -= moveSpeed * deltaTimeMs;
        }

        if (Input.IsKeyDown(KeyCodes.KEY_S))
        {
            posY += moveSpeed * deltaTimeMs;
        }
        
        if (Input.IsKeyDown(KeyCodes.KEY_D))
        {
            posX += moveSpeed * deltaTimeMs;
        }
    }
}