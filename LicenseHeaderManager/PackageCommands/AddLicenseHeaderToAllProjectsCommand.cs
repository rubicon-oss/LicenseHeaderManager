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

      if (projectsInSolution.Count == 1 && projectsWithoutLicenseHeaderFile.Count == 1)
      {
        //There is exactly one Project in the Solution and it has no Definition File 
        //--> Offer to add a new one, but do not add License Headers
        if (MessageBoxDoYouWant(Resources.Question_AddNewLicenseHeaderDefinitionFileSingleProject))
        {
          LicenseHeader.AddLicenseHeaderDefinitionFile(projectsInSolution.First(), licenseHeaderPage, true);
        }
      }
      else if (projectsWithoutLicenseHeaderFile.Count == projectsInSolution.Count)
      {
        //There are multiple Projects in the Solution but none of them has a Definition File 
        //--> Offer to add new ones to everyone of them, but do not add License Headers
        if (MessageBoxDoYouWant(Resources.Question_AddNewLicenseHeaderDefinitionFileMultipleProjects))
        {
          AddNewLicenseHeaderDefinitionFilesToProjects(projectsWithoutLicenseHeaderFile, licenseHeaderPage);
          MessageBoxInformation(Resources.Information_DefinitionFileAdded);
          //TODO: Discuss with MK if we add License Headers afterwards or stop
        }
        else
        {
          MessageBoxInformation(Resources.Information_NoDefinitionFileStopUpdating);
        }
      }
      //There are projects with and without Definition File --> Ask if we should add an existing License Header File to them and then add License Headers
      else if (projectsWithoutLicenseHeaderFile.Any())
      {
        if (DefinitionFilesShouldBeAdded(projectsWithoutLicenseHeaderFile))
             new AddExistingLicenseHeaderDefinitionFile().AddDefinitionFileToMultipleProjects(projectsWithoutLicenseHeaderFile);

        AddLicenseHeaderToProjects(projectsInSolution);
      }
      //There are no Projects without Definition File --> Add License Headers
      else
      {
        AddLicenseHeaderToProjects(projectsInSolution);  
      }
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

      var errorResourceString = Resources.Error_MulitpleProjectsNoLicenseHeaderFile;

      //TODO: Wenn mehr als 5, auf 5 abkürzen und "..." anzeigen
      var message = string.Format (errorResourceString, string.Join ("\n", projectsWithoutLicenseHeaderFile
                                                                      .Select(x => x.Name)
                                                                      .ToList())).Replace (@"\n", "\n");

      return MessageBoxDoYouWant(message);
    }

    private static void AddNewLicenseHeaderDefinitionFilesToProjects(List<Project> projectsWithoutLicenseHeader,
      IDefaultLicenseHeaderPage page)
    {
      foreach (Project project in projectsWithoutLicenseHeader)
      {
        if (projectsWithoutLicenseHeader.Contains(project))
          LicenseHeader.AddLicenseHeaderDefinitionFile(project, page, false);
      }
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

    private void MessageBoxInformation(string message)
    {
      MessageBox.Show(message, Resources.LicenseHeaderManagerName, MessageBoxButton.OK,
        MessageBoxImage.Information);
    }

    private bool MessageBoxDoYouWant(string message)
    {
      return MessageBox.Show(message, Resources.LicenseHeaderManagerName, MessageBoxButton.YesNo,
        MessageBoxImage.Information,
        MessageBoxResult.No) == MessageBoxResult.Yes;
    }
  }
}