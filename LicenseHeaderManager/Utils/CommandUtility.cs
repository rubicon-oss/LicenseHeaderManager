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
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using log4net;
using Microsoft.VisualStudio.Shell;

namespace LicenseHeaderManager.Utils
{
  /// <summary>
  ///   Provides utility methods for Visual Studio Commands
  /// </summary>
  public static class CommandUtility
  {
    private static readonly ILog s_log = LogManager.GetLogger (MethodBase.GetCurrentMethod().DeclaringType);

    /// <summary>
    ///   Tries to execute a Visual Studio command.
    /// </summary>
    /// <param name="command">The text identifying the command that should be executed.</param>
    /// <param name="dte">
    ///   The <see cref="DTE2" /> that is used to execute the command identified by <paramref name="command" />
    ///   .
    /// </param>
    /// <returns>Returns <see langword="true" /> if the command was executed successfully, otherwise <see langword="false" />.</returns>
    public static bool TryExecuteCommand (string command, DTE2 dte)
    {
      if (dte.Commands.Cast<Command>().All (
          dteCommand =>
          {
            ThreadHelper.ThrowIfNotOnUIThread();
            return dteCommand.Name != command;
          }))
        return false;

      try
      {
        dte.ExecuteCommand (command);
        s_log.Info ($"Command '{command}' successfully executed.");
      }
      catch (COMException ex)
      {
        s_log.Error (command == "ReSharper_Suspend" ? $"Command '{command}' failed. Maybe ReSharper is already suspended?" : $"Command '{command}' failed.", ex);
        return false;
      }

      return true;
    }

    /// <summary>
    ///   Executes a commands, swallowing possibly occurring exceptions (e. g. if command name does not exist or the command
    ///   execution fails).
    /// </summary>
    /// <param name="command">The text identifying the command that should be executed.</param>
    /// <param name="dte">
    ///   The <see cref="DTE2" /> that is used to execute the command identified by <paramref name="command" />
    ///   .
    /// </param>
    public static void ExecuteCommand (string command, DTE2 dte)
    {
      try
      {
        dte.ExecuteCommand (command);
        s_log.Info ($"Command '{command}' successfully executed.");
      }
      catch (COMException ex)
      {
        s_log.Error (command == "ReSharper_Resume" ? $"Command '{command}' failed. Maybe ReSharper is already suspended at all?" : $"Command '{command}' failed.", ex);
      }
    }
  }
}
