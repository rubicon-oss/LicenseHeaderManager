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
  ///   Represents information linked to a successful license header update operation when the
  ///   <see cref="LicenseHeaderReplacer" /> was invoked with file contents.
  /// </summary>
  public class ReplacerSuccess
  {
    /// <summary>
    ///   Initializes a new <see cref="ReplacerSuccess" /> instance.
    /// </summary>
    /// <param name="filePath">The path of the file whose license headers have been successfully updated.</param>
    /// <param name="newContent">The new content of the file whose license headers have been successfully updated.</param>
    public ReplacerSuccess (string filePath, string newContent)
    {
      FilePath = filePath;
      NewContent = newContent;
    }

    /// <summary>
    ///   The path of the file whose license headers have been successfully updated.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    ///   The new content of the file whose license headers have been successfully updated.
    /// </summary>
    public string NewContent { get; }
  }
}
