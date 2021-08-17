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
using System.Collections.Generic;
using System.IO;

namespace LicenseHeaderManager.Core
{
  /// <summary>
  ///   Encapsulates information that are required in any case by the <see cref="LicenseHeaderReplacer" /> when updating
  ///   license headers.
  /// </summary>
  /// <seealso cref="LicenseHeaderReplacer" />
  public abstract class LicenseHeaderInput
  {
    /// <summary>
    ///   Initializes a new <see cref="LicenseHeaderContentInput" /> instance.
    /// </summary>
    /// <param name="documentPath">The path of the file whose headers are to be modified.</param>
    /// <param name="headers">
    ///   The header definitions of the file whose headers are to be modified. Keys are language
    ///   extensions, values represent the definition for the respective language - one line per array index. The dictionary
    ///   should be null if the headers are to be removed.
    /// </param>
    /// <param name="additionalProperties">
    ///   Additional properties that cannot be expanded by the Core whose tokens should be
    ///   replaced by their values.
    /// </param>
    protected LicenseHeaderInput (IDictionary<string, string[]> headers, IEnumerable<AdditionalProperty> additionalProperties, string documentPath)
    {
      Headers = headers;
      AdditionalProperties = additionalProperties;
      IgnoreNonCommentText = false;
      DocumentPath = documentPath;
      Extension = Path.GetExtension (DocumentPath);
    }

    /// <summary>
    ///   The header definitions of the file whose headers are to be modified. Keys are language extensions, values represent
    ///   the definition for the respective language - one line per array index. Values should be null if headers should only
    ///   be removed.
    /// </summary>
    public IDictionary<string, string[]> Headers { get; }

    /// <summary>
    ///   Additional properties that cannot be expanded by the Core whose tokens should be replaced by their values.
    /// </summary>
    public IEnumerable<AdditionalProperty> AdditionalProperties { get; }

    /// <summary>
    ///   Specifies whether license headers should be inserted into a file even if the license header definition contains
    ///   non-comment text.
    /// </summary>
    public bool IgnoreNonCommentText { get; set; }

    /// <summary>
    ///   The path of the file whose headers are to be modified.
    /// </summary>
    public string DocumentPath { get; }

    /// <summary>
    ///   The extensions of the file whose headers are to be modified. Is implicitly and uniquely determined by the
    ///   <see cref="DocumentPath" /> property.
    /// </summary>
    public string Extension { get; }

    /// <summary>
    ///   Gets the input mode this <see cref="LicenseHeaderInput" /> instance is to be used for.
    /// </summary>
    internal abstract LicenseHeaderInputMode InputMode { get; }
  }
}
