//Sample license text.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using LicenseHeaderManager.Headers;
using LicenseHeaderManager.Utils;
using Microsoft.VisualStudio.Shell.Interop;

namespace LicenseHeaderManager.PackageCommands
{
  class RemoveLicenseHeaderFromAllProjectsCommand
  {
    private IVsStatusbar statusBar;
    private LicenseHeaderReplacer licenseReplacer;

    public RemoveLicenseHeaderFromAllProjectsCommand(IVsStatusbar statusBar, LicenseHeaderReplacer licenseReplacer)
    {
      this.statusBar = statusBar;
      this.licenseReplacer = licenseReplacer;
    }

    public void Execute(Solution solution)
    {
      var allSolutionProjectsSearcher = new AllSolutionProjectsSearcher();
      var projectsInSolution = allSolutionProjectsSearcher.GetAllProjects(solution);

      int progressCount = 1;
      int projectCount = projectsInSolution.Count;
      var removeAllLicenseHeadersCommand = new RemoveLicenseHeaderFromAllFilesCommand(licenseReplacer);
      
      foreach (Project project in projectsInSolution)
      {
        statusBar.SetText(string.Format(Resources.UpdateSolution, progressCount, projectCount));
        removeAllLicenseHeadersCommand.Execute(project);
        progressCount++;
      }

      statusBar.SetText(string.Empty);
    }
  }
}
