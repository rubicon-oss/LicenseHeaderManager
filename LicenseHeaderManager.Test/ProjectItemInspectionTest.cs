using System;
using EnvDTE;
using LicenseHeaderManager.Utils;
using NUnit.Framework;
using Rhino.Mocks;

namespace LicenseHeaderManager.Test
{
  [TestFixture]
  public class ProjectItemInspectionTest
  {
    [Test]
    public void TestIsPhysicalFile()
    {
      ProjectItem physicalFile = MockRepository.GenerateMock<ProjectItem>();
      physicalFile.Expect(x => x.Kind).Return(EnvDTE.Constants.vsProjectItemKindPhysicalFile);

      ProjectItem virtualFolder = MockRepository.GenerateMock<ProjectItem> ();
      virtualFolder.Expect (x => x.Kind).Return (EnvDTE.Constants.vsProjectItemKindVirtualFolder);


      Assert.IsTrue(ProjectItemInspection.IsPhysicalFile(physicalFile));
      Assert.IsFalse (ProjectItemInspection.IsPhysicalFile (virtualFolder));
    }

    [Test]
    public void TestIsLicenseHeader()
    {
      ProjectItem licenseHeader = MockRepository.GenerateMock<ProjectItem>();
      licenseHeader.Expect(x => x.Name).Return("test.licenseheader");

      ProjectItem notLicenseHeader = MockRepository.GenerateMock<ProjectItem>();
      notLicenseHeader.Expect(x => x.Name).Return("test.cs");

      Assert.IsTrue(ProjectItemInspection.IsLicenseHeader(licenseHeader));
      Assert.IsFalse (ProjectItemInspection.IsLicenseHeader (notLicenseHeader));
    }

    [Test]
    [Ignore("#68 changed access to IsLink which cant be mocked anymore.")]
    public void TestIsLink()
    {
      ProjectItem linkedProjectItem = MockRepository.GenerateMock<ProjectItem>();
      
      Property propertyStub = MockRepository.GenerateStub<Property> ();
      propertyStub.Value = true;

      linkedProjectItem.Stub (x => x.Properties).Return (MockRepository.GenerateStub<Properties> ());
      linkedProjectItem.Properties.Stub (x => x.Item ("IsLink")).Return (propertyStub);

      Assert.IsTrue(ProjectItemInspection.IsLink(linkedProjectItem));
    }

    [Test]
    [Ignore("#68 changed access to IsLink which cant be mocked anymore.")]
    public void TestProjectItem_WithoutIsLinkProperty()
    {
      ProjectItem linkedProjectItem = MockRepository.GenerateMock<ProjectItem> ();

      linkedProjectItem.Stub (x => x.Properties).Return (MockRepository.GenerateStub<Properties> ());
      linkedProjectItem.Properties.Stub(x => x.Item("IsLink")).Throw(new ArgumentException());

      Assert.IsFalse(ProjectItemInspection.IsLink(linkedProjectItem));
    }
  }

  
}