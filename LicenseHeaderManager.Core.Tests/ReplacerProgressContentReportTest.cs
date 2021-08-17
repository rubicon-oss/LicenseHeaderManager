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
  public class ReplacerProgressContentReportTest
  {
    [Test]
    public void ReplacerProgressContentReports_ValidInput_ReturnsValidProperties ()
    {
      const int totalFileCount = -1;
      const int processedFileCount = -1;
      const string processedFilePath = @"C:\";
      const string processFileNewContent = "new content text";
      var replacerProgressContentReport = new ReplacerProgressContentReport (totalFileCount, processedFileCount, processedFilePath, processFileNewContent);


      var actualTotalFileCount = replacerProgressContentReport.TotalFileCount;
      var actualProcessedFileCount = replacerProgressContentReport.ProcessedFileCount;
      var actualProcessedFilePath = replacerProgressContentReport.ProcessedFilePath;
      var actualProcessFileNewContent = replacerProgressContentReport.ProcessFileNewContent;

      Assert.That (actualTotalFileCount, Is.EqualTo (totalFileCount));
      Assert.That (actualProcessedFileCount, Is.EqualTo (processedFileCount));
      Assert.That (actualProcessedFilePath, Is.EqualTo (processedFilePath));
      Assert.That (actualProcessFileNewContent, Is.EqualTo (processFileNewContent));
    }
  }
}
