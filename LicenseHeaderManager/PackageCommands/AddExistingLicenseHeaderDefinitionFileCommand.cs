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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using EnvDTE;
using LicenseHeaderManager.Headers;
using Microsoft.Win32;

namespace LicenseHeaderManager.PackageCommands
{
  public class AddExistingLicenseHeaderDefinitionFileCommand
  {
    public ProjectItem AddDefinitionFileToOneProject (string fileName, ProjectItems projectItems)
    {
      var licenseHeaderDefinitionFileName = OpenFileDialogForExistingFile(fileName);

      if (licenseHeaderDefinitionFileName == string.Empty) return null;

      return AddFileToProject(projectItems, licenseHeaderDefinitionFileName);
    }

    public void AddDefinitionFileToMultipleProjects (List<Project> projects)
    {
      var licenseHeaderDefinitionFileName = OpenFileDialogForExistingFile (projects.First().DTE.Solution.FullName);

      if (licenseHeaderDefinitionFileName == string.Empty) return;

      foreach (var project in projects)
      {
        AddFileToProject(project.ProjectItems, licenseHeaderDefinitionFileName);
      }
    }

    private ProjectItem AddFileToProject (ProjectItems projectItems, string licenseHeaderDefinitionFileName)
    {
      int fileCountBefore = projectItems.Count;
      var newProjectItem = projectItems.AddFromFile (licenseHeaderDefinitionFileName);

      int fileCountAfter = projectItems.Count;
      if (fileCountBefore == fileCountAfter)
      {
        MessageBox.Show (Resources.Warning_CantLinkItemInSameProject, Resources.NameOfThisExtension, MessageBoxButton.OK,
          MessageBoxImage.Information);
      }

      return newProjectItem;
    }

    private string OpenFileDialogForExistingFile(string fileName)
    {
      FileDialog dialog = new OpenFileDialog ();
      dialog.CheckFileExists = true;
      dialog.CheckPathExists = true;
      dialog.DefaultExt = LicenseHeader.Extension;
      dialog.DereferenceLinks = true;
      dialog.Filter = "License Header Definitions|*" + LicenseHeader.Extension;
      dialog.InitialDirectory = Path.GetDirectoryName (fileName);
      bool? result = dialog.ShowDialog ();

      if (result.HasValue && result.Value)
        return dialog.FileName;

      return string.Empty;
    }
  }
}