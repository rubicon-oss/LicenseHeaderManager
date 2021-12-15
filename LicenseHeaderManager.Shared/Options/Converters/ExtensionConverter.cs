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
using System.Globalization;
using System.Windows.Data;

namespace LicenseHeaderManager.Options.Converters
{
  /// <summary>
  ///   Joins an enumerable range of <see cref="string" /> instances using a predefined separator and converts such a joined
  ///   <see cref="string" /> back to an <see cref="IEnumerable{T}" /> whose generic type parameter is <see cref="string" />.
  /// </summary>
  internal class ExtensionConverter : IValueConverter
  {
    public ExtensionConverter ()
    {
      Separator = Environment.NewLine;
    }

    /// <summary>
    ///   The separator to be used to join and split the converter inputs.
    /// </summary>
    public string Separator { get; set; }

    public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (!(value is IEnumerable<string> strings))
        return value;

      return string.Join (Separator, strings);
    }

    public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (!(value is string s))
        return value;

      return s.Split (new[] { Separator }, StringSplitOptions.None);
    }
  }
}
