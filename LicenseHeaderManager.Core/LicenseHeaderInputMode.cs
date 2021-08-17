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

namespace LicenseHeaderManager.Core
{
  /// <summary>
  ///   Identifies different kinds of input supported by the <see cref="LicenseHeaderReplacer" />.
  /// </summary>
  /// <seealso cref="LicenseHeaderReplacer" />
  internal enum LicenseHeaderInputMode
  {
    /// <summary>
    ///   License Headers are updated solely based on the input files' paths. Reading and writing files is done by the Core.
    /// </summary>
    FilePath = 0,

    /// <summary>
    ///   License Headers are updated solely based on the input files' contents. Only the content itself, represented as
    ///   <see cref="string" />, is manipulated.
    /// </summary>
    Content = 1
  }
}
