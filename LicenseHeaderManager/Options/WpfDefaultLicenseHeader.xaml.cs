using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LicenseHeaderManager.Options
{
  /// <summary>
  /// Interaction logic for WpfDefaultLicenseHeader.xaml
  /// </summary>
  public partial class WpfDefaultLicenseHeader : UserControl
  {
    private DefaultLicenseHeaderPage _page;

    public WpfDefaultLicenseHeader (DefaultLicenseHeaderPage page)
      : this ()
    {
      _page = page;

      DataContext = _page;

      Loaded += (e, h) =>
      {
        defaultText.Text = _page.LicenseHeaderFileText;
      };
    }

    public WpfDefaultLicenseHeader ()
    {
      InitializeComponent ();
    }
  }
}
