#region copyright
// Copyright (c) 2011 rubicon IT GmbH

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
#endregion

using System;
using Microsoft.VisualStudio.Shell.Interop;

namespace LicenseHeaderManager
{
  internal static class OutputWindowHandler
  {
    private static IVsOutputWindow _outputWindow;
    
    private static Guid _customGuid;

    public static void Initialize(IVsOutputWindow outputWindow)
    {
      _outputWindow = outputWindow;
      _customGuid = new Guid(GuidList.guidVisualStudioOutputWindow);
    }

    public static void WriteMessage (string message)
    {
      var customPane = GetOutputWindow();

      if (customPane == null)
      {
        string customTitle = Resources.NameOfThisExtension;
        _outputWindow.CreatePane ( ref _customGuid, customTitle, 1, 1 );
        customPane = GetOutputWindow();
      }
      
      customPane.OutputString (message + Environment.NewLine);
      customPane.Activate(); // Brings this pane into view
    }

    private static IVsOutputWindowPane GetOutputWindow()
    {
      IVsOutputWindowPane customPane;
      _outputWindow.GetPane ( ref _customGuid, out customPane);
      return customPane;
    }
  }
}
