namespace RawDraw.Engine.Scene;

public class SceneManager
{
    public RenderScene? CurrentScene { get => _scenes.Peek(); }

    private Stack<RenderScene> _scenes;

    public SceneManager()
    {
        _scenes = new Stack<RenderScene>();
    }

    public void Push(RenderScene scene)
    {
        _scenes.Push(scene);
    }

    public void Pop()
    {
        _scenes.Pop();
    }
}