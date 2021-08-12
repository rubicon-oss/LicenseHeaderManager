/* Copyright (c) rubicon IT GmbH
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
 */

using System;
using System.Collections.Generic;
using System.IO;
using Core;
using EnvDTE;
using LicenseHeaderManager.Interfaces;
using LicenseHeaderManager.Utils;
using NUnit.Framework;
using Rhino.Mocks;

namespace LicenseHeaderManager.Tests
{
  [TestFixture]
  internal class LinkedFileFilterTest : VisualStudioBaseTest
  {
    [Test]
    public void Filter_GivenEmptyList_YieldsEmptyProperties ()
    {
      var solution = MockRepository.GenerateMock<Solution>();
      var linkedFileFilter = new LinkedFileFilter (solution);

      linkedFileFilter.Filter (new List<ProjectItem>());

      Assert.That (linkedFileFilter.ToBeProgressed, Is.Empty);
      Assert.That (linkedFileFilter.NoLicenseHeaderFile, Is.Empty);
      Assert.That (linkedFileFilter.NotInSolution, Is.Empty);
    }

    [Test]
    public void Filter_GivenNonSolutionItem_PopulatesNotInSolutionProperty ()
    {
      var solution = MockRepository.GenerateMock<Solution>();
      var linkedFile = MockRepository.GenerateMock<ProjectItem>();
      solution.Expect (x => x.FindProjectItem ("linkedFile.cs")).Return (null);
      linkedFile.Expect (x => x.Name).Return ("linkedFile.cs");

      var linkedFileFilter = new LinkedFileFilter (solution);
      linkedFileFilter.Filter (new List<ProjectItem> { linkedFile });

      Assert.That (linkedFileFilter.ToBeProgressed, Is.Empty);
      Assert.That (linkedFileFilter.NoLicenseHeaderFile, Is.Empty);
      Assert.That (linkedFileFilter.NotInSolution, Is.Not.Empty);
    }

    [Test]
    public void Filter_GivenLicenseHeaderFile_PopulatesToBeProgressedProperty ()
    {
      const string licenseHeaderFileName = "test.licenseheader";

      try
      {
        var solution = MockRepository.GenerateMock<Solution>();
        var linkedFile = MockRepository.GenerateMock<ProjectItem>();

        var licenseHeaderFile = MockRepository.GenerateStub<ProjectItem>();
        licenseHeaderFile.Expect (x => x.FileCount).Return (1);
        licenseHeaderFile.Expect (x => x.FileNames[0]).Return (licenseHeaderFileName);

        using (var writer = new StreamWriter (licenseHeaderFileName))
        {
          writer.WriteLine ("extension: .cs");
          writer.WriteLine ("//test");
        }

        var projectItems = MockRepository.GenerateStub<ProjectItems>();
        projectItems.Stub (x => x.GetEnumerator())
            .Return (null)
            .WhenCalled (x => x.ReturnValue = new List<ProjectItem> { licenseHeaderFile }.GetEnumerator());

        linkedFile.Expect (x => x.ProjectItems).Return (projectItems);
        linkedFile.Expect (x => x.Name).Return ("linkedFile.cs");
        solution.Expect (x => x.FindProjectItem ("linkedFile.cs")).Return (linkedFile);

        // LicenseHeaderFinder is invoked by LinkedFileFilter and uses LicenseHeadersPackage.Instance.LicenseHeaderExtractor.
        // Since LicenseHeaderExtractor is set during MEF-controlled initialization, set the private-set LicenseHeaderExtractor
        // to make it work via tests as well.
        SetPrivateSetPackageProperty (nameof(ILicenseHeaderExtension.LicenseHeaderExtractor), new LicenseHeaderExtractor());

        var linkedFileFilter = new LinkedFileFilter (solution);
        linkedFileFilter.Filter (new List<ProjectItem> { linkedFile });

        Assert.That (linkedFileFilter.ToBeProgressed, Is.Not.Empty);
        Assert.That (linkedFileFilter.NoLicenseHeaderFile, Is.Empty);
        Assert.That (linkedFileFilter.NotInSolution, Is.Empty);
      }
      finally
      {
        File.Delete (licenseHeaderFileName);
      }
    }

    [Test]
    public void Filter_GivenNonLicenseHeaderFile_PopulatesNoLicenseHeaderFileProperty ()
    {
      var solution = MockRepository.GenerateMock<Solution>();
      solution.Expect (x => x.FullName).Return (@"d:\projects\Stuff.sln");

      var dte = MockRepository.GenerateMock<DTE>();
      dte.Expect (x => x.Solution).Return (solution);

      var projectItems = MockRepository.GenerateMock<ProjectItems>();

      var linkedFile = MockRepository.GenerateMock<ProjectItem>();
      linkedFile.Expect (x => x.DTE).Return (dte);
      projectItems.Expect (x => x.Parent).Return (new object());
      linkedFile.Expect (x => x.Collection).Return (projectItems);

      solution.Expect (x => x.FindProjectItem ("linkedFile.cs")).Return (linkedFile);


      linkedFile.Expect (x => x.Name).Return ("linkedFile.cs");
      linkedFile.Expect (x => x.Properties).Return (null);


      var linkedFileFilter = new LinkedFileFilter (solution);
      linkedFileFilter.Filter (new List<ProjectItem> { linkedFile });

      Assert.That (linkedFileFilter.ToBeProgressed, Is.Empty);
      Assert.That (linkedFileFilter.NoLicenseHeaderFile, Is.Not.Empty);
      Assert.That (linkedFileFilter.NotInSolution, Is.Empty);
    }
  }
}
