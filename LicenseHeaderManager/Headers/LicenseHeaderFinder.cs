#region copyright
// Copyright (c) 2011 rubicon IT GmbH

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnvDTE;
using LicenseHeaderManager.Utils;

namespace LicenseHeaderManager.Headers
{
  /// <summary>
  /// Class for finding the nearest license header file for a ProjectItem or a Project
  /// </summary>
  public class LicenseHeaderFinder
  {
    /// <summary>
    /// Lookup the license header file within a project or a projectitem, if there is no onw on this level, moving up to next level.
    /// </summary>
    /// <param name="projectOrItem">An oject which is either is a project or a projectitem</param>
    /// <returns>A dictionary, which contains the extensions and the corresponding lines</returns>
    public static IDictionary<string, string[]> GetHeaderRecursive (object projectOrItem)
    {
      var parentAsProject = projectOrItem as Project;
      if (parentAsProject != null)
        return GetHeader (parentAsProject); //We are on the top --> load project License file if it exists

      var parentAsProjectItem = projectOrItem as ProjectItem;
      if (parentAsProjectItem != null)
        return GetHeaderRecursive (parentAsProjectItem); //Lookup in the item above

      return null;
    }

    /// <summary>
    /// Lookup the license header file within a project , if there is no onw on this level, moving up to next level.
    /// </summary>
    /// <param name="projectItem"></param>
    /// <returns>A dictionary, which contains the extensions and the corresponding lines</returns>
    public static IDictionary<string, string[]> GetHeaderRecursive (ProjectItem projectItem)
    {
      //Check for License-file within this level
      var headerFile = GetLicenseHeaderDefinitions (projectItem.ProjectItems);
      if (!string.IsNullOrEmpty(headerFile))
        return LoadLicenseHeaderDefinition (headerFile); //Found a License header file on this level
      
      var projectItemParent = GetProjectItemParent(projectItem);

      //Lookup in the parent --> Go Up!
      return GetHeaderRecursive (projectItemParent);
    }

    private static object GetProjectItemParent (ProjectItem projectItem)
    {
      object projectItemParent = null;

      if (IsEndlessRecursion (projectItem))
      {
        projectItemParent = TryGetProjectIteamParentViaReflection (projectItem);

        if (projectItemParent == null)
        {
          OutputWindowHandler.WriteMessage ($"Could not find LicenseHeader for {projectItem.Name}." +
                                           "This is probably due to a custom project type." +
                                           "Please report the issue and include the type of your project in the description.");
        }
      }
      else
      {
        projectItemParent = projectItem.Collection.Parent;
      }

      return projectItemParent;
    }

    private static object TryGetProjectIteamParentViaReflection (ProjectItem projectItem)
    {
      try
      {
        if (projectItem.Object != null)
        {
          var parentProperty = projectItem.Object.GetType().GetProperty ("Parent").GetValue (projectItem.Object, null);
          var parentUrl = parentProperty.GetType().GetProperty ("Url").GetValue (parentProperty, null) as string;
          object projectItemParent = projectItem.DTE.Solution.FindProjectItem (parentUrl);

          //If the ProjectItemParent could not be found by "FindProjectItem" this means we are a Folder at TopLevel and only the ContainingProject is above us
          if (projectItemParent == null)
          {
            projectItemParent = projectItem.ContainingProject;
          }

          return projectItemParent;
        }
      }
      catch (Exception exception)
      {
        //We catch everything as a multitude of Exceptions can be thrown if the projectItem.Object is not structured as we assume
        OutputWindowHandler.WriteMessage (
            string.Format (
                "Exception got thrown when searching for the LicenseHeaderFile on FolderItem of Type '{0}' with the name '{1}'. Exception: {2}",
                projectItem.GetType().FullName,
                projectItem.Name,
                exception));
      }

      return null;
    }

    private static bool IsEndlessRecursion (ProjectItem projectItem)
    {
      var projectItemPath = GetPath (projectItem);
      var projectItemParent = projectItem.Collection.Parent as ProjectItem;

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

    private static string GetPath (ProjectItem projectItem)
    {
      var fullPathProperty = projectItem.Properties.Item ("FullPath");

      return fullPathProperty?.Value.ToString() ?? String.Empty;
    }

    /// <summary>
    /// Lookup the license header file within the given projectItems.
    /// </summary>
    /// <param name="projectItems"></param>
    /// <returns>A dictionary, which contains the extensions and the corresponding lines</returns>
    public static IDictionary<string, string[]> GetHeader (ProjectItems projectItems)
    {
      //Check for License-file within this level
      var headerFile = GetLicenseHeaderDefinitions (projectItems);
      if (!string.IsNullOrEmpty (headerFile))
        return LoadLicenseHeaderDefinition (headerFile); //Found a License header file on this level
      return null;
    }

    /// <summary>
    /// Returns the License header file for the specified project
    /// </summary>
    /// <param name="project">The project which is scanned for the License header file</param>
    /// <returns>A dictionary, which contains the extensions and the corresponding lines</returns>
    public static IDictionary<string, string[]> GetHeader (Project project)
    {
      var headerFile = GetLicenseHeaderDefinitions (project.ProjectItems);
      var definition = LoadLicenseHeaderDefinition (headerFile);
      return definition;
    }

    #region Helper Methods
    
    private static Dictionary<string, string[]> LoadLicenseHeaderDefinition (string headerFilePath)
    {
      if (string.IsNullOrEmpty (headerFilePath))
        return null;
      var headers = new Dictionary<string, string[]> ();
      AddHeaders (headers, headerFilePath);
      return headers;
    }

    private static void AddHeaders (IDictionary<string, string[]> headers, string headerFilePath)
    {
      IEnumerable<string> extensions = null;
      IList<string> header = new List<string> ();

      var wholeFile = File.ReadAllText(headerFilePath);

      using (var streamreader = new StreamReader(headerFilePath, true))
      {
        while (!streamreader.EndOfStream)
        {
          var line = streamreader.ReadLine ();
          if (line.StartsWith (LicenseHeader.Keyword))
          {
            if (extensions != null)
            {
              var array = header.ToArray ();
              foreach (var extension in extensions)
                headers[extension] = array;
            }
            extensions = line.Substring (LicenseHeader.Keyword.Length).Split (new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select (LicenseHeader.AddDot);
            header.Clear ();
          }
          else
            header.Add (line);
        }

        if (wholeFile.EndsWith(NewLineConst.CR) || wholeFile.EndsWith(NewLineConst.CRLF) || wholeFile.EndsWith(NewLineConst.LF))
        {
          header.Add(string.Empty);
        }
      }
      
      if (extensions != null)
      {
        var array = header.ToArray ();
        foreach (var extension in extensions)
          headers[extension] = array;
      }
    }

    private static string GetLicenseHeaderDefinitions (ProjectItems projectItems)
    {
      if (projectItems == null)
        return null;

      foreach (ProjectItem item in projectItems)
      {
        if (item.FileCount == 1)
        {
          string fileName = null;
          
          try
          {
            fileName = item.FileNames[0];
          }
          catch (Exception) 
          {
          }

          if (fileName != null && Path.GetExtension (fileName).ToLower () == LicenseHeader.Extension)
            return fileName;
        }
      }

      return string.Empty;
    }
    #endregion

  }
}