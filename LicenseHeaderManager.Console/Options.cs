using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace LicenseHeaderManager.Console
{
  public class Options
  {
    // ReSharper disable once MemberCanBePrivate.Global
    public Options (UpdateMode mode, FileInfo licenseHeaderDefinitionFile, FileInfo configuration, IEnumerable<FileInfo> files, DirectoryInfo directory, bool recursive)
    {
      Mode = mode;
      LicenseHeaderDefinitionFile = licenseHeaderDefinitionFile;
      Configuration = configuration;
      Files = files;
      Directory = directory;
      Recursive = recursive;
    }

    [Option (
        'm',
        "mode",
        Default = UpdateMode.Add,
        HelpText = "Specifies whether license headers should be added or removed. Must be one of {Add, Remove}, case-insensitive.")]
    public UpdateMode Mode { get; }

    [Option (
        'l',
        "license-header-definition",
        Required = true,
        HelpText = "The path to the license header definition file to be used for the update operations.")]
    public FileInfo LicenseHeaderDefinitionFile { get; }

    [Option (
        'c',
        "configuration",
        Default = null,
        HelpText = "The path to the JSON file that configures the behaviour of the Core component. If not present, default values will be used.")]
    public FileInfo Configuration { get; }

    [Option (
        'f',
        "files",
        Separator = ',',
        Default = null,
        HelpText = "Paths to the files whose headers should be updated, separated by comma (','). Must not be present if \"directory\" is present.")]
    public IEnumerable<FileInfo> Files { get; }

    [Option (
        'd',
        "directory",
        Default = null,
        HelpText = "Path of the directory containing the files whose headers should be updated. Must not be present if \"files\" is present.")]
    public DirectoryInfo Directory { get; }

    [Option (
        'r',
        "recursive",
        Default = false,
        HelpText = "Specifies whether the directory represented by \"directory\" should be searched recursively. "
                   + "Ignored if \"files\" is present instead of \"directory\".")]
    public bool Recursive { get; }

    public UpdateTarget Target => IsNullOrEmpty (Files) && Directory != null ? UpdateTarget.Directory : UpdateTarget.Files;

    [Usage (ApplicationAlias = " LicenseHeaderManager.Console.exe")]
    // ReSharper disable once UnusedMember.Global
    public static IEnumerable<Example> Examples
    {
      get
      {
        yield return new Example (
            " Add license headers to one file with a custom configuration",
            new Options (
                UpdateMode.Add,
                new FileInfo ("DefinitionFile.licenseheader"),
                new FileInfo ("CoreOptions.json"),
                new[] { new FileInfo ("file.cs") },
                null,
                false));
        yield return new Example (
            " Remove license headers from multiple files with standard configuration",
            new Options (
                UpdateMode.Remove,
                new FileInfo ("DefinitionFile.licenseheader"),
                null,
                new[] { new FileInfo ("file1.cs"), new FileInfo ("file2.html"), new FileInfo ("file3.xaml") },
                null,
                false));
        yield return new Example (
            " Add license headers to all files in a directory, but not its subdirectories, with custom configuration",
            new Options (
                UpdateMode.Add,
                new FileInfo ("DefinitionFile.licenseheader"),
                new FileInfo ("CoreOptions.json"),
                null,
                new DirectoryInfo (@"C:\SomeDirectory"),
                false));
        yield return new Example (
            " Remove license headers from all files in a directory and its subdirectories with standard configuration",
            new Options (
                UpdateMode.Remove,
                new FileInfo ("DefinitionFile.licenseheader"),
                null,
                null,
                new DirectoryInfo (@"C:\SomeDirectory"),
                true));
      }
    }

    public bool IsValid (out string errorMessage)
    {
      var builder = new StringBuilder();

      if (!IsNullOrEmpty (Files) && Directory != null || IsNullOrEmpty (Files) && Directory == null)
        builder.AppendLine (" Exactly one of the arguments \"files\" and \"directory\" must be present.");

      if (!IsNullOrEmpty (Files))
      {
        foreach (var fileInfo in Files)
        {
          if (!ValidateFileInfoExists (fileInfo, "f, files", out var fileError))
            builder.AppendLine (fileError);
        }
      }

      if (!ValidateFileInfoExists (Directory, "d, directory", out var directoryError))
        builder.AppendLine (directoryError);

      if (!ValidateFileInfoExists (LicenseHeaderDefinitionFile, "l, license-header-definition", out var licenseHeaderDefinitionFileError))
        builder.AppendLine (licenseHeaderDefinitionFileError);

      if (Configuration != null && !ValidateFileInfoExists (Configuration, "c, configuration", out var configurationError))
        builder.AppendLine (configurationError);

      errorMessage = builder.ToString();
      return errorMessage == string.Empty;
    }

    private static bool IsNullOrEmpty<T> (IEnumerable<T> enumerable)
    {
      return enumerable == null || !enumerable.Any();
    }

    private static bool ValidateFileInfoExists (FileSystemInfo fileInfo, string commandLineOption, out string errorMessage)
    {
      errorMessage = string.Empty;

      if (!(fileInfo is { Exists: false }))
        return true;

      errorMessage = $" The path \"{fileInfo.FullName}\" specified by option '{commandLineOption}' does not exist.";
      return false;
    }
  }
}
