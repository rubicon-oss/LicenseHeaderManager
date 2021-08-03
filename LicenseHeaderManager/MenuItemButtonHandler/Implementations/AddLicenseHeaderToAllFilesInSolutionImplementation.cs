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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using LicenseHeaderManager.Headers;
using LicenseHeaderManager.Interfaces;
using LicenseHeaderManager.MenuItemCommands.Common;
using LicenseHeaderManager.MenuItemCommands.SolutionMenu;
using LicenseHeaderManager.UpdateViewModels;
using LicenseHeaderManager.Utils;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using Window = System.Windows.Window;

namespace LicenseHeaderManager.MenuItemButtonHandler.Implementations
{
  public class AddLicenseHeaderToAllFilesInSolutionImplementation : MenuItemButtonHandlerImplementation
  {
    private const string c_commandName = "Add LicenseHeader to all files in Solution";
    private const int c_maxProjectsWithoutDefinitionFileShownInMessage = 5;
    private readonly ILicenseHeaderExtension _licenseHeaderExtension;

    public AddLicenseHeaderToAllFilesInSolutionImplementation (ILicenseHeaderExtension licenseHeaderExtension)
    {
      _licenseHeaderExtension = licenseHeaderExtension;
    }

    public override string Description => c_commandName;

    public override async Task DoWorkAsync (CancellationToken cancellationToken, BaseUpdateViewModel viewModel, Solution solution, Window window)
    {
      if (solution == null)
        return;

      if (!(viewModel is SolutionUpdateViewModel updateViewModel))
        throw new ArgumentException ($"Argument {nameof(viewModel)} must be of type {nameof(SolutionUpdateViewModel)}");

      await _licenseHeaderExtension.JoinableTaskFactory.SwitchToMainThreadAsync();
      var solutionHeaderDefinitions = LicenseHeaderFinder.GetHeaderDefinitionForSolution (solution);

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
        // Every project is covered either by a solution or project level license header definition, go ahead and add them.
        await AddLicenseHeaderToProjectsAsync (cancellationToken, projectsInSolution, updateViewModel);
      }
      else
      {
        // Some projects are not covered by a header.

        var someProjectsHaveDefinition = projectsWithLicenseHeaderFile.Count > 0;
        if (someProjectsHaveDefinition)
        {
          // Some projects have a header. Ask the user if they want to add an existing header to the uncovered projects.
          if (await DefinitionFilesShouldBeAddedAsync (projectsWithoutLicenseHeaderFile, window))
            ExistingLicenseHeaderDefinitionFileAdder.AddDefinitionFileToMultipleProjects (projectsWithoutLicenseHeaderFile);

          await AddLicenseHeaderToProjectsAsync (cancellationToken, projectsInSolution, updateViewModel);
        }
        else
        {
          // No projects have definition. Ask the user if they want to add a solution level header definition.
          if (await MessageBoxHelper.AskYesNoAsync (window, Resources.Question_AddNewLicenseHeaderDefinitionForSolution.ReplaceNewLines()).ConfigureAwait (true))
          {
            AddNewSolutionLicenseHeaderDefinitionFileCommand.Instance.Invoke (solution);

            // They want to go ahead and apply without editing.
            if (!await MessageBoxHelper.AskYesNoAsync (window, Resources.Question_StopForConfiguringDefinitionFilesSingleFile).ConfigureAwait (true))
              await AddLicenseHeaderToProjectsAsync (cancellationToken, projectsInSolution, updateViewModel);
          }
        }
      }
    }

    public override Task DoWorkAsync (CancellationToken cancellationToken, BaseUpdateViewModel viewModel, Solution solution)
    {
      throw new NotSupportedException (UnsupportedOverload);
    }

    public override Task DoWorkAsync (CancellationToken cancellationToken, BaseUpdateViewModel viewModel)
    {
      throw new NotSupportedException (UnsupportedOverload);
    }

    private async Task<bool> DefinitionFilesShouldBeAddedAsync (ICollection<Project> projectsWithoutLicenseHeaderFile, Window window)
    {
      if (!projectsWithoutLicenseHeaderFile.Any()) return false;

      var errorResourceString = Resources.Error_MultipleProjectsNoLicenseHeaderFile;
      string projects;

      if (projectsWithoutLicenseHeaderFile.Count > c_maxProjectsWithoutDefinitionFileShownInMessage)
      {
        projects = string.Join (
            "\n",
            projectsWithoutLicenseHeaderFile.Select (
                x =>
                {
                  ThreadHelper.ThrowIfNotOnUIThread();
                  return x.Name;
                }).Take (c_maxProjectsWithoutDefinitionFileShownInMessage));
        projects += "\n...";
      }
      else
      {
        projects = string.Join (
            "\n",
            projectsWithoutLicenseHeaderFile.Select (
                x =>
                {
                  ThreadHelper.ThrowIfNotOnUIThread();
                  return x.Name;
                }));
      }

      var message = string.Format (errorResourceString, projects).ReplaceNewLines();
      return await MessageBoxHelper.AskYesNoAsync (window, message).ConfigureAwait (true);
    }

    private async Task AddLicenseHeaderToProjectsAsync (CancellationToken cancellationToken, ICollection<Project> projectsInSolution, SolutionUpdateViewModel viewModel)
    {
      viewModel.ProcessedProjectCount = 0;
      viewModel.ProjectCount = projectsInSolution.Count;
      var addAllLicenseHeadersCommand = new AddLicenseHeaderToAllFilesInProjectHelper (cancellationToken, _licenseHeaderExtension, viewModel);

      foreach (var project in projectsInSolution)
      {
        await addAllLicenseHeadersCommand.RemoveOrReplaceHeadersAsync (project);
        await IncrementProjectCountAsync (viewModel).ConfigureAwait (true);
      }
    }

    private async Task IncrementProjectCountAsync (BaseUpdateViewModel viewModel)
    {
      await _licenseHeaderExtension.JoinableTaskFactory.SwitchToMainThreadAsync();
      viewModel.ProcessedProjectCount++;
    }
  }
}
