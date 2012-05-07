using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LicenseHeaderManager.Utils;

namespace LicenseHeaderManager.Headers
{
  public class NewLineManager
  {
    public static string ReplaceAllLineEnds(string inputText, string newLineEnd)
    {
      return inputText.Replace ("\r\n", "\n").Replace ("\r", "\n").Replace ("\n", newLineEnd);
    }

    public static string DetectLineEnd(string inputText)
    {
      var onlyNewLine = inputText.CountOccurrence ("\n");
      var onlyCarriageReturn = inputText.CountOccurrence ("\r");
      if (onlyNewLine == onlyCarriageReturn)
        return "\r\n";

      if (onlyNewLine > onlyCarriageReturn)
        return "\n";
      if (onlyCarriageReturn > onlyNewLine)
        return "\r";

      return Environment.NewLine;
    }
  }
}
