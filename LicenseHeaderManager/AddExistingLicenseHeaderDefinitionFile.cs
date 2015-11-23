using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using EnvDTE;
using LicenseHeaderManager.Headers;
using Microsoft.Win32;

namespace LicenseHeaderManager
{
  public class AddExistingLicenseHeaderDefinitionFile
  {
    public void AddDefinitionFileToOneProject (string fileName, ProjectItems projectItems)
    {
      var licenseHeaderDefinitionFileName = OpenFileDialogForExistingFile(fileName);

      if (licenseHeaderDefinitionFileName == null) return;

      AddFileToProject(projectItems, licenseHeaderDefinitionFileName);
    }

    public void AddDefinitionFileToMultipleProjects (List<Project> projects)
    {
      var licenseHeaderDefinitionFileName = OpenFileDialogForExistingFile (projects.First().FileName);

      if (licenseHeaderDefinitionFileName == null) return;

      foreach (var project in projects)
      {
        AddFileToProject(project.ProjectItems, licenseHeaderDefinitionFileName);
      }
    }

    private static void AddFileToProject (ProjectItems projectItems, string licenseHeaderDefinitionFileName)
    {
      int fileCountBefore = projectItems.Count;
      projectItems.AddFromFile (licenseHeaderDefinitionFileName);

      int fileCountAfter = projectItems.Count;
      if (fileCountBefore == fileCountAfter)
      {
        MessageBox.Show (Resources.Warning_CantLinkItemInSameProject, Resources.NameOfThisExtension, MessageBoxButton.OK,
          MessageBoxImage.Information);
      }
    }

    private string OpenFileDialogForExistingFile(string fileName)
    {
      FileDialog dialog = new OpenFileDialog ();
      dialog.CheckFileExists = true;
      dialog.CheckPathExists = true;
      dialog.DefaultExt = LicenseHeader.Extension;
      dialog.DereferenceLinks = true;
      dialog.Filter = "License Header Definitions|*" + LicenseHeader.Extension;
      dialog.InitialDirectory = Path.GetDirectoryName (fileName);
      bool? result = dialog.ShowDialog ();

      if (result.HasValue && result.Value)
        return dialog.FileName;

      return string.Empty;
    }
  }
}