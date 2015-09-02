//Sample license text.

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
    private LicenseHeaderReplacer licenseReplacer;
    private AddLicenseHeaderToAllFilesCommand addLicenseHeaderToAllFilesCommand;
    private IVsStatusbar statusBar;
    private IDefaultLicenseHeaderPage licenseHeaderPage;

    public AddLicenseHeaderToAllProjectsCommand(LicenseHeaderReplacer licenseReplacer, IVsStatusbar statusBar, IDefaultLicenseHeaderPage licenseHeaderPage)
    {
      this.statusBar = statusBar;
      this.licenseReplacer = licenseReplacer;
      this.licenseHeaderPage = licenseHeaderPage;
    
      addLicenseHeaderToAllFilesCommand = new AddLicenseHeaderToAllFilesCommand(licenseReplacer);
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
        foreach (Project project in projectsWithoutLicenseHeader)
        {
          if (projectsWithoutLicenseHeader.Contains (project))
            LicenseHeader.AddLicenseHeaderDefinitionFile (project, page, false);
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
  }
}