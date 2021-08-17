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
  ///   Provides a type for a result from <see cref="LicenseHeaderReplacer" /> representing a success (including further
  ///   information) or failed license header update operation (including further information).
  /// </summary>
  /// <typeparam name="TError">Type of the additional information describing a potential error.</typeparam>
  /// <typeparam name="TSuccess">Type of the additional information describing a potential success.</typeparam>
  /// <seealso cref="LicenseHeaderReplacer" />
  public class ReplacerResult<TSuccess, TError> : ReplacerResult<TError>
  {
    /// <summary>
    ///   Initializes a new <see cref="ReplacerResult{TError}" /> instance with an error.
    /// </summary>
    /// <param name="error">Additional information describing the error that occurred.</param>
    public ReplacerResult (TError error)
        : base (error)
    {
      Success = default;
    }

    /// <summary>
    ///   Initializes a new <see cref="ReplacerResult{TError}" /> instance with additional information about the successful
    ///   license header update operation.
    /// </summary>
    /// <param name="success">Additional information describing the successful operation.</param>
    public ReplacerResult (TSuccess success)
    {
      IsSuccess = true;
      Error = default;
      Success = success;
    }

    public TSuccess Success { get; }
  }

  /// <summary>
  ///   Provides a type for a result from <see cref="LicenseHeaderReplacer" /> representing a success (without any further
  ///   information) or failed license header update operation (including further information).
  /// </summary>
  /// <typeparam name="TError">Type of the additional information describing a potential error.</typeparam>
  /// <seealso cref="LicenseHeaderReplacer" />
  public class ReplacerResult<TError> : ReplacerResult
  {
    /// <summary>
    ///   Initializes a new successful <see cref="ReplacerResult{TError}" /> instance.
    /// </summary>
    public ReplacerResult ()
    {
      IsSuccess = true;
      Error = default;
    }

    /// <summary>
    ///   Initializes a new <see cref="ReplacerResult{TError}" /> instance with an error.
    /// </summary>
    /// <param name="error">Additional information describing the error that occurred.</param>
    public ReplacerResult (TError error)
    {
      Error = error;
      IsSuccess = false;
    }

    /// <summary>
    ///   Gets the error that occurred during the license header update operation.
    /// </summary>
    public TError Error { get; protected set; }
  }

  /// <summary>
  ///   Provides a base type for result objects to be returned from <see cref="LicenseHeaderReplacer" />.
  /// </summary>
  /// <seealso cref="LicenseHeaderReplacer" />
  public abstract class ReplacerResult
  {
    /// <summary>
    ///   Determines whether the license header update operation was successful.
    /// </summary>
    public bool IsSuccess { get; protected set; }
  }
}
