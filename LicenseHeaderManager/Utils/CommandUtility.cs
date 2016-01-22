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
