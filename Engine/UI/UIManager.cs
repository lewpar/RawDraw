using RawDraw.Engine.Drawing;

namespace RawDraw.Engine.UI;

public class UIManager
{
    private readonly List<IUIElement> _elements = new();
    
    private int width;
    private int height;

    public void Initialize(int width, int height)
    {
        this.width = width;
        this.height = height;
    }

    public void Add(IUIElement element)
    {
        _elements.Add(element);
    }

    public void Remove(IUIElement element)
    {
        _elements.Remove(element);
    }

    public void Draw(FrameBuffer buffer)
    {
        foreach (var element in _elements)
        {
            element.Draw(buffer);
        }
    }

    public void UpdateTouch(float normX, float normY, bool isTouching)
    {
        foreach (var element in _elements)
        {
            element.UpdateTouch(normX, normY, isTouching, width, height);

            if (element is Button btn && btn.IsPressed && btn.OnPressed != null)
            {
                btn.OnPressed();
            }
        }
    }

    public IUIElement? GetById(string id) => _elements.FirstOrDefault(e => e.Id == id);
    public IEnumerable<IUIElement> Elements => _elements;
} 