/* Copyright (c) rubicon IT GmbH
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
 */

using System;

namespace Core
{
  /// <summary>
  ///   Class for summarizing the information about the line end, where is it (index) and what character is it (CR, LR,
  ///   CR+LF).
  /// </summary>
  internal class LineEndInformation
  {
    /// <summary>
    ///   Initializes a new <see cref="LineEndInformation" /> instance.
    /// </summary>
    /// <param name="index">Index of occurrence.</param>
    /// <param name="lineEnd">Line ending (CR, LR or CR+LF).</param>
    public LineEndInformation (int index, string lineEnd)
    {
      Index = index;
      LineEnd = lineEnd;
    }

    /// <summary>
    ///   Index of occurrence.
    /// </summary>
    public int Index { get; }

    /// <summary>
    ///   Length of the line ending this <see cref="LineEndInformation" /> was initialized with.
    /// </summary>
    public int LineEndLength => LineEnd.Length;

    /// <summary>
    ///   Line ending (CR, LR or CR+LF).
    /// </summary>
    private string LineEnd { get; }
  }
}
