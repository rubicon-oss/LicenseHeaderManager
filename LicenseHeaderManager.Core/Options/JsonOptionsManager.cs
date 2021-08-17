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
using System.IO;
using System.Runtime.Serialization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LicenseHeaderManager.Core.Options
{
  /// <summary>
  ///   Provides methods for serializing and deserializing types with the <see cref="LicenseHeaderManagerOptionsAttribute" />
  ///   from and to JSON.
  /// </summary>
  /// <seealso cref="LicenseHeaderManagerOptionsAttribute" />
  public static class JsonOptionsManager
  {
    private const string c_fileStreamNotPresent = "File stream for deserializing configuration was not present";
    private const string c_jsonConverterNotFound = "At least one JSON converter for deserializing configuration members was not found";
    private const string c_fileNotFound = "File to deserialize configuration from was not found";
    private const string c_fileContentFormatNotValid = "The file content is not in a valid format";
    private const string c_unspecifiedError = "An unspecified error occurred while deserializing configuration";

    public static JsonSerializerOptions SerializerDefaultOptions =>
        new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter (JsonNamingPolicy.CamelCase, false) }
        };

    /// <summary>
    ///   Deserializes an instance of a type with the <seealso cref="LicenseHeaderManagerOptionsAttribute" /> from JSON.
    /// </summary>
    /// <typeparam name="T">The type of the instance to be serialized from <paramref name="filePath" />.</typeparam>
    /// <param name="filePath">
    ///   The path to a JSON file containing a serialized representation of the <typeparamref name="T" />
    ///   instance to be deserialized.
    /// </param>
    /// <param name="serializerOptions">The <see cref="JsonSerializerOptions" /> instance to be used while deserializing.</param>
    /// <returns>
    ///   Returns an instance of <typeparamref name="T" /> being equivalent to its JSON representation in
    ///   <paramref name="filePath" />.
    /// </returns>
    /// <exception cref="SerializationException">
    ///   Thrown if an error occurs while deserializing. See the
    ///   <see cref="Exception.InnerException" /> property for further details.
    /// </exception>
    /// <seealso cref="LicenseHeaderManagerOptionsAttribute" />
    public static async Task<T> DeserializeAsync<T> (string filePath, JsonSerializerOptions serializerOptions = null)
    {
      ValidateTypeParameter (typeof (T));

      try
      {
        using var stream = new FileStream (filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return await JsonSerializer.DeserializeAsync<T> (stream, serializerOptions ?? SerializerDefaultOptions);
      }
      catch (ArgumentNullException ex)
      {
        throw new SerializationException (c_fileStreamNotPresent, ex);
      }
      catch (NotSupportedException ex)
      {
        throw new SerializationException (c_jsonConverterNotFound, ex);
      }
      catch (FileNotFoundException ex)
      {
        throw new SerializationException (c_fileNotFound, ex);
      }
      catch (JsonException ex)
      {
        throw new SerializationException (c_fileContentFormatNotValid, ex);
      }
      catch (Exception ex)
      {
        throw new SerializationException (c_unspecifiedError, ex);
      }
    }

    /// <summary>
    ///   Serializes an instance of a type with the <seealso cref="LicenseHeaderManagerOptionsAttribute" /> to JSON.
    /// </summary>
    /// <typeparam name="T">The type of the instance to be serialized to <paramref name="filePath" />.</typeparam>
    /// <param name="options">The instance of type <typeparamref name="T" /> to be serialized.</param>
    /// <param name="filePath">
    ///   The path to a JSON file containing a serialized representation of the <typeparamref name="T" />
    ///   instance to be deserialized.
    /// </param>
    /// <param name="serializerOptions">The <see cref="JsonSerializerOptions" /> instance to be used while serializing.</param>
    /// <exception cref="SerializationException">
    ///   Thrown if an error occurs while serializing. See the
    ///   <see cref="Exception.InnerException" /> property for further details.
    /// </exception>
    /// <seealso cref="LicenseHeaderManagerOptionsAttribute" />
    public static async Task SerializeAsync<T> (T options, string filePath, JsonSerializerOptions serializerOptions = null)
    {
      ValidateTypeParameter (typeof (T));

      try
      {
        Directory.CreateDirectory (Path.GetDirectoryName (filePath) ?? throw new ArgumentException ("Must be valid filepath", nameof(filePath)));
        using var stream = new FileStream (filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync (stream, options, serializerOptions ?? SerializerDefaultOptions);
      }
      catch (NotSupportedException ex)
      {
        throw new SerializationException (c_jsonConverterNotFound, ex);
      }
      catch (Exception ex)
      {
        throw new SerializationException (c_unspecifiedError, ex);
      }
    }

    private static void ValidateTypeParameter (Type type)
    {
      if (!Attribute.IsDefined (type, typeof (LicenseHeaderManagerOptionsAttribute)))
        throw new ArgumentException ($"Type parameter {nameof(type)} must have attribute {nameof(LicenseHeaderManagerOptionsAttribute)}.");
    }
  }
}
