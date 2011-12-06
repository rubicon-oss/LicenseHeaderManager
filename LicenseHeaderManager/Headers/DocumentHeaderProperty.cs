using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LicenseHeaderManager.Headers
{
  internal class DocumentHeaderProperty
  {
    private readonly string _token;
    private readonly Func<FileInfo, string> _valueCreator;

    public DocumentHeaderProperty(string token, Func<FileInfo, string> valueCreator)
    {
      _token = token;
      _valueCreator = valueCreator;
    }

    public string Token { get { return _token; } }

    public string GetValue(FileInfo fileInfo)
    {
      return _valueCreator(fileInfo);
    }
  }
}
