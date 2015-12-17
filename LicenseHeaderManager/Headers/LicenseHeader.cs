#region copyright
// Copyright (c) 2011 rubicon IT GmbH

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
#endregion

using System.IO;
using System.Text;
using System.Windows;
using EnvDTE;
using LicenseHeaderManager.Options;

namespace LicenseHeaderManager.Headers
{
  public static class LicenseHeader
  {
    public const string Keyword = "extensions:";
    public const string Extension = ".licenseheader";

    private static string GetNewFileName (string name)
    {
      var directory = Path.GetDirectoryName (name);
      var projectName = directory.Substring (directory.LastIndexOf ('\\') + 1);
      var fileName = Path.Combine (directory, projectName) + Extension;

      for (var i = 2; File.Exists(fileName); i++)
        fileName = Path.Combine (directory, projectName) + i + Extension;

      return fileName;
    }

    public static bool ShowQuestionForAddingLicenseHeaderFile (Project activeProject, IDefaultLicenseHeaderPage page)
    {
      string message = string.Format(Resources.Error_NoHeaderDefinition, activeProject.Name).Replace (@"\n", "\n");
      var messageBoxResult = MessageBox.Show (message, Resources.Error, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
      if (messageBoxResult != MessageBoxResult.Yes)
        return false;
      var licenseHeaderDefinitionFile = AddLicenseHeaderDefinitionFile(activeProject, page);
      licenseHeaderDefinitionFile.Open(Constants.vsViewKindCode).Activate();
      return true;
    }


    /// <summary>
    /// Adds a new License Header Definition file to the active project.
    /// </summary>
    public static ProjectItem AddLicenseHeaderDefinitionFile (Project activeProject, IDefaultLicenseHeaderPage page)
    {
      if (activeProject == null)
        return null;

      var fileName = GetNewFileName (activeProject.FileName);
      File.WriteAllText (fileName, page.LicenseHeaderFileText, Encoding.UTF8);
      var newProjectItem = activeProject.ProjectItems.AddFromFile (fileName);

      if (newProjectItem == null)
      {
        string message = string.Format(Resources.Error_CreatingFile).Replace(@"\n", "\n");
        MessageBox.Show(message, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
      }

      return newProjectItem;
    }

    /// <summary>
    /// Adds a new License Header Definition file to a folder
    /// </summary>
    public static ProjectItem AddLicenseHeaderDefinitionFile (ProjectItem folder, IDefaultLicenseHeaderPage page)
    {
      if (folder == null || folder.Kind != Constants.vsProjectItemKindPhysicalFolder)
        return null;

      var fileName = GetNewFileName (folder.Properties.Item("FullPath").Value.ToString());
      File.WriteAllText (fileName, page.LicenseHeaderFileText, Encoding.UTF8);

      var newProjectItem = folder.ProjectItems.AddFromFile (fileName);

      OpenNewProjectItem(newProjectItem);

      return newProjectItem;
    }

    private static bool OpenNewProjectItem(ProjectItem newProjectItem)
    {
      if (newProjectItem != null)
      {
        var window = newProjectItem.Open(Constants.vsViewKindCode);
        window.Activate();
        return true;
      }
      string message = string.Format(Resources.Error_CreatingFile).Replace(@"\n", "\n");
      MessageBox.Show(message, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
      return false;
    }

    public static string AddDot (string extension)
    {
      if (extension.StartsWith ("."))
        return extension;
      else
        return "." + extension;
    }

    public static bool Validate (string header, CommentParser commentParser)
    {
      try
      {
        var result = commentParser.Parse (header);
        return result == header;
      }
      catch (ParseException)
      {
        return false;
      }
    }
  }
}