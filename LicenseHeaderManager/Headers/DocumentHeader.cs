#region copyright
// Copyright (c) 2011 rubicon IT GmbH

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
#endregion
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
    private readonly FileInfo _fileInfo;
    private readonly IEnumerable<DocumentHeaderProperty> _properties;

    public DocumentHeader(TextDocument document, string[] rawLines, IEnumerable<DocumentHeaderProperty> properties)
    {
      if (document == null) throw new ArgumentNullException("document");
      if (properties == null) throw new ArgumentNullException("properties");

      _document = document;
      _properties = properties;

      _fileInfo = CreateFileInfo();
      _text = CreateText(rawLines);
    }

    private FileInfo CreateFileInfo()
    {
      string pathToDocument = _document.Parent.FullName;

      if (File.Exists(pathToDocument))
      {
        return new FileInfo(pathToDocument);
      }
      return null;
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

      foreach (DocumentHeaderProperty property in _properties)
      {
        if (property.CanCreateValue(this))
        {
          finalText = finalText.Replace(property.Token, property.CreateValue(this));
        }
      }

      return finalText;
    }

    public bool IsEmpty
    {
      get { return _text == null; }
    }

    public FileInfo FileInfo
    {
      get { return _fileInfo; }
    }

    public string Text
    {
      get { return _text; }
    }
  }
}
