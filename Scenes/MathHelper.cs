namespace RawDraw.Scenes;

public static class MathHelper
{
    public static Vector3 RotateX(Vector3 v, float angle)
    {
        float cos = (float)Math.Cos(angle);
        float sin = (float)Math.Sin(angle);
        return new Vector3(
            v.X,
            v.Y * cos - v.Z * sin,
            v.Y * sin + v.Z * cos);
    }

    public static Vector3 RotateY(Vector3 v, float angle)
    {
        float cos = (float)Math.Cos(angle);
        float sin = (float)Math.Sin(angle);
        return new Vector3(
            v.X * cos + v.Z * sin,
            v.Y,
            -v.X * sin + v.Z * cos);
    }

    public static Vector3 RotateZ(Vector3 v, float angle)
    {
        float cos = (float)Math.Cos(angle);
        float sin = (float)Math.Sin(angle);
        return new Vector3(
            v.X * cos - v.Y * sin,
            v.X * sin + v.Y * cos,
            v.Z);
    }

    // Simple perspective projection
    public static Vector2 Project(Vector3 point, int width, int height, float fov, float viewerDistance)
    {
        float factor = fov / (viewerDistance + point.Z);
        float x = point.X * factor + width / 2;
        float y = -point.Y * factor + height / 2; // minus to flip y-axis if needed
        return new Vector2((int)x, (int)y);
    }
}
