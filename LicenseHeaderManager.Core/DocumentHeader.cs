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
using System.Text.RegularExpressions;

namespace LicenseHeaderManager.Core
{
  /// <summary>
  ///   Represents the license header of a <see cref="Document" />.
  /// </summary>
  /// <seealso cref="Document" />
  internal class DocumentHeader : IDocumentHeader
  {
    private readonly IEnumerable<DocumentHeaderProperty> _properties;

    public DocumentHeader (string documentFilePath, string text, IEnumerable<DocumentHeaderProperty> properties)
    {
      if (documentFilePath == null) throw new ArgumentNullException (nameof(documentFilePath));
      if (properties == null) throw new ArgumentNullException (nameof(properties));

      _properties = properties;

      FileInfo = CreateFileInfo (documentFilePath);
      Text = ExpandProperties (text);
    }

    public bool IsEmpty => Text == null;

    public FileInfo FileInfo { get; }

    public string Text { get; }

    private FileInfo CreateFileInfo (string pathToDocument)
    {
      return File.Exists (pathToDocument) ? new FileInfo (pathToDocument) : null;
    }

    /// <summary>
    ///   Replaces the name of expandable properties by their respective values, if they can be created.
    /// </summary>
    /// <param name="inputText">The text for which the occurring expandable property tokens should be replaced.</param>
    /// <returns>
    ///   Returns a string that is equivalent to <paramref name="inputText" /> with all expandable properties having
    ///   been replaced for which the <see cref="DocumentHeaderProperty.CanCreateValue" /> method evaluated to
    ///   <see langword="true" />.
    /// </returns>
    private string ExpandProperties (string inputText)
    {
      if (inputText == null)
        return null;

      var finalText = inputText;

      foreach (var property in _properties)
        if (property.CanCreateValue (this))
        {
          var regex = new Regex (property.Token, RegexOptions.IgnoreCase);
          finalText = regex.Replace (finalText, property.CreateValue (this));
        }

      return finalText;
    }
  }
}
