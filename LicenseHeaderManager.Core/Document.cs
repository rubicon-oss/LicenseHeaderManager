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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LicenseHeaderManager.Core
{
  /// <summary>
  ///   Represents the abstraction for documents (being represented by physical files or their content) providing members to
  ///   replace their license headers.
  /// </summary>
  internal class Document
  {
    private readonly IEnumerable<AdditionalProperty> _additionalProperties;
    private readonly CommentParser _commentParser;
    private readonly string[] _headerLines;
    private readonly IEnumerable<string> _keywords;
    private readonly Language _language;
    private readonly LicenseHeaderInput _licenseHeaderInput;
    private string _documentTextCache;
    private DocumentHeader _headerCache;
    private string _lineEndingInDocumentCache;

    /// <summary>
    ///   Initializes a new <see cref="Document" /> instance.
    /// </summary>
    /// <param name="licenseHeaderInput">
    ///   The license header input representing the document being encapsulated by this
    ///   <see cref="Document" /> instance.
    /// </param>
    /// <param name="language">
    ///   The <see cref="Language" /> instance to be used when replacing license headers in the document
    ///   represented by <paramref name="licenseHeaderInput" />.
    /// </param>
    /// <param name="headerLines">
    ///   The document's current license header, one line per array index. Null if headers are to be
    ///   removed.
    /// </param>
    /// <param name="additionalProperties">
    ///   Additional properties to be used when inserting the license header that are already
    ///   expanded.
    /// </param>
    /// <param name="keywords">
    ///   If not <see langword="null" />, headers are only removed/replaced if they contain at least one
    ///   keyword of this <see cref="IEnumerable{T}" />.
    /// </param>
    public Document (
        LicenseHeaderInput licenseHeaderInput,
        Language language,
        string[] headerLines,
        IEnumerable<AdditionalProperty> additionalProperties = null,
        IEnumerable<string> keywords = null)
    {
      _licenseHeaderInput = licenseHeaderInput;
      _additionalProperties = additionalProperties;
      _keywords = keywords;
      _language = language;
      _headerLines = headerLines;
      _commentParser = new CommentParser (language.LineComment, language.BeginComment, language.EndComment, language.BeginRegion, language.EndRegion);
    }

    /// <summary>
    ///   Determines whether the header of this <see cref="Document" /> instance is valid, i. e. either empty or equal to the
    ///   result of a <see cref="ICommentParser.Parse" /> invocation.
    /// </summary>
    /// <returns>
    ///   Returns <see langword="true" /> if the header of this <see cref="Document" /> instance is valid, otherwise
    ///   <see langword="false" />.
    /// </returns>
    public async Task<bool> ValidateHeader ()
    {
      return (await GetHeader()).IsEmpty || Validate ((await GetHeader()).Text, _commentParser);
    }

    /// <summary>
    ///   Replaces the license header of the current document if necessary, if this <see cref="Document" /> instance was
    ///   initialized with a <see cref="LicenseHeaderContentInput" /> instance.
    /// </summary>
    /// <param name="cancellationToken">Denotes whether the replacement operation has been cancelled.</param>
    /// <returns>
    ///   The new content of the document represented by this <see cref="Document" /> instance after removing or
    ///   replacing. Can be equal to the content it had before, if no operation was necessary.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///   If this <see cref="Document" /> instance was not initialized with a
    ///   <see cref="LicenseHeaderContentInput" /> instance.
    /// </exception>
    public async Task<string> ReplaceHeaderIfNecessaryContent (CancellationToken cancellationToken)
    {
      if (_licenseHeaderInput.InputMode != LicenseHeaderInputMode.Content)
        throw new InvalidOperationException ($"LicenseHeaderInput Mode must be {nameof(LicenseHeaderInputMode.Content)}");

      await ReplaceHeaderIfNecessary (cancellationToken);
      return _documentTextCache;
    }

    /// <summary>
    ///   Replaces the license header of the current document if necessary, if this <see cref="Document" /> instance was
    ///   initialized with a <see cref="LicenseHeaderPathInput" /> instance.
    /// </summary>
    /// <param name="cancellationToken">Denotes whether the replacement operation has been cancelled.</param>
    /// <exception cref="InvalidOperationException">
    ///   If this <see cref="Document" /> instance was not initialized with a
    ///   <see cref="LicenseHeaderPathInput" /> instance.
    /// </exception>
    public async Task ReplaceHeaderIfNecessaryPath (CancellationToken cancellationToken)
    {
      if (_licenseHeaderInput.InputMode != LicenseHeaderInputMode.FilePath)
        throw new InvalidOperationException ($"LicenseHeaderInput Mode must be {nameof(LicenseHeaderInputMode.FilePath)}");

      await ReplaceHeaderIfNecessary (cancellationToken);
    }

    private async Task ReplaceHeaderIfNecessary (CancellationToken cancellationToken)
    {
      var skippedText = await SkipText();
      if (!string.IsNullOrEmpty (skippedText))
      {
        cancellationToken.ThrowIfCancellationRequested();
        await RemoveHeader (skippedText);
      }

      var existingHeader = await GetExistingHeader();

      if (!(await GetHeader()).IsEmpty)
      {
        if (existingHeader != (await GetHeader()).Text)
        {
          cancellationToken.ThrowIfCancellationRequested();
          await ReplaceHeader (existingHeader, (await GetHeader()).Text);
        }
      }
      else
      {
        cancellationToken.ThrowIfCancellationRequested();
        await RemoveHeader (existingHeader);
      }

      if (!string.IsNullOrEmpty (skippedText))
      {
        cancellationToken.ThrowIfCancellationRequested();
        await AddHeader (skippedText);
      }
    }

    private async Task<IDocumentHeader> GetHeader ()
    {
      if (_headerCache != default)
        return _headerCache;

      var headerText = await CreateHeaderText (_headerLines);
      _headerCache = new DocumentHeader (_licenseHeaderInput.DocumentPath, headerText, new DocumentHeaderProperties (_additionalProperties));
      return _headerCache;
    }

    private async Task<string> CreateHeaderText (string[] headerLines)
    {
      if (headerLines == null)
        return null;

      var inputText = string.Join (await GetLineEndingInDocument(), headerLines);
      inputText += await GetLineEndingInDocument();

      return inputText;
    }

    private async Task<string> GetText ()
    {
      if (string.IsNullOrEmpty (_documentTextCache))
        await RefreshText();

      return _documentTextCache;
    }

    private async Task<string> GetLineEndingInDocument ()
    {
      _lineEndingInDocumentCache ??= NewLineManager.DetectMostFrequentLineEnd (await GetText());
      return _lineEndingInDocumentCache;
    }

    private async Task RefreshText ()
    {
      if (_licenseHeaderInput.InputMode == LicenseHeaderInputMode.Content && _licenseHeaderInput is LicenseHeaderContentInput contentInput)
      {
        _documentTextCache = contentInput.DocumentContent;
        return;
      }

      using var reader = new StreamReader (_licenseHeaderInput.DocumentPath, Encoding.UTF8);
      _documentTextCache = await reader.ReadToEndAsync();
    }

    private async Task<string> GetExistingHeader ()
    {
      var header = _commentParser.Parse (await GetText());

      if (_keywords == null || _keywords.Any (k => header.ToLower().Contains (k.ToLower())))
        return header;
      return string.Empty;
    }

    private async Task<string> SkipText ()
    {
      if (string.IsNullOrEmpty (_language.SkipExpression))
        return null;
      var match = Regex.Match (await GetText(), _language.SkipExpression, RegexOptions.IgnoreCase);
      if (match.Success && match.Index == 0)
        return match.Value;
      return null;
    }

    private async Task ReplaceHeader (string existingHeader, string newHeader)
    {
      await RemoveHeader (existingHeader);
      await AddHeader (LicenseHeaderPreparer.Prepare (newHeader, await GetText(), _commentParser));
    }

    private async Task AddHeader (string header)
    {
      if (!string.IsNullOrEmpty (header))
      {
        var sb = new StringBuilder();
        var newContent = sb.Append (header).Append (await GetText()).ToString();
        await WriteContentAsync (newContent);
      }
    }

    private async Task RemoveHeader (string header)
    {
      if (!string.IsNullOrEmpty (header))
      {
        var newContent = (await GetText()).Substring (header.Length);
        await WriteContentAsync (newContent);
      }
    }

    private async Task WriteContentAsync (string content)
    {
      if (_licenseHeaderInput.InputMode == LicenseHeaderInputMode.FilePath)
      {
        using (var writer = new StreamWriter (_licenseHeaderInput.DocumentPath, false, Encoding.UTF8))
        {
          await writer.WriteAsync (content);
        }

        await RefreshText();
      }
      else
      {
        _documentTextCache = content;
      }
    }

    private static bool Validate (string header, ICommentParser commentParser)
    {
      try
      {
        return commentParser.Parse (header) == header;
      }
      catch (ParseException)
      {
        return false;
      }
    }
  }
}
