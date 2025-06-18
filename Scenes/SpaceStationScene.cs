using System;
using System.Drawing;

namespace RawDraw.Scenes;

public class SpaceStationScene : IScene
{
    private int _frame;
    private Random _rand = new();

    public void Render(FrameBuffer buffer, long deltaTime)
    {
        int width = buffer.Width;
        int height = buffer.Height;
        _frame++;

        // Background space
        buffer.FillRect(0, 0, width, height, Color.Black);
        DrawStars(buffer, 100);
        DrawPlanet(buffer, width / 2, height / 2, 60);
        if (_frame % 200 < 50)
            DrawComet(buffer, (_frame % 200) * 6 - 30, 50 + (_frame % 50), 5 + (_frame % 3));

        DrawWindowFrame(buffer, width, height);
        DrawHudDots(buffer);
    }

    private void DrawStars(FrameBuffer buffer, int count)
    {
        Random rand = new(_frame / 4); // makes stars appear to twinkle but stable per frame
        for (int i = 0; i < count; i++)
        {
            int x = rand.Next(buffer.Width);
            int y = rand.Next(buffer.Height);
            byte intensity = (byte)(rand.Next(100, 255));
            buffer.DrawPixel(x, y, Color.FromArgb(intensity, intensity, intensity));
        }
    }

    private void DrawPlanet(FrameBuffer buffer, int centerX, int centerY, int radius)
    {
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    // Simulate slight rotation with sinusoidal shading
                    int dx = x + (int)(Math.Sin((_frame + y) * 0.01f) * 5);
                    Color c = Color.FromArgb(30, 144, 255); // DodgerBlue planet
                    if ((x + y + _frame / 5) % 10 < 5) c = Color.MediumSlateBlue;
                    buffer.DrawPixel(centerX + dx, centerY + y, c);
                }
            }
        }
    }

    private void DrawComet(FrameBuffer buffer, int x, int y, int size)
    {
        for (int i = 0; i < size; i++)
        {
            buffer.DrawPixel(x - i, y + i / 2, Color.White);
            buffer.DrawPixel(x - i - 1, y + i / 2 + 1, Color.LightGray);
        }
    }

    private void DrawWindowFrame(FrameBuffer buffer, int width, int height)
    {
        buffer.DrawRect(5, 5, width - 10, height - 10, 5, Color.DarkGray);
        buffer.DrawRect(20, 20, width - 40, height - 40, 3, Color.Silver);
    }

    private void DrawHudDots(FrameBuffer buffer)
    {
        if ((_frame / 10) % 2 == 0)
        {
            buffer.FillRect(15, 15, 4, 4, Color.LimeGreen);
            buffer.FillRect(25, 15, 4, 4, Color.Orange);
        }
        else
        {
            buffer.FillRect(15, 15, 4, 4, Color.DarkGreen);
            buffer.FillRect(25, 15, 4, 4, Color.DarkOrange);
        }

        buffer.DrawText(35, 13, "SYS OK", Color.LightGreen);
    }

    public void Input(int keyCode, bool state)
    {
        // Space station scene doesn't need input handling
    }
}
