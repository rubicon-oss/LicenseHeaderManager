using System.ComponentModel;
using System.Windows;
using LicenseHeaderManager.SolutionUpdateViewModels;
using Microsoft.VisualStudio.PlatformUI;

namespace LicenseHeaderManager
{
  /// <summary>
  /// Interaction logic for TestDialog.xaml
  /// </summary>
  public partial class SolutionUpdateDialog : DialogWindow
  {
    public SolutionUpdateDialog (SolutionUpdateViewModel solutionUpdateViewModel)
    {
      InitializeComponent ();
      this.DataContext = solutionUpdateViewModel;
    }


  }
}
