#region copyright
// Copyright (c) 2011 rubicon informationstechnologie gmbh

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
#endregion
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using LicenseHeaderManager.Options.Converters;
using Microsoft.VisualStudio.Shell;

namespace LicenseHeaderManager.Options
{
  [ClassInterface (ClassInterfaceType.AutoDual)]
  [Guid ("D1B5984C-1693-4F26-891E-0BA3BF5760B4")]
  public class LanguagesPage : DialogPage
  {
    //serialized property
    public string Version { get; set; }

    [TypeConverter(typeof(LanguageConverter))]
    [DesignerSerializationVisibility (DesignerSerializationVisibility.Visible)]
    public ObservableCollection<Language> Languages { get; set;}

    public LanguagesPage ()
    {
      ResetSettings ();
    }

    public override void LoadSettingsFromStorage ()
    {
      base.LoadSettingsFromStorage ();
      Update_1_1_3 ();
    }

    private void Update_1_1_3 ()
    {
      if (string.IsNullOrEmpty (Version))
      {
        //add SkipExpression for XML-based languages to replicate the previous hardcoded skipping of XML declarations
        var regex = @"(<\?xml(.|\s)*?\?>)?(\s*<!DOCTYPE(.|\s)*?>)?(\n|\r\n|\r)";
        var extensions = new[] {".htm", ".html", ".xhtml", ".xml", ".xaml", ".resx" };
        Language language;
        foreach (var extension in extensions)
        {
          language = Languages.FirstOrDefault (l => l.Extensions.Contains (extension));
          if (language != null && String.IsNullOrEmpty (language.SkipExpression))
            language.SkipExpression = regex;
        }

        //add SkipExpression for JavaScript
        language = Languages.FirstOrDefault (l => l.Extensions.Contains (".js"));
        if (language != null && String.IsNullOrEmpty (language.SkipExpression))
          language.SkipExpression = "/// *<reference.*/>";

        MessageBox.Show (Resources.Upgrate_1_1_3.Replace(@"\n", "\n"), "Upgrade");

        Version = "1.1.3";
      }
    }

    public override sealed void ResetSettings ()
    {
      Languages = new ObservableCollection<Language> ()
      {
        new Language() { Extensions = new[] { ".cs", ".designer.cs", ".xaml.cs", "aspx.cs", "ascx.cs"}, LineComment = "//", BeginComment = "/*", EndComment = "*/", BeginRegion = "#region", EndRegion = "#endregion"},
        new Language() { Extensions = new[] { ".c", ".cpp", ".cxx", ".h", ".hpp" }, LineComment = "//", BeginComment = "/*", EndComment = "*/"},
        new Language() { Extensions = new[] { ".vb", ".designer.vb", ".xaml.vb" }, LineComment = "'", BeginRegion = "#Region", EndRegion = "End Region" },
        new Language() { Extensions = new[] { ".aspx", ".ascx", }, BeginComment = "<%--", EndComment = "--%>" },
        new Language() { Extensions = new[] { ".htm", ".html", ".xhtml", ".xml", ".xaml", ".resx" }, BeginComment = "<!--", EndComment = "-->", SkipExpression = @"(<\?xml(.|\s)*?\?>)?(\s*<!DOCTYPE(.|\s)*?>)?(\n|\r\n|\r)" },
        new Language() { Extensions = new[] { ".css" }, BeginComment = "/*", EndComment = "*/" },
        new Language() { Extensions = new[] { ".js" }, LineComment = "//", BeginComment = "/*", EndComment = "*/", SkipExpression = "/// *<reference.*/>"}
      };
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
  }
}