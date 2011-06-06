using System;
using System.Collections.Generic;
using System.Windows.Data;

namespace LicenseHeaderManager.Options.Converters
{
  class ExtensionConverter : IValueConverter
  {
    public string Separator { get; set; }

    public ExtensionConverter ()
    {
      Separator = Environment.NewLine;
    }

    public object Convert (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      var strings = value as IEnumerable<string>;
      if (strings == null)
        return value;

      return string.Join (Separator, strings);
    }

    public object ConvertBack (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      var s = value as string;
      if (s == null)
        return value;

      return s.Split (new[] { Separator }, StringSplitOptions.RemoveEmptyEntries);
    }
  }
}
