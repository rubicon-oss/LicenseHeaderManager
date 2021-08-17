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
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using LicenseHeaderManager.Core;
using LicenseHeaderManager.Interfaces;
using LicenseHeaderManager.UpdateViewModels;
using LicenseHeaderManager.Utils;

namespace LicenseHeaderManager.MenuItemCommands.Common
{
  public class RemoveLicenseHeaderFromAllFilesInProjectHelper
  {
    private readonly BaseUpdateViewModel _baseUpdateViewModel;
    private readonly CancellationToken _cancellationToken;
    private readonly ILicenseHeaderExtension _licenseHeaderExtension;

    public RemoveLicenseHeaderFromAllFilesInProjectHelper (
        CancellationToken cancellationToken,
        ILicenseHeaderExtension licenseHeaderExtension,
        BaseUpdateViewModel baseUpdateViewModel)
    {
      _cancellationToken = cancellationToken;
      _licenseHeaderExtension = licenseHeaderExtension;
      _baseUpdateViewModel = baseUpdateViewModel;
    }

    public async Task ExecuteAsync (object projectOrProjectItem)
    {
      await _licenseHeaderExtension.JoinableTaskFactory.SwitchToMainThreadAsync();
      switch (projectOrProjectItem)
      {
        case Project project:
        {
          var replacerInput = new List<LicenseHeaderContentInput>();
          var fileOpenedStatus = new Dictionary<string, bool>();
          foreach (ProjectItem item in project.ProjectItems)
          {
            replacerInput.AddRange (CoreHelpers.GetFilesToProcess (item, null, out _, out var subFileOpenedStatus, false));
            foreach (var status in subFileOpenedStatus)
              fileOpenedStatus[status.Key] = status.Value;
          }

          await RemoveOrReplaceHeaderAndHandleResultAsync (replacerInput, fileOpenedStatus, project.Name);

          break;
        }
        case ProjectItem item:
        {
          var handlerInput = CoreHelpers.GetFilesToProcess (item, null, out _, out var subFileOpenedStatus, false);
          await RemoveOrReplaceHeaderAndHandleResultAsync (handlerInput, subFileOpenedStatus);

          break;
        }
      }
    }

    private async Task RemoveOrReplaceHeaderAndHandleResultAsync (
        ICollection<LicenseHeaderContentInput> replacerInput,
        IDictionary<string, bool> fileOpenedStatus,
        string projectName = null)
    {
      replacerInput.IgnoreNonCommentText();
      var result = await _licenseHeaderExtension.LicenseHeaderReplacer.RemoveOrReplaceHeader (
          replacerInput,
          CoreHelpers.CreateProgress (_baseUpdateViewModel, projectName, fileOpenedStatus, _cancellationToken),
          _cancellationToken);
      await CoreHelpers.HandleResultAsync (result, _licenseHeaderExtension, _baseUpdateViewModel, projectName, fileOpenedStatus, _cancellationToken);
    }
  }
}
