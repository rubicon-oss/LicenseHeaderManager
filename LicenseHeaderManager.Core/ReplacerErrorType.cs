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
  ///   Identifies different types of errors that might occur while updating license headers.
  /// </summary>
  /// <seealso cref="LicenseHeaderReplacer" />
  public enum ReplacerErrorType
  {
    /// <summary>
    ///   Specifies that the license header definition file contains text that is not configured as comment for the file's
    ///   language.
    /// </summary>
    NonCommentText,

    /// <summary>
    ///   Specifies that no configuration could be found for the file's language.
    /// </summary>
    LanguageNotFound,

    /// <summary>
    ///   Specifies that an error occurred while parsing the license header that was found in the file.
    /// </summary>
    ParsingError,

    /// <summary>
    ///   Specifies that the file's language currently has no header defined in the license header definition file.
    /// </summary>
    NoHeaderFound,

    /// <summary>
    ///   An unspecified error occurred.
    /// </summary>
    Miscellaneous,

    /// <summary>
    ///   Specifies that the header definition for the file's language is null or empty.
    /// </summary>
    EmptyHeader,

    /// <summary>
    ///   Specifies that the file could not be found, i. e. it does not exist.
    /// </summary>
    FileNotFound,

    /// <summary>
    ///   Specifies that the operation was cancelled because the file is a license header definition document.
    /// </summary>
    LicenseHeaderDocument
  }
}
