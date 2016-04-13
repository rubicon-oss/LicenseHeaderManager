// Copyright (c) 2011 rubicon IT GmbH
using System;
using System.Windows.Data;

namespace LicenseHeaderManager.Options.Converters
{
  internal class BoolMultiValueConverter : IMultiValueConverter
  {
    #region IMultiValueConverter Members

    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      try
      {
        bool value1 = (bool)(values[0]);
        bool value2 = (bool)(values[1]);

        return value1 && value2;
      }
      catch (Exception)
      {
        return false;
      }
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotSupportedException("ConvertBack is not supported.");
    }

    #endregion IMultiValueConverter Members
  }
}
