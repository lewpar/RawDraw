using RawDraw.Engine.UI;

namespace RawDraw.Engine.Scene;

public class SceneManager
{
    public RenderScene? CurrentScene { get => _scenes.Peek(); }

    private Stack<RenderScene> _scenes;

    public SceneManager()
    {
        _scenes = new Stack<RenderScene>();
    }

    public void Push(RenderScene renderScene)
    {
        if (!string.IsNullOrWhiteSpace(renderScene.UI))
        {
            var frame = XmlParser.Load(renderScene, renderScene.UI);
            renderScene.Frame = frame;   
        }

        _scenes.Push(renderScene);
    }

    public void Pop()
    {
        _scenes.Pop();
    }
}