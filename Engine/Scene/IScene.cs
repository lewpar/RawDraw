using RawDraw.Engine.Drawing;

namespace RawDraw.Engine.Scene;

public interface IScene
{
    public void Draw(FrameBuffer buffer);
    public void Update(float deltaTimeMs);
}