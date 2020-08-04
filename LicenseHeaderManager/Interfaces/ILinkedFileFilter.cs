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
using EnvDTE;

namespace LicenseHeaderManager.Interfaces
{
  /// <summary>
  ///   Analyzes linked files and categorizes them into three types: to be processed, not a part of the solution and no
  ///   associated license header definition file.
  /// </summary>
  public interface ILinkedFileFilter
  {
    /// <summary>
    ///   Gets a list of linked files that should be included in a license header update operation.
    /// </summary>
    List<ProjectItem> ToBeProgressed { get; }

    /// <summary>
    ///   Gets a list of linked files that do not have a license header definition file associated with them.
    /// </summary>
    List<ProjectItem> NoLicenseHeaderFile { get; }

    /// <summary>
    ///   Gets a list of linked files that do not belong to the solution.
    /// </summary>
    List<ProjectItem> NotInSolution { get; }

    /// <summary>
    ///   Filters given linked files into the categories represented by the properties provided by this interface.
    /// </summary>
    /// <param name="projectItems">The linked files to be filtered.</param>
    void Filter (IEnumerable<ProjectItem> projectItems);
  }
}
