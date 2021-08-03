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
using EnvDTE;

namespace LicenseHeaderManager.ResultObjects
{
  /// <summary>
  ///   Represents result information of a folder and project wide invocation of "AddLicenseHeadersToAllFiles".
  /// </summary>
  public class AddLicenseHeaderToAllFilesResult
  {
    /// <summary>
    /// </summary>
    /// <param name="countSubLicenseHeadersFound"></param>
    /// <param name="baseHeaderFound"></param>
    /// <param name="linkedItems"></param>
    public AddLicenseHeaderToAllFilesResult (int countSubLicenseHeadersFound, bool baseHeaderFound, List<ProjectItem> linkedItems)
    {
      CountSubLicenseHeadersFound = countSubLicenseHeadersFound;
      BaseHeaderFound = baseHeaderFound;
      LinkedItems = linkedItems;
    }

    public int CountSubLicenseHeadersFound { get; set; }

    public bool BaseHeaderFound { get; set; }

    public List<ProjectItem> LinkedItems { get; set; }

    public bool NoHeaderFound => BaseHeaderFound && CountSubLicenseHeadersFound == 0;
  }
}
