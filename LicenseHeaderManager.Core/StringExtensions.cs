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
  internal static class StringExtensions
  {
    /// <summary>
    ///   Given a string representing a file extensions, inserts a dot at the beginning, if it does not already start with a
    ///   period ".".
    /// </summary>
    /// <param name="extension">The string representing a file extension.</param>
    /// <returns>
    ///   Returns a <see cref="string" /> that starts with a period "." in any case. If <paramref name="extension" />
    ///   already had a period as its first character, it is returned without any changes. However, if its first character is
    ///   anything other than a period ".", a new <see cref="string" /> based on <paramref name="extension" /> with a period at
    ///   the first position (index 0) is returned.
    /// </returns>
    internal static string InsertDotIfNecessary (this string extension)
    {
      if (extension.StartsWith ("."))
        return extension;

      return "." + extension;
    }

    /// <summary>
    ///   Replaces occurrences of "\n" in a string by new line characters.
    /// </summary>
    /// <returns>A <see cref="string" /> where all occurrences of "\n" have been replaced by new line characters</returns>
    public static string ReplaceNewLines (this string input)
    {
      return input.Replace (@"\n", "\n");
    }

    /// <summary>
    ///   Joins a <see cref="string" /> array using <see cref="Environment.NewLine" /> as separator.
    /// </summary>
    /// <returns>A <see cref="string" /> that represents the input array's elements having been joined by new line characters.</returns>
    public static string JoinWithNewLine (this string[] array)
    {
      return string.Join (Environment.NewLine, array);
    }

    /// <summary>
    ///   Counts the occurrences of a given string in another given string.
    /// </summary>
    /// <param name="inputString">The string to be searched for occurrences of <paramref name="searchString" />.</param>
    /// <param name="searchString">The string whose number of occurrences should be determined in <paramref name="inputString" />.</param>
    /// <returns>The number of occurrences of <paramref name="searchString" /> in <paramref name="inputString" />.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="inputString" /> or <paramref name="searchString" /> is <see langword="null" />.</exception>
    internal static int CountOccurrences (this string inputString, string searchString)
    {
      if (inputString == null)
        throw new ArgumentNullException (nameof(inputString));
      if (string.IsNullOrEmpty (searchString))
        throw new ArgumentNullException (nameof(searchString));

      var idx = 0;
      var count = 0;
      while ((idx = inputString.IndexOf (searchString, idx, StringComparison.Ordinal)) != -1)
      {
        idx += searchString.Length;
        count++;
      }

      return count;
    }
  }
}
