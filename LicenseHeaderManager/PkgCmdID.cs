// PkgCmdID.cs
// MUST match PkgCmdID.h
using System;

namespace LicenseHeaderManager
{
    static class PkgCmdIDList
    {
      public const uint cmdIdLicenseHeaderOptions = 0x0001;
      public const uint cmdIdAddLicenseHeader = 0x0002;
      public const uint cmdIdRemoveLicenseHeader = 0x0003;
      public const uint cmdIdAddLicenseHeadersToAllFiles = 0x004;
      public const uint cmdIdRemoveLicenseHeadersFromAllFiles = 0x0005;
      public const uint cmdIdAddLicenseHeaderDefinitionFile = 0x0006;
      public const uint cmdIdAddExistingLicenseHeaderDefinitionFile = 0x0007;
    };
}