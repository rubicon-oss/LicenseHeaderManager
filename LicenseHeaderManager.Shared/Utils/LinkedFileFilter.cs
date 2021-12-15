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
using LicenseHeaderManager.Headers;
using LicenseHeaderManager.Interfaces;
using Microsoft.VisualStudio.Shell;

namespace LicenseHeaderManager.Utils
{
  public class LinkedFileFilter : ILinkedFileFilter
  {
    private readonly Solution _solution;

    public LinkedFileFilter (Solution solution)
    {
      _solution = solution;

      ToBeProgressed = new List<ProjectItem>();
      NoLicenseHeaderFile = new List<ProjectItem>();
      NotInSolution = new List<ProjectItem>();
    }

    public List<ProjectItem> ToBeProgressed { get; }
    public List<ProjectItem> NoLicenseHeaderFile { get; }
    public List<ProjectItem> NotInSolution { get; }

    public void Filter (IEnumerable<ProjectItem> projectItems)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      foreach (var projectItem in projectItems)
      {
        var foundProjectItem = _solution.FindProjectItem (projectItem.Name);
        if (foundProjectItem == null)
          NotInSolution.Add (projectItem);
        else
          CheckForLicenseHeaderFile (foundProjectItem);
      }
    }

    private void CheckForLicenseHeaderFile (ProjectItem projectItem)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      var headers = LicenseHeaderFinder.GetHeaderDefinitionForItem (projectItem);
      if (headers == null)
        NoLicenseHeaderFile.Add (projectItem);
      else
        ToBeProgressed.Add (projectItem);
    }
  }
}
