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

using EnvDTE;
using LicenseHeaderManager.Headers;

namespace LicenseHeaderManager.PackageCommands
{
  public class RemoveLicenseHeaderFromAllFilesInProjectCommand
  {
    private LicenseHeaderReplacer _licenseReplacer;

    public RemoveLicenseHeaderFromAllFilesInProjectCommand(LicenseHeaderReplacer licenseReplacer)
    {
      _licenseReplacer = licenseReplacer;
    }

    public void Execute(object projectOrProjectItem)
    {
      var project = projectOrProjectItem as Project;
      var item = projectOrProjectItem as ProjectItem;
      if (project == null && item == null) return;

      _licenseReplacer.ResetExtensionsWithInvalidHeaders ();
      if (project != null)
      {
        foreach (ProjectItem i in project.ProjectItems)
          _licenseReplacer.RemoveOrReplaceHeaderRecursive (i, null, false);
      }
      else
      {
        foreach (ProjectItem i in item.ProjectItems)
          _licenseReplacer.RemoveOrReplaceHeaderRecursive (i, null, false);
      }
    }
  }
}
