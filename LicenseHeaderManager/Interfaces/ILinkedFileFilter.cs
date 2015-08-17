//Sample license text.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using EnvDTE;

namespace LicenseHeaderManager.Interfaces
{
  public interface ILinkedFileFilter
  {
    void Filter(List<ProjectItem> projectItems);

    List<ProjectItem> ToBeProgressed { get; }
    List<ProjectItem> NoLicenseHeaderFile { get; }
    List<ProjectItem> NotInSolution { get; }
  }
}
