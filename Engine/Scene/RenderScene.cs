using RawDraw.Engine.Drawing;
using RawDraw.Engine.Input;

namespace RawDraw.Engine.Scene;

public abstract class RenderScene : IScene
{
    public InputManager? Input { get; set; }
    
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
}
