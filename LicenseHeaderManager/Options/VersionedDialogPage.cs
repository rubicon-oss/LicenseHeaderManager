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
using System.Linq;
using LicenseHeaderManager.Utils;
using Microsoft.VisualStudio.Shell;

namespace LicenseHeaderManager.Options
{
  public class VersionedDialogPage : DialogPage
  {
    //Serialized properties
    //Is managed by VisualStudio and placed persistently in the Registry
    public string Version { get; set; }

    private static bool s_firstDialogPageLoaded = true;

    public override void LoadSettingsFromStorage()
    {
      base.LoadSettingsFromStorage();
      
      //Could happen if you install a LicenseHeaderManager (LHM) version which is older than the ever installed highest version
      //Should only happen to developers of LHM, but could theoretically also happen if someone downgrades LHM.
      if (GetParsedRegistryVersion() > GetCurrentlyInstalledVersion())
      {
        if (s_firstDialogPageLoaded)
        {
            MessageBoxHelper.Information (
                    "We detected that you are downgrading LicenseHeaderManager from a higher version." + Environment.NewLine +
                    "As we dont know what you did to get to that state, it is possible that you missed an update for the Language Settings."
                    + Environment.NewLine +
                    "If some of your license headers do not update, check if your Language Settings (Options -> LicenseHeaderManager -> Languages) "
                    + Environment.NewLine +
                    "contain all the extensions you require.");

            s_firstDialogPageLoaded = false;
        }
        
        Version = LicenseHeadersPackage.Version;
        SaveSettingsToStorage();
      }
      else
      {
        var saveRequired = false;

        foreach (var updateStep in GetVersionUpdateSteps())
          saveRequired |= Update (updateStep);

        if (Version != LicenseHeadersPackage.Version)
          saveRequired |= Update (new UpdateStep (GetCurrentlyInstalledVersion()));
        
        if (saveRequired)
          SaveSettingsToStorage();
      }
    }

    protected virtual IEnumerable<UpdateStep> GetVersionUpdateSteps()
    {
      return Enumerable.Empty<UpdateStep>();
    }

    private bool Update (UpdateStep updateStep)
    {
      var registryVersion = GetParsedRegistryVersion();
      if (registryVersion >= updateStep.TargetVersion)
        return false;

      updateStep.ExecuteActions();

      Version = updateStep.TargetVersion.ToString();
      return true;
    }

    private Version GetParsedRegistryVersion()
    {
      Version result;
      System.Version.TryParse (Version, out result);
      return result;
    }

    private Version GetCurrentlyInstalledVersion ()
    {
      return System.Version.Parse (LicenseHeadersPackage.Version);
    }
  }
}