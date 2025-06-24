using System.Xml.Serialization;

namespace RawDraw.Engine.UI;

public static class XmlParser
{
    public const string NAMESPACE = "http://rawdraw.com/ui";

    private static void SetParent(UIElement element, UIElement parent)
    {
        element.Parent = parent;

        foreach (var child in element.Children)
        {
            SetParent(child, element);
        }
    }

    public static FrameElement Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"No ui exists at path '{path}'.");
        }

        var xml = File.OpenRead(path);
        var serializer = new XmlSerializer(typeof(FrameElement), NAMESPACE);

        var frame = serializer.Deserialize(xml) as FrameElement;
        if (frame is null)
        {
            throw new Exception($"Failed to deserialize frame element from path '{path}'.");
        }

        foreach (var child in frame.Children)
        {
            SetParent(child, frame);
        }

        return frame;
    }
}