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

namespace Core.Tests
{
  [TestFixture]
  public class PathUtilityTest
  {
    private List<string> _paths;

    [SetUp]
    public void Setup ()
    {
      _paths = new List<string>();
    }

    [TearDown]
    public void TearDown ()
    {
      foreach (var path in _paths)
        File.Delete (path);
    }

    [Test]
    public void GetProperFilePathCapitalization_InputIsNull_ThrowsArgumentNullException ()
    {
      FileInfo fileInfo = null;

      Assert.Throws<ArgumentNullException> (() => PathUtility.GetProperFilePathCapitalization (fileInfo));
    }

    [Test]
    [TestCase ("AAA")]
    [TestCase ("aaa")]
    [TestCase ("AaA")]
    public void GetProperFilePathCapitalization_ValidFileInfo_ReturnsFilePath (string fileName)
    {
      var fullPath = Path.Combine (Path.GetTempPath(), fileName + ".cs");
      var fileInfo = new FileInfo (CreateTestFile (fullPath));

      var actual = PathUtility.GetProperFilePathCapitalization (fileInfo);

      Assert.That (actual, Is.EqualTo (fullPath));
    }

    private string CreateTestFile (string fileName)
    {
      var testFile = Path.Combine (Path.GetTempPath(), fileName);
      _paths.Add (testFile);

      using (var fs = File.Create (testFile))
      {
        var content = Encoding.UTF8.GetBytes ("");
        fs.Write (content, 0, content.Length);
      }

      return testFile;
    }
  }
}
