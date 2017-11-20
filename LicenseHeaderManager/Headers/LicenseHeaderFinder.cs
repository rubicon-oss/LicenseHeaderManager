﻿#region copyright
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
    /// Gets the license header file definition to use on the given project item.
    /// </summary>
    /// <param name="projectItem"></param>
    /// <returns>A dictionary, which contains the extensions and the corresponding lines</returns>
    public static IDictionary<string, string[]> GetHeaderDefinitionForItem(ProjectItem projectItem)
    {
      // First search for the definition within the project
      IDictionary<string, string[]> headerDefinition = SearchWithinProjectGetHeaderDefinitionForItem(projectItem);
      if (headerDefinition != null)
      {
        return headerDefinition;
      }

      // Next look for the solution-level definition
      return GetHeaderDefinitionForSolution(projectItem.DTE.Solution);
    }

    /// <summary>
    /// Lookup the license header file within the given projectItems.
    /// </summary>
    /// <param name="projectItems"></param>
    /// <returns>A dictionary, which contains the extensions and the corresponding lines</returns>
    public static IDictionary<string, string[]> SearchItemsDirectlyGetHeaderDefinition(ProjectItems projectItems)
    {
      // Check for License-file within this level
      var headerFileName = SearchItemsDirectlyGetHeaderDefinitionFileName(projectItems);
      if (!string.IsNullOrEmpty(headerFileName))
        return LoadHeaderDefinition(headerFileName); // Found a License header file on this level
      return null;
    }

    /// <summary>
    /// Returns the License header file for the specified project, or the solution header file if it can't be found.
    /// </summary>
    /// <param name="project">The project which is scanned for the License header file</param>
    /// <returns>A dictionary, which contains the extensions and the corresponding lines</returns>
    public static IDictionary<string, string[]> GetHeaderDefinitionForProjectWithFallback(Project project)
    {
      // First look for a header definition for the project
      var definition = GetHeaderDefinitionForProjectWithoutFallback(project);

      if (definition != null)
      {
        return definition;
      }

      // Next look for the solution-level definition
      return GetHeaderDefinitionForSolution(project.DTE.Solution);
    }

    /// <summary>
    /// Returns the License header file for the specified project. Does not fall back to the solution header file.
    /// </summary>
    /// <param name="project">The project which is scanned for the License header file</param>
    /// <returns>A dictionary, which contains the extensions and the corresponding lines</returns>
    public static IDictionary<string, string[]> GetHeaderDefinitionForProjectWithoutFallback(Project project)
    {
      var headerFile = SearchItemsDirectlyGetHeaderDefinitionFileName(project.ProjectItems);
      return LoadHeaderDefinition(headerFile);
    }

    /// <summary>
    /// Returns the License header definition file for the specified solution.
    /// </summary>
    /// <param name="solution">The solution to look in.</param>
    /// <returns>A dictionary, which contains the extensions and the corresponding lines</returns>
    public static IDictionary<string, string[]> GetHeaderDefinitionForSolution(Solution solution)
    {
      string solutionDirectory = Path.GetDirectoryName(solution.FullName);
      string solutionFileName = Path.GetFileName(solution.FullName);

      string solutionHeaderFilePath = Path.Combine(solutionDirectory, solutionFileName + LicenseHeader.Extension);
      if (File.Exists(solutionHeaderFilePath))
      {
        return LoadHeaderDefinition(solutionHeaderFilePath);
      }

      return null;
    }

    #region Helper Methods

    /// <summary>
    /// Returns the License header file that is directly attached to the project.
    /// </summary>
    /// <param name="project">The project which is scanned for the License header file</param>
    /// <returns>A dictionary, which contains the extensions and the corresponding lines</returns>
    private static IDictionary<string, string[]> GetHeaderDefinitionDirectlyOnProject(Project project)
    {
      var headerFile = SearchItemsDirectlyGetHeaderDefinitionFileName(project.ProjectItems);
      var definition = LoadHeaderDefinition(headerFile);
      return definition;
    }

    /// <summary>
    /// Lookup the license header file within a project , if there is none on this level, moving up to next level.
    /// </summary>
    /// <param name="projectItem"></param>
    /// <returns>A dictionary, which contains the extensions and the corresponding lines</returns>
    private static IDictionary<string, string[]> SearchWithinProjectGetHeaderDefinitionForItem(ProjectItem projectItem)
    {
      //Check for License-file within this level
      var headerFile = SearchItemsDirectlyGetHeaderDefinitionFileName(projectItem.ProjectItems);
      if (!string.IsNullOrEmpty(headerFile))
        return LoadHeaderDefinition(headerFile); //Found a License header file on this level

      var projectItemParent = ProjectItemParentFinder.GetProjectItemParent(projectItem);

      //Lookup in the parent --> Go Up!
      return SearchWithinProjectGetHeaderDefinitionForProjectOrItem(projectItemParent);
    }

    /// <summary>
    /// Lookup the license header file within a project or a projectitem, if there is none on this level, moving up to next level.
    /// </summary>
    /// <param name="projectOrItem">An oject which is either is a project or a projectitem</param>
    /// <returns>A dictionary, which contains the extensions and the corresponding lines</returns>
    private static IDictionary<string, string[]> SearchWithinProjectGetHeaderDefinitionForProjectOrItem(object projectOrItem)
    {
      var parentAsProject = projectOrItem as Project;
      if (parentAsProject != null)
        return GetHeaderDefinitionDirectlyOnProject(parentAsProject); //We are on the top --> load project License file if it exists

      var parentAsProjectItem = projectOrItem as ProjectItem;
      if (parentAsProjectItem != null)
        return SearchWithinProjectGetHeaderDefinitionForItem(parentAsProjectItem); //Lookup in the item above

      return null;
    }

    private static Dictionary<string, string[]> LoadHeaderDefinition (string headerFilePath)
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

        if (wholeFile.EndsWith(NewLineConst.CR) || wholeFile.EndsWith(NewLineConst.LF))
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

    private static string SearchItemsDirectlyGetHeaderDefinitionFileName(ProjectItems projectItems)
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

          if (fileName != null && Path.GetExtension(fileName).ToLowerInvariant() == LicenseHeader.Extension)
            return fileName;
        }
      }

      return string.Empty;
    }
    #endregion

  }
}