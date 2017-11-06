using System.Collections.Generic;
using EnvDTE;
using LicenseHeaderManager.Headers;
using LicenseHeaderManager.Interfaces;
using LicenseHeaderManager.Utils;
using NUnit.Framework;
using Rhino.Mocks;

namespace LicenseHeaderManager.Test
{
  [TestFixture]
  class LinkedFileHandlerTest
  {
    [Test]
    public void TestNoProjectItems()
    {
      Solution solution = MockRepository.GenerateStub<Solution>();
      ILicenseHeaderExtension extension = MockRepository.GenerateStub<ILicenseHeaderExtension>();

      LinkedFileFilter linkedFileFilter = MockRepository.GenerateStrictMock<LinkedFileFilter>(solution);
      LicenseHeaderReplacer licenseHeaderReplacer = MockRepository.GenerateStrictMock<LicenseHeaderReplacer>(extension);

      LinkedFileHandler linkedFileHandler = new LinkedFileHandler();
      linkedFileHandler.Handle(licenseHeaderReplacer, linkedFileFilter);

      Assert.AreEqual(string.Empty, linkedFileHandler.Message);
    }

    [Test]
    public void TestNoLicenseHeaderFile()
    {
      ILicenseHeaderExtension extension = MockRepository.GenerateStub<ILicenseHeaderExtension> ();
      ProjectItem projectItem = MockRepository.GenerateMock<ProjectItem>();
      projectItem.Expect(x => x.Name).Return("projectItem.cs");

      ILinkedFileFilter linkedFileFilter = MockRepository.GenerateMock<ILinkedFileFilter> ();
      LicenseHeaderReplacer licenseHeaderReplacer = MockRepository.GenerateStrictMock<LicenseHeaderReplacer> (extension);

      linkedFileFilter.Expect (x => x.NoLicenseHeaderFile).Return (new List<ProjectItem> { projectItem });
      linkedFileFilter.Expect (x => x.ToBeProgressed).Return (new List<ProjectItem> ());
      linkedFileFilter.Expect (x => x.NotInSolution).Return (new List<ProjectItem> ());


      LinkedFileHandler linkedFileHandler = new LinkedFileHandler ();
      linkedFileHandler.Handle (licenseHeaderReplacer, linkedFileFilter);

      string expectedMessage = string.Format(Resources.LinkedFileUpdateInformation, "projectItem.cs")
        .Replace(@"\n", "\n");

      Assert.AreEqual(expectedMessage, linkedFileHandler.Message);

    }
  }

  
}
