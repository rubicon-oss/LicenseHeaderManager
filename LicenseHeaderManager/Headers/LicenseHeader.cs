using System;
using System.IO;
using System.Linq;
using System.Text;
using EnvDTE;
using System.Collections.Generic;

namespace LicenseHeaderManager.Headers
{
  public static class LicenseHeader
  {
    private const string c_keyword = "extensions:";
    public const string Cextension = ".licenseheader";

    public static string GetNewFileName (Project project)
    {
      string directory = Path.GetDirectoryName (project.FileName);
      string fileName = project.Name + Cextension;
      for (int i = 2; File.Exists (Path.Combine (directory, fileName)); i++)
        fileName = project.Name + i + Cextension;

      return fileName;
    }

    public static IEnumerable<string> GetLicenseHeaderDefinitions (Project project)
    {
      foreach (ProjectItem item in project.ProjectItems)
      {
        if (item.FileCount == 1)
        {
          string fileName = null;
          try
          {
            fileName = item.FileNames[0];
          }
          catch (ArgumentOutOfRangeException) { }
          if (fileName != null && Path.GetExtension (fileName).ToLower () == Cextension)
            yield return fileName;
        }
      }
    }

    public static IDictionary<string, string[]> GetLicenseHeaders (Project project)
    {
      IDictionary<string, string[]> headers = new Dictionary<string, string[]> ();
      
      var definitions = GetLicenseHeaderDefinitions (project);
      foreach (var definition in definitions)
        AddHeaders (headers, definition);

      return headers;
    }

    public static bool Validate (string header, Parser parser)
    {
      try
      {
        var result = parser.Parse (header);
        return result == header;
      }
      catch (ParseException)
      {
        return false;
      }
    }

    private static void AddHeaders (IDictionary<string, string[]> headers, string definition)
    {
      IEnumerable<string> extensions = null;
      IList<string> header = new List<string> ();
      
      foreach (var line in File.ReadAllLines (definition, Encoding.Default))
      {
        if (line.StartsWith (c_keyword))
        {
          if (extensions != null)
          {
            var array = header.ToArray ();
            foreach (var extension in extensions)
              headers[extension] = array;
          }
          extensions = line.Substring (c_keyword.Length).Split (new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select (AddDot);
          header.Clear ();
        }
        else
          header.Add (line);
      }
      if (extensions != null)
      {
        var array = header.ToArray ();
        foreach (var extension in extensions)
          headers[extension] = array;
      }
    }

    public static string AddDot (string extension)
    {
      if (extension.StartsWith("."))
        return extension;
      else
        return "." + extension;
    }
  }
}