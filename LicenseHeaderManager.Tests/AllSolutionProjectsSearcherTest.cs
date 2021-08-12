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
using System.Linq;
using EnvDTE;
using EnvDTE80;
using LicenseHeaderManager.Utils;
using NUnit.Framework;
using Rhino.Mocks;

namespace LicenseHeaderManager.Tests
{
  [TestFixture]
  internal class AllSolutionProjectsSearcherBaseTest : VisualStudioBaseTest
  {
    [Test]
    public void TestGetAllProjects_ReturnsOnlyProjects ()
    {
      var solution = MockRepository.GenerateStub<Solution>();

      var legitProject1 = MockRepository.GenerateStub<Project>();
      var solutionFolder = MockRepository.GenerateStub<Project>();

      var projectList = new List<Project> { legitProject1, solutionFolder };

      solutionFolder.Stub (x => x.Kind).Return (ProjectKinds.vsProjectKindSolutionFolder);
      solutionFolder.Stub (x => x.ProjectItems.Count).Return (0);

      solution.Stub (x => x.GetEnumerator()).Return (projectList.GetEnumerator());


      var allSolutionProjectsSearcher = new AllSolutionProjectsSearcher();
      var returnedProjects = allSolutionProjectsSearcher.GetAllProjects (solution);


      Assert.That (legitProject1, Is.EqualTo (returnedProjects.First()));
    }

    [Test]
    public void TestGetAllProjects_FindsNestedProject ()
    {
      var solution = MockRepository.GenerateStub<Solution>();

      var legitProject1 = MockRepository.GenerateStub<Project>();
      var solutionFolder = MockRepository.GenerateStub<Project>();
      var projectInSolutionFolder = MockRepository.GenerateStub<Project>();

      var projectList = new List<Project> { legitProject1, solutionFolder };

      solutionFolder.Stub (x => x.Kind).Return (ProjectKinds.vsProjectKindSolutionFolder);
      solutionFolder.Stub (x => x.ProjectItems.Count).Return (1);
      solutionFolder.Stub (x => x.ProjectItems.Item (1).SubProject).Return (projectInSolutionFolder);

      solution.Stub (x => x.GetEnumerator()).Return (projectList.GetEnumerator());


      var allSolutionProjectsSearcher = new AllSolutionProjectsSearcher();
      var returnedProjects = allSolutionProjectsSearcher.GetAllProjects (solution);


      Assert.Contains (projectInSolutionFolder, returnedProjects.ToList());
    }

    [Test]
    public void TestGetAllProjects_ShouldReturnListOfProjects ()
    {
      var solution = MockRepository.GenerateStub<Solution>();

      var legitProject1 = MockRepository.GenerateStub<Project>();
      var legitProject2 = MockRepository.GenerateStub<Project>();

      var projectList = new List<Project> { legitProject1, legitProject2 };

      solution.Stub (x => x.GetEnumerator()).Return (projectList.GetEnumerator());


      var allSolutionProjectsSearcher = new AllSolutionProjectsSearcher();
      var returnedProjects = allSolutionProjectsSearcher.GetAllProjects (solution);


      Assert.That (returnedProjects.Count, Is.EqualTo (2));
    }
  }
}
