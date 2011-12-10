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
    private readonly Predicate<DocumentHeader> _canCreateValue;
    private readonly Func<DocumentHeader, string> _createValue;

    public DocumentHeaderProperty(string token, Predicate<DocumentHeader> canCreateValue, Func<DocumentHeader, string> createValue)
    {
      _token = token;
      _canCreateValue = canCreateValue;
      _createValue = createValue;
    }

    public string Token { get { return _token; } }

    public bool CanCreateValue(DocumentHeader documentHeader)
    {
      return _canCreateValue(documentHeader);
    }

    public string CreateValue(DocumentHeader documentHeader)
    {
      return _createValue(documentHeader);
    }
  }
}
