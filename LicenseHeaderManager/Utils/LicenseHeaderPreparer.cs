using LicenseHeaderManager.Headers;
using LicenseHeaderManager.Interfaces;

namespace LicenseHeaderManager.Utils
{
  public static class LicenseHeaderPreparer
  {
    public static string Prepare (string headerText, string currentHeaderText, ICommentParser commentParser)
    {
      var lineEndingInDocument = NewLineManager.DetectMostFrequentLineEnd (headerText);


      var headerWithNewLine = headerText;
      var newLine = NewLineManager.DetectMostFrequentLineEnd (headerWithNewLine);

      headerWithNewLine = NewLineManager.ReplaceAllLineEnds (headerWithNewLine, lineEndingInDocument);
      
      //if there's a comment right at the beginning of the file,
      //we need to add an empty line so that the comment doesn't
      //become a part of the header
      if (!string.IsNullOrEmpty (commentParser.Parse (currentHeaderText)) && !headerWithNewLine.EndsWith(lineEndingInDocument + lineEndingInDocument))
        headerWithNewLine += newLine;
      

      return headerWithNewLine;
    }
  }
}
