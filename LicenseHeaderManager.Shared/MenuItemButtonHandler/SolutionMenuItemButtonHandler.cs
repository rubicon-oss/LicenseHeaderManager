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
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using EnvDTE;
using EnvDTE80;
using LicenseHeaderManager.Interfaces;
using LicenseHeaderManager.UpdateViewModels;
using LicenseHeaderManager.UpdateViews;
using LicenseHeaderManager.Utils;
using log4net;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace LicenseHeaderManager.MenuItemButtonHandler
{
  internal class SolutionMenuItemButtonHandler : IMenuItemButtonHandler
  {
    private static readonly ILog s_log = LogManager.GetLogger (MethodBase.GetCurrentMethod().DeclaringType);
    private readonly DTE2 _dte2;
    private readonly MenuItemButtonHandlerImplementation _handler;
    private CancellationTokenSource _cancellationTokenSource;

    private SolutionUpdateDialog _dialog;
    private bool _reSharperSuspended;

    public SolutionMenuItemButtonHandler (DTE2 dte2, MenuItemButtonOperation mode, MenuItemButtonHandlerImplementation handler)
    {
      _dte2 = dte2;
      Mode = mode;
      _handler = handler;
    }

    public MenuItemButtonLevel Level => MenuItemButtonLevel.Solution;

    public MenuItemButtonOperation Mode { get; }

    public void HandleButton (object sender, EventArgs e)
    {
      Mouse.OverrideCursor = Cursors.Wait;
      var solutionUpdateViewModel = new SolutionUpdateViewModel();

      _dialog = new SolutionUpdateDialog (solutionUpdateViewModel) { WindowStartupLocation = WindowStartupLocation.CenterOwner };
      _dialog.Closing += DialogOnClosing;
      ThreadHelper.ThrowIfNotOnUIThread();
      _reSharperSuspended = CommandUtility.TryExecuteCommand ("ReSharper_Suspend", _dte2);

      Task.Run (() => HandleButtonInternalAsync (_dte2.Solution, _handler, solutionUpdateViewModel)).FireAndForget();
      Mouse.OverrideCursor = null;
      _dialog.ShowModal();
    }

    private async Task HandleButtonInternalAsync (object solutionObject, MenuItemButtonHandlerImplementation handler, BaseUpdateViewModel solutionUpdateViewModel)
    {
      await LicenseHeadersPackage.Instance.JoinableTaskFactory.SwitchToMainThreadAsync();

      if (!(solutionObject is Solution solution))
        return;

      _cancellationTokenSource = new CancellationTokenSource();
      try
      {
        await handler.DoWorkAsync (_cancellationTokenSource.Token, solutionUpdateViewModel, solution, _dialog);
      }
      catch (OperationCanceledException)
      {
        await LicenseHeadersPackage.Instance.JoinableTaskFactory.SwitchToMainThreadAsync();
        _dialog.Close();
      }
      catch (Exception ex)
      {
        MessageBoxHelper.ShowMessage (
            $"The operation '{handler.Description}' failed with the exception '{ex.Message}'. See Visual Studio Output Window for Details.");
        s_log.Error ($"The operation '{handler.Description}' failed.", ex);
      }

      await LicenseHeadersPackage.Instance.JoinableTaskFactory.SwitchToMainThreadAsync();
      _dialog.Close();
    }

    private void DialogOnClosing (object sender, CancelEventArgs e)
    {
      _cancellationTokenSource?.Cancel (true);
      ResumeReSharper();
    }

    private void ResumeReSharper ()
    {
      if (_reSharperSuspended)
        CommandUtility.ExecuteCommand ("ReSharper_Resume", _dte2);
    }
  }
}
