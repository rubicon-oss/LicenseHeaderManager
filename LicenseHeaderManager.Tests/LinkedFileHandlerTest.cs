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
using System.Threading.Tasks;
using EnvDTE;
using LicenseHeaderManager.Core;
using LicenseHeaderManager.Interfaces;
using LicenseHeaderManager.Utils;
using Microsoft.VisualStudio.Threading;
using NUnit.Framework;
using Rhino.Mocks;

namespace LicenseHeaderManager.Tests
{
  [TestFixture]
  internal class LinkedFileHandlerTest : VisualStudioBaseTest
  {
    [SetUp]
    public void SetUp ()
    {
      // In order to make the "await LicenseHeadersPackage.Instance.JoinableTaskFactory.SwitchToMainThreadAsync();" call in
      // LinkedFileHandler.HandleAsync work, we need to set the private JoinableTaskFactory property accordingly.
      // Source: https://github.com/microsoft/vs-threading/blob/main/doc/testing_vs.md
      var jtc = new JoinableTaskContext();
      SetPrivateSetPackageProperty (nameof(ILicenseHeaderExtension.JoinableTaskFactory), jtc.Factory);
    }

    [Test]
    public async Task HandleAsync_NoLicenseHeaderFileGiven_MessageInformsAboutFilesThatCouldNotBeProcessed ()
    {
      var extension = MockRepository.GenerateStub<ILicenseHeaderExtension>();
      var licenseHeaderReplacer = MockRepository.GenerateStrictMock<LicenseHeaderReplacer>();
      extension.Expect (x => x.LicenseHeaderReplacer).Return (licenseHeaderReplacer);

      var noLicenseHeaderFile = MockRepository.GenerateMock<ProjectItem>();
      noLicenseHeaderFile.Expect (x => x.Name).Return ("projectItem1.cs");

      var notInSolution = MockRepository.GenerateMock<ProjectItem>();
      notInSolution.Expect (x => x.Name).Return ("projectItem2.cs");

      var linkedFileFilter = MockRepository.GenerateMock<ILinkedFileFilter>();
      linkedFileFilter.Expect (x => x.NoLicenseHeaderFile).Return (new List<ProjectItem> { noLicenseHeaderFile });
      linkedFileFilter.Expect (x => x.ToBeProgressed).Return (new List<ProjectItem>());
      linkedFileFilter.Expect (x => x.NotInSolution).Return (new List<ProjectItem> { notInSolution });

      var linkedFileHandler = new LinkedFileHandler (extension);
      await linkedFileHandler.HandleAsync (linkedFileFilter);

      const string expectedMessage = "We could not update following linked files, because there is no license header definition file in their original project, or the "
                                     + "original project is not part of this solution.\n\nprojectItem1.cs\nprojectItem2.cs";
      Assert.That (linkedFileHandler.Message, Is.EqualTo (expectedMessage));
    }

    [Test]
    public async Task HandleAsync_NoProjectItemsGiven_NothingToBeDoneAndErrorMessageEmpty ()
    {
      var solution = MockRepository.GenerateStub<Solution>();
      var extension = MockRepository.GenerateStub<ILicenseHeaderExtension>();
      var linkedFileFilter = MockRepository.GenerateStrictMock<LinkedFileFilter> (solution);
      var licenseHeaderReplacer = MockRepository.GenerateStrictMock<LicenseHeaderReplacer>();

      extension.Expect (x => x.LicenseHeaderReplacer).Return (licenseHeaderReplacer);

      var linkedFileHandler = new LinkedFileHandler (extension);
      await linkedFileHandler.HandleAsync (linkedFileFilter);

      Assert.That (linkedFileHandler.Message, Is.EqualTo (string.Empty));
    }
  }
}
