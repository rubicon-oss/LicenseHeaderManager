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
  public class LinkedFileHandler
  {
    public string Message { get; private set; }

    public LinkedFileHandler ()
    {
      Message = string.Empty;
    }

    public void Handle (LicenseHeaderReplacer licenseHeaderReplacer, ILinkedFileFilter linkedFileFilter)
    {
      foreach (ProjectItem projectItem in linkedFileFilter.ToBeProgressed)
      {
        var headers = LicenseHeaderFinder.GetHeaderRecursive (projectItem);
        licenseHeaderReplacer.RemoveOrReplaceHeader (projectItem, headers, true);
      }

      if (linkedFileFilter.NoLicenseHeaderFile.Any () || linkedFileFilter.NotInSolution.Any ())
      {
        List<ProjectItem> notProgressedItems =
          linkedFileFilter.NoLicenseHeaderFile.Concat (linkedFileFilter.NotInSolution).ToList ();

        List<string> notProgressedNames = notProgressedItems.Select(x => x.Name).ToList();

        Message +=
          string.Format (Resources.LinkedFileUpdateInformation, string.Join ("\n", notProgressedNames))
            .Replace (@"\n", "\n");
      }
    }
  }
}
