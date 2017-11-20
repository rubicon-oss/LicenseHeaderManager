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
using System.Linq;
using EnvDTE;
using LicenseHeaderManager.Headers;
using LicenseHeaderManager.Interfaces;
using LicenseHeaderManager.SolutionUpdateViewModels;
using LicenseHeaderManager.Utils;

namespace LicenseHeaderManager.PackageCommands
{
  public class AddLicenseHeaderToAllFilesInSolutionCommand : ISolutionLevelCommand
  {
    private const string c_commandName = "Add LicenseHeader to all files in Solution";
    private const int MaxProjectsWithoutDefinitionFileShownInMessage = 5;

    private readonly LicenseHeaderReplacer _licenseReplacer;
    private readonly SolutionUpdateViewModel _solutionUpdateViewModel;

    public AddLicenseHeaderToAllFilesInSolutionCommand (LicenseHeaderReplacer licenseReplacer, SolutionUpdateViewModel solutionUpdateViewModel)
    {
      _licenseReplacer = licenseReplacer;
      _solutionUpdateViewModel = solutionUpdateViewModel;
    }

    public string GetCommandName ()
    {
      return c_commandName;
    }

    public void Execute (Solution solution)
    {
      if (solution == null) return;

      IDictionary<string, string[]> solutionHeaderDefinitions = LicenseHeaderFinder.GetHeaderDefinitionForSolution (solution);

      var allSolutionProjectsSearcher = new AllSolutionProjectsSearcher();
      var projectsInSolution = allSolutionProjectsSearcher.GetAllProjects (solution);

      var projectsWithoutLicenseHeaderFile = projectsInSolution
          .Where (project => LicenseHeaderFinder.GetHeaderDefinitionForProjectWithoutFallback (project) == null)
          .ToList();

      var projectsWithLicenseHeaderFile = projectsInSolution
          .Where (project => LicenseHeaderFinder.GetHeaderDefinitionForProjectWithoutFallback (project) != null)
          .ToList();

      if (solutionHeaderDefinitions != null || !projectsWithoutLicenseHeaderFile.Any())
      {
        // Every project is covered either by a solution or project level license header defintion, go ahead and add them.
        AddLicenseHeaderToProjects (projectsInSolution);
      }
      else
      {
        // Some projects are not covered by a header.

        bool someProjectsHaveDefinition = projectsWithLicenseHeaderFile.Count > 0;
        if (someProjectsHaveDefinition)
        {
          // Some projects have a header. Ask the user if they want to add an existing header to the uncovered projects.
          if (DefinitionFilesShouldBeAdded (projectsWithoutLicenseHeaderFile))
            new AddExistingLicenseHeaderDefinitionFileToProjectCommand().AddDefinitionFileToMultipleProjects (projectsWithoutLicenseHeaderFile);

          AddLicenseHeaderToProjects (projectsInSolution);
        }
        else
        {
          // No projects have definition. Ask the user if they want to add a solution level header definition.
          if (MessageBoxHelper.DoYouWant (Resources.Question_AddNewLicenseHeaderDefinitionForSolution))
          {
            AddNewSolutionLicenseHeaderDefinitionFileCommand.Instance.Execute (solution);

            if (!MessageBoxHelper.DoYouWant (Resources.Question_StopForConfiguringDefinitionFilesSingleFile))
            {
              // They want to go ahead and apply without editing.
              AddLicenseHeaderToProjects (projectsInSolution);
            }
          }
        }
      }
    }

    private bool DefinitionFilesShouldBeAdded (List<Project> projectsWithoutLicenseHeaderFile)
    {
      if (!projectsWithoutLicenseHeaderFile.Any()) return false;

      var errorResourceString = Resources.Error_MultipleProjectsNoLicenseHeaderFile;
      string projects;

      if (projectsWithoutLicenseHeaderFile.Count > MaxProjectsWithoutDefinitionFileShownInMessage)
      {
        projects = string.Join (
            "\n",
            projectsWithoutLicenseHeaderFile
                .Select (x => x.Name)
                .Take (5)
                .ToList());

        projects += "\n...";
      }
      else
      {
        projects = string.Join (
            "\n",
            projectsWithoutLicenseHeaderFile
                .Select (x => x.Name)
                .ToList());
      }

      var message = string.Format (errorResourceString, projects).Replace (@"\n", "\n");


      return MessageBoxHelper.DoYouWant (message);
    }

    private void AddLicenseHeaderToProjects (List<Project> projectsInSolution)
    {
      int progressCount = 1;
      int projectCount = projectsInSolution.Count;

      foreach (Project project in projectsInSolution)
      {
        _solutionUpdateViewModel.ProgressText = string.Format (
            "Currently updating '{0}'. Updating {1}/{2} Projects.",
            project.Name,
            progressCount,
            projectCount);
        new AddLicenseHeaderToAllFilesInProjectCommand (_licenseReplacer).Execute (project);
        progressCount++;
      }
    }
  }
}