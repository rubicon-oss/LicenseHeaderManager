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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Core;
using EnvDTE;
using LicenseHeaderManager.Headers;
using LicenseHeaderManager.Interfaces;
using LicenseHeaderManager.UpdateViewModels;
using log4net;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace LicenseHeaderManager.Utils
{
  internal static class CoreHelpers
  {
    private static readonly ILog s_log = LogManager.GetLogger (MethodBase.GetCurrentMethod().DeclaringType);

    /// <summary>
    ///   Creates a replacer progress that represents the progress of updated files in folders and projects.
    /// </summary>
    /// <param name="viewModel">
    ///   Specifies the view model that contains the progress bar whose update progress should be
    ///   visualized.
    /// </param>
    /// <param name="projectName">Specifies the name of the project that is updated.</param>
    /// <param name="fileOpenedStatus">Specifies what files are currently open in the current project.</param>
    /// <param name="cancellationToken">
    ///   Specifies the cancellation token that indicates if the current updating progress has
    ///   been cancelled by the user.
    /// </param>
    public static IProgress<ReplacerProgressContentReport> CreateProgress (
        BaseUpdateViewModel viewModel,
        string projectName,
        IDictionary<string, bool> fileOpenedStatus,
        CancellationToken cancellationToken)
    {
      return new ReplacerProgress<ReplacerProgressContentReport> (
          report => OnProgressReportedAsync (report, viewModel, projectName, fileOpenedStatus, cancellationToken).FireAndForget());
    }

    /// <summary>
    ///   Determines a collection of <see cref="LicenseHeaderContentInput" /> instance to be processed, based on a
    ///   <see cref="ProjectItem" /> and its child items.
    /// </summary>
    /// <param name="item">
    ///   The <see cref="ProjectItem" /> to be used as starting point when looking for child items whose
    ///   license headers should also be updated.
    /// </param>
    /// <param name="headers">
    ///   The parsed headers based on a license header definition file. Keys are file extensions, values
    ///   represent the header, one line per array-element.
    /// </param>
    /// <param name="countSubLicenseHeaders">
    ///   Number of license header definition files found in child
    ///   <see cref="ProjectItem" />s
    /// </param>
    /// <param name="fileOpenedStatus">
    ///   Provides information on which files (full path, dictionary key) are currently opened
    ///   (values)
    /// </param>
    /// <param name="searchForLicenseHeaders">
    ///   Determines whether child <see cref="ProjectItem" />s should also be searched for
    ///   license header definition files and its corresponding header definitions. If <see langword="false" />, only the
    ///   license header definitions represented by <paramref name="headers" /> are used, even for all child items.
    /// </param>
    /// <returns>
    ///   Returns a <see cref="ICollection{T}" /> whose generic type parameter is
    ///   <see cref="LicenseHeaderContentInput" /> that encompasses the input for a <see cref="LicenseHeaderReplacer" />
    ///   instance based on the given parameters.
    /// </returns>
    public static ICollection<LicenseHeaderContentInput> GetFilesToProcess (
        ProjectItem item,
        IDictionary<string, string[]> headers,
        out int countSubLicenseHeaders,
        out IDictionary<string, bool> fileOpenedStatus,
        bool searchForLicenseHeaders = true)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      fileOpenedStatus = new Dictionary<string, bool>();
      var files = new List<LicenseHeaderContentInput>();
      countSubLicenseHeaders = 0;

      if (item == null)
        return files;

      if (item.FileCount == 1 && File.Exists (item.FileNames[1]))
      {
        var content = item.GetContent (out var wasAlreadyOpen, LicenseHeadersPackage.Instance);
        if (content != null)
        {
          files.Add (new LicenseHeaderContentInput (content, item.FileNames[1], headers, item.GetAdditionalProperties()));
          fileOpenedStatus[item.FileNames[1]] = wasAlreadyOpen;
        }
      }


      if (item.ProjectItems == null)
        return files;

      var childHeaders = headers;
      if (searchForLicenseHeaders)
      {
        childHeaders = LicenseHeaderFinder.SearchItemsDirectlyGetHeaderDefinition (item.ProjectItems);
        if (childHeaders != null)
          countSubLicenseHeaders++;
        else
          childHeaders = headers;
      }

      foreach (ProjectItem child in item.ProjectItems)
      {
        var subFiles = GetFilesToProcess (child, childHeaders, out var subLicenseHeaders, out var subFileOpenedStatus, searchForLicenseHeaders);

        files.AddRange (subFiles);
        foreach (var status in subFileOpenedStatus)
          fileOpenedStatus[status.Key] = status.Value;

        countSubLicenseHeaders += subLicenseHeaders;
      }

      return files;
    }

    /// <summary>
    ///   Processes the given <see cref="ReplacerResult{TSuccess,TError}" /> object, including handling errors that possibly
    ///   occurred.
    /// </summary>
    /// <param name="result">Specifies the replacer result. Indicates whether the specific operation succeeded or failed.</param>
    /// <param name="extension">
    ///   A <see cref="ILicenseHeaderExtension" /> instance used to access members exposed by the LHM
    ///   Extension Package.
    /// </param>
    /// <param name="isOpen">Specifies if the current file is currently open.</param>
    /// <param name="calledByUser">
    ///   Specifies whether this method was called explicitly by the user or implicitly by the
    ///   program.
    /// </param>
    public static async Task HandleResultAsync (
        ReplacerResult<ReplacerSuccess, ReplacerError<LicenseHeaderContentInput>> result,
        ILicenseHeaderExtension extension,
        bool isOpen,
        bool calledByUser)
    {
      if (result.IsSuccess)
      {
        await extension.JoinableTaskFactory.SwitchToMainThreadAsync();
        ProcessSuccess (result.Success, extension, isOpen);
        return;
      }

      if (!calledByUser)
        return;

      var error = result.Error;
      switch (error.Type)
      {
        case ReplacerErrorType.NonCommentText:
          error.Input.IgnoreNonCommentText = true;
          if (!MessageBoxHelper.AskYesNo (error.Description, Resources.Warning, true))
            return;

          var resultIgnoringNonCommentText = await extension.LicenseHeaderReplacer.RemoveOrReplaceHeader (error.Input);
          if (resultIgnoringNonCommentText.IsSuccess)
          {
            ProcessSuccess (resultIgnoringNonCommentText.Success, extension, isOpen);
            return;
          }

          error = resultIgnoringNonCommentText.Error;
          break;

        case ReplacerErrorType.LanguageNotFound:
          // TODO possible feature: show languages page if language is not found
          // in below code, languages page closes immediately after opening because we return from the method -> remedy for this would be needed 
          // var showLanguagePage = MessageBoxHelper.AskYesNo (error.Description, Resources.Error);
          // if (showLanguagePage) extension.ShowLanguagesPage();
          return;

        case ReplacerErrorType.LicenseHeaderDocument:
          return; // ignore such an error (i. e. do not propagate to user)

        default:
          s_log.Warn ($"File '{error.Input.DocumentPath}' failed with error '{error.Type}': {error.Description}");
          MessageBoxHelper.ShowMessage ($"Could not modify license headers of file '{error.Input.DocumentPath}':\n{error.Description}", Resources.Warning, true);
          break;
      }
    }

    ///// <summary>
    /////   Handles the given result object and shows the corresponding message box if an error occurred.
    ///// </summary>
    ///// <param name="result">Specifies the replacer result. Indicates whether the specific operation succeeded or failed.</param>
    ///// <param name="extension">Specifies the extension of the language.</param>
    ///// <param name="isOpen">Specifies if the current file is currently open.</param>
    ///// <param name="calledByUser">Specifies whether this method was called explicitly by the user or by the program.</param>

    /// <summary>
    ///   Processes a range of <see cref="ReplacerResult{TSuccess,TError}" /> objects, including possible error handling.
    /// </summary>
    /// <param name="result">Specifies the replacer result. Indicates whether the specific operation succeeded or failed.</param>
    /// <param name="licenseHeaderExtension">
    ///   A <see cref="ILicenseHeaderExtension" /> instance used to access members exposed
    ///   by the LHM Extension Package.
    /// </param>
    /// <param name="viewModel">
    ///   A <see cref="BaseUpdateViewModel" /> instance used to update progress indicator properties if a
    ///   <see cref="LicenseHeaderReplacer" /> newly needs to be invoked.
    /// </param>
    /// <param name="projectName">
    ///   The name of the project the files updated by a <see cref="LicenseHeaderReplacer" /> operation
    ///   belong to.
    /// </param>
    /// <param name="fileOpenedStatus">
    ///   Provides information on which files (full path, dictionary key) are currently opened
    ///   (values).
    /// </param>
    /// <param name="cancellationToken">
    ///   A <see cref="CancellationToken" /> that can be used to cancel pending Core operations
    ///   if they have not been started yet.
    /// </param>
    public static async Task HandleResultAsync (
        IEnumerable<ReplacerResult<ReplacerSuccess, ReplacerError<LicenseHeaderContentInput>>> result,
        ILicenseHeaderExtension licenseHeaderExtension,
        BaseUpdateViewModel viewModel,
        string projectName,
        IDictionary<string, bool> fileOpenedStatus,
        CancellationToken cancellationToken)
    {
      // collect NonCommentText-errors and ask if license header should still be inserted
      var errors = result.Where (replacerResult => !replacerResult.IsSuccess).Select (replacerResult => replacerResult.Error).ToList();

      var nonCommentTextErrorsByExtension = errors.Where (x => x.Type == ReplacerErrorType.NonCommentText).GroupBy (x => Path.GetExtension (x.Input.DocumentPath));

      var inputIgnoringNonCommentText = new List<LicenseHeaderContentInput>();
      foreach (var extension in nonCommentTextErrorsByExtension)
      {
        var message = string.Format (Resources.Warning_InvalidLicenseHeader, extension.Key).ReplaceNewLines();
        if (!MessageBoxHelper.AskYesNo (message, Resources.Warning, true))
          continue;

        foreach (var failedFile in extension)
        {
          failedFile.Input.IgnoreNonCommentText = true;
          inputIgnoringNonCommentText.Add (failedFile.Input);
        }
      }

      // collect other errors and the ones that occurred while "force-inserting" headers with non-comment-text
      var overallErrors = errors.Where (x => x.Type != ReplacerErrorType.NonCommentText && x.Type != ReplacerErrorType.LicenseHeaderDocument).ToList();
      if (inputIgnoringNonCommentText.Count > 0)
      {
        viewModel.FileCountCurrentProject = inputIgnoringNonCommentText.Count;
        var resultIgnoringNonCommentText = await licenseHeaderExtension.LicenseHeaderReplacer.RemoveOrReplaceHeader (
            inputIgnoringNonCommentText,
            CreateProgress (viewModel, projectName, fileOpenedStatus, cancellationToken),
            cancellationToken);

        overallErrors.AddRange (resultIgnoringNonCommentText.Where (replacerResult => !replacerResult.IsSuccess).Select (replacerResult => replacerResult.Error));
      }

      // display all errors collected from "first attempt" and "force-insertion"
      if (overallErrors.Count == 0)
        return;

      MessageBoxHelper.ShowError ($"{overallErrors.Count} unexpected errors have occurred. See output window or log file for more details");
      foreach (var otherError in overallErrors)
        s_log.Error ($"File '{otherError.Input.DocumentPath}' failed: {otherError.Description}");
    }

    /// <summary>
    ///   Tries to open a <see cref="ProjectItem" /> such that content modification operations are possible.
    /// </summary>
    /// <param name="item">The <see cref="ProjectItem" /> to be opened.</param>
    /// <param name="extension">
    ///   The <see cref="ILicenseHeaderExtension" /> instance used to access the currently configured
    ///   language definitions.
    /// </param>
    /// <returns>
    ///   Returns <see langword="true" /> if opening <paramref name="item" /> succeeded or was not necessary, otherwise
    ///   <see langword="false" />.
    /// </returns>
    public static bool TryOpenDocument (ProjectItem item, ILicenseHeaderExtension extension)
    {
      try
      {
        ThreadHelper.ThrowIfNotOnUIThread();

        // Opening files potentially having non-text content (.png, .snk) might result in a Visual Studio error "Some bytes have been replaced with the
        // Unicode substitution character while loading file ...". In order to avoid this, files with unknown extensions are not opened. However, in order
        // to keep such files eligible as Core input, still return true
        var languageForExtension = extension.LicenseHeaderReplacer.GetLanguageFromExtension (Path.GetExtension (item.FileNames[1]));
        if (languageForExtension == null)
          return true;

        item.Open (Constants.vsViewKindTextView);
        return true;
      }
      catch (COMException)
      {
        return false;
      }
      catch (IOException)
      {
        return false;
      }
    }

    /// <summary>
    ///   Saves the current content of <see cref="ProjectItem" /> if it was saved before and closes it afterwards, if it not
    ///   opened before.
    /// </summary>
    /// <param name="item">The item whose content should be saved.</param>
    /// <param name="wasOpen">Determines whether the <paramref name="item" /> is currently opened.</param>
    /// <param name="wasSaved">
    ///   Determines whether the contents of <paramref name="item" /> were initially saved before they
    ///   have been updated.
    /// </param>
    public static void SaveAndCloseIfNecessary (ProjectItem item, bool wasOpen, bool wasSaved)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      if (wasOpen)
      {
        // if document had no unsaved changes before, it should not have any now (analogously for when it did have unsaved changes)
        if (wasSaved)
          item.Document.Save();
      }
      else
      {
        item.Document.Close (vsSaveChanges.vsSaveChangesYes);
      }
    }

    private static void ProcessSuccess (ReplacerSuccess replacerSuccess, ILicenseHeaderExtension extension, bool isOpen)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      if (!File.Exists (replacerSuccess.FilePath) || TrySetContent (replacerSuccess.FilePath, extension.Dte2.Solution, replacerSuccess.NewContent, isOpen, extension))
        return;

      s_log.Error ($"Updating license header for file {replacerSuccess.FilePath} failed.");
      MessageBoxHelper.ShowError ($"Updating license header for file {replacerSuccess.FilePath} failed.");
    }

    private static bool TrySetContent (string itemPath, Solution solution, string content, bool wasOpen, ILicenseHeaderExtension extension)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      var item = solution.FindProjectItem (itemPath);
      if (item == null)
        return false;

      if (!wasOpen && !TryOpenDocument (item, extension))
        return false;

      // returning false from this method would signify an error, which we do not want since this circumstance is expected to occur with unknown file extensions
      var languageForExtension = extension.LicenseHeaderReplacer.GetLanguageFromExtension (Path.GetExtension (item.FileNames[1]));
      if (languageForExtension == null)
        return true;

      if (!(item.Document.Object ("TextDocument") is TextDocument textDocument))
        return false;

      var wasSaved = item.Document.Saved;

      textDocument.CreateEditPoint (textDocument.StartPoint).Delete (textDocument.EndPoint);
      textDocument.CreateEditPoint (textDocument.StartPoint).Insert (content);

      SaveAndCloseIfNecessary (item, wasOpen, wasSaved);

      return true;
    }

    private static async Task OnProgressReportedAsync (
        ReplacerProgressContentReport progress,
        BaseUpdateViewModel baseUpdateViewModel,
        string projectName,
        IDictionary<string, bool> fileOpenedStatus,
        CancellationToken cancellationToken)
    {
      await LicenseHeadersPackage.Instance.JoinableTaskFactory.SwitchToMainThreadAsync();

      if (!cancellationToken.IsCancellationRequested)
      {
        var result = new ReplacerResult<ReplacerSuccess, ReplacerError<LicenseHeaderContentInput>> (
            new ReplacerSuccess (progress.ProcessedFilePath, progress.ProcessFileNewContent));
        if (fileOpenedStatus.TryGetValue (progress.ProcessedFilePath, out var wasOpen))
          await HandleResultAsync (result, LicenseHeadersPackage.Instance, wasOpen, false);
        else
          await HandleResultAsync (result, LicenseHeadersPackage.Instance, false, false);
      }

      if (baseUpdateViewModel == null)
        return;

      baseUpdateViewModel.FileCountCurrentProject = progress.TotalFileCount;
      baseUpdateViewModel.ProcessedFilesCountCurrentProject = progress.ProcessedFileCount;

      if (baseUpdateViewModel is SolutionUpdateViewModel solutionUpdateViewModel)
        solutionUpdateViewModel.CurrentProject = projectName;
    }
  }
}
