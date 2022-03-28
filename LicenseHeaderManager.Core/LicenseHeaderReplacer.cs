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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LicenseHeaderManager.Core.Properties;

namespace LicenseHeaderManager.Core
{
  /// <summary>
  ///   Updates (i. e. replaces, adds, removes) license headers from files.
  /// </summary>
  public partial class LicenseHeaderReplacer : ILicenseHeaderReplacer
  {
    private readonly IEnumerable<string> _keywords;
    private readonly IEnumerable<Language> _languages;
    private readonly SemaphoreSlim _progressReportSemaphore;
    private readonly SemaphoreSlim _taskStartSemaphore;

    private int _processedFileCount;
    private int _totalFileCount;

    /// <summary>
    ///   Initializes a new <see cref="LicenseHeaderReplacer" /> instance.
    /// </summary>
    /// <param name="languages">
    ///   A range of <see cref="Language" /> objects representing the configured languages the
    ///   <see cref="LicenseHeaderReplacer" /> should act upon.
    /// </param>
    /// <param name="keywords">
    ///   Keywords that must be present for the <see cref="LicenseHeaderReplacer" /> to remove a license
    ///   header or <see langword="null" /> if headers should always be removed.
    /// </param>
    /// <param name="maxParallelism">The maximum number of documents being processed simultaneously.</param>
    public LicenseHeaderReplacer (IEnumerable<Language> languages, IEnumerable<string> keywords, int maxParallelism = 15)
    {
      _languages = languages;
      _keywords = keywords;

      _progressReportSemaphore = new SemaphoreSlim (1, 1);
      _taskStartSemaphore = new SemaphoreSlim (maxParallelism, maxParallelism);
    }

    protected LicenseHeaderReplacer ()
    {
    }

    public Language GetLanguageFromExtension (string extension)
    {
      if (extension == null)
        return null;

      return _languages?.FirstOrDefault (x => x.Extensions.Any (y => extension.EndsWith (y, StringComparison.OrdinalIgnoreCase)));
    }

    public bool IsValidPathInput (string path)
    {
      return File.Exists (path) && TryCreateDocument (new LicenseHeaderPathInput (path, null), out _) == CreateDocumentResult.DocumentCreated;
    }

    public async Task<ReplacerResult<ReplacerError<LicenseHeaderPathInput>>> RemoveOrReplaceHeader (LicenseHeaderPathInput licenseHeaderInput)
    {
      return await RemoveOrReplaceHeader (
          licenseHeaderInput,
          async (input, document) =>
          {
            await document.ReplaceHeaderIfNecessaryPath (new CancellationToken());
            return new ReplacerResult<ReplacerError<LicenseHeaderPathInput>>();
          },
          (input, errorType, message) =>
              new ReplacerResult<ReplacerSuccess, ReplacerError<LicenseHeaderPathInput>> (new ReplacerError<LicenseHeaderPathInput> (input, errorType, message)));
    }

    public async Task<IEnumerable<ReplacerResult<ReplacerSuccess, ReplacerError<LicenseHeaderContentInput>>>> RemoveOrReplaceHeader (
        ICollection<LicenseHeaderContentInput> licenseHeaderInputs,
        IProgress<ReplacerProgressContentReport> progress,
        CancellationToken cancellationToken)
    {
      var results = new List<ReplacerResult<ReplacerSuccess, ReplacerError<LicenseHeaderContentInput>>>();
      ResetProgress (licenseHeaderInputs.Count);

      var tasks = new List<Task<ReplacerResult<ReplacerSuccess, ReplacerError<LicenseHeaderContentInput>>>>();
      foreach (var licenseHeaderInput in licenseHeaderInputs)
      {
        await _taskStartSemaphore.WaitAsync (cancellationToken);
        tasks.Add (
            Task.Run (
                () =>
                {
                  try
                  {
                    return RemoveOrReplaceHeaderForOneFile (licenseHeaderInput, progress, cancellationToken);
                  }
                  finally
                  {
                    _taskStartSemaphore.Release();
                  }
                },
                cancellationToken));
      }

      await Task.WhenAll (tasks);

      foreach (var replacerResultTask in tasks)
      {
        var replacerResult = await replacerResultTask;
        if (replacerResult == null)
          continue;

        results.Add (replacerResult);
      }

      return results;
    }

    public async Task<ReplacerResult<IEnumerable<ReplacerError<LicenseHeaderPathInput>>>> RemoveOrReplaceHeader (
        ICollection<LicenseHeaderPathInput> licenseHeaderInputs,
        IProgress<ReplacerProgressReport> progress,
        CancellationToken cancellationToken)
    {
      var errorList = new ConcurrentQueue<ReplacerError<LicenseHeaderPathInput>>();
      ResetProgress (licenseHeaderInputs.Count);

      var tasks = new List<Task>();
      foreach (var licenseHeaderInput in licenseHeaderInputs)
      {
        await _taskStartSemaphore.WaitAsync (cancellationToken);
        tasks.Add (
            Task.Run (
                () =>
                {
                  try
                  {
                    return RemoveOrReplaceHeaderForOneFile (licenseHeaderInput, progress, cancellationToken, errorList);
                  }
                  finally
                  {
                    _taskStartSemaphore.Release();
                  }
                },
                cancellationToken));
      }

      await Task.WhenAll (tasks);

      return errorList.Count == 0
          ? new ReplacerResult<IEnumerable<ReplacerError<LicenseHeaderPathInput>>>()
          : new ReplacerResult<IEnumerable<ReplacerError<LicenseHeaderPathInput>>> (errorList);
    }

    /// <summary>
    ///   Updates license headers in a file based on its content, i. e. the new content is returned. Setting the new content is
    ///   the caller's responsibility.
    /// </summary>
    /// <param name="licenseHeaderInput">
    ///   An <see cref="LicenseHeaderContentInput" /> instance representing the file whose
    ///   license headers are to be updated.
    /// </param>
    /// <returns>
    ///   Returns a <see cref="ReplacerResult{TSuccess,TError}" /> instance containing information about the success or
    ///   error of the operation.
    /// </returns>
    public async Task<ReplacerResult<ReplacerSuccess, ReplacerError<LicenseHeaderContentInput>>> RemoveOrReplaceHeader (LicenseHeaderContentInput licenseHeaderInput)
    {
      return await RemoveOrReplaceHeader (
          licenseHeaderInput,
          async (input, document) =>
          {
            var newContent = await document.ReplaceHeaderIfNecessaryContent (new CancellationToken());
            return new ReplacerResult<ReplacerSuccess, ReplacerError<LicenseHeaderContentInput>> (new ReplacerSuccess (licenseHeaderInput.DocumentPath, newContent));
          },
          (input, errorType, message) =>
              new ReplacerResult<ReplacerSuccess, ReplacerError<LicenseHeaderContentInput>> (new ReplacerError<LicenseHeaderContentInput> (input, errorType, message)));
    }

    /// <summary>
    ///   Updates license headers in a file.
    /// </summary>
    /// <typeparam name="TReturn">The return type of this function.</typeparam>
    /// <typeparam name="TInput">The <see cref="LicenseHeaderInput" /> subtype used as input within this method.</typeparam>
    /// <param name="licenseHeaderInput">The file whose license headers should be updated.</param>
    /// <param name="successSupplier">
    ///   A function that generates the return value in the case of an success, given the
    ///   respective <see cref="LicenseHeaderInput" /> and the <see cref="Document" /> that was created in the process.
    /// </param>
    /// <param name="errorSupplier">
    ///   A function that generates the return value in the case of an error, given the respective
    ///   <see cref="LicenseHeaderInput" />, determined <see cref="ReplacerErrorType" /> and error description.
    /// </param>
    /// <returns>
    ///   Returns either object representing a success, supplied by <paramref name="successSupplier" />, or an object
    ///   representing an error, supplied by <paramref name="errorSupplier" />.
    /// </returns>
    private async Task<TReturn> RemoveOrReplaceHeader<TReturn, TInput> (
        TInput licenseHeaderInput,
        Func<TInput, Document, Task<TReturn>> successSupplier,
        Func<TInput, ReplacerErrorType, string, TReturn> errorSupplier)
        where TInput : LicenseHeaderInput
        where TReturn : ReplacerResult
    {
      try
      {
        var result = TryCreateDocument (licenseHeaderInput, out var document);

        string message;
        switch (result)
        {
          case CreateDocumentResult.DocumentCreated:
            if (!await document.ValidateHeader() && !licenseHeaderInput.IgnoreNonCommentText)
            {
              message = string.Format (Resources.Warning_InvalidLicenseHeader, Path.GetExtension (licenseHeaderInput.DocumentPath)).ReplaceNewLines();
              return errorSupplier (licenseHeaderInput, ReplacerErrorType.NonCommentText, message);
            }

            try
            {
              return await successSupplier (licenseHeaderInput, document);
            }
            catch (ParseException)
            {
              message = string.Format (Resources.Error_InvalidLicenseHeader, licenseHeaderInput.DocumentPath).ReplaceNewLines();
              return errorSupplier (licenseHeaderInput, ReplacerErrorType.ParsingError, message);
            }

          case CreateDocumentResult.LanguageNotFound:
            message = string.Format (Resources.Error_LanguageNotFound, Path.GetExtension (licenseHeaderInput.DocumentPath)).ReplaceNewLines();
            return errorSupplier (licenseHeaderInput, ReplacerErrorType.LanguageNotFound, message);

          case CreateDocumentResult.EmptyHeader:
            message = string.Format (Resources.Error_HeaderNullOrEmpty, licenseHeaderInput.Extension).ReplaceNewLines();
            return errorSupplier (licenseHeaderInput, ReplacerErrorType.EmptyHeader, message);

          case CreateDocumentResult.NoHeaderFound:
            message = string.Format (Resources.Error_NoHeaderFound, Path.GetExtension (licenseHeaderInput.DocumentPath)).ReplaceNewLines();
            return errorSupplier (licenseHeaderInput, ReplacerErrorType.NoHeaderFound, message);

          case CreateDocumentResult.LicenseHeaderDocument:
            message = string.Format (Resources.Error_LicenseHeaderDefinition_InvalidTarget, Path.GetExtension (licenseHeaderInput.DocumentPath)).ReplaceNewLines();
            return errorSupplier (licenseHeaderInput, ReplacerErrorType.LicenseHeaderDocument, message);

          case CreateDocumentResult.FileNotFound:
            message = string.Format (Resources.Error_FileNotFound, licenseHeaderInput.DocumentPath).ReplaceNewLines();
            return errorSupplier (licenseHeaderInput, ReplacerErrorType.FileNotFound, message);
          default:
            throw new ArgumentOutOfRangeException();
        }
      }
      catch (ArgumentException ex)
      {
        var message = $"{ex.Message} {licenseHeaderInput.DocumentPath}";
        return errorSupplier (licenseHeaderInput, ReplacerErrorType.Miscellaneous, message);
      }
    }

    private async Task<ReplacerResult<ReplacerSuccess, ReplacerError<LicenseHeaderContentInput>>> RemoveOrReplaceHeaderForOneFile (
        LicenseHeaderContentInput licenseHeaderInput,
        IProgress<ReplacerProgressContentReport> progress,
        CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();
      if (TryCreateDocument (licenseHeaderInput, out var document) != CreateDocumentResult.DocumentCreated)
      {
        await ReportProgress (progress, cancellationToken, licenseHeaderInput.DocumentPath, licenseHeaderInput.DocumentContent);
        return null;
      }

      string message;
      if (!await document.ValidateHeader() && !licenseHeaderInput.IgnoreNonCommentText)
      {
        cancellationToken.ThrowIfCancellationRequested();
        await ReportProgress (progress, cancellationToken, licenseHeaderInput.DocumentPath, licenseHeaderInput.DocumentContent);

        var extension = Path.GetExtension (licenseHeaderInput.DocumentPath);
        message = string.Format (Resources.Warning_InvalidLicenseHeader, extension).ReplaceNewLines();
        return new ReplacerResult<ReplacerSuccess, ReplacerError<LicenseHeaderContentInput>> (
            new ReplacerError<LicenseHeaderContentInput> (licenseHeaderInput, ReplacerErrorType.NonCommentText, message));
      }

      try
      {
        cancellationToken.ThrowIfCancellationRequested();
        var newContent = await document.ReplaceHeaderIfNecessaryContent (cancellationToken);
        await ReportProgress (progress, cancellationToken, licenseHeaderInput.DocumentPath, newContent);
        return new ReplacerResult<ReplacerSuccess, ReplacerError<LicenseHeaderContentInput>> (new ReplacerSuccess (licenseHeaderInput.DocumentPath, newContent));
      }
      catch (ParseException)
      {
        message = string.Format (Resources.Error_InvalidLicenseHeader, licenseHeaderInput.DocumentPath).ReplaceNewLines();
        return new ReplacerResult<ReplacerSuccess, ReplacerError<LicenseHeaderContentInput>> (
            new ReplacerError<LicenseHeaderContentInput> (licenseHeaderInput, ReplacerErrorType.ParsingError, message));
      }
    }

    /// <summary>
    ///   Tries to open a given project item as a Document which can be used to add or remove headers.
    /// </summary>
    /// <param name="licenseHeaderInput">A <see cref="LicenseHeaderInput" /> instance representing the document to be opened.</param>
    /// <param name="document">
    ///   In case of a success, i. e. return value of <see cref="CreateDocumentResult.DocumentCreated" />,
    ///   this parameter represents the <see cref="Document" /> instance that was created in the process.Otherwise
    ///   <see langword="null" />.
    /// </param>
    /// <returns>
    ///   Returns a <see cref="CreateDocumentResult" /> member describing the success status of the document opening
    ///   attempt.
    /// </returns>
    internal virtual CreateDocumentResult TryCreateDocument (LicenseHeaderInput licenseHeaderInput, out Document document)
    {
      document = null;

      if (licenseHeaderInput is LicenseHeaderPathInput pathInput && !File.Exists (pathInput.DocumentPath))
        return CreateDocumentResult.FileNotFound;

      if (licenseHeaderInput.Extension == LicenseHeaderExtractor.HeaderDefinitionExtension)
        return CreateDocumentResult.LicenseHeaderDocument;

      var language = GetLanguageFromExtension (licenseHeaderInput.Extension);
      if (language == null)
        return CreateDocumentResult.LanguageNotFound;

      string[] header = null;
      if (licenseHeaderInput.Headers != null)
      {
        var extension = licenseHeaderInput.Headers.Keys
            .OrderByDescending (x => x.Length)
            .FirstOrDefault (x => licenseHeaderInput.Extension.EndsWith (x, StringComparison.OrdinalIgnoreCase));

        if (extension == null)
          return CreateDocumentResult.NoHeaderFound;

        header = licenseHeaderInput.Headers[extension];

        if (header.All (string.IsNullOrEmpty))
          return CreateDocumentResult.EmptyHeader;
      }

      document = new Document (licenseHeaderInput, language, header, licenseHeaderInput.AdditionalProperties, _keywords);

      return CreateDocumentResult.DocumentCreated;
    }

    private async Task RemoveOrReplaceHeaderForOneFile (
        LicenseHeaderPathInput licenseHeaderInput,
        IProgress<ReplacerProgressReport> progress,
        CancellationToken cancellationToken,
        ConcurrentQueue<ReplacerError<LicenseHeaderPathInput>> errors)
    {
      cancellationToken.ThrowIfCancellationRequested();
      if (TryCreateDocument (licenseHeaderInput, out var document) != CreateDocumentResult.DocumentCreated)
      {
        await ReportProgress (progress, licenseHeaderInput.DocumentPath, cancellationToken);
        return;
      }

      string message;
      if (!await document.ValidateHeader() && !licenseHeaderInput.IgnoreNonCommentText)
      {
        var extension = Path.GetExtension (licenseHeaderInput.DocumentPath);
        message = string.Format (Resources.Warning_InvalidLicenseHeader, extension).ReplaceNewLines();
        errors.Enqueue (new ReplacerError<LicenseHeaderPathInput> (licenseHeaderInput, ReplacerErrorType.NonCommentText, message));

        cancellationToken.ThrowIfCancellationRequested();
        await ReportProgress (progress, licenseHeaderInput.DocumentPath, cancellationToken);
        return;
      }

      try
      {
        cancellationToken.ThrowIfCancellationRequested();
        await document.ReplaceHeaderIfNecessaryPath (cancellationToken);
      }
      catch (ParseException)
      {
        message = string.Format (Resources.Error_InvalidLicenseHeader, licenseHeaderInput.DocumentPath).ReplaceNewLines();
        errors.Enqueue (new ReplacerError<LicenseHeaderPathInput> (licenseHeaderInput, ReplacerErrorType.ParsingError, message));
      }

      await ReportProgress (progress, licenseHeaderInput.DocumentPath, cancellationToken);
    }

    private void ResetProgress (int totalFileCount)
    {
      _processedFileCount = 0;
      _totalFileCount = totalFileCount;
    }

    private async Task ReportProgress (IProgress<ReplacerProgressReport> progress, string filePath, CancellationToken cancellationToken)
    {
      await _progressReportSemaphore.WaitAsync (cancellationToken);
      try
      {
        _processedFileCount++;
        progress.Report (new ReplacerProgressReport (_totalFileCount, _processedFileCount, filePath));
      }
      finally
      {
        _progressReportSemaphore.Release();
      }
    }

    private async Task ReportProgress (IProgress<ReplacerProgressContentReport> progress, CancellationToken cancellationToken, string filePath, string newContent)
    {
      await _progressReportSemaphore.WaitAsync (cancellationToken);
      try
      {
        _processedFileCount++;
        progress.Report (new ReplacerProgressContentReport (_totalFileCount, _processedFileCount, filePath, newContent));
      }
      finally
      {
        _progressReportSemaphore.Release();
      }
    }
  }
}
