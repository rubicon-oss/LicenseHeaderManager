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
using NUnit.Framework;

namespace LicenseHeaderManager.Core.Tests
{
  [TestFixture]
  public class LicenseHeaderExtractorTest
  {
    private readonly List<string> _paths = new List<string>();

    [TearDown]
    public void TearDown ()
    {
      foreach (var path in _paths)
        File.Delete (path);
    }

    [Test]
    public void ExtractHeaderDefinitions_FilePathIsNull_ReturnsNull ()
    {
      string filePath = null;
      var extractor = new LicenseHeaderExtractor();
      var actual = extractor.ExtractHeaderDefinitions (filePath);

      Assert.That (actual, Is.Null);
    }

    [Test]
    public void ExtractHeaderDefinitions_ValidFilePathTextStartsWithExtension_ReturnsTwoHeaders ()
    {
      var filePath = CreateLicenseHeaderDefinitionFile();

      var extractor = new LicenseHeaderExtractor();
      var actual = extractor.ExtractHeaderDefinitions (filePath);

      Assert.That (actual.Count, Is.EqualTo (2));
    }

    [Test]
    public void ExtractHeaderDefinitions_ValidFilePathTextDoesNotStartWithExtension_ReturnsOneHeader ()
    {
      var filePath = CreateLicenseHeaderDefinitionFile (false);

      var extractor = new LicenseHeaderExtractor();
      var actual = extractor.ExtractHeaderDefinitions (filePath);

      Assert.That (actual.Count, Is.EqualTo (1));
    }

    [Test]
    public void ExtractHeaderDefinitions_ValidFilePathTextStartsWithExtension_ReturnsCorrectItems ()
    {
      var filePath = CreateLicenseHeaderDefinitionFile();

      var extractor = new LicenseHeaderExtractor();
      const string expectedCs =
          "/* Copyright (c) rubicon IT GmbH\r\n *\r\n * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the \"Software\"),\r\n * to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,\r\n * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:\r\n * \r\n * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.\r\n * \r\n * THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS\r\n * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,\r\n * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. \r\n */\r\n";
      const string expectedXaml =
          "<!--\r\nCopyright (c) rubicon IT GmbH\r\n\r\nPermission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the \"Software\"),\r\nto deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,\r\nand/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:\r\n\r\nThe above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.\r\n\r\nTHE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS\r\nFOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,\r\nWHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.\r\n-->\r\n";
      var actual = extractor.ExtractHeaderDefinitions (filePath);

      Assert.That (actual.Count, Is.EqualTo (2));
      Assert.That (actual[".cs"].JoinWithNewLine(), Is.EqualTo (expectedCs));
      Assert.That (actual[".xaml"].JoinWithNewLine(), Is.EqualTo (expectedXaml));
    }

    private string CreateLicenseHeaderDefinitionFile (bool startsWithExtension = true)
    {
      var licenseHeaderDefinitionFile = GetPath (".licenseheader");
      _paths.Add (licenseHeaderDefinitionFile);

      using (var fs = File.Create (licenseHeaderDefinitionFile))
      {
        var text =
            ".cs\r\n/* Copyright (c) rubicon IT GmbH\r\n *\r\n * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the \"Software\"),\r\n * to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,\r\n * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:\r\n * \r\n * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.\r\n * \r\n * THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS\r\n * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,\r\n * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. \r\n */\r\n\r\nextensions: .xaml\r\n<!--\r\nCopyright (c) rubicon IT GmbH\r\n\r\nPermission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the \"Software\"),\r\nto deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,\r\nand/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:\r\n\r\nThe above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.\r\n\r\nTHE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS\r\nFOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,\r\nWHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.\r\n-->\r\n";
        if (startsWithExtension)
          text = "extensions: " + text;

        var content = Encoding.UTF8.GetBytes (text);
        fs.Write (content, 0, content.Length);
      }

      return licenseHeaderDefinitionFile;
    }

    private string GetPath (string filename)
    {
      return Path.Combine (Path.GetTempPath(), Guid.NewGuid() + filename);
    }
  }
}
