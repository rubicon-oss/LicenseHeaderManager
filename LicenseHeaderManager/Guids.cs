// Guids.cs
// MUST match guids.h
using System;

namespace LicenseHeaderManager
{
    static class GuidList
    {
        public const string guidLicenseHeadersPkgString = "4c570677-8476-4d33-bd0c-da36c89287c8";
        public const string guidLicenseHeadersCmdSetString = "88ce72ac-651d-4441-be9c-dc74c2dc44b6";

        public static readonly Guid guidLicenseHeadersCmdSet = new Guid(guidLicenseHeadersCmdSetString);
    };
}