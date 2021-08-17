using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CommandLine;
using LicenseHeaderManager.Core;
using LicenseHeaderManager.Core.Options;

namespace LicenseHeaderManager.Console
{
  public static class Program
  {
    private const int c_exitSuccess = 0;
    private const int c_exitError = 1;

    private const ConsoleColor c_colorError = ConsoleColor.Red;
    private const ConsoleColor c_colorSuccess = ConsoleColor.Green;
    private const ConsoleColor c_colorWarning = ConsoleColor.Yellow;

    private static LicenseHeaderExtractor s_headerExtractor;
    private static CoreOptions s_defaultCoreSettings;
    private static LicenseHeaderReplacer s_replacer;

    public static int Main (string[] args)
    {
      try
      {
        var parser = new Parser (
            with =>
            {
              with.CaseSensitive = false;
              with.CaseInsensitiveEnumValues = true;
              with.EnableDashDash = true;
              with.AutoHelp = true;
              with.HelpWriter = System.Console.Out;
            });
        parser.ParseArguments<Options> (args)
            .WithParsed (RunProgram)
            .WithNotParsed (HandleParsingError);

        return c_exitSuccess;
      }
      catch (Exception ex)
      {
        WriteLineColor ("\nEncountered an unhandled error:", c_colorError);
        WriteLineColor (ex.ToString(), c_colorError);
        return c_exitError;
      }
    }

    private static void RunProgram (Options options)
    {
      if (!options.IsValid (out var errorMessage))
      {
        WriteLineColor ("The command-line invocation was syntactically correct, but the following semantic errors were detected:", c_colorError);
        WriteLineColor (errorMessage, c_colorError);
        Exit (false);
      }

      if (options.Configuration != null)
      {
        System.Console.WriteLine ($"Loading configuration from \"{options.Configuration.FullName}\"");
        s_defaultCoreSettings = JsonOptionsManager.DeserializeAsync<CoreOptions> (options.Configuration.FullName).Result;
      }
      else
      {
        System.Console.WriteLine ("No configuration file specified, using default configuration.");
        s_defaultCoreSettings = new CoreOptions (true);
      }

      s_headerExtractor = new LicenseHeaderExtractor();
      s_replacer = new LicenseHeaderReplacer (s_defaultCoreSettings.Languages, CoreOptions.RequiredKeywordsAsEnumerable (s_defaultCoreSettings.RequiredKeywords));
      UpdateLicenseHeaders (options);
    }

    private static void HandleParsingError (IEnumerable<Error> errors)
    {
      var errorList = errors.ToList();
      if (errorList.Count == 1 && errorList[0].Tag == ErrorType.HelpRequestedError)
        return;

      WriteLineColor ("The command-line invocation was syntactically incorrect. Aborting execution.", c_colorError);
      Exit (false);
    }

    /// <summary>
    ///   Updates the headers in the files of the given paths or in the files in the given directory, depending on the options passed.
    /// </summary>
    /// <param name="options">Encapsulates option that were passed to the application via the command line and provide information required to apply license headers.</param>
    private static void UpdateLicenseHeaders (Options options)
    {
      switch (options.Target)
      {
        case UpdateTarget.Files:
          UpdateLicenseHeadersForFiles (options.Mode, options.LicenseHeaderDefinitionFile, options.Files.ToList());
          break;
        case UpdateTarget.Directory:
          UpdateLicenseHeadersForDirectory (options.Mode, options.LicenseHeaderDefinitionFile, options.Directory, options.Recursive);
          break;
        default:
          WriteLineColor ("Cannot determine target of license header application (files, directory). Aborting execution.", c_colorError);
          Exit (true);
          break;
      }
    }

    /// <summary>
    ///   Updates the headers of one file or multiple files.
    /// </summary>
    /// <param name="mode">Specifies whether the license headers should be added or removed to/from the files.</param>
    /// <param name="headerDefinitionFile">Specifies the path to the license header definition file.</param>
    /// <param name="files">Specifies the path (or paths) to the files that should be updated.</param>
    private static void UpdateLicenseHeadersForFiles (UpdateMode mode, FileSystemInfo headerDefinitionFile, IReadOnlyList<FileInfo> files)
    {
      if (files.Count == 1)
        UpdateLicenseHeaderForOneFile (mode, headerDefinitionFile.FullName, files[0].FullName);
      else if (files.Count > 1)
        UpdateLicenseHeadersForMultipleFiles (mode, headerDefinitionFile.FullName, files.Select (x => x.FullName).ToList());
      else
        WriteLineColor ("No files given to add license headers to. Nothing to be done.", c_colorWarning);
    }

    /// <summary>
    ///   Updates the headers of one file.
    /// </summary>
    /// <param name="mode">Specifies whether the license headers should be added or removed to/from the files.</param>
    /// <param name="definitionFilePath">Specifies the path to the license header definition file.</param>
    /// <param name="filePath">Specifies the path to the file that should be updated.</param>
    private static void UpdateLicenseHeaderForOneFile (UpdateMode mode, string definitionFilePath, string filePath)
    {
      System.Console.WriteLine ($"Updating license headers for one file: \"{filePath}\".");
      System.Console.WriteLine ($"Using license header definition file: \"{definitionFilePath}\".");

      var headers = mode == UpdateMode.Add ? s_headerExtractor.ExtractHeaderDefinitions (definitionFilePath) : null;
      var replacerInput = new LicenseHeaderPathInput (filePath, headers);

      var replacerResult = s_replacer.RemoveOrReplaceHeader (replacerInput).Result;

      if (!replacerResult.IsSuccess)
      {
        WriteLineColor ($"\nAn error of type '{replacerResult.Error.Type}' occurred: '{replacerResult.Error.Description}'", c_colorError);
        Exit (false);
      }

      WriteLineColor ($"\n{(mode == UpdateMode.Add ? "Adding/Replacing" : "Removing")} succeeded.", c_colorSuccess);
      Exit (true);
    }

    /// <summary>
    ///   Updates the headers of multiple files.
    /// </summary>
    /// <param name="mode">Specifies whether the license headers should be added or removed to/from the files.</param>
    /// <param name="definitionFilePath">Specifies the path to the license header definition file.</param>
    /// <param name="filePaths">Specifies the paths to the files that should be updated.</param>
    private static void UpdateLicenseHeadersForMultipleFiles (UpdateMode mode, string definitionFilePath, IReadOnlyCollection<string> filePaths)
    {
      System.Console.WriteLine ($"Updating license headers for {filePaths.Count} files.");
      System.Console.WriteLine ($"Using license header definition file: \"{definitionFilePath}\".");

      var headers = mode == UpdateMode.Add ? s_headerExtractor.ExtractHeaderDefinitions (definitionFilePath) : null;
      var replacerInput = filePaths.Select (x => new LicenseHeaderPathInput (x, headers)).ToList();

      var replacerResult = s_replacer.RemoveOrReplaceHeader (
          replacerInput,
          new ConsoleProgress<ReplacerProgressReport> (
              progress => System.Console.WriteLine ($"File '{progress.ProcessedFilePath}' updated ({progress.ProcessedFileCount}/{progress.TotalFileCount})")),
          new CancellationToken()).Result;

      if (replacerResult.IsSuccess)
      {
        WriteLineColor ($"\n{(mode == UpdateMode.Add ? "Adding/Replacing" : "Removing")} succeeded.", c_colorSuccess);
        Exit (true);
      }

      foreach (var error in replacerResult.Error)
        WriteLineColor ($"\nAn error of type '{error.Type}' occurred: '{error.Description}'", c_colorError);

      Exit (false);
    }

    /// <summary>
    ///   Updates the headers of all files in the directory passed. If \"recursive\" is true, all files in the subfolders are updated as well.
    ///   Otherwise only the files in the top-level directory are updated.
    /// </summary>
    /// <param name="mode">Specifies whether the license headers should be added or removed to/from the files in the directory passed.</param>
    /// <param name="headerDefinitionFile"></param>
    /// <param name="directory">Specifies the path of the directory containing the files that should be updated.</param>
    /// <param name="recursive">Specifies whether the directory should be searched recursively.</param>
    private static void UpdateLicenseHeadersForDirectory (UpdateMode mode, FileSystemInfo headerDefinitionFile, DirectoryInfo directory, bool recursive)
    {
      System.Console.WriteLine (
          $"Searching for files in directory \"{directory.FullName}\" to apply license headers to ({(recursive ? string.Empty : "non-")}recursively).");

      var files = directory.EnumerateFiles ("*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
      System.Console.WriteLine ($"Found {files.Count} files.");

      UpdateLicenseHeadersForMultipleFiles (mode, headerDefinitionFile.FullName, files.Select (x => x.FullName).ToList());
    }

    /// <summary>
    ///   Writes string of text in a specifiable <see cref="ConsoleColor" /> to the console, with a trailing new line.
    ///   Resets the color to the default afterwards.
    /// </summary>
    /// <param name="line">The text to be written to the console.</param>
    /// <param name="color">The color the text should be written in.</param>
    private static void WriteLineColor (string line, ConsoleColor color)
    {
      System.Console.ForegroundColor = color;
      System.Console.WriteLine (line);
      System.Console.ResetColor();
    }

    /// <summary>
    ///   Terminates the console application and exits with the exit code according to the success parameter.
    /// </summary>
    /// <param name="success">Specifies whether the console application should terminate with the success exit code (true) or with the failure exit code (false).</param>
    private static void Exit (bool success)
    {
      Environment.Exit (success ? c_exitSuccess : c_exitError);
    }
  }
}
