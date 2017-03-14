#region copyright
// Copyright (c) rubicon IT GmbH

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
#endregion

using System;

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
