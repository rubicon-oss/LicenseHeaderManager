using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LicenseHeaderManager.Utils;

namespace LicenseHeaderManager.Headers
{
  public class NewLineManager
  {
    private const string CR = "\r";
    private const string LF = "\n";
    private const string CRLF = "\r\n";

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
    /// Detects the most frequent LineEnd (CR,LF,CR+LF) in a string
    /// </summary>
    /// <param name="inputText">The input test which is parsed</param>
    /// <returns>The most frequent line end (CR,LF,CR+LF)</returns>
    public static string DetectLineEnd(string inputText)
    {
      if(inputText == null)
        throw new ArgumentNullException("inputText");

      var onlyNewLine = inputText.CountOccurrence (LF);
      var onlyCarriageReturn = inputText.CountOccurrence (CR);

      if (onlyNewLine == onlyCarriageReturn) //\n and \r occur equals
        return CRLF;

      var fullNewLine = inputText.CountOccurrence (CRLF);

      if (fullNewLine > onlyNewLine && fullNewLine > onlyCarriageReturn) //CRCL is occuring more frequently
        return CRLF;

      if (onlyNewLine > onlyCarriageReturn)
        return LF;

      if (onlyCarriageReturn > onlyNewLine)
        return CR;

      return Environment.NewLine;
    }
  }
}
