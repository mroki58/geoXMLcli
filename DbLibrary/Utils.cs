using System.Xml;
using System.Xml.XPath;

using System.Text.RegularExpressions;

namespace DbLibrary;

class Utils
{
    public static XmlDocument validateXML(string xml)
    {
        XmlDocument? xmlDocument = new XmlDocument();
        try
        {
            xmlDocument.LoadXml(xml);
        }
        catch (XmlException ex)
        {
            throw new InvalidOperationException("Invalid XML format.", ex);
        }

        return xmlDocument;
    }

    public static void validateXPath(string path)
    {
        try
        {
            XPathExpression.Compile(path); // Sprawdzenie poprawności XPath
        }
        catch (XPathException)
        {
            throw new ArgumentException("Invalid XPath.");
        }

    }

    public static void validateValue(string value)
    {
        if (!Regex.IsMatch(value, @"^[a-zA-Z0-9.,]+$"))
        {
            throw new ArgumentException("Invalid Value");
        }
    }
    
}