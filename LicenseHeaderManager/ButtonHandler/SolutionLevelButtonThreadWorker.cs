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
using EnvDTE;
using LicenseHeaderManager.Interfaces;
using LicenseHeaderManager.Utils;

namespace LicenseHeaderManager.ButtonHandler
{
  class SolutionLevelButtonThreadWorker
  {
    private readonly ISolutionLevelCommand _solutionLevelCommand;

    public SolutionLevelButtonThreadWorker (ISolutionLevelCommand solutionLevelCommand)
    {
      _solutionLevelCommand = solutionLevelCommand;
    }

    public event EventHandler ThreadDone;

    public void Run (object solutionObject)
    {
      Solution solution = solutionObject as Solution;
      if (solution == null) return;

      try
      {
        _solutionLevelCommand.Execute (solution);
      }
      catch (Exception exception)
      {
        MessageBoxHelper.Information (
            string.Format (
                "The command '{0}' failed with the exception '{1}'. See Visual Studio Output Window for Details.",
                _solutionLevelCommand.GetCommandName(),
                exception.Message));
        OutputWindowHandler.WriteMessage (exception.ToString());
      }

      ThreadDone?.Invoke (this, EventArgs.Empty);
    }
  }
}