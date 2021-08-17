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
  public partial class LicenseHeaderReplacer
  {
    /// <summary>
    ///   Represents the result of a <see cref="TryCreateDocument" /> invocation.
    /// </summary>
    internal enum CreateDocumentResult
    {
      /// <summary>
      ///   Document was created successfully.
      /// </summary>
      DocumentCreated,

      /// <summary>
      ///   The file that denotes the basis of the document does not exists.
      /// </summary>
      FileNotFound,

      /// <summary>
      ///   The <see cref="LicenseHeaderReplacer" /> instance was not initialized with a <see cref="Language" /> instance
      ///   representing the document's language, as determined by its file extension.
      /// </summary>
      LanguageNotFound,

      /// <summary>
      ///   Header definition for the language specified by the document's extension was not found in the license header
      ///   definition file.
      /// </summary>
      NoHeaderFound,

      /// <summary>
      ///   The document is a license header definition document.
      /// </summary>
      LicenseHeaderDocument,

      /// <summary>
      ///   The header definition for the language specified by the document's extension was found in the license header
      ///   definition, but is null or empty.
      /// </summary>
      EmptyHeader
    }
  }
}
