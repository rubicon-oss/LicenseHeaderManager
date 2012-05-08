using System;
using System.Linq;
using LicenseHeaderManager.Utils;
using System.Diagnostics;

namespace LicenseHeaderManager.Headers
{
  /// <summary>
  /// Detects the most common line ending (CR, LF, CR+LF) in a string and provides functionality to replace line endings.
  /// </summary>
  public class NewLineManager
  {
    private const string CR = "\r";
    private const string LF = "\n";
    private const string CRLF = "\r\n";
    private static readonly string[] _allLineEndings = new[] { CR, LF, CRLF };

    /// <summary>
    /// Replaces all LineEnds (CR,LF,CR+LF) in a string with the given newLineEnd
    /// </summary>
    /// <param name="inputText">The input text which is parsed</param>
    /// <param name="newLineEnd">The new line end</param>
    /// <returns></returns>
    public static string ReplaceAllLineEnds(string inputText, string newLineEnd)
    {
      if (inputText == null)
        throw new ArgumentNullException ("inputText");
      if (newLineEnd == null)
        throw new ArgumentNullException ("newLineEnd");

      return inputText.Replace (CRLF, LF).Replace (CR, LF).Replace (LF, newLineEnd);
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
                   new LineEndInformation(inputText.IndexOf (CR, startIndex, count), CR),
                   new LineEndInformation(inputText.IndexOf (LF, startIndex, count), LF),
                   new LineEndInformation(inputText.IndexOf (CRLF, startIndex, count), CRLF),
                 };

      var nearestLineEnd = ends.Where (lineEnd => lineEnd.Index != -1).OrderBy (x => x.Index).OrderByDescending(x => x.LineEndLenght);
      return nearestLineEnd.FirstOrDefault ();
    }

    /// <summary>
    /// Detects the most frequent LineEnd (CR,LF,CR+LF) in a string
    /// </summary>
    /// <param name="inputText">The input test which is parsed</param>
    /// <returns>The most frequent line end (CR,LF,CR+LF)</returns>
    public static string DetectMostFrequentLineEnd(string inputText)
    {
      if (inputText == null)
        throw new ArgumentNullException ("inputText");

      var lineEndStatistics = _allLineEndings.Select (
          le =>
          new
          {
            LineEnding = le,
            LineEndingLength = le.Length,
            Count = le == CRLF ?
              inputText.CountOccurrence (le) :
              inputText.Replace (CRLF, "").CountOccurrence (le)
          });

      var mostFrequentLineEnding = lineEndStatistics.OrderByDescending (x => x.Count).ThenByDescending (x => x.LineEndingLength).First ();
      return mostFrequentLineEnding.LineEnding;
    }
  }
}
