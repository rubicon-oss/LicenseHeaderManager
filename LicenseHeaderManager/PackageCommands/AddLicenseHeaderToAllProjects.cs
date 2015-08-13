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
  class AddLicenseHeaderToAllProjects
  {
    public static void AddLicenseHeaderToAllProjectsCommand(Solution solution, LicenseHeadersPackage package)
    {
      List<Project> projectsInSolution = new List<Project>();

      PopulateProjectsList(solution, projectsInSolution);
 
      List<Project> projectsWithoutLicenseHeader = CheckForLicenseHeaderFileInProjects (projectsInSolution);

     
      if (projectsWithoutLicenseHeader.Count > 0)
      {
        if (AskIfDefinitionFilesShouldBeAdded(projectsWithoutLicenseHeader))
        {
          AddMissingLicenseHeaderFiles(projectsWithoutLicenseHeader, package.DefaultLicenseHeaderPage);
       
          foreach (Project project in projectsInSolution)
          {          
              package.AddLicenseHeaderToAllFiles(project);
          }
          return;
        }
      }

      foreach (Project project in projectsInSolution)
      {
        if (!projectsWithoutLicenseHeader.Contains (project))
        {
          package.AddLicenseHeaderToAllFiles (project);
        }
      }
    }

    private static void PopulateProjectsList(Solution solution, List<Project> projectList)
    {
      foreach (Project project in solution)
      {
        if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
          projectList.AddRange(GetSolutionFolderProjects(project));
        else
          projectList.Add(project);
      }
    }

    private static IEnumerable<Project> GetSolutionFolderProjects(Project project)
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


    private static List<Project> CheckForLicenseHeaderFileInProjects (List<Project> projects)
    {
      return (projects
        .Where(project => LicenseHeaderFinder.GetHeader(project) == null).ToList());
    }

    private static bool AskIfDefinitionFilesShouldBeAdded (List<Project> projectsWithoutLicenseHeader)
    {
      var errorResource = projectsWithoutLicenseHeader.Count == 1
          ? Resources.Error_NoHeaderDefinition
          : Resources.Error_MulitpleProjectsNoLicenseHeaderFile;

      var message = string.Format (errorResource, string.Join ("\n", projectsWithoutLicenseHeader.Select(x => x.Name).ToList())).Replace (@"\n", "\n");

      return MessageBox.Show (message, Resources.LicenseHeaderManagerName, MessageBoxButton.YesNo,
        MessageBoxImage.Information,
        MessageBoxResult.No) == MessageBoxResult.Yes;
    }

    private static void AddMissingLicenseHeaderFiles (List<Project> projectsWithoutLicenseHeader, IDefaultLicenseHeaderPage page)
    {
        foreach (Project project in projectsWithoutLicenseHeader)
        {
          if (projectsWithoutLicenseHeader.Contains (project))
            LicenseHeader.AddLicenseHeaderDefinitionFile (project, page, false);
        }   
    }
  }
}
