using RawDraw.Engine.UI;

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
        if (!string.IsNullOrWhiteSpace(scene.UI))
        {
            var frame = XmlParser.Load(scene.UI);
            scene.Frame = frame;   
        }

        _scenes.Push(scene);
    }

    public void Pop()
    {
        _scenes.Pop();
    }
}