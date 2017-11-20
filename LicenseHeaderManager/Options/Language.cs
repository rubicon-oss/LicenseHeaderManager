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
using System.Collections.Generic;
using System.Linq;
using LicenseHeaderManager.Headers;

namespace LicenseHeaderManager.Options
{
  public class Language : ICloneable
  {
    public IEnumerable<string> Extensions { get; set; }
    public string LineComment { get; set; }
    public string BeginComment { get; set; }
    public string EndComment { get; set; }
    public string BeginRegion { get; set; }
    public string EndRegion { get; set; }
    public string SkipExpression { get; set; }

    public bool IsValid
    {
      get
      {
        if (Extensions == null || !Extensions.Any())
          return false;

        if (string.IsNullOrEmpty (BeginRegion) != string.IsNullOrEmpty (EndRegion))
          return false;

        if (string.IsNullOrEmpty (LineComment))
          return (!string.IsNullOrEmpty (BeginComment) &&
              !string.IsNullOrEmpty (EndComment));

        return string.IsNullOrEmpty (BeginComment) == string.IsNullOrEmpty (EndComment);
      }
    }

    public object Clone ()
    {
      return new Language()
             {
               Extensions = Extensions.ToList(),
               LineComment = LineComment,
               BeginComment = BeginComment,
               EndComment = EndComment,
               BeginRegion = BeginRegion,
               EndRegion = EndRegion,
               SkipExpression = SkipExpression
             };
    }

    public void NormalizeExtensions ()
    {
      Extensions = Extensions.Where (e => !string.IsNullOrWhiteSpace (e)).Select (e => LicenseHeader.AddDot (e).ToLower()).ToArray();
    }
  }
}