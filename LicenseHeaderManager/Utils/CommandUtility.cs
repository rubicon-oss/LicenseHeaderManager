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

using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;

namespace LicenseHeaderManager.Utils
{
  public class CommandUtility
  {
    public static bool ExecuteCommandIfExists(string command, DTE2 dte)
    {
      if (dte.Commands.Cast<Command>().Any(dtecommand => dtecommand.Name == command))
      {
        try
        {
          dte.ExecuteCommand(command);
          OutputWindowHandler.WriteMessage("Command executed");
        }
        catch (COMException e)
        {
          if(command == "ReSharper_Suspend")
          {
            OutputWindowHandler.WriteMessage("Excecution of '" + command +
                                             "' failed. Maybe ReSharper is already suspended? \n " + e.ToString());
          }
          else
          {
            //Command may be found but cannot be executed
            OutputWindowHandler.WriteMessage("Excecution of '" + command + "' failed. \n " + e.ToString());
          }
          return false;
        }
        return true;
      }

      return false;
    }

    public static void ExecuteCommand(string command, DTE2 dte)
    {
      dte.ExecuteCommand(command);
    }
  }
}
