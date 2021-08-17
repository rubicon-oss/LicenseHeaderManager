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
using System.Reflection;
using System.Windows.Forms;
using LicenseHeaderManager.Core.Options;
using LicenseHeaderManager.Options.DialogPageControls;
using LicenseHeaderManager.Options.Model;
using log4net;

namespace LicenseHeaderManager.Options.DialogPages
{
  public class OptionsPage : BaseOptionPage<GeneralOptionsPageModel>
  {
    private static readonly ILog s_log = LogManager.GetLogger (MethodBase.GetCurrentMethod().DeclaringType);

    protected override IWin32Window Window => new WpfHost (new WpfOptions ((IGeneralOptionsPageModel) Model));

    public override void ResetSettings ()
    {
      ((IGeneralOptionsPageModel) Model).Reset();
    }

    protected override IEnumerable<UpdateStep> GetVersionUpdateSteps ()
    {
      yield return new UpdateStep (new Version (4, 0, 0), MigrateStorageLocation_4_0_0);
    }

    private void MigrateStorageLocation_4_0_0 ()
    {
      s_log.Info ($"Start migration to License Header Manager Version 4.0.0 for page {GetType().Name}");
      if (!System.Version.TryParse (Version, out var version) || version < new Version (4, 0, 0))
      {
        s_log.Debug ($"Current version: {Version}");
        LoadCurrentRegistryValues_3_0_3();
      }
      else
      {
        s_log.Info ("Migration to 3.0.1 with existing options page");
        var migratedOptionsPage = new GeneralOptionsPageModel();
        LoadCurrentRegistryValues_3_0_3 (migratedOptionsPage);

        OptionsFacade.CurrentOptions.InsertInNewFiles = ThreeWaySelectionForMigration (
            OptionsFacade.CurrentOptions.InsertInNewFiles,
            migratedOptionsPage.InsertInNewFiles,
            VisualStudioOptions.DefaultInsertInNewFiles);
        OptionsFacade.CurrentOptions.UseRequiredKeywords = ThreeWaySelectionForMigration (
            OptionsFacade.CurrentOptions.UseRequiredKeywords,
            migratedOptionsPage.UseRequiredKeywords,
            CoreOptions.DefaultUseRequiredKeywords);
        OptionsFacade.CurrentOptions.RequiredKeywords = ThreeWaySelectionForMigration (
            OptionsFacade.CurrentOptions.RequiredKeywords,
            migratedOptionsPage.RequiredKeywords,
            CoreOptions.DefaultRequiredKeywords);
        OptionsFacade.CurrentOptions.LinkedCommands = migratedOptionsPage.LinkedCommands;
      }
    }
  }
}
