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
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using LicenseHeaderManager.Core;

namespace LicenseHeaderManager.Options.Converters
{
  /// <summary>
  ///   Provides means of converting an enumerable range of <see cref="Language" /> objects to and from XML.
  /// </summary>
  internal class LanguageConverter : XmlTypeConverter<IEnumerable<Language>>
  {
    private const string c_language = "Language";
    private const string c_languages = "Languages";
    private const string c_extension = "Extension";
    private const string c_extensions = "Extensions";
    private const string c_lineComment = "LineComment";
    private const string c_beginComment = "BeginComment";
    private const string c_endComment = "EndComment";
    private const string c_beginRegion = "BeginRegion";
    private const string c_endRegion = "EndRegion";
    private const string c_skipExpression = "SkipExpression";

    public override string ToXml (IEnumerable<Language> languages)
    {
      try
      {
        var xml = from l in languages
            select new XElement (
                c_language,
                new XElement (
                    c_extensions,
                    from e in l.Extensions
                    select new XElement (c_extension, e)),
                new XAttribute (c_lineComment, l.LineComment ?? string.Empty),
                new XAttribute (c_beginComment, l.BeginComment ?? string.Empty),
                new XAttribute (c_endComment, l.EndComment ?? string.Empty),
                new XAttribute (c_beginRegion, l.BeginRegion ?? string.Empty),
                new XAttribute (c_endRegion, l.EndRegion ?? string.Empty),
                new XAttribute (c_skipExpression, l.SkipExpression ?? string.Empty));

        return new XElement (c_languages, xml).ToString();
      }
      catch (Exception)
      {
        return new XElement (c_languages).ToString();
      }
    }

    public override IEnumerable<Language> FromXml (string xml)
    {
      try
      {
        var languages = from l in XElement.Parse (xml).Descendants (c_language)
            select new Language
                   {
                       Extensions =
                           from e in l.Descendants (c_extension)
                           select e.Value,
                       LineComment = GetAttributeValue (l, c_lineComment),
                       BeginComment = GetAttributeValue (l, c_beginComment),
                       EndComment = GetAttributeValue (l, c_endComment),
                       BeginRegion = GetAttributeValue (l, c_beginRegion),
                       EndRegion = GetAttributeValue (l, c_endRegion),
                       SkipExpression = GetAttributeValue (l, c_skipExpression)
                   };
        return new ObservableCollection<Language> (languages);
      }
      catch (Exception)
      {
        return new ObservableCollection<Language>();
      }
    }
  }
}
