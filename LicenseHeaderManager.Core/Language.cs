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
using System.Linq;

namespace LicenseHeaderManager.Core
{
  /// <summary>
  ///   Provides an abstraction representing a programming language with information necessary to update license headers.
  /// </summary>
  public class Language
  {
    /// <summary>
    ///   Gets the file extensions (including the leading dot ".") which this <see cref="Language" /> refers to.
    /// </summary>
    public IEnumerable<string> Extensions { get; set; }

    /// <summary>
    ///   Gets or sets the string denoting a comment that applies to a single line.
    /// </summary>
    public string LineComment { get; set; }

    /// <summary>
    ///   Gets or sets the string denoting the start of a comment that spans across multiple lines.
    /// </summary>
    public string BeginComment { get; set; }

    /// <summary>
    ///   Gets or sets the string denoting the end of a comment that spans across multiple lines.
    /// </summary>
    public string EndComment { get; set; }

    /// <summary>
    ///   Gets or sets the string denoting the start of a (collapsible) region, if available in this language, otherwise
    ///   <see langword="null" /> or <see cref="string.Empty" />.
    /// </summary>
    public string BeginRegion { get; set; }

    /// <summary>
    ///   Gets or sets the string denoting the end of a (collapsible) region, if available in this language, otherwise
    ///   <see langword="null" /> or <see cref="string.Empty" />.
    /// </summary>
    public string EndRegion { get; set; }

    /// <summary>
    ///   Gets or sets a regular expression used to determine text that should be skipped when updating license headers.
    /// </summary>
    public string SkipExpression { get; set; }

    /// <summary>
    ///   Determines whether this <see cref="Language" /> instance is semantically consistent (e. g. at least one extension,
    ///   neither or both region parts must be specified and either a line comment or begin and end comment tags).
    /// </summary>
    /// <returns>
    ///   Returns <see langword="true" /> if this <see cref="Language" /> instance is semantically valid, otherwise
    ///   <see langword="false" />.
    /// </returns>
    public bool IsValid ()
    {
      if (Extensions == null || !Extensions.Any())
        return false;

      if (string.IsNullOrEmpty (BeginRegion) != string.IsNullOrEmpty (EndRegion))
        return false;

      if (string.IsNullOrEmpty (LineComment))
        return !string.IsNullOrEmpty (BeginComment) && !string.IsNullOrEmpty (EndComment);

      return string.IsNullOrEmpty (BeginComment) == string.IsNullOrEmpty (EndComment);
    }

    /// <summary>
    ///   Creates a deep copy of this <see cref="Language" /> instance.
    /// </summary>
    /// <returns>
    ///   Returns a new <see cref="Language" /> instance with members equal to those of this <see cref="Language" />
    ///   instance.
    /// </returns>
    public Language Clone ()
    {
      return new Language
             {
                 Extensions = Extensions.ToList(),
                 LineComment = LineComment,
                 BeginComment = BeginComment,
                 EndComment = EndComment,
                 BeginRegion = BeginRegion,
                 EndRegion = EndRegion,
                 SkipExpression = SkipExpression
             };
    }

    /// <summary>
    ///   Brings all members of the <see cref="Extensions" /> property in a uniform format: lowercase and including leading dot
    ///   ".":
    /// </summary>
    public void NormalizeExtensions ()
    {
      Extensions = Extensions.Where (e => !string.IsNullOrWhiteSpace (e)).Select (e => e.InsertDotIfNecessary().ToLower());
    }
  }
}
