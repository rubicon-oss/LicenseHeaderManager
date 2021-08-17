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
using LicenseHeaderManager.Core.Options;

namespace LicenseHeaderManager.Core.Tests
{
  /// <summary>
  ///   This option class that is used for testing purposes is not supported because there is no built-in
  ///   converter to (de-)serialize the <c>Dictionary&lt;int, string&gt;</c> type of the property <c>NotSupportedMember</c>.
  /// </summary>
  [LicenseHeaderManagerOptions]
  internal class NotSupportedOptions
  {
    public NotSupportedOptions ()
    {
      NotSupportedMember = new Dictionary<int, string>
                           {
                               { 1, "one" },
                               { 2, "two" }
                           };
    }

    public Dictionary<int, string> NotSupportedMember { get; set; }
  }
}
