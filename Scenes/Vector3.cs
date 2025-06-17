namespace RawDraw.Scenes;

public struct Vector3
{
    public float X, Y, Z;

    public Vector3(float x, float y, float z) 
    {
        X = x; Y = y; Z = z;
    }

    public static Vector3 operator +(Vector3 a, Vector3 b) =>
        new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vector3 operator -(Vector3 a, Vector3 b) =>
        new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vector3 operator *(Vector3 v, float s) =>
        new Vector3(v.X * s, v.Y * s, v.Z * s);

    public static float Dot(Vector3 a, Vector3 b) =>
        a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    public static Vector3 Cross(Vector3 a, Vector3 b) =>
        new Vector3(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X);

    public Vector3 Normalized()
    {
        float length = (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        if (length == 0) return new Vector3(0, 0, 0);
        return new Vector3(X / length, Y / length, Z / length);
    }
}
