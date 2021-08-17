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
using NUnit.Framework;

namespace LicenseHeaderManager.Core.Tests
{
  [TestFixture]
  public class StringExtensionsTest
  {
    [Test]
    public void InsertDotIfNecessary_ExtensionStartsWithLeadingDot_ReturnsExtensionWithLeadingDot ()
    {
      const string extension = ".cs";
      var actual = extension.InsertDotIfNecessary();
      Assert.That (actual, Is.EqualTo (extension));
    }

    [Test]
    public void InsertDotIfNecessary_ExtensionDoesNotStartWithLeadingDot_ReturnsExtensionWithLeadingDot ()
    {
      const string extension = "cs";
      var actual = extension.InsertDotIfNecessary();
      Assert.That (actual, Is.EqualTo (".cs"));
    }

    [Test]
    public void ReplaceNewLines_InputWithNewLine_ReturnsReplacedInput ()
    {
      const string extension = @"test\ntext";
      var actual = extension.ReplaceNewLines();
      Assert.That (actual, Is.EqualTo ("test\ntext"));
    }

    [Test]
    public void ReplaceNewLines_InputWithoutNewLine_ReturnsInput ()
    {
      const string extension = "test text";
      var actual = extension.ReplaceNewLines();
      Assert.That (actual, Is.EqualTo ("test text"));
    }

    [Test]
    public void CountOccurrence_InputStringIsNull_ThrowsArgumentNullException ()
    {
      const string input = null;

      Assert.That (() => input.CountOccurrences ("e"), Throws.ArgumentNullException);
    }

    [Test]
    public void CountOccurrence_SearchStringIsNull_ThrowsArgumentNullException ()
    {
      const string input = "test text";

      Assert.That (() => input.CountOccurrences (null), Throws.ArgumentNullException);
    }

    [Test]
    public void CountOccurrence__Returns ()
    {
      const string input = "test text";
      var actual = input.CountOccurrences ("e");

      Assert.That (actual, Is.EqualTo (2));
    }
  }
}
