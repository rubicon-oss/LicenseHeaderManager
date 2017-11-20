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
using System.ComponentModel;
using System.Windows.Threading;
using EnvDTE80;
using LicenseHeaderManager.Headers;
using LicenseHeaderManager.PackageCommands;
using LicenseHeaderManager.SolutionUpdateViewModels;
using LicenseHeaderManager.Utils;

namespace LicenseHeaderManager.ButtonHandler
{
  public class AddLicenseHeaderToAllProjectsButtonHandler
  {
    private readonly LicenseHeaderReplacer _licenseReplacer;
    private readonly DTE2 _dte2;

    public AddLicenseHeaderToAllProjectsButtonHandler (LicenseHeaderReplacer licenseReplacer, DTE2 dte2)
    {
      _licenseReplacer = licenseReplacer;
      _dte2 = dte2;
    }

    private System.Threading.Thread _solutionUpdateThread;
    private bool _resharperSuspended;

    public void HandleButton (object sender, EventArgs e)
    {
      var solutionUpdateViewModel = new SolutionUpdateViewModel();
      var addHeaderToAllProjectsCommand = new AddLicenseHeaderToAllFilesInSolutionCommand (_licenseReplacer, solutionUpdateViewModel);
      var buttonThreadWorker = new SolutionLevelButtonThreadWorker (addHeaderToAllProjectsCommand);
      var dialog = new SolutionUpdateDialog (solutionUpdateViewModel);


      dialog.Closing += DialogOnClosing;
      _resharperSuspended = CommandUtility.ExecuteCommandIfExists ("ReSharper_Suspend", _dte2);
      Dispatcher uiDispatcher = Dispatcher.CurrentDispatcher;

      buttonThreadWorker.ThreadDone += (o, args) =>
      {
        uiDispatcher.BeginInvoke (new Action (() => { dialog.Close(); }));
        ResumeResharper();
      };

      _solutionUpdateThread = new System.Threading.Thread (buttonThreadWorker.Run)
                                  { IsBackground = true };
      _solutionUpdateThread.Start (_dte2.Solution);

      dialog.ShowModal();
    }

    private void DialogOnClosing (object sender, CancelEventArgs cancelEventArgs)
    {
      _solutionUpdateThread.Abort();

      ResumeResharper();
    }

    private void ResumeResharper ()
    {
      if (_resharperSuspended)
      {
        CommandUtility.ExecuteCommand ("ReSharper_Resume", _dte2);
      }
    }
  }
}