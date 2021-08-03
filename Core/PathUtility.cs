﻿/* Copyright (c) rubicon IT GmbH
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

namespace Core
{
  /// <summary>
  ///   Provides static helper function regarding file paths.
  /// </summary>
  internal static class PathUtility
  {
    /// <summary>
    ///   Determines the correct capitalization of a file path based on a <see cref="FileInfo" /> object.
    /// </summary>
    /// <param name="fileInfo">The <see cref="FileInfo" /> object whose underlying filepath should be correctly capitalized.</param>
    /// <returns>Returns the correct capitalization of the file path captured by <paramref name="fileInfo" />.</returns>
    public static string GetProperFilePathCapitalization (FileInfo fileInfo)
    {
      if (fileInfo == null)
        throw new ArgumentNullException (nameof(fileInfo));

      var dirInfo = fileInfo.Directory;

      var properDirectory = GetProperDirectoryCapitalization (dirInfo);
      return Path.Combine (properDirectory, dirInfo?.GetFiles (fileInfo.Name)[0].Name ?? string.Empty);
    }

    private static string GetProperDirectoryCapitalization (DirectoryInfo dirInfo)
    {
      var parentDirInfo = dirInfo.Parent;

      return null == parentDirInfo
          ? dirInfo.Root.FullName
          : Path.Combine (GetProperDirectoryCapitalization (parentDirInfo), parentDirInfo.GetDirectories (dirInfo.Name)[0].Name);
    }
  }
}
