//Sample license text.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using LicenseHeaderManager.Headers;
using LicenseHeaderManager.Interfaces;

namespace LicenseHeaderManager.Utils
{
  public class LinkedFileFilter : ILinkedFileFilter
  {
    private Solution solution;

    public List<ProjectItem> ToBeProgressed { get; private set; }
    public List<ProjectItem> NoLicenseHeaderFile { get; private set; }
    public List<ProjectItem> NotInSolution { get; private set; }

    public LinkedFileFilter (Solution solution)
    {
      this.solution = solution;
    
      ToBeProgressed = new List<ProjectItem> ();
      NoLicenseHeaderFile = new List<ProjectItem> ();
      NotInSolution = new List<ProjectItem> ();  
    }


    public void Filter (List<ProjectItem> projectItems)
    {
      foreach (ProjectItem projectItem in projectItems)
      {
        ProjectItem foundProjectItem = solution.FindProjectItem(projectItem.Name);
        if (foundProjectItem == null)
          NotInSolution.Add(projectItem);
        else
          CheckForLicenseHeaderFile(foundProjectItem);
      }
    }

    private void CheckForLicenseHeaderFile(ProjectItem projectItem)
    {
      var headers = LicenseHeaderFinder.GetHeaderRecursive(projectItem);
      if (headers == null)
        NoLicenseHeaderFile.Add(projectItem);
      else
        ToBeProgressed.Add(projectItem);
    }
  }
}
