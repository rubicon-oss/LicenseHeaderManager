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
using Core;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace LicenseHeaderManager.Utils
{
  /// <summary>
  ///   This class provides methods to check if a given project item is of type "physical file", "license header", "link" or
  ///   "folder".
  /// </summary>
  public static class ProjectItemInspection
  {
    private const string c_guidItemTypePhysicalFile = "6bb5f8ee-4483-11d3-8bcf-00c04f8ec28c";

    /// <summary>
    ///   Checks whether the given project item is a physical file that lays somewhere in the directory.
    /// </summary>
    /// <param name="projectItem">Specifies the project item to be checked if it is a physical file.</param>
    /// <returns></returns>
    public static bool IsPhysicalFile (ProjectItem projectItem)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      return projectItem.Kind == Constants.vsProjectItemKindPhysicalFile || projectItem.Kind == "{" + c_guidItemTypePhysicalFile + "}";
    }

    /// <summary>
    ///   Checks whether the given project item is a license header definition file and has the <see cref="LicenseHeaderExtractor.HeaderDefinitionExtension" /> extension.
    /// </summary>
    /// <param name="projectItem">Specifies the project item to be checked if it is a license header definition file.</param>
    /// <returns></returns>
    public static bool IsLicenseHeader (ProjectItem projectItem)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      return projectItem.Name.Contains (LicenseHeaderExtractor.HeaderDefinitionExtension);
    }

    /// <summary>
    ///   Checks whether the given project item is a linked file.
    /// </summary>
    /// <param name="projectItem">Specifies the project item to be checked if it is a linked file.</param>
    /// <returns></returns>
    public static bool IsLink (ProjectItem projectItem)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      if (projectItem.Properties == null)
        return false;

      Property isLinkProperty;

      try
      {
        isLinkProperty = projectItem.Properties.Cast<Property>().FirstOrDefault (
            property =>
            {
              ThreadHelper.ThrowIfNotOnUIThread();
              return property.Name == "IsLink";
            });
      }
      catch (ArgumentException)
      {
        return false;
      }

      return isLinkProperty != null && (bool) isLinkProperty.Value;
    }

    /// <summary>
    ///   Checks whether the given project item is a folder.
    /// </summary>
    /// <param name="projectItem">Specifies the project item to be checked if it is a folder.</param>
    /// <returns></returns>
    public static bool IsFolder (ProjectItem projectItem)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      return string.Equals (projectItem.Kind, Constants.vsProjectItemKindPhysicalFolder, StringComparison.InvariantCultureIgnoreCase) ||
             string.Equals (projectItem.Kind, Constants.vsProjectItemKindVirtualFolder, StringComparison.InvariantCultureIgnoreCase);
    }
  }
}
