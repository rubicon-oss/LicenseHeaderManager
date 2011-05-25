using System;
using System.Windows;
using System.Windows.Forms;

namespace LicenseHeaderManager.Options
{
  public partial class WpfHost : UserControl
  {
    public WpfHost (UIElement wpfControl)
    {
      InitializeComponent();
      elementHost.Child = wpfControl;
    }
  }
}