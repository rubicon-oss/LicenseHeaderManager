using System;

namespace LicenseHeaderManager.Headers
{
  public class ParseException : Exception
  {
    public ParseException () : base(Resources.Error_InvalidLicenseHeader)
    {
    }
  }
}
