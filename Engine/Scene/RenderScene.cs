using RawDraw.Engine.Drawing;
using RawDraw.Engine.Input;
using RawDraw.Engine.UI;

namespace RawDraw.Engine.Scene;

public abstract class RenderScene : IScene
{
    public InputManager? Input { get; set; }
    public UIManager UI { get; } = new UIManager();
    
    public virtual void OnDraw(FrameBuffer buffer)
    {

    }

    public void Draw(FrameBuffer buffer)
    {
        OnDraw(buffer);
    }

    public virtual void OnUpdate(float deltaTimeMs)
    {

    }

    public void Update(float deltaTimeMs)
    {
        OnUpdate(deltaTimeMs);
    }

    // UI event hooks
    public virtual void OnTouch(float normX, float normY, bool isTouching) { }
    public virtual void OnButtonPress(string buttonId) { }
}
