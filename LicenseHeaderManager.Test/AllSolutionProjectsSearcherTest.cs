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
    public void TestGetAllProjects_ShouldReturnListOfProjects()
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
    public void TestGetAllProjects_DoesOnlyReturnProjects()
    {
      Solution solution = MockRepository.GenerateStub<Solution>();

      Project legitProject1 = MockRepository.GenerateStub<Project> ();
      Project solutionFolder = MockRepository.GenerateStub<Project> ();

      List<Project> projectList = new List<Project>{legitProject1, solutionFolder};

      solutionFolder.Stub(x => x.Kind).Return(ProjectKinds.vsProjectKindSolutionFolder);
      solutionFolder.Stub(x => x.ProjectItems.Count).Return(0);

      solution.Stub(x => x.GetEnumerator()).Return(projectList.GetEnumerator());


      var allSolutionProjectsSearcher = new AllSolutionProjectsSearcher();
      List<Project> returnedProjects = allSolutionProjectsSearcher.GetAllProjects(solution);


      Assert.AreEqual(legitProject1, returnedProjects.First());
    }

    [Test]
    public void TestGetAllProjects_FindsNestedProject()
    {
      Solution solution = MockRepository.GenerateStub<Solution>();

      Project legitProject1 = MockRepository.GenerateStub<Project>();
      Project solutionFolder = MockRepository.GenerateStub<Project>();
      Project projectInSolutionFolder = MockRepository.GenerateStub<Project>();

      List<Project> projectList = new List<Project> {legitProject1, solutionFolder};

      solutionFolder.Stub(x => x.Kind).Return(ProjectKinds.vsProjectKindSolutionFolder);
      solutionFolder.Stub(x => x.ProjectItems.Count).Return(1);
      solutionFolder.Stub(x => x.ProjectItems.Item(1).SubProject).Return(projectInSolutionFolder);

      solution.Stub(x => x.GetEnumerator()).Return(projectList.GetEnumerator());


      var allSolutionProjectsSearcher = new AllSolutionProjectsSearcher();
      List<Project> returnedProjects = allSolutionProjectsSearcher.GetAllProjects(solution);
    
      
      Assert.Contains(projectInSolutionFolder, returnedProjects);
    }
  }
}
