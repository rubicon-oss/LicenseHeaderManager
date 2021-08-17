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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace LicenseHeaderManager.Core.Tests
{
  [TestFixture]
  public class LicenseHeaderReplacerTest
  {
    private List<string> _paths;
    private List<Language> _languages;

    [SetUp]
    public void Setup ()
    {
      _paths = new List<string>();
      _languages = new List<Language>
                   {
                       new Language
                       {
                           Extensions = new[] { ".cs" }, LineComment = "//", BeginComment = "/*", EndComment = "*/", BeginRegion = "#region",
                           EndRegion = "#endregion"
                       },
                       new Language
                       {
                           Extensions = new[] { ".js", ".ts" }, LineComment = "//", BeginComment = "/*", EndComment = "*/",
                           SkipExpression = @"(/// *<reference.*/>( |\t)*(\n|\r\n|\r)?)*"
                       }
                   };
    }

    [TearDown]
    public void TearDown ()
    {
      foreach (var path in _paths)
        File.Delete (path);
    }

    [Test]
    public void GetLanguageFromExtension_LanguagesAreEmpty_ReturnsNull ()
    {
      var replacer = new LicenseHeaderReplacer (Enumerable.Empty<Language>(), Enumerable.Empty<string>());

      var language = replacer.GetLanguageFromExtension (".cs");

      Assert.That (language, Is.Null);
    }

    [Test]
    public void GetLanguageFromExtension_LanguagesAreNull_DoesNotThrowExceptionAndReturnsNull ()
    {
      var replacer = new LicenseHeaderReplacer (null, Enumerable.Empty<string>());

      Language language = null;

      Assert.That (() => language = replacer.GetLanguageFromExtension (".cs"), Throws.Nothing);
      Assert.That (language, Is.Null);
    }

    [Test]
    public void IsValidPathInput_FileExistsValidDocument_ReturnsTrue ()
    {
      var filePath = @"C:\Windows\explorer.exe";
      var replacer = new ReplacerStub (filePath, true);

      var isValid = replacer.IsValidPathInput (filePath);
      Assert.That (isValid, Is.True);
    }

    [Test]
    public void IsValidPathInput_FileExistsInvalidDocument_ReturnsFalse ()
    {
      var filePath = @"C:\Windows\explorer.exe";
      var replacer = new ReplacerStub (filePath, false);

      var isValid = replacer.IsValidPathInput (filePath);
      Assert.That (isValid, Is.False);
    }

    [Test]
    public async Task RemoveOrReplaceHeader_PathIsNull_ReturnsReplacerResultFileNotFound ()
    {
      var replacer = new LicenseHeaderReplacer (Enumerable.Empty<Language>(), Enumerable.Empty<string>());

      var actual = await replacer.RemoveOrReplaceHeader (new LicenseHeaderPathInput (null, null));

      Assert.That (actual.Error.Type, Is.EqualTo (ReplacerErrorType.FileNotFound));
    }

    [Test]
    public async Task RemoveOrReplaceHeader_DocumentIsLicenseHeaderFile_ReturnsReplacerResultLicenseHeaderDocument ()
    {
      var replacer = new LicenseHeaderReplacer (Enumerable.Empty<Language>(), Enumerable.Empty<string>());
      var path = CreateTestFile();

      var actual = await replacer.RemoveOrReplaceHeader (new LicenseHeaderPathInput (path, null));

      Assert.That (actual.Error.Type, Is.EqualTo (ReplacerErrorType.LicenseHeaderDocument));
    }

    [Test]
    public async Task RemoveOrReplaceHeader_PathInputWithoutHeader_ReturnsReplacerResultNoHeaderFound ()
    {
      var replacer = new LicenseHeaderReplacer (_languages, Enumerable.Empty<string>());
      var path = CreateTestFile (".cs");

      var actual = await replacer.RemoveOrReplaceHeader (new LicenseHeaderPathInput (path, new Dictionary<string, string[]>()));

      Assert.That (actual.Error.Type, Is.EqualTo (ReplacerErrorType.NoHeaderFound));
    }

    [Test]
    public async Task RemoveOrReplaceHeader_PathInputEmptyHeader_ReturnsReplacerResultEmptyHeader ()
    {
      var replacer = new LicenseHeaderReplacer (_languages, Enumerable.Empty<string>());
      var path = CreateTestFile (".cs");
      var headers = new Dictionary<string, string[]> { { ".cs", new[] { "" } } };

      var actual = await replacer.RemoveOrReplaceHeader (new LicenseHeaderPathInput (path, headers));

      Assert.That (actual.Error.Type, Is.EqualTo (ReplacerErrorType.EmptyHeader));
    }

    [Test]
    public async Task RemoveOrReplaceHeader_LanguageNotPresent_ReturnsReplacerResultLanguageNotFound ()
    {
      var replacer = new LicenseHeaderReplacer (Enumerable.Empty<Language>(), Enumerable.Empty<string>());
      var path = CreateTestFile (".cs");

      var actual = await replacer.RemoveOrReplaceHeader (new LicenseHeaderPathInput (path, null));

      Assert.That (actual.Error.Type, Is.EqualTo (ReplacerErrorType.LanguageNotFound));
    }

    [Test]
    public void RemoveOrReplaceHeader_ValidInput_DoesNotThrowExceptionAndReturnsSuccess ()
    {
      var replacer = new LicenseHeaderReplacer (_languages, Enumerable.Empty<string>());
      var path = CreateTestFile (".cs");
      var headers = new Dictionary<string, string[]> { { ".cs", new[] { "// first line 1", "// second line", "// copyright" } } };

      ReplacerResult actual = null;

      Assert.That (async () => actual = await replacer.RemoveOrReplaceHeader (new LicenseHeaderPathInput (path, headers)), Throws.Nothing);
      Assert.That (actual.IsSuccess, Is.True);
    }

    [Test]
    public async Task RemoveOrReplaceHeader_PathInputHeadersWithNonCommentText_ReturnsReplacerResultNonCommentText ()
    {
      var replacer = new LicenseHeaderReplacer (_languages, Enumerable.Empty<string>());
      var path = CreateTestFile (".cs");
      var headers = new Dictionary<string, string[]> { { ".cs", new[] { "// first line 1", "// second line", "copyright" } } };

      var actual = await replacer.RemoveOrReplaceHeader (new LicenseHeaderPathInput (path, headers));
      Assert.That (actual.Error.Type, Is.EqualTo (ReplacerErrorType.NonCommentText));
    }

    [Test]
    public async Task RemoveOrReplaceHeader_PathInputLanguagesWithExtensionNull_ReturnsReplacerResultMiscellaneous ()
    {
      var languages = new List<Language>
                      {
                          new Language
                          {
                              Extensions = null, LineComment = "//", BeginComment = "/*", EndComment = "*/", BeginRegion = "#region",
                              EndRegion = "#endregion"
                          }
                      };
      var replacer = new LicenseHeaderReplacer (languages, Enumerable.Empty<string>());
      var path = CreateTestFile (".cs");
      var headers = new Dictionary<string, string[]> { { ".cs", new[] { "// first line 1", "// second line", "copyright" } } };

      var actual = await replacer.RemoveOrReplaceHeader (new LicenseHeaderPathInput (path, headers));
      Assert.That (actual.Error.Type, Is.EqualTo (ReplacerErrorType.Miscellaneous));
    }

    [Test]
    public async Task RemoveOrReplaceHeader_ContentInputHeadersWithNonCommentText_ReturnsReplacerResultNonCommentText ()
    {
      var replacer = new LicenseHeaderReplacer (_languages, Enumerable.Empty<string>());
      var headers = new Dictionary<string, string[]> { { ".cs", new[] { "// first line 1", "// second line", "copyright" } } };
      var licenseHeaderInputs = new List<LicenseHeaderContentInput>
                                {
                                    new LicenseHeaderContentInput ("test content1", CreateTestFile (".cs"), headers)
                                };

      var actualResults = (await replacer.RemoveOrReplaceHeader (licenseHeaderInputs, new Progress<ReplacerProgressContentReport>(), new CancellationToken())).ToList();

      Assert.That (actualResults.Count, Is.EqualTo (1));
      Assert.That (actualResults.Single().IsSuccess, Is.False);
      Assert.That (actualResults.Single().Error.Type, Is.EqualTo (ReplacerErrorType.NonCommentText));
    }

    [Test]
    public async Task RemoveOrReplaceHeader_ValidContentInput_DoesNotThrowExceptionAndReturnsSuccess ()
    {
      var replacer = new LicenseHeaderReplacer (_languages, Enumerable.Empty<string>());
      var path = CreateTestFile (".cs");
      var headers = new Dictionary<string, string[]> { { ".cs", new[] { "// first line", "// copyright" } } };

      var actual = await replacer.RemoveOrReplaceHeader (new LicenseHeaderContentInput ("test content", path, headers));

      Assert.That (actual.IsSuccess, Is.True);
    }

    [Test]
    public async Task RemoveOrReplaceHeader_ContentInputWithoutHeader_ReturnsReplacerResultNoHeaderFound ()
    {
      var replacer = new LicenseHeaderReplacer (_languages, Enumerable.Empty<string>());
      var path = CreateTestFile (".cs");

      var actual = await replacer.RemoveOrReplaceHeader (new LicenseHeaderContentInput ("test content", path, new Dictionary<string, string[]>()));

      Assert.That (actual.Error.Type, Is.EqualTo (ReplacerErrorType.NoHeaderFound));
    }

    [Test]
    public async Task RemoveOrReplaceHeader_ContentInputInvalidHeader_ReturnsReplacerResultParsingError ()
    {
      var replacer = new LicenseHeaderReplacer (_languages, Enumerable.Empty<string>());
      var path = CreateTestFile (".cs", true);
      var headers = new Dictionary<string, string[]> { { ".cs", new[] { "// first line", "// copyright" } } };

      var actual = await replacer.RemoveOrReplaceHeader (new LicenseHeaderContentInput ("#endregion", path, headers) { IgnoreNonCommentText = true });

      Assert.That (actual.Error.Type, Is.EqualTo (ReplacerErrorType.ParsingError));
    }

    [Test]
    public async Task RemoveOrReplaceHeader_LicenseHeaderContentInputsValid_ReturnsReplacerResultSuccess ()
    {
      var replacer = new LicenseHeaderReplacer (_languages, Enumerable.Empty<string>());
      var headers = new Dictionary<string, string[]>
                    {
                        { ".cs", new[] { "// first line", "// copyright" } },
                        { ".ts", new[] { "// first line", "// copyright" } }
                    };
      var licenseHeaderInputs = new List<LicenseHeaderContentInput>
                                {
                                    new LicenseHeaderContentInput ("test content1", CreateTestFile (".cs"), headers),
                                    new LicenseHeaderContentInput ("test content2", CreateTestFile (".ts"), headers)
                                };

      var actualResults = (await replacer.RemoveOrReplaceHeader (licenseHeaderInputs, new Progress<ReplacerProgressContentReport>(), new CancellationToken())).ToList();

      Assert.That (actualResults.Count, Is.EqualTo (2));
      foreach (var result in actualResults)
        Assert.That (result.IsSuccess, Is.True);
    }

    [Test]
    public async Task RemoveOrReplaceHeader_LicenseHeaderPathInputsValid_ReturnsReplacerResultSuccess ()
    {
      var replacer = new LicenseHeaderReplacer (_languages, Enumerable.Empty<string>());
      var path = CreateTestFile (".cs");
      var headers = new Dictionary<string, string[]> { { ".cs", new[] { "// first line 1", "// second line", "// copyright" } } };

      var licenseHeaderInputs = new List<LicenseHeaderPathInput>
                                {
                                    new LicenseHeaderPathInput (path, headers)
                                };

      var actualResults = await replacer.RemoveOrReplaceHeader (licenseHeaderInputs, new Progress<ReplacerProgressReport>(), new CancellationToken());

      Assert.That (actualResults.Error, Is.Null);
      Assert.That (actualResults.IsSuccess, Is.True);
    }

    [Test]
    public async Task RemoveOrReplaceHeader_LicenseHeaderPathInputsValidLicenseHeaderDefinitionFile_ReturnsReplacerResultSuccess ()
    {
      var replacer = new LicenseHeaderReplacer (_languages, Enumerable.Empty<string>());
      var path = CreateTestFile();
      var headers = new Dictionary<string, string[]> { { ".cs", new[] { "// first line 1", "// second line", "// copyright" } } };

      var licenseHeaderInputs = new List<LicenseHeaderPathInput>
                                {
                                    new LicenseHeaderPathInput (path, headers)
                                };

      var actualResults = await replacer.RemoveOrReplaceHeader (licenseHeaderInputs, new Progress<ReplacerProgressReport>(), new CancellationToken());

      Assert.That (actualResults.Error, Is.Null);
      Assert.That (actualResults.IsSuccess, Is.True);
    }

    [Test]
    public async Task RemoveOrReplaceHeader_PathInputWithNonCommentText_ReturnsReplacerResultNonCommentText ()
    {
      var replacer = new LicenseHeaderReplacer (_languages, Enumerable.Empty<string>());
      var path = CreateTestFile (".cs");
      var headers = new Dictionary<string, string[]> { { ".cs", new[] { "first line 1" } } };

      var licenseHeaderInputs = new List<LicenseHeaderPathInput>
                                {
                                    new LicenseHeaderPathInput (path, headers)
                                };

      var actualResults = await replacer.RemoveOrReplaceHeader (licenseHeaderInputs, new Progress<ReplacerProgressReport>(), new CancellationToken());

      Assert.That (actualResults.IsSuccess, Is.False);
      Assert.That (actualResults.Error, Has.Count.EqualTo (1));
      Assert.That (actualResults.Error.Single().Type, Is.EqualTo (ReplacerErrorType.NonCommentText));
    }

    [Test]
    public async Task RemoveOrReplaceHeader_LicenseHeaderContentInputsInvalid_ReturnsNoReplacerResult ()
    {
      var replacer = new LicenseHeaderReplacer (_languages, Enumerable.Empty<string>());
      var licenseHeaderInputs = new List<LicenseHeaderContentInput>
                                {
                                    new LicenseHeaderContentInput ("test content3", CreateTestFile (".js"), new Dictionary<string, string[]>())
                                };

      var actualResults = (await replacer.RemoveOrReplaceHeader (licenseHeaderInputs, new Progress<ReplacerProgressContentReport>(), new CancellationToken())).ToList();

      Assert.That (actualResults.Count, Is.EqualTo (0));
    }

    [Test]
    public async Task RemoveOrReplaceHeader_ContentInputWithInvalidHeader_ReturnsReplacerResultParsingError ()
    {
      var replacer = new LicenseHeaderReplacer (_languages, Enumerable.Empty<string>());
      var headers = new Dictionary<string, string[]>
                    {
                        { ".cs", new[] { "// test header" } }
                    };
      var licenseHeaderInputs = new List<LicenseHeaderContentInput>
                                {
                                    new LicenseHeaderContentInput ("#endregion", CreateTestFile (".cs"), headers) { IgnoreNonCommentText = true }
                                };

      var actualResults = (await replacer.RemoveOrReplaceHeader (licenseHeaderInputs, new Progress<ReplacerProgressContentReport>(), new CancellationToken())).ToList();
      var replacerResult = actualResults[0];

      Assert.That (replacerResult.IsSuccess, Is.False);
      Assert.That (replacerResult.Error.Type, Is.EqualTo (ReplacerErrorType.ParsingError));
    }

    [Test]
    public async Task RemoveOrReplaceHeader_PathInputWithInvalidHeader_ReturnsReplacerResultParsingError ()
    {
      var replacer = new LicenseHeaderReplacer (_languages, Enumerable.Empty<string>());
      var headers = new Dictionary<string, string[]> { { ".cs", new[] { "// first line" } } };
      var licenseHeaderInputs = new List<LicenseHeaderPathInput>
                                {
                                    new LicenseHeaderPathInput (CreateTestFile (".cs", true), headers) { IgnoreNonCommentText = true }
                                };

      var actualResults = await replacer.RemoveOrReplaceHeader (licenseHeaderInputs, new Progress<ReplacerProgressReport>(), new CancellationToken());

      Assert.That (actualResults.IsSuccess, Is.False);
      Assert.That (actualResults.Error, Has.Count.EqualTo (1));
      Assert.That (actualResults.Error.Single().Type, Is.EqualTo (ReplacerErrorType.ParsingError));
    }

    private string CreateTestFile (string extension = null, bool withContent = false)
    {
      if (extension == null)
        extension = ".licenseheader";

      var testFile = Path.Combine (Path.GetTempPath(), Guid.NewGuid() + extension);
      _paths.Add (testFile);

      using (var fs = File.Create (testFile))
      {
        var content = Encoding.UTF8.GetBytes (withContent ? "#endregion" : "");
        fs.Write (content, 0, content.Length);
      }

      return testFile;
    }
  }

  internal class ReplacerStub : LicenseHeaderReplacer
  {
    private readonly string _path;
    private readonly bool _validDocument;

    public ReplacerStub (string path, bool validDocument)
    {
      _path = path;
      _validDocument = validDocument;
    }

    internal override CreateDocumentResult TryCreateDocument (LicenseHeaderInput licenseHeaderInput, out Document document)
    {
      document = null;
      if (licenseHeaderInput.DocumentPath == _path && _validDocument)
        return CreateDocumentResult.DocumentCreated;
      return CreateDocumentResult.NoHeaderFound;
    }
  }
}
