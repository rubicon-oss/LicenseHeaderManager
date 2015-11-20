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

using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using LicenseHeaderManager.Headers;
using LicenseHeaderManager.Interfaces;

namespace LicenseHeaderManager.Utils
{
  public class LinkedFileHandler
  {
    public string Message { get; private set; }

    public LinkedFileHandler ()
    {
      Message = string.Empty;
    }

    public void Handle (LicenseHeaderReplacer licenseHeaderReplacer, ILinkedFileFilter linkedFileFilter)
    {
      foreach (ProjectItem projectItem in linkedFileFilter.ToBeProgressed)
      {
        var headers = LicenseHeaderFinder.GetHeaderRecursive (projectItem);
        licenseHeaderReplacer.RemoveOrReplaceHeader (projectItem, headers, true);
      }

      if (linkedFileFilter.NoLicenseHeaderFile.Any () || linkedFileFilter.NotInSolution.Any ())
      {
        List<ProjectItem> notProgressedItems =
          linkedFileFilter.NoLicenseHeaderFile.Concat (linkedFileFilter.NotInSolution).ToList ();

        List<string> notProgressedNames = notProgressedItems.Select(x => x.Name).ToList();

        Message +=
          string.Format (Resources.LinkedFileUpdateInformation, string.Join ("\n", notProgressedNames))
            .Replace (@"\n", "\n");
      }
    }
  }
}
