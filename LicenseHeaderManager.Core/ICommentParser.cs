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
  public interface ICommentParser
  {
    /// <summary>
    ///   Parses a given text according to a language-specific comment syntax. Given a text, it extracts the comments.
    /// </summary>
    /// <param name="text">The text to be parsed.</param>
    /// <returns>Returns the comments contained in <paramref name="text" />, according to the language-specific comment syntax.</returns>
    /// <exception cref="ParseException">Thrown if parsing comments fails due to invalid syntax.</exception>
    /// <remarks>
    ///   The language-specific comment syntax may be configured using the constructor of an
    ///   <see cref="ICommentParser" /> implementation.
    /// </remarks>
    string Parse (string text);
  }
}
