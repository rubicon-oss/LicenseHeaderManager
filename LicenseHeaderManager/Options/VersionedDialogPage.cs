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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.Shell;

namespace LicenseHeaderManager.Options
{
  public class VersionedDialogPage : DialogPage
  {
    // serialized properties
    public string Version { get; set; }

    public override void LoadSettingsFromStorage()
    {
      base.LoadSettingsFromStorage();

      bool saveRequired = false;
      foreach (var updateStep in GetVersionUpdateSteps())
        saveRequired |= Update (updateStep);

      if (Version != LicenseHeadersPackage.CVersion)
        saveRequired |= Update (new UpdateStep (System.Version.Parse (LicenseHeadersPackage.CVersion)));

      Trace.Assert (Version == LicenseHeadersPackage.CVersion, "Settings update to " + LicenseHeadersPackage.CVersion + " didn't work.");

      if (saveRequired)
        SaveSettingsToStorage();
    }

    protected virtual IEnumerable<UpdateStep> GetVersionUpdateSteps()
    {
      return Enumerable.Empty<UpdateStep>();
    }

    private bool Update (UpdateStep updateStep)
    {
      var currentVersion = GetParsedCurrentVersion();
      if (currentVersion >= updateStep.TargetVersion)
        return false;

      updateStep.ExecuteActions();

      Version = updateStep.TargetVersion.ToString();
      return true;
    }

    private Version GetParsedCurrentVersion()
    {
      Version result;
      System.Version.TryParse (Version, out result);
      return result;
    }
  }
}