#region copyright
// Copyright (c) rubicon IT GmbH

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
#endregion

using LicenseHeaderManager.Headers;
using LicenseHeaderManager.Interfaces;

namespace LicenseHeaderManager.ButtonHandler
{
  class ButtonHandlerFactory
  {
    private readonly ILicenseHeaderExtension _licenseHeadersPackage;
    private readonly LicenseHeaderReplacer _licenseHeaderReplacer;

    public ButtonHandlerFactory (ILicenseHeaderExtension licenseHeadersPackage, LicenseHeaderReplacer licenseHeaderReplacer)
    {
      _licenseHeadersPackage = licenseHeadersPackage;
      _licenseHeaderReplacer = licenseHeaderReplacer;
    }

    public AddLicenseHeaderToAllProjectsButtonHandler CreateAddLicenseHeaderToAllProjectsButtonHandler ()
    {
      return new AddLicenseHeaderToAllProjectsButtonHandler (_licenseHeaderReplacer, _licenseHeadersPackage.Dte2);
    }
  }
}