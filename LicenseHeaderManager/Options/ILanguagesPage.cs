using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using LicenseHeaderManager.Options.Converters;

namespace LicenseHeaderManager.Options
{
  public interface ILanguagesPage
  {
    IList<Language> Languages { get; set; }
  }
}