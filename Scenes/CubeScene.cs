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

        angleX += 0.001f * deltaTime;
        angleY += 0.002f * deltaTime;

        Vector2[] projected = new Vector2[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 rotated = MathHelper.RotateX(vertices[i], angleX);
            rotated = MathHelper.RotateY(rotated, angleY);
            rotated = rotated * 100;
            Vector3 translated = new Vector3(rotated.X, rotated.Y, rotated.Z + 400);
            projected[i] = MathHelper.Project(translated, width, height, fov: 256, viewerDistance: 3);
        }

        // Helper function to convert Vector2 to Point (int)
        Point ToPoint(Vector2 v) => new Point(v.X, v.Y);

        // Draw cube faces as 2 triangles each:
        // Cube vertex indices reference:
        // 0: (-1,  1, -1)
        // 1: ( 1,  1, -1)
        // 2: ( 1, -1, -1)
        // 3: (-1, -1, -1)
        // 4: (-1,  1,  1)
        // 5: ( 1,  1,  1)
        // 6: ( 1, -1,  1)
        // 7: (-1, -1,  1)

        // Back face (0,1,2,3)
        buffer.FillTriangle(ToPoint(projected[0]), ToPoint(projected[1]), ToPoint(projected[2]), Color.White);
        buffer.FillTriangle(ToPoint(projected[0]), ToPoint(projected[2]), ToPoint(projected[3]), Color.White);

        // Front face (4,5,6,7)
        buffer.FillTriangle(ToPoint(projected[4]), ToPoint(projected[5]), ToPoint(projected[6]), Color.White);
        buffer.FillTriangle(ToPoint(projected[4]), ToPoint(projected[6]), ToPoint(projected[7]), Color.White);

        // Top face (0,1,5,4)
        buffer.FillTriangle(ToPoint(projected[0]), ToPoint(projected[1]), ToPoint(projected[5]), Color.White);
        buffer.FillTriangle(ToPoint(projected[0]), ToPoint(projected[5]), ToPoint(projected[4]), Color.White);

        // Bottom face (3,2,6,7)
        buffer.FillTriangle(ToPoint(projected[3]), ToPoint(projected[2]), ToPoint(projected[6]), Color.White);
        buffer.FillTriangle(ToPoint(projected[3]), ToPoint(projected[6]), ToPoint(projected[7]), Color.White);

        // Left face (0,3,7,4)
        buffer.FillTriangle(ToPoint(projected[0]), ToPoint(projected[3]), ToPoint(projected[7]), Color.White);
        buffer.FillTriangle(ToPoint(projected[0]), ToPoint(projected[7]), ToPoint(projected[4]), Color.White);

        // Right face (1,2,6,5)
        buffer.FillTriangle(ToPoint(projected[1]), ToPoint(projected[2]), ToPoint(projected[6]), Color.White);
        buffer.FillTriangle(ToPoint(projected[1]), ToPoint(projected[6]), ToPoint(projected[5]), Color.White);
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
}
