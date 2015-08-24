//Sample license text.
using EnvDTE;
using LicenseHeaderManager.Headers;

namespace LicenseHeaderManager.Utils
{
  public static class ProjectItemInspection
  {
    public static bool IsPhysicalFile (ProjectItem projectItem)
    {
      return (projectItem.Kind == Constants.vsProjectItemKindPhysicalFile ||
              projectItem.Kind == "{" + GuidList.guidItemTypePhysicalFile + "}");
    }

    public static bool IsLicenseHeader(ProjectItem projectItem)
    {
      return projectItem.Name.Contains(LicenseHeader.Extension);
    }
  }
}
