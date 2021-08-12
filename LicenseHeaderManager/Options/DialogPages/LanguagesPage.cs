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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Forms;
using Core;
using LicenseHeaderManager.Options.DialogPageControls;
using LicenseHeaderManager.Options.Model;
using LicenseHeaderManager.Utils;
using log4net;

namespace LicenseHeaderManager.Options.DialogPages
{
  public class LanguagesPage : BaseOptionPage<LanguagesPageModel>
  {
    private static readonly ILog s_log = LogManager.GetLogger (MethodBase.GetCurrentMethod().DeclaringType);
    protected override IWin32Window Window => new WpfHost (new WpfLanguages ((ILanguagesPageModel) Model));

    public override void ResetSettings ()
    {
      ((ILanguagesPageModel) Model).Reset();
    }

    protected override IEnumerable<UpdateStep> GetVersionUpdateSteps ()
    {
      yield return new UpdateStep (new Version (1, 1, 4), AddDefaultSkipExpressions_1_1_4);
      yield return new UpdateStep (new Version (1, 2, 1), AddDefaultRegionSettings_1_2_1);
      yield return new UpdateStep (new Version (1, 2, 2), AdjustDefaultXmlSkipExpression_1_2_2);
      yield return new UpdateStep (new Version (1, 3, 2), AddXmlXsd_1_3_2);
      yield return new UpdateStep (new Version (1, 3, 6), ReduceToBaseExtensions_1_3_6);
      yield return new UpdateStep (new Version (1, 7, 3), AddMultipleDefaultExtensions_1_7_3);
      yield return new UpdateStep (new Version (3, 0, 1), AddDefaultRazorFileSettings_3_0_1);
      yield return new UpdateStep (new Version (4, 0, 0), MigrateStorageLocation_4_0_0);
    }

    private void AddDefaultSkipExpressions_1_1_4 ()
    {
      var updated = false;

      // Add SkipExpression for XML-based languages to replicate the previous hardcoded skipping of XML declarations
      UpdateLanguages (
          new[] { ".htm", ".html", ".xhtml", ".xml", ".resx" },
          l => updated |= UpdateIfNullOrEmpty (l, lang => lang.SkipExpression, @"(<\?xml(.|\s)*?\?>)?(\s*<!DOCTYPE(.|\s)*?>)?(\n|\r\n|\r)"));

      // Add SkipExpression for JavaScript
      UpdateLanguages (
          new[] { ".js" },
          l => updated |= UpdateIfNullOrEmpty (l, lang => lang.SkipExpression, "/// *<reference.*/>"));

      if (updated)
        MessageBoxHelper.Show (Resources.Update_1_1_3.ReplaceNewLines(), Resources.Update);
    }

    private void AddDefaultRegionSettings_1_2_1 ()
    {
      var updated = false;

      // Add regions for CS files
      UpdateLanguages (
          new[] { ".cs", ".designer.cs", ".xaml.cs", "aspx.cs", "ascx.cs" },
          l =>
          {
            updated |= UpdateIfNullOrEmpty (l, lang => lang.BeginRegion, "#region");
            updated |= UpdateIfNullOrEmpty (l, lang => lang.EndRegion, "#endregion");
          });

      // Add regions for VB files
      UpdateLanguages (
          new[] { ".vb", ".designer.vb", ".xaml.vb" },
          l =>
          {
            updated |= UpdateIfNullOrEmpty (l, lang => lang.BeginRegion, "#Region");

            if (!string.IsNullOrEmpty (l.EndRegion) && l.EndRegion != "End Region")
              return;

            l.EndRegion = "#End Region";
            updated = true;
          });

      if (updated)
        MessageBoxHelper.Show (Resources.Update_RegionSettings_1_2_1.ReplaceNewLines(), Resources.Update);
    }

    private void AdjustDefaultXmlSkipExpression_1_2_2 ()
    {
      var updated = false;
      // SkipExpression for XML-based languages was suboptimal, it didn't work for nearly empty files 
      UpdateLanguages (
          new[] { ".htm", ".html", ".xhtml", ".xml", ".resx" },
          l =>
          {
            if (l.SkipExpression != @"(<\?xml(.|\s)*?\?>)?(\s*<!DOCTYPE(.|\s)*?>)?(\n|\r\n|\r)")
              return;

            l.SkipExpression = @"(<\?xml(.|\s)*?\?>)?(\s*<!DOCTYPE(.|\s)*?>)?( |\t)*(\n|\r\n|\r)?";
            updated = true;
          });

      // SkipExpression for JS-based languages was suboptimal, it didn't work for nearly empty files 
      UpdateLanguages (
          new[] { ".js" },
          l =>
          {
            if (l.SkipExpression != @"/// *<reference.*/>")
              return;

            l.SkipExpression = @"/// *<reference.*/>( |\t)*(\n|\r\n|\r)?";
            updated = true;
          });

      if (updated)
        MessageBoxHelper.Show (Resources.Update_SkipExpressions_1_2_2.ReplaceNewLines(), Resources.Update);
    }

    private void AddXmlXsd_1_3_2 ()
    {
      // Add a default rule for config/xsd
      var added = false;

      added |= AddExtensionToExistingExtension (".xml", ".config");
      added |= AddExtensionToExistingExtension (".xml", ".xsd");

      if (added)
        MessageBoxHelper.Show (Resources.Update_1_3_1.ReplaceNewLines(), Resources.Update);
    }

    private void ReduceToBaseExtensions_1_3_6 ()
    {
      UpdateLanguages (
          new[] { ".cs" },
          l => l.Extensions = new[] { ".cs" });
      UpdateLanguages (
          new[] { ".vb" },
          l => l.Extensions = new[] { ".vb" });
    }

    private void AddMultipleDefaultExtensions_1_7_3 ()
    {
      var added = false;

      added |= AddExtensionToExistingExtension (".js", ".ts");
      added |= AddExtensionToExistingExtension (".xml", ".wxi");
      added |= AddExtensionToExistingExtension (".xml", ".wxl");
      added |= AddExtensionToExistingExtension (".xml", ".wxs");

      added |= AddLanguageIfNotExistent (".fs", new Language { Extensions = new[] { ".fs" }, BeginComment = "(*", EndComment = "*)", LineComment = "//" });
      added |= AddLanguageIfNotExistent (".php", new Language { Extensions = new[] { ".php" }, BeginComment = "/*", EndComment = "*/", LineComment = "//" });
      added |= AddLanguageIfNotExistent (".py", new Language { Extensions = new[] { ".py" }, BeginComment = "\"\"\"", EndComment = "\"\"\"" });
      added |= AddLanguageIfNotExistent (".sql", new Language { Extensions = new[] { ".sql" }, BeginComment = "/*", EndComment = "*/", LineComment = "--" });

      if (added)
        MessageBoxHelper.Show (
            "License Header Manager has automatically updated its configuration to add Language settings for multiple file extensions."
            + Environment.NewLine +
            "You can see all Language Settings at 'Options -> License Header Manager -> Languages'." + Environment.NewLine +
            Environment.NewLine +
            "Added file extension settings:" + Environment.NewLine +
            ".ts" + Environment.NewLine +
            ".wxi;.wxl;.wxs" + Environment.NewLine +
            ".fs" + Environment.NewLine +
            ".php" + Environment.NewLine +
            ".py" + Environment.NewLine +
            ".sql",
            Resources.Update);
    }

    private void AddDefaultRazorFileSettings_3_0_1 ()
    {
      var added = false;

      added |= AddLanguageIfNotExistsOrAddExtensionsIfExists (
          new Language { Extensions = new[] { ".cshtml", ".vbhtml" }, BeginComment = "@*", EndComment = "*@" });

      if (added)
        MessageBoxHelper.Show (
            "License Header Manager has automatically updated its configuration to add Language settings for multiple file extensions."
            + Environment.NewLine +
            "You can see all Language Settings at 'Options -> License Header Manager -> Languages'." + Environment.NewLine +
            Environment.NewLine +
            "Added file extension settings:" + Environment.NewLine +
            ".cshtml & .vbhtml",
            Resources.Update);
    }

    private bool AddLanguageIfNotExistsOrAddExtensionsIfExists (Language language)
    {
      if (language.Extensions.All (ExtensionExists))
        return false;

      if (language.Extensions.Any (ExtensionExists))
      {
        var extensionsToAdd = language.Extensions.Where (e => !ExtensionExists (e));
        var existingExtension = language.Extensions.First (ExtensionExists);
        foreach (var extension in extensionsToAdd)
          AddExtensionToExistingExtension (existingExtension, extension);
        return true;
      }

      if (language.Extensions.Any (ExtensionExists))
        return false;

      OptionsFacade.CurrentOptions.Languages.Add (language);
      return true;
    }

    private bool AddExtensionToExistingExtension (string existingExtension, string newExtension)
    {
      if (OptionsFacade.CurrentOptions.Languages.Any (x => x.Extensions.Contains (newExtension)))
        return false;

      UpdateLanguages (new[] { existingExtension }, l => { l.Extensions = l.Extensions.Concat (new[] { newExtension }); });
      return true;
    }

    private bool UpdateIfNullOrEmpty (Language l, Expression<Func<Language, string>> propertyAccessExpression, string value)
    {
      var property = (PropertyInfo) ((MemberExpression) propertyAccessExpression.Body).Member;

      if (!string.IsNullOrEmpty ((string) property.GetValue (l, null)))
        return false;

      property.SetValue (l, value, null);
      return true;
    }

    private void UpdateLanguages (IEnumerable<string> extensions, Action<Language> updateAction)
    {
      foreach (var extension in extensions)
      {
        var language = OptionsFacade.CurrentOptions.Languages.FirstOrDefault (l => l.Extensions.Contains (extension));
        if (language != null)
          updateAction (language);
      }
    }

    private bool AddLanguageIfNotExistent (string extension, Language language)
    {
      // We just want to check if our extension is already added as extension somewhere and add it as new Language if not
      if (ExtensionExists (extension))
        return false;

      OptionsFacade.CurrentOptions.Languages.Add (language);
      return true;
    }

    private bool ExtensionExists (string extension)
    {
      return OptionsFacade.CurrentOptions.Languages.Any (x => x.Extensions.Contains (extension));
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
        s_log.Info ("Migration to 3.0.1 with existing languages page");
        var migratedLanguagesPage = new LanguagesPageModel();
        LoadCurrentRegistryValues_3_0_3 (migratedLanguagesPage);

        OptionsFacade.CurrentOptions.Languages = migratedLanguagesPage.Languages;
      }
    }
  }
}
