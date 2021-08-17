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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LicenseHeaderManager.Core
{
  /// <summary>
  ///   Encapsulates multiple <see cref="DocumentHeaderProperty" /> objects in an enumerable data structure.
  /// </summary>
  internal class DocumentHeaderProperties : IEnumerable<DocumentHeaderProperty>
  {
    private readonly IEnumerable<DocumentHeaderProperty> _properties;

    /// <summary>
    ///   Initializes this <see cref="DocumentHeaderProperties" /> instance with a given range of
    ///   <see cref="AdditionalProperty" /> objects. Other properties that can be expanded by the Core itself, are created in
    ///   the process as well.
    /// </summary>
    /// <param name="additionalProperties">The additional properties to be used for initialization.</param>
    public DocumentHeaderProperties (IEnumerable<AdditionalProperty> additionalProperties = null)
    {
      _properties = CreateProperties (additionalProperties);
    }

    public IEnumerator<DocumentHeaderProperty> GetEnumerator ()
    {
      return _properties.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator ()
    {
      return GetEnumerator();
    }

    /// <summary>
    ///   Creates all properties that are used while inserting/replacing/removing license headers via a
    ///   <see cref="LicenseHeaderReplacer" /> instance. This also entails predefined properties that can be expanded by the
    ///   Core itself.
    /// </summary>
    /// <param name="additionalProperties">
    ///   The additional properties to be used when updating license headers that cannot be
    ///   replaced by the Core itself.
    /// </param>
    /// <returns>
    ///   Returns a <see cref="IEnumerable{T}" /> whose generic type argument is <see cref="DocumentHeaderProperty" />
    ///   that may used to provide an enumerator for this <see cref="DocumentHeaderProperties" /> instance.
    /// </returns>
    private IEnumerable<DocumentHeaderProperty> CreateProperties (IEnumerable<AdditionalProperty> additionalProperties)
    {
      var properties = new List<DocumentHeaderProperty>
                       {
                           new DocumentHeaderProperty (
                               "%FullFileName%",
                               documentHeader => documentHeader.FileInfo != null,
                               documentHeader => GetProperFilePathCapitalization (documentHeader.FileInfo)),
                           new DocumentHeaderProperty (
                               "%FileName%",
                               documentHeader => documentHeader.FileInfo != null,
                               documentHeader => GetProperFileNameCapitalization (documentHeader.FileInfo)),
                           new DocumentHeaderProperty (
                               "%CreationYear%",
                               documentHeader => documentHeader.FileInfo != null,
                               documentHeader => documentHeader.FileInfo.CreationTime.Year.ToString()),
                           new DocumentHeaderProperty (
                               "%CreationMonth%",
                               documentHeader => documentHeader.FileInfo != null,
                               documentHeader => documentHeader.FileInfo.CreationTime.Month.ToString()),
                           new DocumentHeaderProperty (
                               "%CreationDay%",
                               documentHeader => documentHeader.FileInfo != null,
                               documentHeader => documentHeader.FileInfo.CreationTime.Day.ToString()),
                           new DocumentHeaderProperty (
                               "%CreationTime%",
                               documentHeader => documentHeader.FileInfo != null,
                               documentHeader => documentHeader.FileInfo.CreationTime.ToShortTimeString()),
                           new DocumentHeaderProperty (
                               "%CurrentYear%",
                               documentHeader => true,
                               documentHeader => DateTime.Now.Year.ToString()),
                           new DocumentHeaderProperty (
                               "%CurrentMonth%",
                               documentHeader => true,
                               documentHeader => DateTime.Now.Month.ToString()),
                           new DocumentHeaderProperty (
                               "%CurrentDay%",
                               documentHeader => true,
                               documentHeader => DateTime.Now.Day.ToString()),
                           new DocumentHeaderProperty (
                               "%CurrentTime%",
                               documentHeader => true,
                               documentHeader => DateTime.Now.ToShortTimeString()),
                           new DocumentHeaderProperty (
                               "%UserName%",
                               documentHeader => UserInfo.Name != null,
                               documentHeader => UserInfo.Name),
                           new DocumentHeaderProperty (
                               "%UserDisplayName%",
                               documentHeader => UserInfo.DisplayName != null,
                               documentHeader => UserInfo.DisplayName)
                       };

      if (additionalProperties != null)
        properties.AddRange (
            additionalProperties.Select (
                property => new DocumentHeaderProperty (
                    property.Token,
                    documentHeader => true,
                    documentHeader => property.Value)));

      return properties;
    }

    private string GetProperFilePathCapitalization (FileInfo fileInfo)
    {
      try
      {
        return PathUtility.GetProperFilePathCapitalization (fileInfo);
      }
      catch (ArgumentNullException)
      {
        throw;
      }
      catch (Exception)
      {
        //Use the FilePath in the same capitalization as we got it
        return fileInfo.FullName;
      }
    }

    private string GetProperFileNameCapitalization (FileInfo fileInfo)
    {
      try
      {
        return Path.GetFileName (PathUtility.GetProperFilePathCapitalization (fileInfo));
      }
      catch (ArgumentNullException)
      {
        throw;
      }
      catch (Exception)
      {
        //Use the FileName in the same capitalization as we got it
        return fileInfo.Name;
      }
    }
  }
}
