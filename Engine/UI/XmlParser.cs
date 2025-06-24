using System.Xml;
using System.Xml.Schema;
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
        
        var xmlSettings = new XmlReaderSettings();
        
        xmlSettings.ValidationType = ValidationType.Schema;
        xmlSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
        xmlSettings.Schemas.Add(NAMESPACE, "./UI/ui.xsd");

        xmlSettings.ValidationEventHandler += (sender, args) => throw new Exception(args.Message);
        
        var xmlStream = File.OpenRead(path);
        var xmlReader = XmlReader.Create(xmlStream, xmlSettings);
        var serializer = new XmlSerializer(typeof(FrameElement), NAMESPACE);

        var frame = serializer.Deserialize(xmlReader) as FrameElement;
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