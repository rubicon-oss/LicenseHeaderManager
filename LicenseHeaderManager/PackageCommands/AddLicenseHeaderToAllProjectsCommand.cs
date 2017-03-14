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
using System.Linq;
using System.Windows;
using EnvDTE;
using LicenseHeaderManager.Headers;
using LicenseHeaderManager.Interfaces;
using LicenseHeaderManager.Options;
using LicenseHeaderManager.SolutionUpdateViewModels;
using LicenseHeaderManager.Utils;
using Microsoft.VisualStudio.Shell.Interop;
using Constants = EnvDTE.Constants;

namespace LicenseHeaderManager.PackageCommands
{
  public class AddLicenseHeaderToAllProjectsCommand : ISolutionLevelCommand
  {
    private readonly IDefaultLicenseHeaderPage _licenseHeaderPage;
    private readonly LicenseHeaderReplacer _licenseReplacer;
    private readonly SolutionUpdateViewModel _solutionUpdateViewModel;

    public AddLicenseHeaderToAllProjectsCommand(LicenseHeaderReplacer licenseReplacer, IDefaultLicenseHeaderPage licenseHeaderPage, SolutionUpdateViewModel solutionUpdateViewModel)
    {
      _licenseHeaderPage = licenseHeaderPage;
      _licenseReplacer = licenseReplacer;
      _solutionUpdateViewModel = solutionUpdateViewModel;
    }

    public void Execute(Solution solution)
    {
      if (solution == null) return;

      var allSolutionProjectsSearcher = new AllSolutionProjectsSearcher();
      var projectsInSolution = allSolutionProjectsSearcher.GetAllProjects(solution);

      var projectsWithoutLicenseHeaderFile = CheckForLicenseHeaderFileInProjects (projectsInSolution);

      if (projectsInSolution.Count == 1 && projectsWithoutLicenseHeaderFile.Count == 1)
      {
        //There is exactly one Project in the Solution and it has no Definition File 
        //--> Offer to add a new one and ask if they want to stop the update process to configure them
        if (MessageBoxHelper.DoYouWant(Resources.Question_AddNewLicenseHeaderDefinitionFileSingleProject))
        {
          var licenseHeader = LicenseHeader.AddLicenseHeaderDefinitionFile(projectsInSolution.First(), _licenseHeaderPage);

          if (!MessageBoxHelper.DoYouWant(Resources.Question_StopForConfiguringDefinitionFilesSingleFile))
            AddLicenseHeaderToProjects(projectsInSolution);
          else if (licenseHeader != null)
            licenseHeader.Open(Constants.vsViewKindCode).Activate();
        }
      }
      else if (projectsWithoutLicenseHeaderFile.Count == projectsInSolution.Count)
      {
        //There are multiple Projects in the Solution but none of them has a Definition File 
        //--> Offer to add new ones to everyone of them and ask if they want to stop the update process to configure them
        if (MessageBoxHelper.DoYouWant(Resources.Question_AddNewLicenseHeaderDefinitionFileMultipleProjects))
        {
          var newLicenseHeaders = AddNewLicenseHeaderDefinitionFilesToProjects(projectsWithoutLicenseHeaderFile, _licenseHeaderPage);
          
          if (!MessageBoxHelper.DoYouWant(Resources.Question_StopForConfiguringDefinitionFilesMultipleFiles))
            AddLicenseHeaderToProjects(projectsInSolution);
          else if (newLicenseHeaders.Count() <= Resources.Constant_MaxNumberOfProjectItemsWhereOpeningDefinitionFilesInEditor)
          {
            foreach (var licenseHeader in newLicenseHeaders)
            {
              licenseHeader.Open(Constants.vsViewKindCode).Activate();
            }            
          }
        }
        else
        {
          MessageBoxHelper.Information(Resources.Information_NoDefinitionFileStopUpdating);
        }
      }
      else if (projectsWithoutLicenseHeaderFile.Any())
      {
        //There are projects with and without Definition File --> Ask if we should add an existing License Header File to them and then add License Headers
        if (DefinitionFilesShouldBeAdded(projectsWithoutLicenseHeaderFile))
             new AddExistingLicenseHeaderDefinitionFileCommand().AddDefinitionFileToMultipleProjects(projectsWithoutLicenseHeaderFile);

        AddLicenseHeaderToProjects(projectsInSolution);
      }
      else
      {
        //There are no Projects without Definition File --> Add License Headers
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

      var errorResourceString = Resources.Error_MultipleProjectsNoLicenseHeaderFile;
      var projects = "";

      if (projectsWithoutLicenseHeaderFile.Count > Resources.Constant_MaxProjectsWithoutDefinitionFileShownInMessage)
      {
        projects = string.Join("\n", projectsWithoutLicenseHeaderFile
                         .Select(x => x.Name)
                         .Take(5)
                         .ToList());

        projects += "\n...";

      }
      else
      {
        projects = string.Join("\n", projectsWithoutLicenseHeaderFile
                         .Select(x => x.Name)
                         .ToList());
      }

      var message = string.Format (errorResourceString, projects).Replace (@"\n", "\n");
      
      
      return MessageBoxHelper.DoYouWant(message);
    }

    private IEnumerable<ProjectItem> AddNewLicenseHeaderDefinitionFilesToProjects(List<Project> projectsWithoutLicenseHeader,
      IDefaultLicenseHeaderPage page)
    {
      List<ProjectItem> newLicenseHeaders = new List<ProjectItem>();

      foreach (Project project in projectsWithoutLicenseHeader)
      {
        if (projectsWithoutLicenseHeader.Contains(project))
          newLicenseHeaders.Add(LicenseHeader.AddLicenseHeaderDefinitionFile(project, page));
      }

      return newLicenseHeaders;
    }

    private void AddLicenseHeaderToProjects (List<Project> projectsInSolution)
    {
      int progressCount = 1;
      int projectCount = projectsInSolution.Count();

      foreach (Project project in projectsInSolution)
      {
        _solutionUpdateViewModel.ProgressText = string.Format("Currently updating '{0}'. Updating {1}/{2} Projects.", project.Name, progressCount, projectCount);
        new AddLicenseHeaderToAllFilesCommand (_licenseReplacer).Execute(project);
        progressCount++;
      }

    }
  }
}