//Sample license text.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using LicenseHeaderManager.Headers;
using LicenseHeaderManager.ReturnObjects;
using LicenseHeaderManager.Utils;
using Microsoft.VisualStudio.Shell.Interop;

namespace LicenseHeaderManager.PackageCommands
{
  internal class AddLicenseHeaderToAllFilesCommand
  {
    private LicenseHeaderReplacer licenseReplacer;

    public AddLicenseHeaderToAllFilesCommand(LicenseHeaderReplacer licenseReplacer)
    {
      this.licenseReplacer = licenseReplacer;
    }

    public AddLicenseHeaderToAllFilesReturn Execute(object projectOrProjectItem)
    {
      var project = projectOrProjectItem as Project;
      var projectItem = projectOrProjectItem as ProjectItem;
      
      int countSubLicenseHeadersFound = 0;
      IDictionary<string, string[]> headers = null;
      List<ProjectItem> linkedItems = new List<ProjectItem> ();

      if (project != null || projectItem != null)
      {

        licenseReplacer.ResetExtensionsWithInvalidHeaders();
        ProjectItems projectItems;

        if (project != null)
        {
          headers = LicenseHeaderFinder.GetHeader(project);
          projectItems = project.ProjectItems;
        }
        else
        {
          headers = LicenseHeaderFinder.GetHeaderRecursive(projectItem);
          projectItems = projectItem.ProjectItems;
        }
       
        foreach (ProjectItem item in projectItems)
        {
          if (ProjectItemInspection.IsLink(item))
            linkedItems.Add(item);
          else
            countSubLicenseHeadersFound = licenseReplacer.RemoveOrReplaceHeaderRecursive(item, headers);
        }
      }

      return new AddLicenseHeaderToAllFilesReturn(countSubLicenseHeadersFound, headers == null, linkedItems);
    }
  }
}
