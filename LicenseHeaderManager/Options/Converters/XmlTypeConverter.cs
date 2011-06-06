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
