using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LicenseHeaderManager.Headers
{
  class DocumentHeaderProperties : IEnumerable<DocumentHeaderProperty>
  {
    private readonly IEnumerable<DocumentHeaderProperty> _properties;

    public DocumentHeaderProperties()
    {
      _properties = CreateProperties();
    }

    public IEnumerator<DocumentHeaderProperty> GetEnumerator()
    {
      return _properties.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    private IEnumerable<DocumentHeaderProperty> CreateProperties()
    {
      List<DocumentHeaderProperty> properties = new List<DocumentHeaderProperty>()
      {
        new DocumentHeaderProperty(
          "%FullFileName%", 
          documentHeader => documentHeader.FileInfo != null, 
          documentHeader => documentHeader.FileInfo.FullName),
        new DocumentHeaderProperty(
          "%FileName%", 
          documentHeader => documentHeader.FileInfo != null, 
          documentHeader => documentHeader.FileInfo.Name),
        new DocumentHeaderProperty(
          "%CurrentYear%", 
          documentHeader => true, 
          documentHeader => DateTime.Now.Year.ToString()),
        new DocumentHeaderProperty(
          "%CurrentMonth%", 
          documentHeader => true, 
          documentHeader => DateTime.Now.Month.ToString()),
        new DocumentHeaderProperty(
          "%CurrentDay%", 
          documentHeader => true, 
          documentHeader => DateTime.Now.Day.ToString()),
      };
      return properties;
    }
  }
}
