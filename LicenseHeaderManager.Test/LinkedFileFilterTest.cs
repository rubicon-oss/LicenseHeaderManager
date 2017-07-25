using System;
using System.Collections.Generic;
using System.IO;
using EnvDTE;
using LicenseHeaderManager.Utils;
using NUnit.Framework;
using Rhino.Mocks;


namespace LicenseHeaderManager.Test
{
  [TestFixture]
  class LinkedFileFilterTest
  {
    [Test]
    public void TestEmptyList()
    {
      Solution solution = MockRepository.GenerateMock<Solution>();
      LinkedFileFilter linkedFileFilter = new LinkedFileFilter(solution);

      linkedFileFilter.Filter(new List<ProjectItem>());

      Assert.IsEmpty (linkedFileFilter.ToBeProgressed);
      Assert.IsEmpty (linkedFileFilter.NoLicenseHeaderFile);
      Assert.IsEmpty (linkedFileFilter.NotInSolution);
    }

    [Test]
    public void TestProjectItemWithLicenseHeaderFile()
    {
      string licenseHeaderFileName = "test.licenseheader";

      Solution solution = MockRepository.GenerateMock<Solution>();
      ProjectItem linkedFile = MockRepository.GenerateMock<ProjectItem>();
      
      ProjectItem licenseHeaderFile = MockRepository.GenerateStub<ProjectItem>();
      licenseHeaderFile.Expect(x => x.FileCount).Return(1);
      licenseHeaderFile.Expect (x => x.FileNames[0]).Return (licenseHeaderFileName);

      using (var writer = new StreamWriter (licenseHeaderFileName))
      {
        writer.WriteLine("extension: .cs");
        writer.WriteLine("//test");
      }

      ProjectItems projectItems = MockRepository.GenerateStub<ProjectItems>();
      projectItems.Stub(x => x.GetEnumerator())
                     .Return(null)
                     .WhenCalled(x => x.ReturnValue = 
                                    new List<ProjectItem> { licenseHeaderFile }.GetEnumerator()
                                 );

      linkedFile.Expect(x => x.ProjectItems).Return(projectItems);
      linkedFile.Expect (x => x.Name).Return ("linkedFile.cs");
      solution.Expect(x => x.FindProjectItem("linkedFile.cs")).Return(linkedFile);

      
      LinkedFileFilter linkedFileFilter = new LinkedFileFilter(solution);
      linkedFileFilter.Filter(new List<ProjectItem>{linkedFile});
    
      Assert.IsNotEmpty(linkedFileFilter.ToBeProgressed);
      Assert.IsEmpty (linkedFileFilter.NoLicenseHeaderFile);
      Assert.IsEmpty (linkedFileFilter.NotInSolution);

      //Cleanup
      File.Delete(licenseHeaderFileName);
    }

    [Test]
    public void TestProjectItemWithoutLicenseHeaderFile()
    {
      Solution solution = MockRepository.GenerateMock<Solution> ();
      ProjectItems projectItems = MockRepository.GenerateMock<ProjectItems>();

      ProjectItem linkedFile = MockRepository.GenerateMock<ProjectItem> ();
      projectItems.Expect (x => x.Parent).Return (new object());
      linkedFile.Expect (x => x.Collection).Return (projectItems);

      solution.Expect (x => x.FindProjectItem ("linkedFile.cs")).Return (linkedFile);
      

      linkedFile.Expect (x => x.Name).Return ("linkedFile.cs");
      linkedFile.Expect (x => x.Properties).Return (null);
      
      
      LinkedFileFilter linkedFileFilter = new LinkedFileFilter(solution);
      linkedFileFilter.Filter(new List<ProjectItem>{linkedFile});

      Assert.IsEmpty (linkedFileFilter.ToBeProgressed);
      Assert.IsNotEmpty (linkedFileFilter.NoLicenseHeaderFile);
      Assert.IsEmpty (linkedFileFilter.NotInSolution);
    }

    [Test]
    public void TestProjectItemNotInSolution()
    {
      Solution solution = MockRepository.GenerateMock<Solution> ();
      ProjectItem linkedFile = MockRepository.GenerateMock<ProjectItem> ();
      solution.Expect (x => x.FindProjectItem ("linkedFile.cs")).Return (null);
      linkedFile.Expect (x => x.Name).Return ("linkedFile.cs");

      LinkedFileFilter linkedFileFilter = new LinkedFileFilter (solution);
      linkedFileFilter.Filter (new List<ProjectItem> { linkedFile });

      Assert.IsEmpty (linkedFileFilter.ToBeProgressed);
      Assert.IsEmpty (linkedFileFilter.NoLicenseHeaderFile);
      Assert.IsNotEmpty (linkedFileFilter.NotInSolution);
    }
  }
}
