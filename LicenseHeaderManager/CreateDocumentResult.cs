using System;

namespace LicenseHeaderManager
{
  enum CreateDocumentResult
  {
    DocumentCreated,
    NoPhyiscalFile,
    NoTextDocument,
    LanguageNotFound,
    NoHeaderFound,
    LicenseHeaderDocument
  }
}
