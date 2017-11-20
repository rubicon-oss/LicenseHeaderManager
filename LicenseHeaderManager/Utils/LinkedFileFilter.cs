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

using System.Collections.Generic;
using EnvDTE;
using LicenseHeaderManager.Headers;
using LicenseHeaderManager.Interfaces;

namespace LicenseHeaderManager.Utils
{
  public class LinkedFileFilter : ILinkedFileFilter
  {
    private Solution solution;

    public List<ProjectItem> ToBeProgressed { get; private set; }
    public List<ProjectItem> NoLicenseHeaderFile { get; private set; }
    public List<ProjectItem> NotInSolution { get; private set; }

    public LinkedFileFilter (Solution solution)
    {
      this.solution = solution;

      ToBeProgressed = new List<ProjectItem>();
      NoLicenseHeaderFile = new List<ProjectItem>();
      NotInSolution = new List<ProjectItem>();
    }


    public void Filter (List<ProjectItem> projectItems)
    {
      foreach (ProjectItem projectItem in projectItems)
      {
        ProjectItem foundProjectItem = solution.FindProjectItem (projectItem.Name);
        if (foundProjectItem == null)
          NotInSolution.Add (projectItem);
        else
          CheckForLicenseHeaderFile (foundProjectItem);
      }
    }

    private void CheckForLicenseHeaderFile (ProjectItem projectItem)
    {
      var headers = LicenseHeaderFinder.GetHeaderDefinitionForItem (projectItem);
      if (headers == null)
        NoLicenseHeaderFile.Add (projectItem);
      else
        ToBeProgressed.Add (projectItem);
    }
  }
}