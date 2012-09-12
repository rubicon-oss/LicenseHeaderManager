using System.Collections.ObjectModel;
using System.ComponentModel;
using LicenseHeaderManager.Options.Converters;

namespace LicenseHeaderManager.Options
{
  public interface ILanguagesPage
  {
    [TypeConverter (typeof (LanguageConverter))]
    [DesignerSerializationVisibility (DesignerSerializationVisibility.Visible)]
    ObservableCollection<Language> Languages { get; set; }
  }
}