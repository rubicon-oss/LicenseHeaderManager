//Sample license text.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using EnvDTE;
using EnvDTE80;
using LicenseHeaderManager.Headers;
using LicenseHeaderManager.Options;

namespace LicenseHeaderManager.PackageCommands
{
  public class AddLicenseHeaderToAllProjectsCommand
  {
    private LicenseHeadersPackage package;

    public AddLicenseHeaderToAllProjectsCommand(LicenseHeadersPackage package)
    {
      this.package = package;
    }

    public void Execute(Solution solution)
    {
      var projectsInSolution = new List<Project>();

      PopulateProjectsList(solution, projectsInSolution);
 
      var projectsWithoutLicenseHeaderFile = CheckForLicenseHeaderFileInProjects (projectsInSolution);


      if (DefinitionFilesShouldBeAdded(projectsWithoutLicenseHeaderFile))
      {
        AddMissingLicenseHeaderFiles(projectsWithoutLicenseHeaderFile, package.DefaultLicenseHeaderPage);

        AddLicenseHeaderToProjects(projectsInSolution);
      }
      else
      {
        AddLicenseHeaderToProjects(projectsInSolution
          .Except(projectsWithoutLicenseHeaderFile)
          .ToList()); 
      }
    }

    private void PopulateProjectsList(Solution solution, List<Project> projectList)
    {
      foreach (Project project in solution)
      {
        if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
          projectList.AddRange(GetSolutionFolderProjects(project));
        else
          projectList.Add(project);
      }
    }

    private IEnumerable<Project> GetSolutionFolderProjects(Project project)
    {
      List<Project> list = new List<Project> ();
      for (var i = 1; i <= project.ProjectItems.Count; i++)
      {
        var subProject = project.ProjectItems.Item (i).SubProject;
        if (subProject == null)
        {
          continue;
        }

        // If this is another solution folder, do a recursive call, otherwise add
        if (subProject.Kind == ProjectKinds.vsProjectKindSolutionFolder)
        {
          list.AddRange (GetSolutionFolderProjects (subProject));
        }
        else
        {
          list.Add (subProject);
        }
      }
      return list;
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
      foreach (Project project in projectsInSolution)
      {
        package.AddLicenseHeaderToAllFiles (project);
      }
    }
  }
}
