//Sample license text.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using EnvDTE;
using LicenseHeaderManager.Interfaces;
using LicenseHeaderManager.Options;
using LicenseHeaderManager.Utils;
using Microsoft.VisualStudio.Shell;
using Language = LicenseHeaderManager.Options.Language;
using Window = EnvDTE.Window;

namespace LicenseHeaderManager.Headers
{
  public class LicenseHeaderReplacer
  {
    /// <summary>
    /// Used to keep track of the user selection when he is trying to insert invalid headers into all files,
    /// so that the warning is only displayed once per file extension.
    /// </summary>
    private readonly IDictionary<string, bool> _extensionsWithInvalidHeaders = new Dictionary<string, bool> ();

    private readonly ILicenseHeaderExtension _licenseHeaderExtension;

    public LicenseHeaderReplacer(ILicenseHeaderExtension licenseHeaderExtension)
    {
      _licenseHeaderExtension = licenseHeaderExtension;
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
              _licenseHeaderExtension.ShowLanguagesPage ();
            break;
          case CreateDocumentResult.EmptyHeader:
            break;
          case CreateDocumentResult.NoHeaderFound:
            if (calledbyUser)
            {
              message = string.Format(Resources.Error_NoHeaderFound).Replace(@"\n", "\n");
              MessageBox.Show(message, Resources.NameOfThisExtension, MessageBoxButton.OK, MessageBoxImage.Question);
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
      
      Document document;
      if (TryCreateDocument (item, out document, headers) == CreateDocumentResult.DocumentCreated)
      {
        // item.Saved is not implemented for web_folders, therefore this check must be after the TryCreateDocument
        bool isSaved = item.Saved;
        
        //item.isOpen is not implemented for SQL/DBProject, therefore this check mus be after TryCreateDocument
        bool isOpen = item.IsOpen[Constants.vsViewKindAny];

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
          {
            item.Document.Save();
            item.Document.Close(vsSaveChanges.vsSaveChangesYes);
          }
          else
          {
            item.Document.Close (vsSaveChanges.vsSaveChangesNo);    
          }
        }
        
      }

      if (item.ProjectItems != null)
      {
        var childHeaders = headers;
        if (searchForLicenseHeaders)
        {
          childHeaders = LicenseHeaderFinder.GetHeader (item.ProjectItems);
          if (childHeaders != null)
            headersFound++;
          else
            childHeaders = headers;
        }

        foreach (ProjectItem child in item.ProjectItems)
          headersFound += RemoveOrReplaceHeaderRecursive (child, childHeaders, searchForLicenseHeaders);
      }
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

      if (!ProjectItemInspection.IsPhysicalFile(item))
        return CreateDocumentResult.NoPhysicalFile;

      if (ProjectItemInspection.IsLicenseHeader(item))
        return CreateDocumentResult.LicenseHeaderDocument;

      if(IsLink (item))
        return CreateDocumentResult.LinkedFile;

      var language = _licenseHeaderExtension.LanguagesPage.Languages
          .Where(x => x.Extensions.Any(y => item.Name.EndsWith(y, StringComparison.OrdinalIgnoreCase)))
          .FirstOrDefault();

      if (language == null)
          return CreateDocumentResult.LanguageNotFound;

      Window window = null;
      //try to open the document as a text document
      try
      {
        if (!item.IsOpen[Constants.vsViewKindTextView])
        {
          window = item.Open(Constants.vsViewKindTextView);
        }
      }
      catch (COMException)
      {
        return CreateDocumentResult.NoTextDocument;
      }
      catch (IOException)
      {
        return CreateDocumentResult.NoPhysicalFile;
      }

      var itemDocument = item.Document;
      if (item.Document == null)
      {
        //CloseItemWindow(window);
        return CreateDocumentResult.NoPhysicalFile;  
      }
      
      
      var textDocument = itemDocument.Object ("TextDocument") as TextDocument;
      if (textDocument == null)
      {
        //CloseItemWindow(window);
        return CreateDocumentResult.NoTextDocument;  
      }
      

      string[] header = null;
      if (headers != null)
      {
        var extension = headers.Keys
            .OrderByDescending (x => x.Length)
            .Where (x => item.Name.EndsWith (x, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        if (extension == null)
        {
          //CloseItemWindow(window);
          return CreateDocumentResult.NoHeaderFound;  
        }
        
        header = headers[extension];

        if (header.All(string.IsNullOrEmpty))
        {
          //CloseItemWindow(window);
          return CreateDocumentResult.EmptyHeader;    
        }
      }

      var optionsPage = _licenseHeaderExtension.OptionsPage;

      document = new Document (
          textDocument,
          language,
          header,
          item,
          optionsPage.UseRequiredKeywords
              ? optionsPage.RequiredKeywords.Split (new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select (k => k.Trim())
              : null);

      //CloseItemWindow(window);
      return CreateDocumentResult.DocumentCreated;
    }

    private void CloseItemWindow(Window window)
    {
      if (window != null)
        window.Close ();
    }

    private bool IsLink (ProjectItem item)
    {
      return (item.Properties != null && (bool) item.Properties.Item("IsLink").Value);
    }
  }
}
