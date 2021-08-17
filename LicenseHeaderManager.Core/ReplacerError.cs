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
  ///   Represents an error that occurred during updating license headers, along with the input that provoked it.
  /// </summary>
  /// <seealso cref="LicenseHeaderReplacer" />
  /// <typeparam name="TInput">Type of input the <see cref="LicenseHeaderReplacer" />was invoked with.</typeparam>
  public class ReplacerError<TInput>
      where TInput : LicenseHeaderInput
  {
    /// <summary>
    ///   Initializes a new <see cref="ReplacerError{TInput}" /> instance.
    /// </summary>
    /// <param name="input">The input the <see cref="LicenseHeaderReplacer" />was invoked with.</param>
    /// <param name="type">The type of error that occurred.</param>
    /// <param name="description">A description of the error that occurred.</param>
    public ReplacerError (TInput input, ReplacerErrorType type, string description)
    {
      Input = input;
      Type = type;
      Description = description;
    }

    /// <summary>
    ///   The type of error that occurred.
    /// </summary>
    public ReplacerErrorType Type { get; }

    /// <summary>
    ///   A description of the error that occurred.
    /// </summary>
    public string Description { get; }

    /// <summary>
    ///   Gets the <see cref="TInput" /> instance the <see cref="LicenseHeaderReplacer" /> was invoked with.
    /// </summary>
    public TInput Input { get; }
  }
}
