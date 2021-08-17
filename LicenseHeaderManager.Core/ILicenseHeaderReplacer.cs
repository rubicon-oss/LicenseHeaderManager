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
using System.Threading;
using System.Threading.Tasks;

namespace LicenseHeaderManager.Core
{
  public interface ILicenseHeaderReplacer
  {
    Language GetLanguageFromExtension (string extension);

    /// <summary>
    ///   Determines whether a file is a generally valid input file for
    ///   <see cref="RemoveOrReplaceHeader(LicenseHeaderPathInput)" /> or
    ///   <see
    ///     cref="RemoveOrReplaceHeader(System.Collections.Generic.ICollection{Core.LicenseHeaderPathInput},IProgress{ReplacerProgressReport},CancellationToken)" />
    ///   .
    /// </summary>
    /// <param name="path">The path to the file to be examined.</param>
    /// <returns>
    ///   Returns <see langword="true" /> if the file specified by <paramref name="path" /> is a valid path input,
    ///   otherwise false.
    /// </returns>
    /// <remarks>
    ///   A return value of <see langword="true" /> does not necessarily mean that an invocation of
    ///   <see cref="RemoveOrReplaceHeader(LicenseHeaderPathInput)" /> or
    ///   <see
    ///     cref="RemoveOrReplaceHeader(System.Collections.Generic.ICollection{LicenseHeaderManager.Core.LicenseHeaderPathInput},IProgress{ReplacerProgressReport},CancellationToken)" />
    ///   would not yield a <see cref="ReplacerErrorType.NonCommentText" /> or <see cref="ReplacerErrorType.ParsingError" />
    ///   error, but only that is a fundamentally valid input - i. e. the file exists and this
    ///   <see cref="LicenseHeaderReplacer" /> instance is able to interpret it in the way required.
    /// </remarks>
    bool IsValidPathInput (string path);

    /// <summary>
    ///   Updates license headers in a file based on its path, i. e. the new content is written directly to the file.
    /// </summary>
    /// <param name="licenseHeaderInput">
    ///   An <see cref="LicenseHeaderContentInput" /> instance representing the file whose
    ///   license headers are to be updated.
    /// </param>
    /// <returns>
    ///   Returns a <see cref="ReplacerResult{TSuccess,TError}" /> instance containing information about the success or
    ///   error of the operation.
    /// </returns>
    Task<ReplacerResult<ReplacerError<LicenseHeaderPathInput>>> RemoveOrReplaceHeader (LicenseHeaderPathInput licenseHeaderInput);

    /// <summary>
    ///   Updates license headers of files based on their contents, i. e. the new contents are returned. Setting the new
    ///   contents is the caller's responsibility.
    /// </summary>
    /// <param name="licenseHeaderInputs">
    ///   An range of <see cref="LicenseHeaderContentInput" /> instances representing the files
    ///   whose license headers are to be updated.
    /// </param>
    /// <param name="progress">
    ///   A <see cref="IProgress{T}" /> whose generic type parameter is
    ///   <see cref="ReplacerProgressContentReport" /> that invokes callbacks for each reported progress value.
    /// </param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
    /// <returns>
    ///   Returns a range of <see cref="ReplacerResult{TSuccess,TError}" /> instances containing information about the
    ///   success or error of the operations.
    /// </returns>
    Task<IEnumerable<ReplacerResult<ReplacerSuccess, ReplacerError<LicenseHeaderContentInput>>>> RemoveOrReplaceHeader (
        ICollection<LicenseHeaderContentInput> licenseHeaderInputs,
        IProgress<ReplacerProgressContentReport> progress,
        CancellationToken cancellationToken);

    /// <summary>
    ///   Updates license headers of files based on their paths, i. e. the new contents written directly to the files.
    /// </summary>
    /// <param name="licenseHeaderInputs">
    ///   An range of <see cref="LicenseHeaderPathInput" /> instances representing the files
    ///   whose license headers are to be updated.
    /// </param>
    /// <param name="progress">
    ///   A <see cref="IProgress{T}" /> whose generic type parameter is
    ///   <see cref="ReplacerProgressReport" /> that invokes callbacks for each reported progress value.
    /// </param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
    /// <returns>
    ///   Returns a <see cref="ReplacerResult{TSuccess,TError}" /> instance containing information about errors that
    ///   have potentially occurred during the update operations.
    /// </returns>
    Task<ReplacerResult<IEnumerable<ReplacerError<LicenseHeaderPathInput>>>> RemoveOrReplaceHeader (
        ICollection<LicenseHeaderPathInput> licenseHeaderInputs,
        IProgress<ReplacerProgressReport> progress,
        CancellationToken cancellationToken);
  }
}
