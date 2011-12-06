using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using System.IO;

namespace LicenseHeaderManager.Headers
{
  internal class DocumentHeader
  {
    private readonly TextDocument _document;
    private readonly string _text;
    private readonly IEnumerable<DocumentHeaderProperty> _properties;

    public DocumentHeader(TextDocument document, string[] rawLines)
    {
      _document = document;
      _properties = CreateProperties();
      _text = CreateText(rawLines);
    }

    private IEnumerable<DocumentHeaderProperty> CreateProperties()
    {
      List<DocumentHeaderProperty> properties = new List<DocumentHeaderProperty>()
      {
        new DocumentHeaderProperty("%FullFileName%", fi => fi.FullName),
        new DocumentHeaderProperty("%FileName%", fi => fi.Name),
      };
      return properties;
    }

    private string CreateText(string[] rawLines)
    {
      if (rawLines == null)
      {
        return null;
      }

      string rawText = string.Join(Environment.NewLine, rawLines);
      string finalText = CreateFinalText(rawText);
      return finalText;
    }

    private string CreateFinalText(string rawText)
    {
      string pathToDocument = _document.Parent.FullName;

      string finalText = rawText;

      if (File.Exists(pathToDocument))
      {
        FileInfo fileInfo = new FileInfo(pathToDocument);
        foreach (DocumentHeaderProperty property in _properties)
        {
          finalText = finalText.Replace(property.Token, property.GetValue(fileInfo));
        }
      }

      return finalText;
    }

    public bool IsEmpty
    {
      get { return _text == null; }
    }

    public string Text
    {
      get { return _text; }
    }
  }
}
