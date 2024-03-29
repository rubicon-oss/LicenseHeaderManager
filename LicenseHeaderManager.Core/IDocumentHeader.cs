﻿/* Copyright (c) rubicon IT GmbH
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
using System.IO;

namespace LicenseHeaderManager.Core
{
  public interface IDocumentHeader
  {
    /// <summary>
    ///   Gets a value denoting the header represented by this <see cref="DocumentHeader" /> is non-existing, i. e. if the
    ///   <see cref="Text" /> property is null.
    /// </summary>
    bool IsEmpty { get; }

    /// <summary>
    ///   Gets the <see cref="FileInfo" /> object associated with the file this <see cref="DocumentHeader" /> refers to.
    /// </summary>
    FileInfo FileInfo { get; }

    /// <summary>
    ///   Gets the effective text denoting the license header text represented by this <see cref="DocumentHeader" /> instance,
    ///   with expandable properties having been replaced.
    /// </summary>
    string Text { get; }
  }
}
