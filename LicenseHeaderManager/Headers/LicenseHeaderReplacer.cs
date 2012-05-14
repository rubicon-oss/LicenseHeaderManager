using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using EnvDTE;
using LicenseHeaderManager.Options;
using Microsoft.VisualStudio.Shell;
using Language = LicenseHeaderManager.Options.Language;

namespace LicenseHeaderManager.Headers
{
  class LicenseHeaderReplacer
  {
    /// <summary>
    /// Used to keep track of the user selection when he is trying to insert invalid headers into all files,
    /// so that the warning is only displayed once per file extension.
    /// </summary>
    private readonly IDictionary<string, bool> _extensionsWithInvalidHeaders = new Dictionary<string, bool> ();
    private readonly LicenseHeadersPackage _vsPackage;

    public LicenseHeaderReplacer (LicenseHeadersPackage vsPackage)
    {
      _vsPackage = vsPackage;
    }


    public void ResetExtensionsWithInvalidHeaders()
    {
      _extensionsWithInvalidHeaders.Clear();
    }

    /// <summary>
    /// Removes or replaces the header of a given project item.
    /// </summary>
    /// <param name="item">The project item.</param>
    /// <param name="headers">A dictionary of headers using the file extension as key and the header as value or null if headers should only be removed.</param>
    /// <param name="calledbyUser">Specifies whether the command was called by the user (as opposed to automatically by a linked command or by ItemAdded)</param>
    public void RemoveOrReplaceHeader (ProjectItem item, IDictionary<string, string[]> headers, bool calledbyUser = true)
    {
      try
      {
        Document document;
        CreateDocumentResult result = TryCreateDocument (item, out document, headers);
        string message;

        switch (result)
        {
          case CreateDocumentResult.DocumentCreated:
            if (!document.ValidateHeader ())
            {
              message = string.Format (Resources.Warning_InvalidLicenseHeader, Path.GetExtension (item.Name)).Replace (@"\n", "\n");
              if (MessageBox.Show (message, Resources.Warning, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No)
                  == MessageBoxResult.No)
                break;
            }
            try
            {
              document.ReplaceHeaderIfNecessary ();
            }
            catch (ParseException)
            {
              message = string.Format (Resources.Error_InvalidLicenseHeader, item.Name).Replace (@"\n", "\n");
              MessageBox.Show (message, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            break;
          case CreateDocumentResult.LanguageNotFound:
            message = string.Format (Resources.Error_LanguageNotFound, Path.GetExtension (item.Name)).Replace (@"\n", "\n");
            if (calledbyUser && MessageBox.Show (message, Resources.Error, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No)
                == MessageBoxResult.Yes)
              _vsPackage.ShowOptionPage (typeof (LanguagesPage));
            break;
          case CreateDocumentResult.NoHeaderFound:
            if (calledbyUser)
            {
              var page = (DefaultLicenseHeaderPage) _vsPackage.GetDialog (typeof (DefaultLicenseHeaderPage));
              LicenseHeader.ShowQuestionForAddingLicenseHeaderFile (item.ContainingProject, page);
            }
            break;
        }
      }
      catch (ArgumentException ex)
      {
        MessageBox.Show (ex.Message, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Warning);
      }
    }

    /// <summary>
    /// Removes or replaces the header of a given project item and all of its child items.
    /// </summary>
    /// <param name="item">The project item.</param>
    /// <param name="headers">A dictionary of headers using the file extension as key and the header as value or null if headers should only be removed.</param>
    /// <param name="searchForLicenseHeaders"></param>
    public int RemoveOrReplaceHeaderRecursive (ProjectItem item, IDictionary<string, string[]> headers, bool searchForLicenseHeaders = true)
    {
      int headersFound = 0;
      bool isOpen = item.IsOpen[Constants.vsViewKindAny];
      bool isSaved = item.Saved;

      Document document;
      if (TryCreateDocument (item, out document, headers) == CreateDocumentResult.DocumentCreated)
      {
        string message;
        bool replace = true;

        if (!document.ValidateHeader ())
        {
          string extension = Path.GetExtension (item.Name);
          if (!_extensionsWithInvalidHeaders.TryGetValue (extension, out replace))
          {
            message = string.Format (Resources.Warning_InvalidLicenseHeader, extension).Replace (@"\n", "\n");
            replace = MessageBox.Show (message, Resources.Warning, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No)
                      == MessageBoxResult.Yes;
            _extensionsWithInvalidHeaders[extension] = replace;
          }
        }

        if (replace)
        {
          try
          {
            document.ReplaceHeaderIfNecessary ();
          }
          catch (ParseException)
          {
            message = string.Format (Resources.Error_InvalidLicenseHeader, item.Name).Replace (@"\n", "\n");
            MessageBox.Show (message, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
          }
        }

        if (isOpen)
        {
          if (isSaved)
            item.Document.Save ();
        }
        else
          item.Document.Close (vsSaveChanges.vsSaveChangesYes);
      }


      var childHeaders = headers;
      if (searchForLicenseHeaders)
      {
        childHeaders = LicenseHeaderFinder.GetHeader (item);
        if (childHeaders != null)
          headersFound++;
        else
          childHeaders = headers;
      }

      foreach (ProjectItem child in item.ProjectItems)
        headersFound += RemoveOrReplaceHeaderRecursive (child, childHeaders, searchForLicenseHeaders);
      return headersFound;
    }

    /// <summary>
    /// Tries to open a given project item as a Document which can be used to add or remove headers.
    /// </summary>
    /// <param name="item">The project item.</param>
    /// <param name="document">The document which was created or null if an error occured (see return value).</param>
    /// <param name="headers">A dictionary of headers using the file extension as key and the header as value or null if headers should only be removed.</param>
    /// <returns>A value indicating the result of the operation. Document will be null unless DocumentCreated is returned.</returns>
    public CreateDocumentResult TryCreateDocument (ProjectItem item, out Document document, IDictionary<string, string[]> headers = null)
    {
      document = null;

      if (item.Kind != Constants.vsProjectItemKindPhysicalFile)
        return CreateDocumentResult.NoPhyiscalFile;

      //don't insert license header information in license header definitions
      if (item.Name.EndsWith (LicenseHeader.Cextension))
        return CreateDocumentResult.LicenseHeaderDocument;

      //try to open the document as a text document
      try
      {
        if (!item.IsOpen["{FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF}"])
          item.Open (Constants.vsViewKindTextView);
      }
      catch (COMException)
      {
        return CreateDocumentResult.NoTextDocument;
      }

      var textDocument = item.Document.Object ("TextDocument") as TextDocument;
      if (textDocument == null)
        return CreateDocumentResult.NoTextDocument;

      //try to find a comment definitions for the language of the document
      var languagePage = (LanguagesPage) _vsPackage.GetDialog (typeof (LanguagesPage));
      var extensions = from l in languagePage.Languages
                       from e in l.Extensions
                       where item.Name.ToLower ().EndsWith (e)
                       orderby e.Length descending
                       // ".designer.cs" has a higher priority then ".cs" for example
                       select new { Extension = e, Language = l };

      if (!extensions.Any ())
        return CreateDocumentResult.LanguageNotFound;

      Language language = null;

      string[] header = null;

      //if headers is null, we only want to remove the existing headers and thus don't need to find the right header
      if (headers != null)
      {
        //try to find a header for each of the languages (if there's no header for ".designer.cs", use the one for ".cs" files)
        foreach (var extension in extensions)
        {
          if (headers.TryGetValue (extension.Extension, out header))
          {
            language = extension.Language;
            break;
          }
        }

        if (header == null)
          return CreateDocumentResult.NoHeaderFound;
      }
      else
        language = extensions.First ().Language;

      //get the required keywords from the options page
      var optionsPage = (OptionsPage) _vsPackage.GetDialog (typeof (OptionsPage));

      document = new Document (
          textDocument,
          language,
          header,
          item,
          optionsPage.UseRequiredKeywords
              ? optionsPage.RequiredKeywords.Split (new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select (k => k.Trim ())
              : null);

      return CreateDocumentResult.DocumentCreated;
    }
  }
}
