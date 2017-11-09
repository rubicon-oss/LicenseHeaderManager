using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EnvDTE;
using LicenseHeaderManager.Headers;

namespace LicenseHeaderManager.PackageCommands
{
  public class OpenSolutionHeaderDefinitionFileCommand
  {
    private OpenSolutionHeaderDefinitionFileCommand()
    {
    }

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static OpenSolutionHeaderDefinitionFileCommand Instance
    {
      get;
      private set;
    }

    public void Execute(Solution solution)
    {
      string solutionHeaderDefinitionFilePath = LicenseHeader.GetHeaderDefinitionFilePathForSolution(solution);

      if (File.Exists(solutionHeaderDefinitionFilePath))
      {
        solution.DTE.OpenFile(EnvDTE.Constants.vsViewKindTextView, solutionHeaderDefinitionFilePath).Activate();
      }
    }

    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    public static void Initialize()
    {
      Instance = new OpenSolutionHeaderDefinitionFileCommand();
    }
  }
}
