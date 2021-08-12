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
using System.Collections.ObjectModel;
using System.Linq;

namespace Core.Options
{
  /// <summary>
  ///   Encapsulates members that represent the configuration affecting the behaviour of a
  ///   <see cref="LicenseHeaderReplacer" /> instance.
  /// </summary>
  /// <seealso cref="LicenseHeaderReplacer" />
  [LicenseHeaderManagerOptions]
  public class CoreOptions
  {
    public const bool DefaultUseRequiredKeywords = true;
    public const string DefaultRequiredKeywords = "license, copyright, (c), ©";

    public static readonly string DefaultLicenseHeaderFileText = new[]
                                                                 {
                                                                     "extensions: designer.cs generated.cs",
                                                                     "extensions: .cs .cpp .h",
                                                                     "// Copyright (c) 2011 rubicon IT GmbH",
                                                                     "extensions: .aspx .ascx",
                                                                     "<%-- ",
                                                                     "Copyright (c) 2011 rubicon IT GmbH",
                                                                     "--%>",
                                                                     "extensions: .vb",
                                                                     "'Sample license text.",
                                                                     "extensions:  .xml .config .xsd",
                                                                     "<!--",
                                                                     "Sample license text.",
                                                                     "-->"
                                                                 }.JoinWithNewLine();

    public static readonly ObservableCollection<Language> DefaultLanguages = new ObservableCollection<Language>
                                                                             {
                                                                                 new Language
                                                                                 {
                                                                                     Extensions = new[] { ".cs" }, LineComment = "//", BeginComment = "/*",
                                                                                     EndComment = "*/", BeginRegion = "#region", EndRegion = "#endregion"
                                                                                 },
                                                                                 new Language
                                                                                 {
                                                                                     Extensions = new[] { ".c", ".cpp", ".cxx", ".h", ".hpp" },
                                                                                     LineComment = "//", BeginComment = "/*", EndComment = "*/"
                                                                                 },
                                                                                 new Language
                                                                                 {
                                                                                     Extensions = new[] { ".vb" }, LineComment = "'", BeginRegion = "#Region",
                                                                                     EndRegion = "#End Region"
                                                                                 },
                                                                                 new Language
                                                                                 {
                                                                                     Extensions = new[] { ".aspx", ".ascx" }, BeginComment = "<%--",
                                                                                     EndComment = "--%>"
                                                                                 },
                                                                                 new Language
                                                                                 {
                                                                                     Extensions = new[]
                                                                                                  {
                                                                                                      ".htm", ".html", ".xhtml", ".xml", ".xaml", ".resx",
                                                                                                      ".config", ".xsd"
                                                                                                  },
                                                                                     BeginComment = "<!--", EndComment = "-->",
                                                                                     SkipExpression =
                                                                                         @"(<\?xml(.|\s)*?\?>)?(\s*<!DOCTYPE(.|\s)*?>)?( |\t)*(\n|\r\n|\r)?"
                                                                                 },
                                                                                 new Language { Extensions = new[] { ".css" }, BeginComment = "/*", EndComment = "*/" },
                                                                                 new Language
                                                                                 {
                                                                                     Extensions = new[] { ".js", ".ts" }, LineComment = "//", BeginComment = "/*",
                                                                                     EndComment = "*/",
                                                                                     SkipExpression = @"(/// *<reference.*/>( |\t)*(\n|\r\n|\r)?)*"
                                                                                 },
                                                                                 new Language
                                                                                 {
                                                                                     Extensions = new[] { ".sql" }, BeginComment = "/*", EndComment = "*/",
                                                                                     LineComment = "--"
                                                                                 },
                                                                                 new Language
                                                                                 {
                                                                                     Extensions = new[] { ".php" }, BeginComment = "/*", EndComment = "*/",
                                                                                     LineComment = "//"
                                                                                 },
                                                                                 new Language
                                                                                 {
                                                                                     Extensions = new[] { ".wxs", ".wxl", ".wxi" }, BeginComment = "<!--",
                                                                                     EndComment = "-->"
                                                                                 },
                                                                                 new Language
                                                                                 {
                                                                                     Extensions = new[] { ".py" }, BeginComment = "\"\"\"",
                                                                                     EndComment = "\"\"\""
                                                                                 },
                                                                                 new Language
                                                                                 {
                                                                                     Extensions = new[] { ".fs" }, BeginComment = "(*", EndComment = "*)",
                                                                                     LineComment = "//"
                                                                                 },
                                                                                 new Language
                                                                                 {
                                                                                     Extensions = new[] { ".cshtml", ".vbhtml" }, BeginComment = "@*",
                                                                                     EndComment = "*@"
                                                                                 }
                                                                             };

    /// <summary>
    ///   Initializes a new <see cref="CoreOptions" /> instance with its default values.
    /// </summary>
    public CoreOptions ()
    {
      SetDefaultValues();
    }

    /// <summary>
    ///   Initializes a new <see cref="CoreOptions" /> instance.
    /// </summary>
    /// <param name="initializeWithDefaultValues">
    ///   Determines whether default values as specified by static members of this
    ///   class should be used or if type-specific initialization values should be used (e. g. empty list for empty list for
    ///   enumerable types, <see langword="default(T)" /> for other types).
    /// </param>
    public CoreOptions (bool initializeWithDefaultValues)
    {
      if (initializeWithDefaultValues)
        SetDefaultValues();
      else
        InitializeValues();
    }

    /// <summary>
    ///   Gets or sets whether license header comments should be removed only if they contain at least one of the keywords
    ///   specified by <see cref="RequiredKeywords" />.
    /// </summary>
    public virtual bool UseRequiredKeywords { get; set; }

    /// <summary>
    ///   If <see cref="UseRequiredKeywords" /> is <see langword="true" />, only license header comments that contain at
    ///   least one of the words specified by this property (separated by "," and possibly whitespaces) are removed.
    /// </summary>
    public virtual string RequiredKeywords { get; set; }

    /// <summary>
    ///   Gets or sets the text to be used for newly created license header definition files.
    /// </summary>
    public virtual string LicenseHeaderFileText { get; set; }

    //public virtual string DefaultLicenseHeaderFileText { get; } = CoreOptionsRepository.GetDefaultLicenseHeader();

    /// <summary>
    ///   Gets or sets whether license headers are automatically inserted into new files.
    /// </summary>
    public virtual ObservableCollection<Language> Languages { get; set; }

    /// <summary>
    ///   Generates an <see cref="IEnumerable{T}" /> whose generic type argument is <see cref="string" /> that represent all keywords
    ///   in a specifiable string, with each recognized keyword being one entry in the enumerable.
    /// </summary>
    /// <param name="requiredKeywords">
    ///   The string containing keywords to be converted to an enumerable. Keywords are separated by "," and possibly whitespaces.
    /// </param>
    /// <returns>
    ///   Returns an <see cref="IEnumerable{T}" /> whose generic type argument is <see cref="string" /> that represent all keywords
    ///   recognized in <paramref name="requiredKeywords" />.
    /// </returns>
    public static IEnumerable<string> RequiredKeywordsAsEnumerable (string requiredKeywords)
    {
      return requiredKeywords.Split (new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select (k => k.Trim());
    }

    /// <summary>
    ///   Creates a deep copy of the current <see cref="CoreOptions" /> instance.
    /// </summary>
    /// <returns>A deep copy of the this <see cref="CoreOptions" /> instance.</returns>
    public virtual CoreOptions Clone ()
    {
      var clonedObject = new CoreOptions
                         {
                             UseRequiredKeywords = UseRequiredKeywords,
                             RequiredKeywords = RequiredKeywords,
                             LicenseHeaderFileText = LicenseHeaderFileText,
                             Languages = new ObservableCollection<Language> (Languages.Select (x => x.Clone()).ToList())
                         };

      return clonedObject;
    }

    /// <summary>
    ///   Sets all public members of this <see cref="CoreOptions" /> instance to pre-configured default values.
    /// </summary>
    /// <remarks>The default values are implementation-dependent.</remarks>
    private void SetDefaultValues ()
    {
      UseRequiredKeywords = DefaultUseRequiredKeywords;
      RequiredKeywords = DefaultRequiredKeywords;
      LicenseHeaderFileText = DefaultLicenseHeaderFileText;
      Languages = new ObservableCollection<Language> (DefaultLanguages);
    }

    /// <summary>
    ///   Initializes all public members of this <see cref="CoreOptions" /> instance.
    /// </summary>
    /// <remarks>The default values are implementation-dependent.</remarks>
    private void InitializeValues ()
    {
      Languages = new ObservableCollection<Language> (DefaultLanguages);
    }
  }
}
