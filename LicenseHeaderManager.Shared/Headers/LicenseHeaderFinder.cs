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
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using EnvDTE;
using LicenseHeaderManager.Core;
using LicenseHeaderManager.Utils;
using Microsoft.VisualStudio.Shell;

namespace LicenseHeaderManager.Headers
{
  /// <summary>
  ///   Class for finding the nearest license header file for a ProjectItem or a Project
  /// </summary>
  public static class LicenseHeaderFinder
  {
    /// <summary>
    ///   Gets the license header file definition to use on the given project item.
    /// </summary>
    /// <param name="projectItem"></param>
    /// <returns>A dictionary, which contains the extensions and the corresponding lines</returns>
    public static IDictionary<string, string[]> GetHeaderDefinitionForItem (ProjectItem projectItem)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      // First search for the definition within the project, then look for the solution-level definition
      var headerDefinition = SearchWithinProjectGetHeaderDefinitionForItem (projectItem);
      return headerDefinition ?? GetHeaderDefinitionForSolution (projectItem.DTE.Solution);
    }

    /// <summary>
    ///   Lookup the license header file within the given projectItems.
    /// </summary>
    /// <param name="projectItems"></param>
    /// <returns>A dictionary, which contains the extensions and the corresponding lines</returns>
    public static IDictionary<string, string[]> SearchItemsDirectlyGetHeaderDefinition (ProjectItems projectItems)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      // Check for License-file within this level
      var headerFileName = SearchItemsDirectlyGetHeaderDefinitionFileName (projectItems);
      if (!string.IsNullOrEmpty (headerFileName))
        return LicenseHeadersPackage.Instance.LicenseHeaderExtractor.ExtractHeaderDefinitions (headerFileName); // Found a License header file on this level
      return null;
    }

    /// <summary>
    ///   Returns the License header file for the specified project, or the solution header file if it can't be found.
    /// </summary>
    /// <param name="project">The project which is scanned for the License header file</param>
    /// <returns>A dictionary, which contains the extensions and the corresponding lines</returns>
    public static IDictionary<string, string[]> GetHeaderDefinitionForProjectWithFallback (Project project)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      // First look for a header definition for the project, then look for the solution-level definition
      var definition = GetHeaderDefinitionForProjectWithoutFallback (project);
      return definition ?? GetHeaderDefinitionForSolution (project.DTE.Solution);
    }

    /// <summary>
    ///   Returns the License header file for the specified project. Does not fall back to the solution header file.
    /// </summary>
    /// <param name="project">The project which is scanned for the License header file</param>
    /// <returns>A dictionary, which contains the extensions and the corresponding lines</returns>
    public static IDictionary<string, string[]> GetHeaderDefinitionForProjectWithoutFallback (Project project)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      var headerFile = SearchItemsDirectlyGetHeaderDefinitionFileName (project.ProjectItems);
      return LicenseHeadersPackage.Instance.LicenseHeaderExtractor.ExtractHeaderDefinitions (headerFile);
    }

    /// <summary>
    ///   Returns the License header definition file for the specified solution.
    /// </summary>
    /// <param name="solution">The solution to look in.</param>
    /// <returns>A dictionary, which contains the extensions and the corresponding lines</returns>
    public static IDictionary<string, string[]> GetHeaderDefinitionForSolution (Solution solution)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      var solutionDirectory = Path.GetDirectoryName (solution.FullName);
      var solutionFileName = Path.GetFileName (solution.FullName);

      var solutionHeaderFilePath = Path.Combine (solutionDirectory, solutionFileName + LicenseHeaderExtractor.HeaderDefinitionExtension);
      return File.Exists (solutionHeaderFilePath) ? LicenseHeadersPackage.Instance.LicenseHeaderExtractor.ExtractHeaderDefinitions (solutionHeaderFilePath) : null;
    }

    /// <summary>
    ///   Returns the License header file that is directly attached to the project.
    /// </summary>
    /// <param name="project">The project which is scanned for the License header file</param>
    /// <returns>A dictionary, which contains the extensions and the corresponding lines</returns>
    private static IDictionary<string, string[]> GetHeaderDefinitionDirectlyOnProject (Project project)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      string headerFile;
      try
      {
        headerFile = SearchItemsDirectlyGetHeaderDefinitionFileName (project.ProjectItems);
      }
      catch (COMException)
      {
        return new Dictionary<string, string[]>();
      }

      var definition = LicenseHeadersPackage.Instance.LicenseHeaderExtractor.ExtractHeaderDefinitions (headerFile);
      return definition;
    }

    /// <summary>
    ///   Lookup the license header file within a project , if there is none on this level, moving up to next level.
    /// </summary>
    /// <param name="projectItem"></param>
    /// <returns>A dictionary, which contains the extensions and the corresponding lines</returns>
    private static IDictionary<string, string[]> SearchWithinProjectGetHeaderDefinitionForItem (ProjectItem projectItem)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      //Check for License-file within this level
      string headerFile;
      try
      {
        headerFile = SearchItemsDirectlyGetHeaderDefinitionFileName (projectItem.ProjectItems);
      }
      catch (COMException)
      {
        return new Dictionary<string, string[]>();
      }

      if (!string.IsNullOrEmpty (headerFile))
        return LicenseHeadersPackage.Instance.LicenseHeaderExtractor.ExtractHeaderDefinitions (headerFile); //Found a License header file on this level

      var projectItemParent = ProjectItemParentFinder.GetProjectItemParent (projectItem);

      //Lookup in the parent --> Go Up!
      return SearchWithinProjectGetHeaderDefinitionForProjectOrItem (projectItemParent);
    }

    /// <summary>
    ///   Lookup the license header file within a project or a ProjectItem, if there is none on this level, moving up to next
    ///   level.
    /// </summary>
    /// <param name="projectOrItem">An object which is either is a project or a ProjectItem</param>
    /// <returns>A dictionary, which contains the extensions and the corresponding lines</returns>
    private static IDictionary<string, string[]> SearchWithinProjectGetHeaderDefinitionForProjectOrItem (object projectOrItem)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      return projectOrItem switch
      {
          Project parentAsProject => GetHeaderDefinitionDirectlyOnProject (parentAsProject), // We are on the top --> load project License file if it exists
          ProjectItem parentAsProjectItem => SearchWithinProjectGetHeaderDefinitionForItem (parentAsProjectItem), // Lookup in the item above
          _ => null
      };
    }

    /// <summary>
    ///   Lookup the license header definition file within a ProjectItems object.
    /// </summary>
    /// <param name="projectItems">The ProjectItems project to search for the license header definition file.</param>
    /// <returns>The path of the found license header definition file, an empty string if none is found or null if <paramref name="projectItems" /> is null.</returns>
    private static string SearchItemsDirectlyGetHeaderDefinitionFileName (ProjectItems projectItems)
    {
      if (projectItems == null)
        return null;

      ThreadHelper.ThrowIfNotOnUIThread();

      foreach (ProjectItem item in projectItems)
        if (item.FileCount == 1)
        {
          string fileName = null;

          try
          {
            fileName = item.FileNames[0];
          }
          catch (Exception)
          {
            // ignored
          }

          if (fileName != null && Path.GetExtension (fileName).ToLowerInvariant() == LicenseHeaderExtractor.HeaderDefinitionExtension)
            return fileName;
        }

      return string.Empty;
    }
  }
}
