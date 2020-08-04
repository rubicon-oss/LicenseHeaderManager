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
using System.IO;

namespace Core
{
  /// <summary>
  ///   Provides members to parse and extract information describing license headers from given license header definition files.
  /// </summary>
  public interface ILicenseHeaderExtractor
  {
    /// <summary>
    ///   Extracts license header definitions from a license header definition file represented by its path.
    /// </summary>
    /// <param name="definitionFilePath">
    ///   The path to the license header definition file whose header definitions should be
    ///   extracted.
    /// </param>
    /// <exception cref="FileNotFoundException">If the file represented by <paramref name="definitionFilePath" /> does not exists.</exception>
    /// <returns>
    ///   Returns a <see cref="Dictionary{TKey,TValue}" /> whose keys are extensions identifying a language and whose
    ///   values represent the license header definition for the corresponding language. If the dictionary is <see langword="null" />, a
    ///   respective file's header is to be removed.
    /// </returns>
    Dictionary<string, string[]> ExtractHeaderDefinitions (string definitionFilePath);
  }
}
