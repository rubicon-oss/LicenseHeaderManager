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
using LicenseHeaderManager.ReturnObjects;
using LicenseHeaderManager.Utils;

namespace LicenseHeaderManager.PackageCommands
{
  internal class AddLicenseHeaderToAllFilesInProjectCommand
  {
    private LicenseHeaderReplacer _licenseReplacer;

    public AddLicenseHeaderToAllFilesInProjectCommand(LicenseHeaderReplacer licenseReplacer)
    {
      _licenseReplacer = licenseReplacer;
    }

    public AddLicenseHeaderToAllFilesReturn Execute(object projectOrProjectItem)
    {
      var project = projectOrProjectItem as Project;
      var projectItem = projectOrProjectItem as ProjectItem;
      
      int countSubLicenseHeadersFound = 0;
      IDictionary<string, string[]> headers = null;
      List<ProjectItem> linkedItems = new List<ProjectItem> ();

      if (project != null || projectItem != null)
      {
        _licenseReplacer.ResetExtensionsWithInvalidHeaders();
        ProjectItems projectItems;

        if (project != null)
        {
          headers = LicenseHeaderFinder.GetHeaderDefinitionForProjectWithFallback(project);
          projectItems = project.ProjectItems;
        }
        else
        {
          headers = LicenseHeaderFinder.GetHeaderDefinitionForItem(projectItem);
          projectItems = projectItem.ProjectItems;
        }
       
        foreach (ProjectItem item in projectItems)
        {
          if (ProjectItemInspection.IsPhysicalFile(item) && ProjectItemInspection.IsLink(item))
            linkedItems.Add(item);
          else
            countSubLicenseHeadersFound = _licenseReplacer.RemoveOrReplaceHeaderRecursive(item, headers);
        }
      }

      return new AddLicenseHeaderToAllFilesReturn(countSubLicenseHeadersFound, headers == null, linkedItems);
    }
  }
}
