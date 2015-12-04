using System.Windows;

namespace LicenseHeaderManager.Utils
{
  public static class MessageBoxHelper
  {
    public static void Information(string message)
    {
      MessageBox.Show(message, Resources.LicenseHeaderManagerName, MessageBoxButton.OK,
        MessageBoxImage.Information);
    }

    public static bool DoYouWant(string message)
    {
      return MessageBox.Show(message, Resources.LicenseHeaderManagerName, MessageBoxButton.YesNo,
        MessageBoxImage.Information,
        MessageBoxResult.No) == MessageBoxResult.Yes;
    }
  }
}
