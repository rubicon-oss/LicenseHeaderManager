#region copyright
// Copyright (c) rubicon IT GmbH

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
#endregion

using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace LicenseHeaderManager.Options
{
  public partial class WpfHost : UserControl
  {
    private const UInt32 DLGC_WANTARROWS = 0x0001;
    private const UInt32 DLGC_WANTTAB = 0x0002;
    private const UInt32 DLGC_HASSETSEL = 0x0008;
    private const UInt32 DLGC_WANTCHARS = 0x0080;
    private const UInt32 WM_GETDLGCODE = 0x0087;

    public WpfHost (UIElement wpfControl)
    {
      InitializeComponent();
      elementHost.Child = wpfControl;
    }

    protected override void OnLoad (EventArgs e)
    {
      base.OnLoad (e);

      // Workaround for an issue with WPF TextBox controls not working when hosted inside an ElementHost when the surrounding window swallows WM_CHAR 
      // (and other) messages. We add a message hook to the HwndSource hosting the control inside the ElementHost that intercepts WM_GETDLGCODE to 
      // specify that the control needs to handle WM_CHAR messages and similar.
      // (Adapted from http://stackoverflow.com/questions/835878/wpf-textbox-not-accepting-input-when-in-elementhost-in-window-forms)

      var hwndSource = PresentationSource.FromVisual (elementHost.Child) as HwndSource;
      if (hwndSource != null)
        hwndSource.AddHook (ChildHwndSourceHook);
    }

    private IntPtr ChildHwndSourceHook (IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
      if (msg == WM_GETDLGCODE)
      {
        handled = true;
        return new IntPtr (DLGC_WANTCHARS | DLGC_WANTARROWS | DLGC_HASSETSEL | DLGC_WANTTAB);
      }
      return IntPtr.Zero;
    }
  }
}