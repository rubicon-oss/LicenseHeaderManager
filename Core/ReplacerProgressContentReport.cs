﻿/* Copyright (c) rubicon IT GmbH
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
  ///   Encapsulates information to be passed as argument to a <see cref="IProgress{T}" /> callback, specifically for when
  ///   the <see cref="LicenseHeaderReplacer" /> was invoked with file contents.
  /// </summary>
  public class ReplacerProgressContentReport : ReplacerProgressReport
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="ReplacerProgressReport" /> class.
    /// </summary>
    /// <param name="totalFileCount">The overall number of files that are to be updated.</param>
    /// <param name="processedFileCount">The number of file that have already been processed.</param>
    /// <param name="processedFilePath">The path of the file that has just been processed.</param>
    /// <param name="processFileNewContent">The new content of the file that has just been processed.</param>
    public ReplacerProgressContentReport (int totalFileCount, int processedFileCount, string processedFilePath, string processFileNewContent)
        : base (totalFileCount, processedFileCount, processedFilePath)
    {
      ProcessFileNewContent = processFileNewContent;
    }

    /// <summary>
    ///   Gets the new content of the file that has just been processed.
    /// </summary>
    public string ProcessFileNewContent { get; }
  }
}
