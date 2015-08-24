//Sample license text.

using System;
using System.Linq;
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

    public static bool IsLink (ProjectItem projectItem)
    {
      if (projectItem.Properties == null)
        return false;

      Property isLinkProperty;

      try
      {
        isLinkProperty = projectItem.Properties.Item("IsLink");
      }
      catch (ArgumentException e)
      {
        return false;
      }
      
      return isLinkProperty != null && (bool) isLinkProperty.Value;
    }
  }
}
