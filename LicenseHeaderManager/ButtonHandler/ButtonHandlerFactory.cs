using LicenseHeaderManager.Headers;
using LicenseHeaderManager.Interfaces;

namespace LicenseHeaderManager.ButtonHandler
{
  class ButtonHandlerFactory
  {
    private readonly ILicenseHeaderExtension _licenseHeadersPackage;
    private readonly LicenseHeaderReplacer _licenseHeaderReplacer;

    public ButtonHandlerFactory(ILicenseHeaderExtension licenseHeadersPackage, LicenseHeaderReplacer licenseHeaderReplacer)
    {
      _licenseHeadersPackage = licenseHeadersPackage;
      _licenseHeaderReplacer = licenseHeaderReplacer;
    }

    public AddLicenseHeaderToAllProjectsButtonHandler CreateAddLicenseHeaderToAllProjectsButtonHandler() 
    { 
      return new AddLicenseHeaderToAllProjectsButtonHandler(_licenseHeaderReplacer, _licenseHeadersPackage.DefaultLicenseHeaderPage, _licenseHeadersPackage.Dte2);
    }
  }
}
