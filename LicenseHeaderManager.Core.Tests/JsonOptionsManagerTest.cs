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
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using LicenseHeaderManager.Core.Options;
using NUnit.Framework;

namespace LicenseHeaderManager.Core.Tests
{
  [TestFixture]
  public class JsonOptionsManagerTest
  {
    private List<string> _paths;
    private CoreOptions _options;

    [SetUp]
    public void Setup ()
    {
      _paths = new List<string>();
      _options = new CoreOptions();
    }

    [TearDown]
    public void TearDown ()
    {
      foreach (var path in _paths)
        File.Delete (path);
    }

    [Test]
    public void Deserialize_InvalidTypeParameter_ThrowsArgumentException ()
    {
      Assert.That (async () => await JsonOptionsManager.DeserializeAsync<string> (""), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public async Task DeserializeAsync_ValidTypeParameter_ReturnsContent ()
    {
      var testFilePath = CreateTestFile();
      var content = await JsonOptionsManager.DeserializeAsync<CoreOptions> (testFilePath);
      Assert.That (content, Is.TypeOf (typeof (CoreOptions)));
      Assert.That (content.UseRequiredKeywords, Is.False);
    }

    [Test]
    public void Deserialize_NoFileStream_ThrowsArgumentNullException ()
    {
      Assert.That (
          async () => await JsonOptionsManager.DeserializeAsync<CoreOptions> (null),
          Throws.InstanceOf<SerializationException>().With.Message.EqualTo ("File stream for deserializing configuration was not present"));
    }

    [Test]
    public void Deserialize_NoFile_ThrowsFileNotFoundException ()
    {
      Assert.That (
          async () => await JsonOptionsManager.DeserializeAsync<CoreOptions> (Path.Combine (Path.GetTempPath(), Guid.NewGuid() + ".json")),
          Throws.InstanceOf<SerializationException>().With.Message.EqualTo ("File to deserialize configuration from was not found"));
    }

    [Test]
    public void Deserialize_NotValidFormat_ThrowsJsonException ()
    {
      var testFilePath = CreateTestFile ("Invalid format text");
      Assert.That (
          async () => await JsonOptionsManager.DeserializeAsync<CoreOptions> (testFilePath),
          Throws.InstanceOf<SerializationException>().With.Message.EqualTo ("The file content is not in a valid format"));
    }

    [Test]
    public void Deserialize_EmptyPath_ThrowsException ()
    {
      Assert.That (
          async () => await JsonOptionsManager.DeserializeAsync<CoreOptions> (""),
          Throws.InstanceOf<SerializationException>().With.Message.EqualTo ("An unspecified error occurred while deserializing configuration"));
    }

    [Test]
    public void SerializeAsync_InvalidTypeParameter_ThrowsArgumentException ()
    {
      Assert.That (
          async () => await JsonOptionsManager.SerializeAsync ("{\r\n}", Path.Combine (Path.GetTempPath(), Guid.NewGuid() + ".json")),
          Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public void SerializeAsync_ValidTypeParameter_DoesNotThrowException ()
    {
      var testFile = Path.Combine (Path.GetTempPath(), Guid.NewGuid() + ".json");
      _paths.Add (testFile);
      Assert.That (async () => await JsonOptionsManager.SerializeAsync (_options, testFile), Throws.Nothing);
    }

    [Test]
    public void SerializeAsync_EmptyPath_ThrowsException ()
    {
      Assert.That (
          async () => await JsonOptionsManager.SerializeAsync (_options, null),
          Throws.InstanceOf<SerializationException>().With.Message.EqualTo ("An unspecified error occurred while deserializing configuration"));
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
