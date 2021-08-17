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
  ///   Represents an expandable property similar to <see cref="DocumentHeaderProperty" /> whose value has already been
  ///   created.
  /// </summary>
  /// <remarks>
  ///   This class may be utilized in order to pass additional properties to <see cref="LicenseHeaderReplacer" />
  ///   which cannot be expanded by the Core itself.
  /// </remarks>
  /// <seealso cref="DocumentHeaderProperty" />
  /// <seealso cref="LicenseHeaderReplacer" />
  public class AdditionalProperty
  {
    /// <summary>
    ///   Initializes a new <see cref="AdditionalProperty" /> instance.
    /// </summary>
    /// <param name="token">
    ///   The token whose occurrences in a license header definition should be replaced by
    ///   <paramref name="value" />. E. g.: "%CustomProperty%"
    /// </param>
    /// <param name="value">
    ///   The value which should be used to replace the occurrences of <paramref name="token" /> in license
    ///   header definitions. E. g.: "Custom Value"
    /// </param>
    public AdditionalProperty (string token, string value)
    {
      Token = token;
      Value = value;
    }

    /// <summary>
    ///   The token whose occurrences in a license header definition should be replaced by <see cref="Token" />. E. g.:
    ///   "%CustomProperty%"
    /// </summary>
    public string Token { get; }

    /// <summary>
    ///   The value which should be used to replace the occurrences of <see cref="Token" /> in license header definitions.
    ///   e.g.: "Custom Value"
    /// </summary>
    public string Value { get; }
  }
}
