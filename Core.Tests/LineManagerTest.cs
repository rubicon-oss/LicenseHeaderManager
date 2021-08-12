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

namespace Core.Tests
{
  [TestFixture]
  public class LineManagerTest
  {
    [Test]
    public void DetectMostFrequentLineEnd_InputTextIsNull_ThrowsArgumentNullException ()
    {
      string inputText = null;

      Assert.That (() => NewLineManager.DetectMostFrequentLineEnd (inputText), Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void DetectMostFrequentLineEnd_CRLF_ReturnsCRLF ()
    {
      const string textWithCRLF = "test\r\ntest";
      var actual = NewLineManager.DetectMostFrequentLineEnd (textWithCRLF);

      Assert.That (actual, Is.EqualTo ("\r\n"));
    }

    [Test]
    public void DetectMostFrequentLineEnd_CR_ReturnsCR ()
    {
      const string textWithCR = "test\rtest";
      var actual = NewLineManager.DetectMostFrequentLineEnd (textWithCR);

      Assert.That (actual, Is.EqualTo ("\r"));
    }

    [Test]
    public void DetectMostFrequentLineEnd_LF_ReturnsLF ()
    {
      const string textWithLF = "test\ntest";
      var actual = NewLineManager.DetectMostFrequentLineEnd (textWithLF);

      Assert.That (actual, Is.EqualTo ("\n"));
    }

    [Test]
    public void DetectMostFrequentLineEnd_DifferentCRLF_ReturnsFullLineEnding ()
    {
      const string textWithDifferentCRLF = "test\n\r\n\r\n test\r\n te\r restasd";
      var actual = NewLineManager.DetectMostFrequentLineEnd (textWithDifferentCRLF);

      Assert.That (actual, Is.EqualTo ("\r\n"));
    }

    [Test]
    [TestCase ("t\ne\ns\ntte\r\nst\r\nest te\r\nsttest test\ntest test", "\n")]
    [TestCase ("t\ne\ns\ntte\r\nst\r\nest te\r\nsttest test\rtest test", "\r\n")]
    [TestCase ("t\ne\ns\ntte\r\nst\r\nest te\r\nsttest test\rte\rst\r t\rest", "\r")]
    public void DetectMostFrequentLineEnd_DifferentEndings_ReturnsMostFrequentEnding (string text, string mostFrequentEnding)
    {
      var actual = NewLineManager.DetectMostFrequentLineEnd (text);

      Assert.That (actual, Is.EqualTo (mostFrequentEnding));
    }

    [Test]
    public void ReplaceAllLineEnds_InputTextIsNull_ThrowsArgumentNullException ()
    {
      string inputText = null;
      var newLineEnd = string.Empty;

      Assert.That (() => NewLineManager.ReplaceAllLineEnds (inputText, newLineEnd), Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void ReplaceAllLineEnds_NewLineEndIsNull_ThrowsArgumentNullException ()
    {
      var inputText = string.Empty;
      string newLineEnd = null;

      Assert.That (() => NewLineManager.ReplaceAllLineEnds (inputText, newLineEnd), Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void ReplaceAllLineEnds_NewLineEndIsEmpty_ReturnsTextWithoutLineEnds ()
    {
      const string text1 = "test1";
      const string text2 = "test2";
      const string text3 = "test3";
      const string text4 = "test4";
      const string fulltext = text1 + "\r\n" + text2 + "\n" + text3 + "\r" + text4;
      var actual = NewLineManager.ReplaceAllLineEnds (fulltext, string.Empty);

      Assert.That (actual, Is.EqualTo (text1 + text2 + text3 + text4));
    }

    [Test]
    public void NextLineEndPosition_InputTextNull_ThrowsArgumentNullException ()
    {
      string inputText = null;
      const int startIndex = -1;

      Assert.That (() => NewLineManager.NextLineEndPosition (inputText, startIndex), Throws.InstanceOf<ArgumentNullException>());
    }

    [TestCase ("testtext\n\rtest\r\n te\r restasd", 0, 8)]
    [TestCase ("testtext\n\rtest\r\n te\r restasd", 13, 14)]
    public void NextLineEndPosition_GivenStartIndex_ReturnsCorrectPosition (string inputText, int startIndex, int expectedPosition)
    {
      var actual = NewLineManager.NextLineEndPosition (inputText, startIndex);

      Assert.That (actual, Is.EqualTo (expectedPosition));
    }

    [Test]
    public void NextLineEndPosition_GivenStartIndexAndCountAndInputTextNull_ThrowsArgumentNullException ()
    {
      string inputText = null;
      const int startIndex = 0;
      const int count = -1;

      Assert.That (() => NewLineManager.NextLineEndPosition (inputText, startIndex, count), Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void NextLineEndPositionInformation_GivenStartIndexAndCountAndInputTextNull_ThrowsArgumentNullException ()
    {
      string inputText = null;
      const int startIndex = 0;
      const int count = -1;

      Assert.That (() => NewLineManager.NextLineEndPositionInformation (inputText, startIndex, count), Throws.InstanceOf<ArgumentNullException>());
    }
  }
}
