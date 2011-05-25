using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace LicenseHeaderManager.Options
{
  class LanguagesConverter : TypeConverter
  {
    private const string c_language = "Language";
    private const string c_languages = "Languages";
    private const string c_extension = "Extension";
    private const string c_extensions = "Extensions";
    private const string c_linecomment = "LineComment";
    private const string c_beginComment = "BeginComment";
    private const string c_endComment = "EndComment";
    private const string c_beginRegion = "BeginRegion";
    private const string c_endRegion = "EndRegion";

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
      return type == typeof (string) || type == typeof (ObservableCollection<Language>);
    }

    public override object ConvertFrom (ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
      return Convert(value);
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
        return ToXml (value as ObservableCollection<Language>);
    }

    private string ToXml (ObservableCollection<Language> languages)
    {
      var xml = from l in languages
                select new XElement (c_language,
                  new XElement (c_extensions,
                    from e in l.Extensions
                    select new XElement (c_extension, e)),
                  new XAttribute (c_linecomment, l.LineComment ?? string.Empty),
                  new XAttribute (c_beginComment, l.BeginComment ?? string.Empty),
                  new XAttribute (c_endComment, l.EndComment ?? string.Empty),
                  new XAttribute (c_beginRegion, l.BeginRegion ?? string.Empty),
                  new XAttribute (c_endRegion, l.EndRegion ?? string.Empty));

      return new XElement (c_languages, xml).ToString ();
    }

    private ObservableCollection<Language> FromXml (string xml)
    {
      try
      {
        var languages = from l in XElement.Parse (xml).Descendants (c_language)
                        select new Language()
                               {
                                 Extensions = 
                                   from e in l.Descendants(c_extension)
                                   select e.Value,
                                 LineComment = GetAttributeValue (l, c_linecomment),
                                 BeginComment = GetAttributeValue (l, c_beginComment),
                                 EndComment = GetAttributeValue (l, c_endComment),
                                 BeginRegion = GetAttributeValue (l, c_beginRegion),
                                 EndRegion = GetAttributeValue (l, c_endRegion)
                               };
        return new ObservableCollection<Language> (languages);
      }
      catch(Exception)
      {
        return new ObservableCollection<Language> ();
      }
    }

    private string GetAttributeValue (XElement element, string name)
    {
      var attribute = element.Attribute (name);
      if (attribute != null)
        return attribute.Value;
      else
        return null;
    }
  }
}
