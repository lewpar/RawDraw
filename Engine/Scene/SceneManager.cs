namespace RawDraw.Engine.Scene;

public class SceneManager
{
    public Scene? CurrentScene { get => _scenes.Peek(); }

    private Stack<Scene> _scenes;

    public SceneManager()
    {
        _scenes = new Stack<Scene>();
    }

    public void Push(Scene scene)
    {
        _scenes.Push(scene);
    }

    public void Pop()
    {
        _scenes.Pop();
    }
}