using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;

namespace LicenseHeaderManager.Options
{
  [ClassInterface (ClassInterfaceType.AutoDual)]
  [Guid ("D1B5984C-1693-4F26-891E-0BA3BF5760B4")]
  public class LanguagesPage : DialogPage
  {
    //serialized property
    [TypeConverter(typeof(LanguagesConverter))]
    [DesignerSerializationVisibility (DesignerSerializationVisibility.Visible)]
    public ObservableCollection<Language> Languages { get; set;}

    public LanguagesPage ()
    {
      ResetSettings ();
    }

    public override void ResetSettings ()
    {
      Languages = new ObservableCollection<Language> ()
      {
        new Language() { Extensions = new[] { ".cs", ".designer.cs", ".xaml.cs", "aspx.cs", "ascx.cs"}, LineComment = "//", BeginComment = "/*", EndComment = "*/" },
        new Language() { Extensions = new[] { ".c", ".cpp", ".cxx", ".h", ".hpp" }, LineComment = "//", BeginComment = "/*", EndComment = "*/" },
        new Language() { Extensions = new[] { ".vb", ".designer.vb", ".xaml.vb" }, LineComment = "'" },
        new Language() { Extensions = new[] { ".aspx", ".ascx", }, BeginComment = "<%--", EndComment = "--%>" },
        new Language() { Extensions = new[] { ".htm", ".html", ".xhtml", ".xml", ".xaml", ".resx" }, BeginComment = "<!--", EndComment = "-->" },
        new Language() { Extensions = new[] { ".css" }, BeginComment = "/*", EndComment = "*/" },
        new Language() { Extensions = new[] { ".js" }, LineComment = "//", BeginComment = "/*", EndComment = "*/" }
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