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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace LicenseHeaderManager.Core.Tests
{
  [TestFixture]
  public class DocumentTest
  {
    private List<string> _paths;
    private Language _language;
    private IDictionary<string, string[]> _headers;

    [SetUp]
    public void Setup ()
    {
      _paths = new List<string>();
      _language = new Language
                  {
                      Extensions = new List<string> { ".cs" },
                      LineComment = "//",
                      BeginComment = "/*",
                      EndComment = "*/",
                      BeginRegion = "",
                      EndRegion = "",
                      SkipExpression = ""
                  };
      _headers = new Dictionary<string, string[]>();
    }

    [TearDown]
    public void TearDown ()
    {
      foreach (var path in _paths)
        File.Delete (path);
    }

    [Test]
    public async Task ValidateHeader_EmptyHeader_ReturnsTrue ()
    {
      var document = new Document (new LicenseHeaderPathInput ("", null), new Language(), null);
      var isValidHeader = await document.ValidateHeader();

      Assert.That (isValidHeader, Is.True);
    }

    [Test]
    public async Task ValidateHeader_NonEmptyAndValidHeader_ReturnsTrue ()
    {
      var headerLines = new[] { "// Copyright (c) 2011 rubicon IT GmbH" };

      var document = new Document (new LicenseHeaderPathInput (CreateTestFile(), null), _language, headerLines);
      var isValidHeader = await document.ValidateHeader();

      Assert.That (isValidHeader, Is.True);
    }

    [Test]
    public async Task ValidateHeader_InvalidHeaderEndRegion_ReturnsFalse ()
    {
      var headerLines = new[] { "#endregion" };
      _language.EndRegion = "#endregion";

      var document = new Document (new LicenseHeaderPathInput (CreateTestFile(), null), _language, headerLines);
      var isValidHeader = await document.ValidateHeader();

      Assert.That (isValidHeader, Is.False);
    }

    [Test]
    public async Task ValidateHeader_NonEmptyAndInvalidHeader_ReturnsFalse ()
    {
      var headerLines = new[] { "Invalid header" };
      var document = new Document (new LicenseHeaderPathInput (CreateTestFile(), null), _language, headerLines);
      var isValidHeader = await document.ValidateHeader();

      Assert.That (isValidHeader, Is.False);
    }

    [Test]
    public void ReplaceHeaderIfNecessaryContent_PathInputMode_ThrowsInvalidOperationException ()
    {
      var document = new Document (new LicenseHeaderPathInput (CreateTestFile(), null), _language, null);

      Assert.That (async () => await document.ReplaceHeaderIfNecessaryContent (new CancellationToken()), Throws.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public void ReplaceHeaderIfNecessaryContent_ContentInputMode_ReturnsContent ()
    {
      const string testContent = "test content";
      var document = new Document (new LicenseHeaderContentInput (testContent, CreateTestFile(), _headers), _language, null);

      Assert.That (async () => await document.ReplaceHeaderIfNecessaryContent (new CancellationToken()), Is.EqualTo (testContent));
    }

    [Test]
    public void ReplaceHeaderIfNecessaryPath_ContentInputMode_ThrowsInvalidOperationException ()
    {
      const string testContent = "test content";
      var document = new Document (new LicenseHeaderContentInput (testContent, CreateTestFile(), _headers), _language, null);

      Assert.That (async () => await document.ReplaceHeaderIfNecessaryPath (new CancellationToken()), Throws.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public void ReplaceHeaderIfNecessaryPath_PathInputMode_DoesNotThrowException ()
    {
      var document = new Document (new LicenseHeaderPathInput (CreateTestFile(), _headers), _language, null);

      Assert.That (async () => await document.ReplaceHeaderIfNecessaryPath (new CancellationToken()), Throws.Nothing);
    }

    [Test]
    public void ReplaceHeaderIfNecessaryPath_PathInputModeWithMatchingSkipExpression_DoesNotThrowException ()
    {
      var document = new Document (new LicenseHeaderPathInput (CreateTestFile(), _headers), _language, null);
      _language.SkipExpression = "@/";

      Assert.That (async () => await document.ReplaceHeaderIfNecessaryPath (new CancellationToken()), Throws.Nothing);
    }

    [Test]
    public void ReplaceHeaderIfNecessaryPath_PathInputModeWithMatchingSkipExpressionAndHeaderLines_DoesNotThrowException ()
    {
      var document = new Document (new LicenseHeaderPathInput (CreateTestFile(), _headers), _language, new[] { "test" });
      _language.SkipExpression = "@/";

      Assert.That (async () => await document.ReplaceHeaderIfNecessaryPath (new CancellationToken()), Throws.Nothing);
    }

    [Test]
    public void ReplaceHeaderIfNecessaryContent_ContentInputModeAndKeywordsNotNullAndContentNotContainingKeyword_ReturnsComment ()
    {
      const string testContent = "//test content";
      var document = new Document (new LicenseHeaderContentInput (testContent, CreateTestFile(), _headers), _language, null, null, new List<string> { "copyright" });

      Assert.That (async () => await document.ReplaceHeaderIfNecessaryContent (new CancellationToken()), Is.EqualTo (testContent));
    }

    [Test]
    public void ReplaceHeaderIfNecessaryContent_ContentInputModeAndKeywordsNotNullAndContentContainsKeyword_ReturnsEmpty ()
    {
      const string testContent = "//test content copyright";
      var document = new Document (new LicenseHeaderContentInput (testContent, CreateTestFile(), _headers), _language, null, null, new List<string> { "copyright" });

      Assert.That (async () => await document.ReplaceHeaderIfNecessaryContent (new CancellationToken()), Is.Empty);
    }

    [Test]
    public void ReplaceHeaderIfNecessaryPath_PathInputModeWithNonMatchingSkipExpressionAndHeaderLines_DoesNotThrowException ()
    {
      var document = new Document (new LicenseHeaderPathInput (CreateTestFile(), _headers), _language, new[] { "test" });
      _language.SkipExpression = "/@";

      Assert.That (async () => await document.ReplaceHeaderIfNecessaryPath (new CancellationToken()), Throws.Nothing);
    }

    [Test]
    public void ReplaceHeaderIfNecessaryContent_ContentInputModeWithMatchingSkipExpression_ReturnsContent ()
    {
      const string testContent = "@/ test content";
      _language.SkipExpression = "@/";
      var document = new Document (new LicenseHeaderContentInput (testContent, CreateTestFile(), _headers), _language, null, null, new List<string>());

      Assert.That (async () => await document.ReplaceHeaderIfNecessaryContent (new CancellationToken()), Is.EqualTo (testContent));
    }

    private string CreateTestFile ()
    {
      var testFile = Path.Combine (Path.GetTempPath(), Guid.NewGuid() + ".cs");
      _paths.Add (testFile);

      using (var fs = File.Create (testFile))
      {
        const string text = "@/testText";
        var content = Encoding.UTF8.GetBytes (text);
        fs.Write (content, 0, content.Length);
      }

      return testFile;
    }
  }
}
