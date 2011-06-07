#region copyright
// Copyright (c) 2011 rubicon informationstechnologie gmbh

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
#endregion
using System;
using System.ComponentModel;
using System.Xml.Linq;

namespace LicenseHeaderManager.Options.Converters
{
  abstract class XmlTypeConverter<T> : TypeConverter
  {
    public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
    {
      return CanConvertType (sourceType);
    }

    public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
    {
      return CanConvertType (destinationType);
    }

    private bool CanConvertType (Type type)
    {
      return type == typeof (string) || typeof (T).IsAssignableFrom (type);
    }

    public override object ConvertFrom (ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
      return Convert (value);
    }

    public override object ConvertTo (ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
    {
      return Convert (value);
    }

    private object Convert (object value)
    {
      if (value is string)
        return FromXml (value as string);
      else
        return ToXml ((T)value);
    }

    protected string GetAttributeValue (XElement element, string name)
    {
      var attribute = element.Attribute (name);
      if (attribute != null)
        return attribute.Value;
      else
        return null;
    }

    public abstract T FromXml (string xml);

    public abstract string ToXml (T t);
  }
}
