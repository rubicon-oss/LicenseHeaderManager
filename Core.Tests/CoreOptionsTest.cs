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
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Core.Options;
using NUnit.Framework;

namespace Core.Tests
{
  [TestFixture]
  public class CoreOptionsTest
  {
    private List<string> _paths;
    private CoreOptions _options;
    private ObservableCollection<Language> _languages;

    [SetUp]
    public void Setup ()
    {
      _paths = new List<string>();
      _options = new CoreOptions (true);
      _languages = _options.Languages;
    }

    [TearDown]
    public void TearDown ()
    {
      foreach (var path in _paths)
        File.Delete (path);
    }

    [Test]
    public void CoreOptions_ConstructorInitializeWithDefaultValuesDefaultLicenseHeaderNotExistent_ReturnsDefaultLicenseHeader ()
    {
      _options = new CoreOptions();

      Assert.That (
          _options.LicenseHeaderFileText,
          Is.EqualTo (
              "extensions: designer.cs generated.cs\r\nextensions: .cs .cpp .h\r\n// Copyright (c) 2011 rubicon IT GmbH\r\nextensions: .aspx .ascx\r\n<%-- \r\nCopyright (c) 2011 rubicon IT GmbH\r\n--%>\r\nextensions: .vb\r\n'Sample license text.\r\nextensions:  .xml .config .xsd\r\n<!--\r\nSample license text.\r\n-->"));
    }

    [Test]
    public void RequiredKeywordsAsEnumerable_ValidInput_ReturnsListOfRequiredKeywords ()
    {
      IEnumerable<string> keywords = new List<string> { "license", "copyright", "test", "(c)", "©" };
      var actual = CoreOptions.RequiredKeywordsAsEnumerable ("license, copyright,test,(c), ©");
      Assert.That (keywords, Is.EqualTo (actual));
    }

    [Test]
    public void SaveAsync_ValidInput_DoesNotThrowException ()
    {
      var testFile = Path.Combine (Path.GetTempPath(), Guid.NewGuid() + ".json");
      _paths.Add (testFile);
      Assert.That (async () => await CoreOptionsRepository.SaveAsync (_options, testFile), Throws.Nothing);
    }

    [Test]
    public async Task LoadAsync_ValidInput_DoesNotThrowException ()
    {
      var options = await CoreOptionsRepository.LoadAsync (CreateTestFile());
      Assert.That (options, Is.TypeOf (typeof (CoreOptions)));
      Assert.That (options.UseRequiredKeywords, Is.False);
    }

    [Test]
    public void Clone_ExistingAndValidCoreOptions_ReturnsClonedOptions ()
    {
      _options.UseRequiredKeywords = false;
      _options.RequiredKeywords = "new, keywords";
      _options.LicenseHeaderFileText = "sample text";
      _options.Languages = _languages;

      var actual = _options.Clone();
      Assert.That (_options.UseRequiredKeywords, Is.EqualTo (actual.UseRequiredKeywords));
      Assert.That (_options.RequiredKeywords, Is.EqualTo (actual.RequiredKeywords));
      Assert.That (_options.LicenseHeaderFileText, Is.EqualTo (actual.LicenseHeaderFileText));
      Assert.That (_options.Languages.Count, Is.EqualTo (actual.Languages.Count));
    }

    private string CreateTestFile (string text = null)
    {
      var testFile = Path.Combine (Path.GetTempPath(), Guid.NewGuid() + ".json");
      _paths.Add (testFile);

      using (var fs = File.Create (testFile))
      {
        if (text == null)
          text = "{\r\n\"useRequiredKeywords\": false\r\n}";
        var content = Encoding.UTF8.GetBytes (text);
        fs.Write (content, 0, content.Length);
      }

      return testFile;
    }
  }
}
