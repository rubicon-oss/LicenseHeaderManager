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
using System.Globalization;
using System.Windows.Data;

namespace LicenseHeaderManager.UpdateViews
{
  /// <summary>
  ///   This class is used to convert the values of a progress bar.
  ///   To prevent the progress bar from initially setting to 100% when 0 files have been
  ///   processed (because '0/0 files processed' will result in a full progress bar), a value of 0 will return 1.
  /// </summary>
  internal class IntToMaximumConverter : IValueConverter
  {
    /// <summary>
    ///   Converts an integer value for the progress bars.
    /// </summary>
    /// <param name="value">The value to be converted.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>A <see cref="object" /> where the value is 1 if <see cref="value" /> is 0 and <see cref="value" /> otherwise.</returns>
    public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (!(value is int intValue))
        return 0;

      return intValue == 0 ? 1 : intValue;
    }

    public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
