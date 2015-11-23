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
using System.Linq;
using System.Windows;
using EnvDTE;
using LicenseHeaderManager.Headers;
using LicenseHeaderManager.Options;
using LicenseHeaderManager.Utils;
using Microsoft.VisualStudio.Shell.Interop;

namespace LicenseHeaderManager.PackageCommands
{
  public class AddLicenseHeaderToAllProjectsCommand
  {
    private AddLicenseHeaderToAllFilesCommand addLicenseHeaderToAllFilesCommand;
    private IVsStatusbar statusBar;
    private IDefaultLicenseHeaderPage licenseHeaderPage;

    public AddLicenseHeaderToAllProjectsCommand(LicenseHeaderReplacer licenseReplacer, IVsStatusbar statusBar, IDefaultLicenseHeaderPage licenseHeaderPage)
    {
      addLicenseHeaderToAllFilesCommand = new AddLicenseHeaderToAllFilesCommand (licenseReplacer);
      this.statusBar = statusBar;
      this.licenseHeaderPage = licenseHeaderPage;
     }

    public void Execute(Solution solution)
    {
      var allSolutionProjectsSearcher = new AllSolutionProjectsSearcher();
      var projectsInSolution = allSolutionProjectsSearcher.GetAllProjects(solution);

      var projectsWithoutLicenseHeaderFile = CheckForLicenseHeaderFileInProjects (projectsInSolution);

      if (DefinitionFilesShouldBeAdded(projectsWithoutLicenseHeaderFile))
        AddMissingLicenseHeaderFiles(projectsWithoutLicenseHeaderFile, licenseHeaderPage);
    
      AddLicenseHeaderToProjects(projectsInSolution); 
    }

    private List<Project> CheckForLicenseHeaderFileInProjects (List<Project> projects)
    {
      return (projects
        .Where(project => LicenseHeaderFinder.GetHeader(project) == null)
        .ToList());
    }

    private bool DefinitionFilesShouldBeAdded (List<Project> projectsWithoutLicenseHeaderFile)
    {
      if (!projectsWithoutLicenseHeaderFile.Any()) return false;

      var errorResourceString = projectsWithoutLicenseHeaderFile.Count == 1
          ? Resources.Error_NoHeaderDefinition
          : Resources.Error_MulitpleProjectsNoLicenseHeaderFile;

      var message = string.Format (errorResourceString, string.Join ("\n", projectsWithoutLicenseHeaderFile
                                                                      .Select(x => x.Name)
                                                                      .ToList())).Replace (@"\n", "\n");

      return MessageBox.Show (message, Resources.LicenseHeaderManagerName, MessageBoxButton.YesNo,
        MessageBoxImage.Information,
        MessageBoxResult.No) == MessageBoxResult.Yes;
    }

    private void AddMissingLicenseHeaderFiles (List<Project> projectsWithoutLicenseHeader, IDefaultLicenseHeaderPage page)
    {
      if (AskAddExistingLicenseHeaderFile())
      {
        new AddExistingLicenseHeaderDefinitionFile().AddDefinitionFileToMultipleProjects(projectsWithoutLicenseHeader);
      }
      else
      {
        foreach (Project project in projectsWithoutLicenseHeader)
        {
          if (projectsWithoutLicenseHeader.Contains (project))
            LicenseHeader.AddLicenseHeaderDefinitionFile (project, page, false);
        }   
      }
    }

    private bool AskAddExistingLicenseHeaderFile()
    {
      return MessageBox.Show (Resources.AddExistingLicenseHeaderFileQuestion, Resources.LicenseHeaderManagerName, MessageBoxButton.YesNo,
        MessageBoxImage.Information,
        MessageBoxResult.No) == MessageBoxResult.Yes;
    }

    private void AddLicenseHeaderToProjects (List<Project> projectsInSolution)
    {
      int progressCount = 1;
      int projectCount = projectsInSolution.Count();

      foreach (Project project in projectsInSolution)
      {
        statusBar.SetText(string.Format(Resources.UpdateSolution, progressCount, projectCount));
        addLicenseHeaderToAllFilesCommand.Execute(project);
        progressCount++;
      }

      statusBar.SetText (String.Empty);
    }
  }
}