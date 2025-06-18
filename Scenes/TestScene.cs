using RawDraw.Engine.Drawing;
using RawDraw.Engine.Input;
using RawDraw.Engine.Primitive;
using RawDraw.Engine.Scene;

namespace RawDraw.Scenes;

public class TestScene : RenderScene
{
    private float posX = 80;
    private float posY = 80;
    private float moveSpeed = 100f;

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
            throw new InvalidOperationException("Input manager is not initialized in TestScene");
        }

        float normalizedDelta = deltaTimeMs / 1000f; // Convert to seconds

        if (Input.IsKeyDown(KeyCodes.KEY_W))
        {
            posY -= moveSpeed * normalizedDelta;
        }

        if (Input.IsKeyDown(KeyCodes.KEY_S))
        {
            posY += moveSpeed * normalizedDelta;
        }

        if (Input.IsKeyDown(KeyCodes.KEY_A))
        {
            posX -= moveSpeed * normalizedDelta;
        }

        if (Input.IsKeyDown(KeyCodes.KEY_D))
        {
            posX += moveSpeed * normalizedDelta;
        }
    }
}