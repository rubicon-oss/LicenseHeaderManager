//Sample license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;

namespace LicenseHeaderManager.Options
{
  [ClassInterface (ClassInterfaceType.AutoDual)]
  [Guid ("E0E8C0E8-0E8E-4251-B885-1ABF58837366")]
  public sealed class DefaultLicenseHeaderPage : VersionedDialogPage, IDefaultLicenseHeaderPage
  {
    public DefaultLicenseHeaderPage ()
    {
      ResetSettings ();
    }

    public string LicenseHeaderFileText { get; set; }

    public override void ResetSettings ()
    {
      InitializeFromResource ();

      base.ResetSettings ();
    }

    protected override IEnumerable<UpdateStep> GetVersionUpdateSteps ()
    {
      yield return new UpdateStep (new Version (1, 2, 1), InitializeFromResourceIfRequired);
    }

    [Browsable (false)]
    [DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
    protected override IWin32Window Window
    {
      get
      {
        var host = new WpfHost (new WpfDefaultLicenseHeader (this));
        return host;
      }
    }

    private void InitializeFromResourceIfRequired ()
    {
      if (string.IsNullOrEmpty (LicenseHeaderFileText))
      {
        InitializeFromResource ();
      }

      MessageBox.Show (Resources.Update_DefaultLicenseHeader_1_2_1.Replace (@"\n", "\n"), "Update");
    }

    private void InitializeFromResource ()
    {
      using (var resource = Assembly.GetExecutingAssembly ().GetManifestResourceStream (typeof (LicenseHeadersPackage), "default.licenseheader"))
      {
        string text = new StreamReader (resource, Encoding.UTF8).ReadToEnd ();
        LicenseHeaderFileText = text;
      }
    }
  }
}
