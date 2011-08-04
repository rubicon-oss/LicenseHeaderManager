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
using System.Windows.Controls;
using System.Windows.Interop;

namespace LicenseHeaderManager.Options
{
  public class InteropTextBox : TextBox
  {
    private const UInt32 DLGC_WANTARROWS = 0x0001;
    private const UInt32 DLGC_WANTTAB = 0x0002;
    private const UInt32 DLGC_HASSETSEL = 0x0008;
    private const UInt32 DLGC_WANTCHARS = 0x0080;
    private const UInt32 WM_GETDLGCODE = 0x0087;

    public InteropTextBox ()
    {
      Loaded += delegate
      {
        HwndSource s = HwndSource.FromVisual (this) as HwndSource;
        if (s != null)
          s.AddHook (new HwndSourceHook (ChildHwndSourceHook));
      };
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