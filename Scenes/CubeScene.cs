using System.Drawing;

namespace RawDraw.Scenes;

public class CubeScene : IScene
{
    private Vector3[] vertices;
    private (int, int)[] edges;

    private float angleX = 0;
    private float angleY = 0;

    private int width;
    private int height;

    public CubeScene(int width, int height)
    {
        this.width = width;
        this.height = height;

        // Define cube vertices (size 1, centered at origin)
        vertices = new Vector3[]
        {
            new Vector3(-1,  1, -1),
            new Vector3( 1,  1, -1),
            new Vector3( 1, -1, -1),
            new Vector3(-1, -1, -1),
            new Vector3(-1,  1,  1),
            new Vector3( 1,  1,  1),
            new Vector3( 1, -1,  1),
            new Vector3(-1, -1,  1),
        };

        // Define edges between vertices (pairs of indices)
        edges = new (int, int)[]
        {
            (0,1),(1,2),(2,3),(3,0), // back face
            (4,5),(5,6),(6,7),(7,4), // front face
            (0,4),(1,5),(2,6),(3,7)  // connecting edges
        };
    }

    public void Render(FrameBuffer buffer, long deltaTime)
    {
        buffer.Clear(Color.Black);

        angleX += 0.001f * deltaTime; // rotate based on deltaTime
        angleY += 0.002f * deltaTime;

        Vector2[] projected = new Vector2[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            // Rotate around X and Y
            Vector3 rotated = MathHelper.RotateX(vertices[i], angleX);
            rotated = MathHelper.RotateY(rotated, angleY);

            // Scale cube size
            rotated = rotated * 100;

            // Translate cube away from camera along Z axis
            Vector3 translated = new Vector3(rotated.X, rotated.Y, rotated.Z + 400);

            // Project 3D point to 2D screen coordinates
            projected[i] = MathHelper.Project(translated, width, height, fov: 256, viewerDistance: 3);
        }

        // Draw edges
        foreach (var (startIdx, endIdx) in edges)
        {
            DrawLine(buffer, projected[startIdx].X, projected[startIdx].Y, projected[endIdx].X, projected[endIdx].Y, Color.White);
        }
    }

    // Bresenham line algorithm to draw edges
    private void DrawLine(FrameBuffer buffer, int x0, int y0, int x1, int y1, Color color)
    {
        int dx = Math.Abs(x1 - x0);
        int dy = -Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;

        while (true)
        {
            buffer.DrawPixel(x0, y0, color);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 >= dy)
            {
                err += dy;
                x0 += sx;
            }
            if (e2 <= dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    public void Input(int keyCode, bool state)
    {
        // Cube scene doesn't need input handling
    }
}
