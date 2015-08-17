//Sample license text.
using LicenseHeaderManager.Options;

namespace LicenseHeaderManager.Interfaces
{
  public interface ILicenseHeaderExtension
  {
    void ShowLanguagesPage ();

    IDefaultLicenseHeaderPage DefaultLicenseHeaderPage { get; }
    ILanguagesPage LanguagesPage { get; }
    IOptionsPage OptionsPage { get; }
  }
}
