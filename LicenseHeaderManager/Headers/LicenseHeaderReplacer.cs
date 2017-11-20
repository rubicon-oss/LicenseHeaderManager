#region copyright
// Copyright (c) rubicon IT GmbH

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using EnvDTE;
using LicenseHeaderManager.Interfaces;
using LicenseHeaderManager.Utils;
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
        bool wasOpen;

        CreateDocumentResult result = TryCreateDocument (item, out document, out wasOpen, headers);
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
        MessageBox.Show (ex.Message + " " + item.Name, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Warning);
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
      bool wasOpen;

      Document document;
      if (TryCreateDocument (item, out document, out wasOpen, headers) == CreateDocumentResult.DocumentCreated)
      {
        // item.Saved is not implemented for web_folders, therefore this check must be after the TryCreateDocument
        bool isSaved = item.Saved;

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

        if (wasOpen)
        {
          if (isSaved)
          {
            item.Document.Save();
          }
        }
        else
        {
          item.Document.Close (vsSaveChanges.vsSaveChangesYes);
        }
        
      }

      if (item.ProjectItems != null)
      {
        var childHeaders = headers;
        if (searchForLicenseHeaders)
        {
          childHeaders = LicenseHeaderFinder.SearchItemsDirectlyGetHeaderDefinition (item.ProjectItems);
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
    public CreateDocumentResult TryCreateDocument (ProjectItem item, out Document document, out bool wasOpen, IDictionary<string, string[]> headers = null)
    {
      document = null;
      wasOpen = true;

      if (!ProjectItemInspection.IsPhysicalFile(item))
        return CreateDocumentResult.NoPhysicalFile;

      if (ProjectItemInspection.IsLicenseHeader(item))
        return CreateDocumentResult.LicenseHeaderDocument;

      if(ProjectItemInspection.IsLink (item))
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
          wasOpen = false;
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
        return CreateDocumentResult.NoPhysicalFile;  
      }
      
      
      var textDocument = itemDocument.Object ("TextDocument") as TextDocument;
      if (textDocument == null)
      {
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
          return CreateDocumentResult.NoHeaderFound;  
        }
        
        header = headers[extension];

        if (header.All(string.IsNullOrEmpty))
        {
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

      return CreateDocumentResult.DocumentCreated;
    }
  }
}
