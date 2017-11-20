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
using System.Linq;
using LicenseHeaderManager.Utils;

namespace LicenseHeaderManager.Headers
{
  /// <summary>
  /// Detects the most common line ending (CR, LF, CR+LF) in a string and provides functionality to replace line endings.
  /// </summary>
  public class NewLineManager
  {
    private static readonly string[] _allLineEndings = new[] { NewLineConst.CR, NewLineConst.LF, NewLineConst.CRLF };

    /// <summary>
    /// Replaces all LineEnds (CR,LF,CR+LF) in a string with the given newLineEnd
    /// </summary>
    /// <param name="inputText">The input text which is parsed</param>
    /// <param name="newLineEnd">The new line end</param>
    /// <returns></returns>
    public static string ReplaceAllLineEnds (string inputText, string newLineEnd)
    {
      if (inputText == null)
        throw new ArgumentNullException ("inputText");
      if (newLineEnd == null)
        throw new ArgumentNullException ("newLineEnd");

      return inputText.Replace (NewLineConst.CRLF, NewLineConst.LF).Replace (NewLineConst.CR, NewLineConst.LF).Replace (NewLineConst.LF, newLineEnd);
    }

    /// <summary>
    /// Returns the position of the next line ending
    /// </summary>
    /// <param name="inputText">The text which is parsed</param>
    /// <param name="startIndex">Offset within the given string</param>
    /// <returns>The position of the next line ending, if none exists -1</returns>
    public static int NextLineEndPosition (string inputText, int startIndex)
    {
      if (inputText == null)
        throw new ArgumentNullException ("inputText");

      return NextLineEndPosition (inputText, startIndex, inputText.Length - startIndex);
    }

    /// <summary>
    /// Returns the position of the next line ending
    /// </summary>
    /// <param name="inputText">The text which is parsed</param>
    /// <param name="startIndex">Offset within the given string</param>
    /// <param name="count">The amount of characters to scan</param>
    /// <returns>The position of the next line ending, if none exists -1</returns>
    public static int NextLineEndPosition (string inputText, int startIndex, int count)
    {
      if (inputText == null)
        throw new ArgumentNullException ("inputText");

      var lineEndInformations = NextLineEndPositionInformation (inputText, startIndex, count);
      if (lineEndInformations == null)
        return -1;
      return lineEndInformations.Index;
    }

    /// <summary>
    /// Return the information about the nearest line ending
    /// </summary>
    /// <param name="inputText"></param>
    /// <returns>Information about the line endings</returns>
    public static LineEndInformation NextLineEndPositionInformation (string inputText)
    {
      if (inputText == null)
        throw new ArgumentNullException ("inputText");

      return NextLineEndPositionInformation (inputText, 0);
    }

    /// <summary>
    /// Return the information about the nearest line ending
    /// </summary>
    /// <param name="inputText">The parsed input text</param>
    /// <param name="startIndex">The offset to begin the search</param>
    /// <returns>Information about the line endings</returns>
    public static LineEndInformation NextLineEndPositionInformation (string inputText, int startIndex)
    {
      if (inputText == null)
        throw new ArgumentNullException ("inputText");

      return NextLineEndPositionInformation (inputText, startIndex, inputText.Length - startIndex);
    }

    /// <summary>
    /// Return the information about the nearest line ending
    /// </summary>
    /// <param name="inputText">The parsed input text</param>
    /// <param name="startIndex">The offset to begin the search</param>
    /// <param name="count">The amount of characters to check</param>
    /// <returns>Information about the line endings</returns>
    public static LineEndInformation NextLineEndPositionInformation (string inputText, int startIndex, int count)
    {
      if (inputText == null)
        throw new ArgumentNullException ("inputText");

      var ends = new[]
                 {
                   new LineEndInformation (inputText.IndexOf (NewLineConst.CR, startIndex, count), NewLineConst.CR),
                   new LineEndInformation (inputText.IndexOf (NewLineConst.LF, startIndex, count), NewLineConst.LF),
                   new LineEndInformation (inputText.IndexOf (NewLineConst.CRLF, startIndex, count), NewLineConst.CRLF),
                 };

      var nearestLineEnd = ends.Where (lineEnd => lineEnd.Index >= 0).OrderBy (x => x.Index).ThenByDescending (x => x.LineEndLenght);
      return nearestLineEnd.FirstOrDefault();
    }

    /// <summary>
    /// Detects the most frequent LineEnd (CR,LF,CR+LF) in a string
    /// </summary>
    /// <param name="inputText">The input test which is parsed</param>
    /// <returns>The most frequent line end (CR,LF,CR+LF)</returns>
    public static string DetectMostFrequentLineEnd (string inputText)
    {
      if (inputText == null)
        throw new ArgumentNullException ("inputText");

      var lineEndStatistics = _allLineEndings.Select (
          le =>
            new
            {
              LineEnding = le,
              LineEndingLength = le.Length,
              Count =
              le == NewLineConst.CRLF
                  ? inputText.CountOccurrence (le)
                  : inputText.Replace (NewLineConst.CRLF, "").CountOccurrence (le) //To avoid that in an \r\n the \r is counted..
            });

      var mostFrequentLineEnding = lineEndStatistics.OrderByDescending (x => x.Count).ThenByDescending (x => x.LineEndingLength).First();
      return mostFrequentLineEnding.LineEnding;
    }
  }
}