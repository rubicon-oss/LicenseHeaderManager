using System.Collections.ObjectModel;
using System.ComponentModel;
using LicenseHeaderManager.Options.Converters;

namespace LicenseHeaderManager.Options
{
  public interface IOptionsPage
  {
    bool UseRequiredKeywords { get; }
    string RequiredKeywords { get; }
  }
}