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
using System.Threading;
using Core;
using EnvDTE;
using LicenseHeaderManager.Utils;
using Microsoft.VisualStudio.Shell;
using Microsoft.Win32;
using Thread = System.Threading.Thread;

namespace LicenseHeaderManager.MenuItemCommands.Common
{
  public static class ExistingLicenseHeaderDefinitionFileAdder
  {
    public static ProjectItem AddDefinitionFileToOneProject (string fileName, ProjectItems projectItems)
    {
      var licenseHeaderDefinitionFileName = OpenFileDialogForExistingFile (fileName);
      ThreadHelper.ThrowIfNotOnUIThread();
      return licenseHeaderDefinitionFileName == string.Empty ? null : AddFileToProject (projectItems, licenseHeaderDefinitionFileName);
    }

    public static void AddDefinitionFileToMultipleProjects (List<Project> projects)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      var licenseHeaderDefinitionFileName = OpenFileDialogForExistingFile (projects.First().DTE.Solution.FullName);
      if (licenseHeaderDefinitionFileName == string.Empty)
        return;

      foreach (var project in projects)
        AddFileToProject (project.ProjectItems, licenseHeaderDefinitionFileName);
    }

    private static ProjectItem AddFileToProject (ProjectItems projectItems, string licenseHeaderDefinitionFileName)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      var fileCountBefore = projectItems.Count;
      var newProjectItem = projectItems.AddFromFile (licenseHeaderDefinitionFileName);

      var fileCountAfter = projectItems.Count;
      if (fileCountBefore == fileCountAfter)
        MessageBoxHelper.ShowMessage (Resources.Warning_CantLinkItemInSameProject);


      return newProjectItem;
    }

    private static void OpenFileDialogForExistingFileInternal (string fileName, out string selectedFileName)
    {
      FileDialog dialog = new OpenFileDialog
                          {
                              CheckFileExists = true,
                              CheckPathExists = true,
                              DefaultExt = LicenseHeaderExtractor.HeaderDefinitionExtension,
                              DereferenceLinks = true,
                              Filter = "License Header Definitions|*" + LicenseHeaderExtractor.HeaderDefinitionExtension,
                              InitialDirectory = Path.GetDirectoryName (fileName)
                          };
      var result = dialog.ShowDialog();

      if (result.HasValue && result.Value)
      {
        selectedFileName = dialog.FileName;
        return;
      }

      selectedFileName = string.Empty;
    }

    private static string OpenFileDialogForExistingFile (string fileName)
    {
      var returnValue = string.Empty;

      var staThread = new Thread (
          () =>
          {
            OpenFileDialogForExistingFileInternal (fileName, out var selectedFileName);
            returnValue = selectedFileName;
          });
      staThread.SetApartmentState (ApartmentState.STA);
      staThread.Start();
      staThread.Join();

      return returnValue;
    }
  }
}
