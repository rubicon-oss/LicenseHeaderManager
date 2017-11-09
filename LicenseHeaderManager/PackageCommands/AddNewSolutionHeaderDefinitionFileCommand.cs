using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EnvDTE;
using EnvDTE80;
using LicenseHeaderManager.Headers;
using LicenseHeaderManager.Options;
using Microsoft.VisualStudio.Shell;

namespace LicenseHeaderManager.PackageCommands
{
  public class AddNewSolutionHeaderDefinitionFileCommand
  {
    private readonly Func<string> defaultHeaderDefinitionFunc;

    private AddNewSolutionHeaderDefinitionFileCommand(Func<string> defaultHeaderDefinitionFunc)
    {
      this.defaultHeaderDefinitionFunc = defaultHeaderDefinitionFunc;
    }

    public void Execute(Solution solution)
    {
      string solutionHeaderDefinitionFilePath = LicenseHeader.GetHeaderDefinitionFilePathForSolution(solution);

      // Add file
      string defaultLicenseHeaderFileText = this.defaultHeaderDefinitionFunc();

      File.WriteAllText(solutionHeaderDefinitionFilePath, defaultLicenseHeaderFileText);

      solution.DTE.OpenFile(EnvDTE.Constants.vsViewKindTextView, solutionHeaderDefinitionFilePath).Activate();
    }

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static AddNewSolutionHeaderDefinitionFileCommand Instance
    {
      get;
      private set;
    }

    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    public static void Initialize(Func<string> defaultHeaderDefinitionFunc)
    {
      Instance = new AddNewSolutionHeaderDefinitionFileCommand(defaultHeaderDefinitionFunc);
    }
  }
}
