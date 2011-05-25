using System;
using System.Windows;

namespace LicenseHeaderManager.Options
{
  public partial class WpfLanguageDialog : Window
  {
    public new Language Language
    {
      get { return DataContext as Language; }
      set { DataContext = value; }
    }

    public WpfLanguageDialog ()
    {
      InitializeComponent ();
    }

    private void OnClick (object sender, RoutedEventArgs e)
    {
      if (sender == cancel)
      {
        DialogResult = false;
        Close();
      }
      else if (sender == ok)
      {
        if (Language != null)
        {
          if (Language.IsValid)
          {
            Language.NormalizeExtensions ();
            DialogResult = true;
            Close();
          }
          else
            MessageBox.Show (LicenseHeaderManager.Resources.Error_LanguageInvalid, LicenseHeaderManager.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
      }
    }
  }
}
