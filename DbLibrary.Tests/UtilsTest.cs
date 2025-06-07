using System;
using System.Xml;

namespace DbLibrary.Tests;

public class UnitTest1
{
    [Fact]
    public void ValidateXML_ValidXml_DoesNotThrow()
    {
        string xml = "<root><child>test</child></root>";
        var doc = Utils.validateXML(xml);
        Assert.IsType<XmlDocument>(doc);
        Assert.Equal("root", doc.DocumentElement.Name);
    }

    [Fact]
    public void ValidateXML_InvalidXml_ThrowsInvalidOperationException()
    {
        string xml = "<root><child></root>";
        Assert.Throws<InvalidOperationException>(() => Utils.validateXML(xml));
    }

    [Fact]
    public void ValidateXPath_ValidXPath_DoesNotThrow()
    {
        string path = "/root/child";
        var ex = Record.Exception(() => Utils.validateXPath(path));
        Assert.Null(ex);
    }

    [Fact]
    public void ValidateXPath_InvalidXPath_ThrowsArgumentException()
    {
        string path = "/root/child[";
        Assert.Throws<ArgumentException>(() => Utils.validateXPath(path));
    }

    [Theory]
    [InlineData("Zażółć gęślą jaźń")]
    [InlineData("Test123")]
    [InlineData("Łódź 2024-06-07")]
    [InlineData("Ala ma kota.")]
    [InlineData("123,456.789-0")]
    public void ValidateValue_ValidValues_DoesNotThrow(string value)
    {
        var ex = Record.Exception(() => Utils.validateValue(value));
        Assert.Null(ex);
    }

    [Theory]
    [InlineData("<script>")]
    [InlineData("Hello@World")]
    [InlineData("slash/test")]
    public void ValidateValue_InvalidValues_ThrowsArgumentException(string value)
    {
        Assert.Throws<ArgumentException>(() => Utils.validateValue(value));
    }

    

}