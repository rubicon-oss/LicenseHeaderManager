using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using Language = LicenseHeaderManager.Options.Language;

namespace LicenseHeaderManager.Headers
{
  internal class Document
  {
    private readonly string _header;
    private readonly IEnumerable<string> _keywords;

    private readonly TextDocument _document;
    private readonly Parser _parser;

    public Document (TextDocument document, Language language, string[] header, IEnumerable<string> keywords = null)
    {
      _document = document;
      if (header == null)
        _header = null;
      else
        _header = string.Join (Environment.NewLine, header);
      _keywords = keywords;

      _parser = new Parser (language.LineComment, language.BeginComment, language.EndComment, language.BeginRegion, language.EndRegion);
    }

    public bool ValidateHeader ()
    {
      if (_header == null)
        return true;
      else
        return LicenseHeader.Validate (_header, _parser);
    }

    private string GetText (TextPoint start, TextPoint end)
    {
      return _document.CreateEditPoint(start).GetText (end);
    }

    private string GetText()
    {
      return GetText (_document.StartPoint, _document.EndPoint);
    }

    private string GetExistingHeader ()
    {
      string header = _parser.Parse (GetText());

      if (_keywords == null || _keywords.Any (k => header.ToLower().Contains (k.ToLower())))
        return header;
      else
        return string.Empty;
    }

    public void ReplaceHeaderIfNecessary ()
    {
      var xml = GetXmlDeclaration();
      if (!string.IsNullOrEmpty (xml))
        RemoveHeader (xml);

      string existingHeader = GetExistingHeader();

      if (_header != null)
      {
        if (existingHeader != _header)
          ReplaceHeader (existingHeader, _header);
      }
      else
        RemoveHeader (existingHeader);

      if (!string.IsNullOrEmpty (xml))
        AddHeader (xml, false);
    }

    private string GetXmlDeclaration ()
    {
      //If it's an xml document, the xml declaration must be the first thing in the file. 
      //Luckily we can use the same logic we use for finding comments to look for this
      //special tag and temporarily remove it so that the rest works as usual.

      return new Parser (null, "<?", "?>", null, null).Parse (GetText());

      //Don't forget to add the tag again after modifying the file though!
    }

    private void ReplaceHeader (string existingHeader, string newHeader)
    {
      RemoveHeader (existingHeader);
      AddHeader (newHeader);
    }

    private void AddHeader (string header, bool appendLineBreak = true, bool withEmptyLineIfNecessary = true)
    {
      if (!string.IsNullOrEmpty (header))
      {
        if (appendLineBreak)
        {
          header += Environment.NewLine;

          if (withEmptyLineIfNecessary)
          {
            //if there's a comment right at the beginning of the file,
            //we need to add an empty line so that the comment doesn't
            //become a part of the header
            if (!string.IsNullOrEmpty (_parser.Parse (GetText())))
              header += Environment.NewLine;
          }
        }

        var start = _document.CreateEditPoint (_document.StartPoint);
        start.Insert (header);
      }
    }

    private EditPoint EndOfHeader(string header, TextPoint start = null)
    {
      if (start == null)
        start = _document.CreateEditPoint (_document.StartPoint);
      var end = _document.CreateEditPoint (start);
      end.CharRight (header.Length);

      //CharRight always treats NewLines as single characters, so if that's not true in the current environment we need to take care of it
      if (Environment.NewLine.Length > 1)
      {
        //count the NewLines in the header
        int count = -1;
        for (int index = 0; index >= 0; index = header.IndexOf (Environment.NewLine, index + Environment.NewLine.Length))
          count++;

        end.CharLeft (count * (Environment.NewLine.Length - 1));
      }

      return end;
    }

    private void RemoveHeader (string header)
    {
      if (!string.IsNullOrEmpty (header))
      {
        var start = _document.CreateEditPoint (_document.StartPoint);
        var end = EndOfHeader (header, _document.StartPoint);
        
        start.Delete (end);
      }
    }
  }
}