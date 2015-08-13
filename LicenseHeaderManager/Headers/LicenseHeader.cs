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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    public static string GetNewFileName (Project project)
    {
      var directory = Path.GetDirectoryName (project.FullName);
      var projectName = directory.Substring (directory.LastIndexOf ('\\') + 1);
      var filename = Path.Combine (directory, projectName) + Extension;

      for (var i = 2; File.Exists(filename); i++)
        filename = Path.Combine (directory, projectName) + i + Extension;

      return filename;
    }

    public static bool ShowQuestionForAddingLicenseHeaderFile (Project activeProject, IDefaultLicenseHeaderPage page)
    {
      string message = Resources.Error_NoHeaderDefinition.Replace (@"\n", "\n");
      var messageBoxResult = MessageBox.Show (message, Resources.Error, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
      if (messageBoxResult != MessageBoxResult.Yes)
        return false;
      return AddLicenseHeaderDefinitionFileToProject(activeProject, page);
    }


    /// <summary>
    /// Adds a new License Header Definition file to the active project.
    /// </summary>
    public static bool AddLicenseHeaderDefinitionFileToProject (Project activeProject, IDefaultLicenseHeaderPage page)
    {
      if (activeProject == null)
        return false;

      var fileName = GetNewFileName (activeProject);
      File.WriteAllText (fileName, page.LicenseHeaderFileText);
      var newProjectItem = activeProject.ProjectItems.AddFromFile (fileName);

      if (newProjectItem != null)
      {
        var window = newProjectItem.Open (Constants.vsViewKindCode);
        window.Activate();
        return true;
      }
      else
      {
        string message = string.Format (Resources.Error_CreatingFile).Replace (@"\n", "\n");
        MessageBox.Show (message, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
        return false;
      }
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