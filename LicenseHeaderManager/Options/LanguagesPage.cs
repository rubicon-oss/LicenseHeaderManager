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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using LicenseHeaderManager.Options.Converters;

namespace LicenseHeaderManager.Options
{
  [ClassInterface (ClassInterfaceType.AutoDual)]
  [Guid ("D1B5984C-1693-4F26-891E-0BA3BF5760B4")]
  public class LanguagesPage : VersionedDialogPage, ILanguagesPage
  {
    private readonly IList<Language> _defaultLanguages = new ObservableCollection<Language>
    {
      new Language { Extensions = new[] { ".cs" }, LineComment = "//", BeginComment = "/*", EndComment = "*/", BeginRegion = "#region", EndRegion = "#endregion" },
      new Language { Extensions = new[] { ".c", ".cpp", ".cxx", ".h", ".hpp" }, LineComment = "//", BeginComment = "/*", EndComment = "*/" },
      new Language { Extensions = new[] { ".vb" }, LineComment = "'", BeginRegion = "#Region", EndRegion = "#End Region" },
      new Language { Extensions = new[] { ".aspx", ".ascx", }, BeginComment = "<%--", EndComment = "--%>" },
      new Language { Extensions = new[] { ".htm", ".html", ".xhtml", ".xml", ".xaml", ".resx", ".config", ".xsd" }, BeginComment = "<!--", EndComment = "-->", SkipExpression = @"(<\?xml(.|\s)*?\?>)?(\s*<!DOCTYPE(.|\s)*?>)?( |\t)*(\n|\r\n|\r)?" },
      new Language { Extensions = new[] { ".css" }, BeginComment = "/*", EndComment = "*/" },
      new Language { Extensions = new[] { ".js", ".ts" }, LineComment = "//", BeginComment = "/*", EndComment = "*/", SkipExpression = @"(/// *<reference.*/>( |\t)*(\n|\r\n|\r)?)*" },
      new Language { Extensions = new[] { ".sql" }, BeginComment = "/*", EndComment = "*/", LineComment = "--" },
      new Language { Extensions = new[] { ".php" }, BeginComment = "/*", EndComment = "*/", LineComment = "//" },
      new Language { Extensions = new[] { ".wxs", ".wxl", ".wxi" }, BeginComment = "<!--", EndComment = "-->" },
      new Language { Extensions = new[] { ".py" }, BeginComment = "\"\"\"", EndComment = "\"\"\"" },
      new Language { Extensions = new[] { ".fs" }, BeginComment = "(*", EndComment = "*)", LineComment = "//" },
      new Language { Extensions = new[] { ".cshtml", ".vbhtml" }, BeginComment = "@*", EndComment = "*@" },
    };

    private readonly LanguageConverter _languageConverter = new LanguageConverter();

    //serialized properties

    [DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
    public IList<Language> Languages { get; set; }

    [DesignerSerializationVisibility (DesignerSerializationVisibility.Visible)]
    // ReSharper disable once UnusedMember.Global
    public string LanguagesSerialized
    {
      get { return _languageConverter.ToXml (Languages); }
      set { Languages = new ObservableCollection<Language> (_languageConverter.FromXml (value)); }
    }

    public LanguagesPage ()
    {
      ResetSettings();
    }

    public override sealed void ResetSettings ()
    {
      Languages = _defaultLanguages;
      base.ResetSettings ();
    }

    [Browsable (false)]
    [DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
    protected override IWin32Window Window
    {
      get
      {
        var host = new WpfHost (new WpfLanguages (this));
        return host;
      }
    }

    #region version updates

    protected override IEnumerable<UpdateStep> GetVersionUpdateSteps ()
    {
      yield return new UpdateStep (new Version (1, 1, 4), AddDefaultSkipExpressions_1_1_4);
      yield return new UpdateStep (new Version (1, 2, 1), AddDefaultRegionSettings_1_2_1);
      yield return new UpdateStep (new Version (1, 2, 2), AdjustDefaultXmlSkipExpression_1_2_2);
      yield return new UpdateStep (new Version (1, 3, 2), AddXmlXsd_1_3_2);
      yield return new UpdateStep (new Version (1, 3, 6), ReduceToBaseExtensions_1_3_6);
      yield return new UpdateStep (new Version (1, 7, 3), AddMultipleDefaultExtensions_1_7_3);
      yield return new UpdateStep (new Version (3, 0, 1), MigrateStorageLocation_3_0_1, AddDefaultRazorFileSettings_3_0_1);
    }

    private void AddDefaultSkipExpressions_1_1_4 ()
    {
      bool updated = false;

      //add SkipExpression for XML-based languages to replicate the previous hardcoded skipping of XML declarations
      UpdateLanguages (
          new[] { ".htm", ".html", ".xhtml", ".xml", ".resx" },
          l => updated |= UpdateIfNullOrEmpty (l, lang => lang.SkipExpression, @"(<\?xml(.|\s)*?\?>)?(\s*<!DOCTYPE(.|\s)*?>)?(\n|\r\n|\r)"));

      //add SkipExpression for JavaScript
      UpdateLanguages (
          new[] { ".js" },
          l => updated |= UpdateIfNullOrEmpty (l, lang => lang.SkipExpression, "/// *<reference.*/>"));

      if (updated)
        MessageBox.Show (Resources.Update_1_1_3.Replace (@"\n", "\n"), "Update");
    }

    private void AddDefaultRegionSettings_1_2_1 ()
    {
      bool updated = false;

      //add regions for CS files
      UpdateLanguages (
          new[] { ".cs", ".designer.cs", ".xaml.cs", "aspx.cs", "ascx.cs" },
          l =>
          {
            updated |= UpdateIfNullOrEmpty (l, lang => lang.BeginRegion, "#region");
            updated |= UpdateIfNullOrEmpty (l, lang => lang.EndRegion, "#endregion");
          });

      //add regions for VB files
      UpdateLanguages (
          new[] { ".vb", ".designer.vb", ".xaml.vb" },
          l =>
          {
            updated |= UpdateIfNullOrEmpty (l, lang => lang.BeginRegion, "#Region");

            if (string.IsNullOrEmpty (l.EndRegion) || l.EndRegion == "End Region")
            {
              l.EndRegion = "#End Region";
              updated = true;
            }
          });

      if (updated)
        MessageBox.Show (Resources.Update_RegionSettings_1_2_1.Replace (@"\n", "\n"), "Update");
    }

    private void AdjustDefaultXmlSkipExpression_1_2_2 ()
    {
      bool updated = false;
      // SkipExpression for XML-based languages was suboptimal, it didn't work for nearly empty files 
      UpdateLanguages (
          new[] { ".htm", ".html", ".xhtml", ".xml", ".resx" },
          l =>
          {
            if (l.SkipExpression == @"(<\?xml(.|\s)*?\?>)?(\s*<!DOCTYPE(.|\s)*?>)?(\n|\r\n|\r)")
            {
              l.SkipExpression = @"(<\?xml(.|\s)*?\?>)?(\s*<!DOCTYPE(.|\s)*?>)?( |\t)*(\n|\r\n|\r)?";
              updated = true;
            }
          });

      // SkipExpression for JS-based languages was suboptimal, it didn't work for nearly empty files 
      UpdateLanguages (
          new[] { ".js" },
          l =>
          {
            if (l.SkipExpression == @"/// *<reference.*/>")
            {
              l.SkipExpression = @"/// *<reference.*/>( |\t)*(\n|\r\n|\r)?";
              updated = true;
            }
          });

      if (updated)
        MessageBox.Show (Resources.Update_SkipExpressions_1_2_2.Replace (@"\n", "\n"), "Update");
    }


    private void AddXmlXsd_1_3_2 ()
    {
      //Add a default rule for config/xsd
      bool added = false;

      added |= AddExtensionToExistingExtension (".xml", ".config");
      added |= AddExtensionToExistingExtension (".xml", ".xsd");

      if (added)
        MessageBox.Show (Resources.Update_1_3_1.Replace (@"\n", "\n"), "Update");
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
      {
        MessageBox.Show (
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
            "License Header Manager Update");
      }
    }

    private void MigrateStorageLocation_3_0_1 ()
    {
      if (!System.Version.TryParse (Version, out var version) || version < new Version (3, 0, 0))
      {
        LoadRegistryValuesBefore_3_0_0();
      }
      else
      {
        var migratedLanguagesPage = new LanguagesPage();
        LoadRegistryValuesBefore_3_0_0 (migratedLanguagesPage);

        Languages = migratedLanguagesPage.Languages;
      }
    }

    private void AddDefaultRazorFileSettings_3_0_1 ()
    {
      var added = false;

      added |= AddLanguageIfNotExistsOrAddExtensionsIfExists (
          new Language { Extensions = new[] { ".cshtml", ".vbhtml" }, BeginComment = "@*", EndComment = "*@" });

      if (added)
      {
        MessageBox.Show (
            "License Header Manager has automatically updated its configuration to add Language settings for multiple file extensions."
            + Environment.NewLine +
            "You can see all Language Settings at 'Options -> License Header Manager -> Languages'." + Environment.NewLine +
            Environment.NewLine +
            "Added file extension settings:" + Environment.NewLine +
            ".cshtml & .vbhtml",
            "License Header Manager Update");
      }
    }

    private bool AddLanguageIfNotExistsOrAddExtensionsIfExists (Language language)
    {
      if (language.Extensions.All (ExtensionExists))
      {
        return false;
      }

      if (language.Extensions.Any (ExtensionExists))
      {
        var extensionsToAdd = language.Extensions.Where (e => !ExtensionExists (e));
        var existingExtension = language.Extensions.First (ExtensionExists);
        foreach (var extension in extensionsToAdd)
        {
          AddExtensionToExistingExtension (existingExtension, extension);
        }
        return true;
      }

      if (!language.Extensions.Any (ExtensionExists))
      {
        Languages.Add (language);
        return true;
      }

      return false;
    }

    private bool AddExtensionToExistingExtension (string existingExtension, string newExtension)
    {
      if (Languages.Any (x => x.Extensions.Contains (newExtension)))
        return false;

      UpdateLanguages (new[] { existingExtension }, l => { l.Extensions = l.Extensions.Concat (new[] { newExtension }); });
      return true;
    }

    private bool UpdateIfNullOrEmpty (Language l, Expression<Func<Language, string>> propertyAccessExpression, string value)
    {
      var property = (PropertyInfo) ((MemberExpression) propertyAccessExpression.Body).Member;

      if (string.IsNullOrEmpty ((string) property.GetValue (l, null)))
      {
        property.SetValue (l, value, null);
        return true;
      }

      return false;
    }

    private void UpdateLanguages (IEnumerable<string> extensions, Action<Language> updateAction)
    {
      foreach (var extension in extensions)
      {
        var language = Languages.FirstOrDefault (l => l.Extensions.Contains (extension));
        if (language != null)
          updateAction (language);
      }
    }

    private bool AddLanguageIfNotExistent (string extension, Language language)
    {
      //We just want to check if our extension is already added as extension somewhere and add it as new Language if not
      if (!ExtensionExists (extension))
      {
        Languages.Add (language);
        return true;
      }

      return false;
    }

    private bool ExtensionExists (string extension)
    {
      return Languages.Any (x => x.Extensions.Contains (extension));
    }

    #endregion
  }
}