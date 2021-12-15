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

namespace LicenseHeaderManager.Options
{
  /// <summary>
  ///   Encapsulates logic to be executed when upgrading from an earlier LHM version to a newer one.
  /// </summary>
  public class UpdateStep
  {
    /// <summary>
    ///   Initializes a new <see cref="UpdateStep" /> instance.
    /// </summary>
    /// <param name="targetVersion">The version this <see cref="UpdateStep" /> updates to.</param>
    /// <param name="customUpdateActions">
    ///   The actions to be taken in order to achieve the update to the version specified by
    ///   <paramref name="targetVersion" />.
    /// </param>
    public UpdateStep (Version targetVersion, params Action[] customUpdateActions)
    {
      TargetVersion = targetVersion;
      CustomUpdateActions = customUpdateActions;
    }

    /// <summary>
    ///   The target version this <see cref="UpdateStep" /> instance updates to.
    /// </summary>
    public Version TargetVersion { get; }

    /// <summary>
    ///   The actions that need to be taken in order to perform the update to the version specified by
    ///   <see cref="TargetVersion" />.
    /// </summary>
    public Action[] CustomUpdateActions { get; }

    /// <summary>
    ///   Executes all update actions required for the update, as specified by <see cref="CustomUpdateActions" />
    /// </summary>
    public void ExecuteActions ()
    {
      foreach (var customUpdateStep in CustomUpdateActions)
        customUpdateStep();
    }
  }
}
