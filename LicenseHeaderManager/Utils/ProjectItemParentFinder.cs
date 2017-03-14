#region copyright
// Copyright (c) rubicon IT GmbH

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
#endregion

using System;
using EnvDTE;

namespace LicenseHeaderManager.Utils
{
  public static class ProjectItemParentFinder
  {
    public static object GetProjectItemParent (ProjectItem projectItem)
    {
      if (IsEndlessRecursion (projectItem))
        return GetProjectItemParentViaReflection (projectItem);

      return GetParent (projectItem);
    }

    //Some custom project types return the current item instead of the parent which lead to an endless recursion.
    private static bool IsEndlessRecursion (ProjectItem projectItem)
    {
      var projectItemPath = GetPath (projectItem);
      var projectItemParent = GetParent (projectItem) as ProjectItem;

      //ProjectItemParent is no ProjectItem --> 100% no endless recursion
      if (projectItemParent == null)
        return false;

      var parentProjectItemPath = GetPath (projectItemParent);

      //If both paths are empty it is impossible to say if we are in an endless recursion
      if (String.IsNullOrEmpty (projectItemPath) && String.IsNullOrEmpty (parentProjectItemPath))
        return false;

      //If the Paths are the same, we are in an endless recursion (projectItem.Parent == projectItem)
      return projectItemPath == parentProjectItemPath;
    }

    private static object GetProjectItemParentViaReflection (ProjectItem projectItem)
    {
      try
      {
        object projectItemParentViaReflection;

        if (TryGetProjectItemParentViaReflection (projectItem, out projectItemParentViaReflection))
          return projectItemParentViaReflection;
      }
      catch (Exception exception)
      {
        //We catch everything as a multitude of Exceptions can be thrown if the projectItem.Object is not structured as we assume
        OutputWindowHandler.WriteMessage (
            string.Format (
                "Exception got thrown when searching for the LicenseHeaderFile on ProjectItem of Type '{0}' with the name '{1}'. " + "Exception: {2}",
                projectItem.GetType().FullName,
                projectItem.Name,
                exception));
      }

      OutputWindowHandler.WriteMessage (
          string.Format (
              "Could not find .licenseheaderfile for {0}." +
              "This is probably due to a custom project type." +
              "Please report the issue and include the type of your project in the description.",
              projectItem.Name));

      return null;
    }

    private static bool TryGetProjectItemParentViaReflection (ProjectItem projectItem, out object projectItemParentViaReflection)
    {
      if (projectItem.Object != null)
      {
        var parentProperty = projectItem.Object.GetType().GetProperty ("Parent").GetValue (projectItem.Object, null);
        var parentUrl = parentProperty.GetType().GetProperty ("Url").GetValue (parentProperty, null) as string;
        ProjectItem projectItemParent = projectItem.DTE.Solution.FindProjectItem (parentUrl);

        //If the ProjectItemParent could not be found by "FindProjectItem" this means we are probably a Folder at TopLevel 
        //and only the ContainingProject is above us.
        if (projectItemParent == null)
        {
          Project containingProject = projectItem.ContainingProject;
          
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
      var fullPathProperty = projectItem.Properties.Item ("FullPath");

      if (fullPathProperty != null && fullPathProperty.Value != null)
        return fullPathProperty.Value.ToString();
      else
        return String.Empty;
    }

    private static object GetParent (ProjectItem projectItem)
    {
      return projectItem.Collection.Parent;
    }
  }
}