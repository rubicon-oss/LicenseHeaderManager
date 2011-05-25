using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace LicenseHeaderManager.Options
{
  [ClassInterface (ClassInterfaceType.AutoDual)]
  [Guid ("EB6F9B18-D203-43E3-8033-35AD9BEFC70D")]
  public class OptionsPage : DialogPage
  {
    public event Action<object, EventArgs> OptionsChanged;

    //serialized properties
    public bool UseRequiredKeywords { get; set; }
    public string RequiredKeywords { get; set; }
    public bool AttachToCommand { get; set; }
    public string AttachedCommandGuid { get; set; }
    public int AttachedCommandId { get; set; }

    public Commands Commands { get { return Dte.Commands; } }

    private DTE2 Dte { get { return GetService (typeof (DTE)) as DTE2; } }

    public OptionsPage ()
    {
      ResetSettings ();
    }

    public override void ResetSettings ()
    {
      UseRequiredKeywords = true;
      RequiredKeywords = "license, copyright, (c)";
      AttachToCommand = false;
      base.ResetSettings ();
    }

    protected override void OnApply (PageApplyEventArgs e)
    {
      base.OnApply (e);
      if (OptionsChanged != null)
        OptionsChanged (this, EventArgs.Empty);
    }

    public Command GetAttachedCommand ()
    {
      if (string.IsNullOrEmpty (AttachedCommandGuid) || AttachedCommandId < 0)
        return null;
      else
        return Commands.Item (AttachedCommandGuid, AttachedCommandId);
    }

    [Browsable (false)]
    [DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
    protected override IWin32Window Window
    {
      get
      {
        var host = new WpfHost (new WpfOptions (this));
        return host;
      }
    }
  }
}