/* Copyright (c) rubicon IT GmbH
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LicenseHeaderManager.Core
{
  public class LicenseHeaderExtractor : ILicenseHeaderExtractor
  {
    /// <summary>
    ///   The file extension of License Header Definition files.
    /// </summary>
    public const string HeaderDefinitionExtension = ".licenseheader";

    /// <summary>
    ///   The text representing the start of the listing of extensions belonging to one license header definition within a
    ///   license header definition file.
    /// </summary>
    internal const string ExtensionKeyword = "extensions:";

    public Dictionary<string, string[]> ExtractHeaderDefinitions (string definitionFilePath)
    {
      if (string.IsNullOrEmpty (definitionFilePath))
        return null;

      var headers = new Dictionary<string, string[]>();
      AddHeaders (headers, definitionFilePath);
      return headers;
    }

    private static void AddHeaders (IDictionary<string, string[]> headers, string headerFilePath)
    {
      IEnumerable<string> extensions = null;
      IList<string> header = new List<string>();

      var wholeFile = File.ReadAllText (headerFilePath);

      using (var reader = new StreamReader (headerFilePath, true))
      {
        while (!reader.EndOfStream)
        {
          var line = reader.ReadLine();
          if (line != null && line.StartsWith (ExtensionKeyword))
          {
            UpdateHeaders (headers, extensions, header);

            extensions = line.Substring (ExtensionKeyword.Length).Split (new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select (extension => extension.InsertDotIfNecessary());
            header.Clear();
          }
          else
          {
            header.Add (line);
          }
        }

        if (wholeFile.EndsWith (NewLineConst.CR) || wholeFile.EndsWith (NewLineConst.LF))
          header.Add (string.Empty);
      }

      UpdateHeaders (headers, extensions, header);
    }

    private static void UpdateHeaders (IDictionary<string, string[]> headers, IEnumerable<string> extensions, IEnumerable<string> header)
    {
      if (extensions == null)
        return;

      var array = header.ToArray();
      foreach (var extension in extensions)
        headers[extension] = array;
    }
  }
}
