using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;

namespace LicenseHeaderManager.Headers
{
  internal class Document
  {
    private readonly string _header;
    private readonly IEnumerable<string> _keywords;

    private readonly TextDocument _document;
    private readonly Parser _parser;

    public Document (TextDocument document, Options.Language language, string[] header, IEnumerable<string> keywords = null)
    {
      _document = document;
      if (header == null)
        _header = null;
      else
        _header = string.Join(Environment.NewLine, header);
      _keywords = keywords;

      _parser = new Parser (language.LineComment, language.BeginComment, language.EndComment, language.BeginRegion, language.EndRegion);
    }

    private string GetText ()
    {
      var start = _document.CreateEditPoint (_document.StartPoint);
      return start.GetText (_document.EndPoint);
    }

    private string GetExistingHeader ()
    {
      string header = _parser.Parse (GetText ());

      if (_keywords == null || _keywords.Any (k => header.ToLower ().Contains (k.ToLower ())))
        return header;
      else
        return string.Empty;
    }

    public void ReplaceHeaderIfNecessary ()
    {
      var xml = GetXmlDeclaration ();
      if (!string.IsNullOrEmpty(xml))
        RemoveHeader (xml);

      string existingHeader = GetExistingHeader ();

      if (_header != null)
      {
        if (existingHeader != _header)
          ReplaceHeader (existingHeader, _header);
      }
      else
        RemoveHeader (existingHeader);

      if (!string.IsNullOrEmpty(xml))
        AddHeader (xml, false);
    }

    private string GetXmlDeclaration ()
    {
      //If it's an xml document, the xml declaration must be the first thing in the file. 
      //Luckily we can use the same logic we use for finding comments to look for this
      //special tag and temporarily remove it so that the rest works as usual.

      return new Parser (null, "<?", "?>", null, null).Parse (GetText ());

      //Don't forget to add the tag again after modifying the file though!
    }

    private void ReplaceHeader (string existingHeader, string newHeader)
    {
      RemoveHeader (existingHeader);
      AddHeader(newHeader);
    }

    private void AddHeader (string header, bool appendEmptyLine = true)
    {
      if (!string.IsNullOrEmpty(header))
      {   
        if (appendEmptyLine)
          header += Environment.NewLine + Environment.NewLine;

        var start = _document.CreateEditPoint (_document.StartPoint);
        start.Insert (header);
      }
    }

    private void RemoveHeader (string header)
    {
      if (!string.IsNullOrEmpty (header))
      { 
        int count = -1;
        for (int index = 0; index >= 0; index = header.IndexOf (Environment.NewLine, index + Environment.NewLine.Length))
          count++;  

        var start = _document.CreateEditPoint (_document.StartPoint);
        var end = _document.CreateEditPoint (_document.StartPoint);
        end.CharRight (header.Length - count * (Environment.NewLine.Length - 1)); //otherwise too many characters would be deleted if NewLine's length is greater than 1
        start.Delete (end);
      }
    }
  }
}