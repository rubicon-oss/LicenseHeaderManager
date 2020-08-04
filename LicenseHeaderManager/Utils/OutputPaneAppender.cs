/* Copyright (c) rubicon IT GmbH
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
 */

using System;
using log4net.Appender;
using log4net.Core;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace LicenseHeaderManager.Utils
{
  internal class OutputPaneAppender : AppenderSkeleton
  {
    private readonly IVsOutputWindowPane _licenseHeaderManagerPane;

    public OutputPaneAppender (IVsOutputWindow outputPane, Level threshold)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      Threshold = threshold;
      outputPane.GetPane (ref LicenseHeadersPackage.GuidOutputPaneAppender, out _licenseHeaderManagerPane);
    }

    protected override void Append (LoggingEvent loggingEvent)
    {
      LogMessageAsync (loggingEvent).FireAndForget();
    }

    private async Task LogMessageAsync (LoggingEvent loggingEvent)
    {
      await LicenseHeadersPackage.Instance.JoinableTaskFactory.SwitchToMainThreadAsync();

      _licenseHeaderManagerPane.OutputStringThreadSafe (
          $"{loggingEvent.TimeStamp:yyyy-MM-dd HH:mm:ss,fff} [{loggingEvent.Level}] {loggingEvent.LoggerName}: {loggingEvent.RenderedMessage}\n");
      if (loggingEvent.ExceptionObject != null)
        _licenseHeaderManagerPane.OutputStringThreadSafe (loggingEvent.GetExceptionString() + "\n");
    }
  }
}
