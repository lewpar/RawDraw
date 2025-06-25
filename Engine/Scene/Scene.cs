using RawDraw.Engine.Drawing;
using RawDraw.Engine.Input;
using RawDraw.Engine.UI;

namespace RawDraw.Engine.Scene;

public abstract class Scene
{   
    public InputManager? Input { get; set; }

    public string? UI { get; set; }
    public FrameElement? Frame { get; set; }

    public virtual void Draw(FrameBuffer buffer) { }
    public virtual void Update(float deltaTimeMs) { }
}