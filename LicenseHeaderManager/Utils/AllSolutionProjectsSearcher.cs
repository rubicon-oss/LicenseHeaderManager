//Sample license text.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using EnvDTE80;

namespace LicenseHeaderManager.Utils
{
  class AllSolutionProjectsSearcher
  {

    public List<Project> GetAllProjects(Solution solution)
    {
      List<Project> projectList = new List<Project>();
      PopulateProjectsList(solution, projectList);

      return projectList;
    }

    private void PopulateProjectsList (Solution solution, List<Project> projectList)
    {
      foreach (Project project in solution)
      {
        if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
          projectList.AddRange (GetSolutionFolderProjects (project));
        else if(IsLoaded(project)) 
          projectList.Add (project);
      }
    }

    private bool IsLoaded(Project project)
    {
      return string.Compare(EnvDTE.Constants.vsProjectKindUnmodeled, project.Kind, StringComparison.OrdinalIgnoreCase) != 0;
    }

    private IEnumerable<Project> GetSolutionFolderProjects (Project project)
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
        else if (IsLoaded(project))
        {
          list.Add (subProject);
        }
      }
      return list;
    }
  }
}
