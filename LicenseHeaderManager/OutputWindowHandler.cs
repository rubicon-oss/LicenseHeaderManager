using System;
using Microsoft.VisualStudio.Shell.Interop;

namespace LicenseHeaderManager
{
  internal static class OutputWindowHandler
  {
    private static IVsOutputWindow _outputWindow;
    private static string customGuidString = "0F44E2D1-F5FA-4d2d-AB30-22BE8ECD9789";
    private static Guid _customGuid;

    public static void Initialize(IVsOutputWindow outputWindow)
    {
      _outputWindow = outputWindow;
      _customGuid = new Guid(customGuidString);
    }

    public static void WriteMessage(string message)
    {
      var customPane = GetOutputWindow();

      if (customPane == null)
      {
        string customTitle = Resources.NameOfThisExtension;
        _outputWindow.CreatePane( ref _customGuid, customTitle, 1, 1 );
        customPane = GetOutputWindow();
      }
      
      customPane.OutputString(message);
      customPane.Activate(); // Brings this pane into view
    }

    private static IVsOutputWindowPane GetOutputWindow()
    {
      IVsOutputWindowPane customPane;
      _outputWindow.GetPane( ref _customGuid, out customPane);
      return customPane;
    }
  }
}
