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
  public class ReplacerResultTest
  {
    [Test]
    public void ReplacerResult_SuccessAndErrorInput_ReturnsError ()
    {
      var result = new ReplacerResult<ReplacerSuccess, ReplacerError<LicenseHeaderContentInput>> (
          new ReplacerError<LicenseHeaderContentInput> (null, ReplacerErrorType.Miscellaneous, "message"));

      Assert.That (result.IsSuccess, Is.False);
      Assert.That (result.Success, Is.Null);
      Assert.That (result.Error.Type, Is.EqualTo (ReplacerErrorType.Miscellaneous));
    }

    [Test]
    public void ReplacerResult_SuccessInput_ReturnsSuccess ()
    {
      const string filePath = @"C:\";
      const string newContent = "new content";

      var result = new ReplacerResult<ReplacerSuccess, ReplacerError<LicenseHeaderContentInput>> (
          new ReplacerSuccess (filePath, newContent));

      Assert.That (result.Success.FilePath, Is.EqualTo (filePath));
      Assert.That (result.Success.NewContent, Is.EqualTo (newContent));
      Assert.That (result.IsSuccess, Is.True);
      Assert.That (result.Error, Is.Null);
    }

    [Test]
    public void ReplacerResult_NoErrorInput_ReturnsSuccess ()
    {
      var result = new ReplacerResult<ReplacerError<LicenseHeaderContentInput>>();

      Assert.That (result.IsSuccess, Is.True);
      Assert.That (result.Error, Is.Null);
    }

    [Test]
    public void ReplacerResult_ErrorInput_ReturnsError ()
    {
      var result = new ReplacerResult<ReplacerError<LicenseHeaderContentInput>> (
          new ReplacerError<LicenseHeaderContentInput> (null, ReplacerErrorType.Miscellaneous, "message"));

      Assert.That (result.IsSuccess, Is.False);
      Assert.That (result.Error.Type, Is.EqualTo (ReplacerErrorType.Miscellaneous));
    }
  }
}
