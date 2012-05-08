using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LicenseHeaderManager.Headers
{
  /// <summary>
  /// Class for summarizing the information about the line end, where is it (index) and what character is it (CR, LR, CR+LF).
  /// </summary>
  public class LineEndInformation
  {
    public int Index { get; private set; }
    public string LineEnd { get; private set; }
    public int LineEndLenght { get { return LineEnd.Length; } }

    public LineEndInformation(int index, string lineEnd)
    {
      this.Index = index;
      this.LineEnd = lineEnd;
    }
  }
}
