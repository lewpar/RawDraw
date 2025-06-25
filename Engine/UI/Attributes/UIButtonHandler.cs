namespace RawDraw.Engine.UI.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class UIButtonHandler : Attribute
{
    public string HandlerName { get; init; }

    public UIButtonHandler(string handlerName)
    {
        HandlerName = handlerName;
    }
}