using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using EnvDTE80;
using LicenseHeaderManager.Utils;
using NUnit.Framework;
using Rhino.Mocks;

namespace LicenseHeaderManager.Test
{
  [TestFixture]
  class AllSolutionProjectsSearcherTest
  {

    [Test]
    public void TestGetAllProjectsShouldReturnListOfProjects()
    {
      Solution solution = MockRepository.GenerateStub<Solution>();
 
      Project legitProject1 = MockRepository.GenerateStub<Project>();
      Project legitProject2 = MockRepository.GenerateStub<Project>();

      List<Project> projectList = new List<Project> {legitProject1, legitProject2};

      solution.Stub(x => x.GetEnumerator()).Return(projectList.GetEnumerator());

      var allSolutionProjectsSearcher = new AllSolutionProjectsSearcher();

      List<Project> returnedProjects = allSolutionProjectsSearcher.GetAllProjects(solution);

      Assert.AreEqual(2, returnedProjects.Count);
    }

    [Test]
    public void TestGetAllProjectsDoesOnlyReturnProjects()
    {
      Solution solution = MockRepository.GenerateStub<Solution>();

      Project legitProject1 = MockRepository.GenerateStub<Project> ();
      Project solutionFolder = MockRepository.GenerateStub<Project> ();

      List<Project> projectList = new List<Project>{legitProject1, solutionFolder};

      solutionFolder.Stub(x => x.Kind).Return(ProjectKinds.vsProjectKindSolutionFolder);

      solution.Stub(x => x.GetEnumerator()).Return(projectList.GetEnumerator());

      var allSolutionProjectsSearcher = new AllSolutionProjectsSearcher();

      List<Project> returnedProjects = allSolutionProjectsSearcher.GetAllProjects(solution);

      Assert.AreEqual(1, returnedProjects.Count);
    }
  }
}
