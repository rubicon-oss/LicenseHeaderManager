using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EnvDTE;
using LicenseHeaderManager.Headers;

namespace LicenseHeaderManager.PackageCommands
{
  public class RemoveSolutionHeaderDefinitionFileCommand
  {
    private RemoveSolutionHeaderDefinitionFileCommand()
    {
    }

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static RemoveSolutionHeaderDefinitionFileCommand Instance
    {
      get;
      private set;
    }

    public void Execute(Solution solution)
    {
      string solutionHeaderDefinitionFilePath = LicenseHeader.GetHeaderDefinitionFilePathForSolution(solution);

      // Look for and close the document if it exists
      foreach (EnvDTE.Document document in solution.DTE.Documents)
      {
        if (string.Equals(solutionHeaderDefinitionFilePath, document.FullName, StringComparison.OrdinalIgnoreCase))
        {
          document.Close();
        }
      }

      // Delete the file
      if (File.Exists(solutionHeaderDefinitionFilePath))
      {
        File.Delete(solutionHeaderDefinitionFilePath);
      }
    }

    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    public static void Initialize()
    {
      Instance = new RemoveSolutionHeaderDefinitionFileCommand();
    }
  }
}
