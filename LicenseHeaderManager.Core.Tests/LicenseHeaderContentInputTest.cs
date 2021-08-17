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
using NUnit.Framework;

namespace LicenseHeaderManager.Core.Tests
{
  [TestFixture]
  public class LicenseHeaderContentInputTest : LicenseHeaderInputBaseTest
  {
    [Test]
    public void LicenseHeaderContentInput_ValidInput_ReturnsCorrectProperties ()
    {
      var headers = new Dictionary<string, string[]>();
      var additionalProperties = new List<AdditionalProperty> { new AdditionalProperty ("%Prop%", "Value") };
      const string extension = ".cs";
      var documentPath = CreateTestFile();

      var licenseHeaderContentInput = new LicenseHeaderContentInput ("", documentPath, headers, additionalProperties);

      var actualHeaders = licenseHeaderContentInput.Headers;
      var actualAdditionalProperties = licenseHeaderContentInput.AdditionalProperties;
      var actualIgnoreNonCommentText = licenseHeaderContentInput.IgnoreNonCommentText;
      var actualDocumentPath = licenseHeaderContentInput.DocumentPath;
      var actualExtension = licenseHeaderContentInput.Extension;
      var actualInputMode = licenseHeaderContentInput.InputMode;
      var actualContent = licenseHeaderContentInput.DocumentContent;

      Assert.That (actualHeaders, Is.EqualTo (headers));
      Assert.That (actualAdditionalProperties, Is.EqualTo (additionalProperties));
      Assert.That (actualIgnoreNonCommentText, Is.False);
      Assert.That (actualDocumentPath, Is.EqualTo (documentPath));
      Assert.That (actualExtension, Is.EqualTo (extension));
      Assert.That (actualInputMode, Is.EqualTo (LicenseHeaderInputMode.Content));
      Assert.That (actualContent, Is.EqualTo (string.Empty));
    }
  }
}
