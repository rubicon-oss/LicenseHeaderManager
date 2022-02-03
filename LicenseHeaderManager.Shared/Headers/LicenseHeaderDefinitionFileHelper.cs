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
using System.IO;
using System.Text;
using EnvDTE;
using LicenseHeaderManager.Core;
using LicenseHeaderManager.Options.Model;
using LicenseHeaderManager.Utils;
using Microsoft.VisualStudio.Shell;

namespace LicenseHeaderManager.Headers
{
  /// <summary>
  ///   Provides static helper methods regarding finding and creating license header definition files based on the solution's
  ///   name and structure.
  /// </summary>
  public static class LicenseHeaderDefinitionFileHelper
  {
    /// <summary>
    ///   Gets the file path to the License Header Definition File in the active solution.
    /// </summary>
    /// <param name="solution">The solution in which the definition file should be located.</param>
    /// <returns>The full file path to the License Header Definition File.</returns>
    public static string GetHeaderDefinitionFilePathForSolution (Solution solution)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      var solutionDirectory = Path.GetDirectoryName (solution.FullName);
      var solutionFileName = Path.GetFileName (solution.FullName);
      return Path.Combine (solutionDirectory, solutionFileName + LicenseHeaderExtractor.HeaderDefinitionExtension);
    }

    /// <summary>
    ///   Shows a message box that asks if a new License Header Definition File should be added to the active project.
    /// </summary>
    /// <param name="activeProject">The project where the definition file will be inserted.</param>
    /// <param name="pageModel">The page model where the license header definition text is stored.</param>
    /// <returns>True if a new License Header Definition file should be added to the current project, otherwise false.</returns>
    public static bool ShowQuestionForAddingLicenseHeaderFile (Project activeProject, IDefaultLicenseHeaderPageModel pageModel)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      var message = string.Format (Resources.Error_NoHeaderDefinition, activeProject.Name).ReplaceNewLines();
      if (!MessageBoxHelper.AskYesNo (message, Resources.Error))
        return false;
      var licenseHeaderDefinitionFile = AddHeaderDefinitionFile (activeProject, pageModel);
      licenseHeaderDefinitionFile.Open (Constants.vsViewKindCode).Activate();
      return true;
    }

    /// <summary>
    ///   Adds a new License Header Definition file to the active project.
    /// </summary>
    /// <param name="activeProject">The project where the defintion file will be inserted.</param>
    /// <param name="pageModel">The page model where the license header definition text is stored.</param>
    /// <returns></returns>
    public static ProjectItem AddHeaderDefinitionFile (Project activeProject, IDefaultLicenseHeaderPageModel pageModel)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      if (IsInvalidProject (activeProject))
        return null;

      var fileName = GetNewFullName (activeProject);
      File.WriteAllText (fileName, pageModel.LicenseHeaderFileText, Encoding.UTF8);
      var newProjectItem = activeProject.ProjectItems.AddFromFile (fileName);

      if (newProjectItem == null)
      {
        var message = string.Format (Resources.Error_CreatingFile).ReplaceNewLines();
        MessageBoxHelper.ShowError (message);
      }

      return newProjectItem;
    }

    /// <summary>
    ///   Adds a new License Header Definition file to the active folder.
    /// </summary>
    /// <param name="folder">The folder where the file will be inserted.</param>
    /// <param name="pageModel">The page model where the license header definition text is stored.</param>
    /// <returns></returns>
    public static ProjectItem AddLicenseHeaderDefinitionFile (ProjectItem folder, IDefaultLicenseHeaderPageModel pageModel)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      if (folder == null || folder.Kind != Constants.vsProjectItemKindPhysicalFolder)
        return null;

      var fileName = GetNewFullName (folder.Properties.Item ("FullPath").Value.ToString());
      File.WriteAllText (fileName, pageModel.LicenseHeaderFileText, Encoding.UTF8);

      var newProjectItem = folder.ProjectItems.AddFromFile (fileName);

      OpenNewProjectItem (newProjectItem);

      return newProjectItem;
    }

    /// <summary>
    ///   Adds a dot to a string if the string does not start with a dot.
    /// </summary>
    /// <param name="extension">The string where the dot will be added.</param>
    /// <returns>The string with one leading dot.</returns>
    public static string AddDot (string extension)
    {
      if (extension.StartsWith ("."))
        return extension;
      return "." + extension;
    }

    /// <summary>
    ///   Gets the name for the new License Header Definition File.
    /// </summary>
    /// <param name="name">The name of the active project.</param>
    /// <returns>The file name of the License Header Definition File.</returns>
    private static string GetNewFullName (string name)
    {
      var directory = Path.GetDirectoryName (name);

      if (string.IsNullOrEmpty (directory))
      {
        MessageBoxHelper.ShowMessage (
            "We could not determine a path and name for the new .licenseheader file." +
            "As a workaround you could create a .licenseheader file manually." +
            "If possible, please report this issue to us." +
            "Additional Information: Path.GetDirectoryName(" + name + ") returned empty string.");

        throw new ArgumentException ("Path.GetDirectoryName(" + name + ") returned empty string.");
      }

      var projectName = directory.Substring (directory.LastIndexOf ('\\') + 1);
      var fileName = Path.Combine (directory, projectName) + LicenseHeaderExtractor.HeaderDefinitionExtension;

      for (var i = 2; File.Exists (fileName); i++)
        fileName = Path.Combine (directory, projectName) + i + LicenseHeaderExtractor.HeaderDefinitionExtension;

      return fileName;
    }

    /// <summary>
    ///   Checks if the given project is invalid.
    /// </summary>
    /// <param name="activeProject">The project to be checked.</param>
    /// <returns>True if the given project is invalid, false if it is valid (exists).</returns>
    private static bool IsInvalidProject (Project activeProject)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      //It is possible that we receive a Project which is missing the Path property entirely.
      return activeProject == null || string.IsNullOrEmpty (activeProject.FullName) && string.IsNullOrEmpty (activeProject.FileName);
    }

    /// <summary>
    ///   Gets the name for the new License Header Definition File.
    /// </summary>
    /// <param name="project">The project where the License Header Definition File is located.</param>
    /// <returns>The file name of the License Header Definition File.</returns>
    private static string GetNewFullName (Project project)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      //This is just to check if activeProject.FullName contains the FullName as expected. 
      //If an Project Type uses this Property incorrectly, we try generating the .licenseheader filename with the .FileName Property
      return GetNewFullName (string.IsNullOrEmpty (Path.GetDirectoryName (project.FullName)) ? project.FileName : project.FullName);
    }

    /// <summary>
    ///   Opens the given project item in a new window.
    /// </summary>
    /// <param name="newProjectItem">The project item to be opened in a new window.</param>
    /// <returns>True if the project item can be opened in a window, otherwise false.</returns>
    private static bool OpenNewProjectItem (ProjectItem newProjectItem)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      if (newProjectItem != null)
      {
        var window = newProjectItem.Open (Constants.vsViewKindCode);
        window.Activate();
        return true;
      }

      var message = string.Format (Resources.Error_CreatingFile).ReplaceNewLines();
      MessageBoxHelper.ShowError (message);
      return false;
    }
  }
}
