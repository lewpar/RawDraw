using RawDraw.Engine.Drawing;

namespace RawDraw.Engine.UI;

public interface IUIElement
{
    string Id { get; }
    void Draw(FrameBuffer buffer);
    void UpdateTouch(float normX, float normY, bool isTouching, int screenWidth, int screenHeight);
} 