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
  public class AddNewHeaderDefinitionFileToSolutionCommand
  {
    private readonly Func<string> defaultHeaderDefinitionFunc;

    private AddNewHeaderDefinitionFileToSolutionCommand(Func<string> defaultHeaderDefinitionFunc)
    {
      this.defaultHeaderDefinitionFunc = defaultHeaderDefinitionFunc;
    }

    public ProjectItem Execute(Solution solution)
    {
      string solutionHeaderDefinitionFilePath = LicenseHeader.GetHeaderDefinitionFilePathForSolution(solution);

      Project solutionItemsProject = GetOrCreateSolutionItems(solution);

      // Add file
      string defaultLicenseHeaderFileText = this.defaultHeaderDefinitionFunc();

      File.WriteAllText(solutionHeaderDefinitionFilePath, defaultLicenseHeaderFileText);

      return solutionItemsProject.ProjectItems.AddFromFile(solutionHeaderDefinitionFilePath);
    }

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static AddNewHeaderDefinitionFileToSolutionCommand Instance
    {
      get;
      private set;
    }

    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    public static void Initialize(Func<string> defaultHeaderDefinitionFunc)
    {
      Instance = new AddNewHeaderDefinitionFileToSolutionCommand(defaultHeaderDefinitionFunc);
    }

    private static Project GetOrCreateSolutionItems(Solution solutionObject)
    {
      var solItems = solutionObject.Projects.Cast<Project>().FirstOrDefault(p => p.Name == "Solution Items" || p.Kind == EnvDTE.Constants.vsProjectItemKindSolutionItems);
      if (solItems == null)
      {
        Solution2 sol2 = (Solution2)solutionObject;
        solItems = sol2.AddSolutionFolder("Solution Items");
      }
      return solItems;
    }
  }
}
