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
using Core;
using EnvDTE;
using EnvDTE80;
using LicenseHeaderManager.Options.Model;
using Microsoft.VisualStudio.Threading;

namespace LicenseHeaderManager.Interfaces
{
  /// <summary>
  ///   Provides members contained within the package representing the LHM Visual Studio extension that might need to be
  ///   accessed from other classes.
  /// </summary>
  public interface ILicenseHeaderExtension
  {
    /// <summary>
    ///   Gets a <see cref="Core.LicenseHeaderReplacer" /> instance used by the package to update license headers.
    /// </summary>
    LicenseHeaderReplacer LicenseHeaderReplacer { get; }

    /// <summary>
    ///   Gets a <see cref="Core.LicenseHeaderExtractor" /> instance used by the package to extract license header definition
    ///   from a license header definiton file.
    /// </summary>
    ILicenseHeaderExtractor LicenseHeaderExtractor { get; }

    /// <summary>
    ///   Gets the <see cref="IDefaultLicenseHeaderPageModel" /> instance representing the current options regarding the
    ///   default text for license header definition files.
    /// </summary>
    IDefaultLicenseHeaderPageModel DefaultLicenseHeaderPageModel { get; }

    /// <summary>
    ///   Gets the <see cref="ILanguagesPageModel" /> instance representing the current options regarding languages, their
    ///   extensions and comment syntax.
    /// </summary>
    ILanguagesPageModel LanguagesPageModel { get; }

    /// <summary>
    ///   Gets the <see cref="IGeneralOptionsPageModel" /> instance representing the current options regarding general
    ///   configuration properties.
    /// </summary>
    IGeneralOptionsPageModel GeneralOptionsPageModel { get; }

    /// <summary>
    ///   Determines whether a Core invocation was implicitly triggered by the execution of a linked command .
    /// </summary>
    bool IsCalledByLinkedCommand { get; }

    /// <summary>
    ///   Gets the <see cref="EnvDTE80.DTE2" /> object that was acquired when initializing the VS package.
    /// </summary>
    DTE2 Dte2 { get; }

    /// <summary>
    ///   Gets the <see cref="JoinableTaskFactory" /> object that is provided by Visual Studio to use for
    ///   asynchronous tasks started by this extension.
    /// </summary>
    JoinableTaskFactory JoinableTaskFactory { get; }

    /// <summary>
    ///   Determines whether there exists a solution-wide license header definition file.
    /// </summary>
    /// <returns>
    ///   Returns <see langword="true" /> if a solution-wide license header definition file exists, otherwise
    ///   <see langword="false" />.
    /// </returns>
    bool SolutionHeaderDefinitionExists ();

    /// <summary>
    ///   Opens the options page that is responsible for configuring languages.
    /// </summary>
    void ShowLanguagesPage ();

    /// <summary>
    ///   Opens the options page that is responsible for configuring general options.
    /// </summary>
    void ShowOptionsPage ();

    /// <summary>
    ///   Determines the <see cref="ProjectItem" /> that is currently active ("opened and selected") in the document well.
    /// </summary>
    /// <returns>
    ///   The <see cref="ProjectItem" /> instance representing the file that is currently active ("opened and selected")
    ///   in the document well.
    /// </returns>
    public ProjectItem GetActiveProjectItem ();

    /// <summary>
    ///   Determines the <see cref="ProjectItem" /> currently selected in the solution explorer.
    /// </summary>
    /// <returns>
    ///   The <see cref="ProjectItem" /> instance representing the file that is currently selected in the solution
    ///   explorer.
    /// </returns>
    public object GetSolutionExplorerItem ();

    /// <summary>
    ///   Determines whether the License Header Manager Context Menu should be visible for a given <see cref="ProjectItem" />.
    /// </summary>
    /// <param name="item">The <see cref="ProjectItem" /> that was right-clicked on.</param>
    /// <returns>Returns <see langword="true" /> if the LHM Context Menu should be visible, otherwise <see langword="false" />.</returns>
    public bool ShouldBeVisible (ProjectItem item);
  }
}
