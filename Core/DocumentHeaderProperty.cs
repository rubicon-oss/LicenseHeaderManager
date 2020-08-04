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

namespace Core
{
  /// <summary>
  ///   Represents an property that can be expanded while updating license headers.
  /// </summary>
  internal class DocumentHeaderProperty
  {
    private readonly Predicate<IDocumentHeader> _canCreateValue;
    private readonly Func<IDocumentHeader, string> _createValue;

    /// <summary>
    ///   Initializes a new <see cref="DocumentHeaderProperty" /> instance.
    /// </summary>
    /// <param name="token">
    ///   The token of the property. It represents the string that is replaced in a license header definition
    ///   while update license headers.
    /// </param>
    /// <param name="canCreateValue">
    ///   A predicate representing constraints that must be satisfied in order to create the value
    ///   to replace <paramref name="token" /> with.
    /// </param>
    /// <param name="createValue">
    ///   A func that provides the value to replace <paramref name="token" /> with, given that
    ///   <paramref name="canCreateValue" /> is satisfied.
    /// </param>
    public DocumentHeaderProperty (string token, Predicate<IDocumentHeader> canCreateValue, Func<IDocumentHeader, string> createValue)
    {
      Token = token;
      _canCreateValue = canCreateValue;
      _createValue = createValue;
    }

    /// <summary>
    ///   The token of the property. It represents the string that is replaced in a license header definition while update
    ///   license headers.
    /// </summary>
    public string Token { get; }

    /// <summary>
    ///   A predicate representing constraints that must be satisfied in order to create the value to replace
    ///   <see cref="Token" /> with.
    /// </summary>
    /// <param name="documentHeader">
    ///   A <see cref="DocumentHeader" /> instance that might be necessary to evaluate determine the
    ///   return value
    /// </param>
    /// <returns>
    ///   Returns <see langword="true" /> if the value of this <see cref="DocumentHeaderProperty" /> can be created,
    ///   otherwise <see langword="false" />.
    /// </returns>
    public bool CanCreateValue (IDocumentHeader documentHeader)
    {
      return _canCreateValue (documentHeader);
    }

    /// <summary>
    ///   A func that provides the value to replace <see cref="Token" /> with, given that <see cref="CanCreateValue" /> is
    ///   satisfied.
    /// </summary>
    /// <param name="documentHeader">
    ///   A <see cref="DocumentHeader" /> instance that might be necessary to evaluate determine the
    ///   return value
    /// </param>
    /// <returns>Returns the value of this <see cref="DocumentHeaderProperty" />.</returns>
    public string CreateValue (IDocumentHeader documentHeader)
    {
      return _createValue (documentHeader);
    }
  }
}
