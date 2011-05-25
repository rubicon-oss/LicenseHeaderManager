using System;
using System.Collections.Generic;
using System.Linq;
using LicenseHeaderManager.Headers;

namespace LicenseHeaderManager.Options
{
  public class Language : ICloneable
  {
    public IEnumerable<string> Extensions { get; set; }
    public string LineComment { get; set; }
    public string BeginComment { get; set; }
    public string EndComment { get; set; }
    public string BeginRegion { get; set; }
    public string EndRegion { get; set; }

    public bool IsValid
    {
      get
      {
        if (Extensions == null || !Extensions.Any())
          return false;

        if (string.IsNullOrEmpty (BeginRegion) != string.IsNullOrEmpty (EndRegion))
          return false;

        if (string.IsNullOrEmpty (LineComment))
          return (!string.IsNullOrEmpty (BeginComment) &&
                  !string.IsNullOrEmpty (EndComment));

        return string.IsNullOrEmpty (BeginComment) == string.IsNullOrEmpty (EndComment);
      }
    }

    public object Clone ()
    {
      return new Language () { Extensions = Extensions.ToList (), LineComment = LineComment, BeginComment = BeginComment, EndComment = EndComment, BeginRegion = BeginRegion, EndRegion = EndRegion };
    }

    public void NormalizeExtensions ()
    {
      Extensions = Extensions.Select (e => LicenseHeader.AddDot (e).ToLower ()).ToArray();
    }
  }
}
