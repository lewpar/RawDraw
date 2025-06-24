using RawDraw.Engine.Drawing;
using RawDraw.Engine.Input;
using RawDraw.Engine.UI;

namespace RawDraw.Engine.Scene;

public abstract class Scene
{
    public List<UIElement> UIElements { get; } = new List<UIElement>();
    public InputManager? Input { get; set; }
    
    public virtual void Draw(FrameBuffer buffer) { }
    public virtual void Update(float deltaTimeMs) { }
}