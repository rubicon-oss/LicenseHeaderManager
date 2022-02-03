/* Copyright (c) rubicon IT GmbH
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
 */

using System;
using System.Linq;
using System.Reflection;
using EnvDTE;
using log4net;
using Microsoft.VisualStudio.Shell;

namespace LicenseHeaderManager.Utils
{
  public static class ProjectItemParentFinder
  {
    private static readonly ILog s_log = LogManager.GetLogger (MethodBase.GetCurrentMethod().DeclaringType);

    public static object GetProjectItemParent (ProjectItem projectItem)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      return IsEndlessRecursion (projectItem) ? GetProjectItemParentViaReflection (projectItem) : GetParent (projectItem);
    }

    //Some custom project types return the current item instead of the parent which lead to an endless recursion.
    private static bool IsEndlessRecursion (ProjectItem projectItem)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      var projectItemPath = GetPath (projectItem);

      //ProjectItemParent is no ProjectItem --> 100% no endless recursion
      if (!(GetParent (projectItem) is ProjectItem projectItemParent))
        return false;

      var parentProjectItemPath = GetPath (projectItemParent);

      //If both paths are empty it is impossible to say if we are in an endless recursion
      if (string.IsNullOrEmpty (projectItemPath) && string.IsNullOrEmpty (parentProjectItemPath))
        return false;

      //If the Paths are the same, we are in an endless recursion (projectItem.Parent == projectItem)
      return projectItemPath == parentProjectItemPath;
    }

    private static object GetProjectItemParentViaReflection (ProjectItem projectItem)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      try
      {
        if (TryGetProjectItemParentViaReflection (projectItem, out var projectItemParentViaReflection))
          return projectItemParentViaReflection;
      }
      catch (Exception ex)
      {
        //We catch everything as a multitude of Exceptions can be thrown if the projectItem.Object is not structured as we assume
        s_log.Error (
            $"Exception got thrown when searching for the LicenseHeaderFile on ProjectItem of Type '{projectItem.GetType().FullName}' with the name '{projectItem.Name}'. ",
            ex);
      }

      s_log.Info (
          $"Could not find .licenseheaderfile for {projectItem.Name}." + "This is probably due to a custom project type."
                                                                       + "Please report the issue and include the type of your project in the description.");
      return null;
    }

    private static bool TryGetProjectItemParentViaReflection (ProjectItem projectItem, out object projectItemParentViaReflection)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      if (projectItem.Object != null)
      {
        var parentProperty = projectItem.Object.GetType().GetProperty ("Parent").GetValue (projectItem.Object, null);
        var parentUrl = parentProperty.GetType().GetProperty ("Url").GetValue (parentProperty, null) as string;
        var projectItemParent = projectItem.DTE.Solution.FindProjectItem (parentUrl);

        //If the ProjectItemParent could not be found by "FindProjectItem" this means we are probably a Folder at TopLevel 
        //and only the ContainingProject is above us.
        if (projectItemParent == null)
        {
          var containingProject = projectItem.ContainingProject;

          if (containingProject.FullName != parentUrl)
          {
            projectItemParentViaReflection = null;
            return false;
          }

          projectItemParentViaReflection = containingProject;
          return true;
        }

        projectItemParentViaReflection = projectItemParent;
        return true;
      }

      projectItemParentViaReflection = null;
      return false;
    }

    private static string GetPath (ProjectItem projectItem)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      var fullPathProperty = projectItem.Properties?.Cast<Property>().FirstOrDefault (
          property =>
          {
            ThreadHelper.ThrowIfNotOnUIThread();
            return property.Name == "FullPath";
          });
      return fullPathProperty?.Value != null ? fullPathProperty.Value.ToString() : string.Empty;
    }

    private static object GetParent (ProjectItem projectItem)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      try
      {
        return projectItem.Collection.Parent;
      }
      catch (Exception ex)
      {
        s_log.Error ($"Could not determine parent of project item {projectItem.FileNames[1]}. Assuming it does not have parent.", ex);
        return null;
      }
    }
  }
}
