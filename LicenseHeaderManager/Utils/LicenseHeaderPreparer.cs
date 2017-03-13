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

using LicenseHeaderManager.Headers;
using LicenseHeaderManager.Interfaces;

namespace LicenseHeaderManager.Utils
{
  public static class LicenseHeaderPreparer
  {
    public static string Prepare (string headerText, string currentHeaderText, ICommentParser commentParser)
    {
      var lineEndingInDocument = NewLineManager.DetectMostFrequentLineEnd (headerText);

      var headerWithNewLine = NewLineManager.ReplaceAllLineEnds (headerText, lineEndingInDocument);

      //If there's a comment right at the beginning of the file, we need to add an empty line so that the comment doesn't
      //become a part of the header. If there already exists an empty line we dont have to add another one
      if (!CurrentFileStartsWithNewLine (currentHeaderText, lineEndingInDocument) &&
          !HeaderEndsWithNewline (headerWithNewLine, lineEndingInDocument) &&
          CurrentFileContainsCommentOnTop (currentHeaderText, commentParser))
      {
        return headerWithNewLine + lineEndingInDocument;
      }
      
      return headerWithNewLine;
    }

    private static bool CurrentFileStartsWithNewLine (string currentHeaderText, string lineEndingInDocument)
    {
      return currentHeaderText.StartsWith (lineEndingInDocument);
    }

    private static bool HeaderEndsWithNewline (string headerWithNewLine, string lineEndingInDocument)
    {
      return headerWithNewLine.EndsWith (lineEndingInDocument + lineEndingInDocument);
    }

    private static bool CurrentFileContainsCommentOnTop (string currentHeaderText, ICommentParser commentParser)
    {
      return !string.IsNullOrEmpty (commentParser.Parse (currentHeaderText));
    }
  }
}
