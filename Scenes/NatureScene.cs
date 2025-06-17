using System;
using System.Drawing;

namespace RawDraw.Scenes;

public class NatureScene : IScene
{
    private int _frame;

    public void Render(FrameBuffer buffer, long deltaTime)
    {
        int width = buffer.Width;
        int height = buffer.Height;
        _frame++;

        // Sky
        buffer.FillRect(0, 0, width, height, Color.CornflowerBlue);

        // Animate sun (bobbing)
        float sunYOffset = (float)Math.Sin(_frame * 0.02f) * 5;
        DrawSun(buffer, 60, 60 + (int)sunYOffset, 30, Color.Yellow);

        // Clouds
        int cloudX1 = (_frame / 3) % (width + 100) - 100;
        DrawCloud(buffer, cloudX1, 40);
        int cloudX2 = (_frame / 4 + 100) % (width + 100) - 100;
        DrawCloud(buffer, cloudX2, 70);

        // Hills
        DrawHill(buffer, 0, height - 100, width / 2, 100, Color.ForestGreen);
        DrawHill(buffer, width / 3, height - 70, width / 2, 100, Color.Green);

        // Tree
        int leafPulse = 1 + (int)(Math.Sin(_frame * 0.05f) * 2);
        DrawTree(buffer, width - 100, height - 120, leafPulse);

        // Flowers on the first hill
        DrawFlowers(buffer, 30, height - 80, 100, 60, 7);

        // Birds flying
        DrawBird(buffer, (_frame * 2) % (width + 50) - 50, 20);
        DrawBird(buffer, (_frame * 2 + 100) % (width + 50) - 50, 35);

        // Grass blades
        DrawGrass(buffer, 0, height - 40, width, 40, 5);
    }

    private void DrawSun(FrameBuffer buffer, int centerX, int centerY, int radius, Color color)
    {
        for (int y = -radius; y <= radius; y++)
            for (int x = -radius; x <= radius; x++)
                if (x * x + y * y <= radius * radius)
                    buffer.DrawPixel(centerX + x, centerY + y, color);
    }

    private void DrawCloud(FrameBuffer buffer, int x, int y)
    {
        DrawCircle(buffer, x + 10, y, 10, Color.White);
        DrawCircle(buffer, x + 20, y - 5, 12, Color.White);
        DrawCircle(buffer, x + 30, y, 10, Color.White);
        DrawCircle(buffer, x + 20, y + 5, 10, Color.White);
    }

    private void DrawHill(FrameBuffer buffer, int x, int y, int width, int height, Color color)
    {
        for (int py = 0; py < height; py++)
        {
            int hillWidth = (int)(width * Math.Sin(Math.PI * py / height));
            int hillX = x + (width - hillWidth) / 2;
            buffer.FillRect(hillX, y + py, hillWidth, 1, color);
        }
    }

    private void DrawTree(FrameBuffer buffer, int baseX, int baseY, int leafRadiusOffset)
    {
        buffer.FillRect(baseX, baseY, 10, 40, Color.SaddleBrown);
        DrawCircle(buffer, baseX + 5, baseY - 10, 18 + leafRadiusOffset, Color.DarkGreen);
        DrawCircle(buffer, baseX - 5, baseY - 25, 18 + leafRadiusOffset, Color.Green);
        DrawCircle(buffer, baseX + 15, baseY - 25, 18 + leafRadiusOffset, Color.ForestGreen);
    }

    private void DrawFlowers(FrameBuffer buffer, int startX, int startY, int areaWidth, int areaHeight, int count)
    {
        Random rand = new(_frame); // deterministic per frame
        for (int i = 0; i < count; i++)
        {
            int x = startX + rand.Next(areaWidth);
            int y = startY + rand.Next(areaHeight);
            Color color = Color.FromArgb(255, rand.Next(256), rand.Next(256), rand.Next(256));
            DrawCircle(buffer, x, y, 2, color);
        }
    }

    private void DrawBird(FrameBuffer buffer, int x, int y)
    {
        // Simple "V" shaped bird
        buffer.DrawPixel(x, y, Color.Black);
        buffer.DrawPixel(x + 1, y - 1, Color.Black);
        buffer.DrawPixel(x + 2, y, Color.Black);
    }

    private void DrawGrass(FrameBuffer buffer, int startX, int startY, int width, int height, int spacing)
    {
        for (int x = startX; x < startX + width; x += spacing)
        {
            int bladeHeight = 3 + (int)(Math.Sin((x + _frame) * 0.2f) * 2);
            for (int y = 0; y < bladeHeight; y++)
            {
                buffer.DrawPixel(x, startY - y, Color.Green);
            }
        }
    }

    private void DrawCircle(FrameBuffer buffer, int centerX, int centerY, int radius, Color color)
    {
        for (int y = -radius; y <= radius; y++)
            for (int x = -radius; x <= radius; x++)
                if (x * x + y * y <= radius * radius)
                    buffer.DrawPixel(centerX + x, centerY + y, color);
    }
}
