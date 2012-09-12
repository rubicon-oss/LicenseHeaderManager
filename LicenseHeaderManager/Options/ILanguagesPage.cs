using System.Collections.ObjectModel;
using System.ComponentModel;
using LicenseHeaderManager.Options.Converters;

namespace LicenseHeaderManager.Options
{
  public interface ILanguagesPage
  {
    ObservableCollection<Language> Languages { get; set; }
  }
}