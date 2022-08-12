using System;
using Moq;
using NUnit.Framework;
using XCV.Data;
using XCV.Entities;
using XCV.Services;

namespace XCV.Tests.UNIT.ManagerTest
{
    public class ProjectManagerTest
    {
        private ProjectManager _projectManager;
        private Project _project;

        [OneTimeSetUp]
        public void SetUp()
        {
            // Init mocked services
            var mockProjectService = new Mock<IProjectService>();
            var mockProjectActivityService = new Mock<IProjectActivityService>();
            
            // Init Service
            _projectManager = new ProjectManager(mockProjectService.Object, mockProjectActivityService.Object);
            _project = new Project("Project 01", new Field("IT"), DateTime.Today, DateTime.Today,
                "test test test");
            
            _projectManager.Projects.Add(_project);
        }

        [Test]
        public void TestUpdateProjectData()
        {
            var project = new Project
            {
                Title = "UpdateProject",
                Field = new Field("UpdateProject"),
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                ProjectDescription = "UpdateProject"
            };

            _projectManager.AddNewProject(project);

            var project2 = new Project(project.Id, "UpdateProject2", DateTime.Now, DateTime.Now, "UpdateProject2");
            project2.Field = new Field("UpdateProject2");

            var managerProject = _projectManager.Projects.Find(x => x.Id.Equals(project2.Id));
            if (managerProject == null)
            {
                Assert.Fail();
                return;
            }
            Assert.AreNotEqual(project2.Title, managerProject.Title);
            Assert.AreNotEqual(project2.Field, managerProject.Field);
            Assert.AreNotEqual(project2.StartDate, managerProject.StartDate);
            Assert.AreNotEqual(project2.EndDate, managerProject.EndDate);
            Assert.AreNotEqual(project2.ProjectDescription, managerProject.ProjectDescription);
            
            _projectManager.UpdateProject(project2);
            managerProject = _projectManager.Projects.Find(x => x.Id.Equals(project2.Id));
            if (managerProject == null)
            {
                Assert.Fail();
                return;
            }
            
            Assert.AreEqual(project2.Title, managerProject.Title);
            Assert.AreEqual(project2.Field, managerProject.Field);
            Assert.AreEqual(project2.StartDate, managerProject.StartDate);
            Assert.AreEqual(project2.EndDate, managerProject.EndDate);
            Assert.AreEqual(project2.ProjectDescription, managerProject.ProjectDescription);
        }

        [Test]
        public void TestAddRemoveProject()
        {
            var temp = new Project("Project 01", new Field("IT"), DateTime.Today, DateTime.Today,
                "test test test");
            _projectManager.AddNewProject(temp);
            Assert.Contains(temp, _projectManager.Projects);
            
            var size = _projectManager.Projects.Count;
            _projectManager.AddNewProject(temp);
            Assert.AreEqual(size, _projectManager.Projects.Count);

            _projectManager.DeleteProjects(temp);
            Assert.False(_projectManager.Projects.Contains(temp));
        }

        [Test]
        public void TestAddRemoveProjectActivity()
        {
            var temp = new ProjectActivity("test activity");
            _projectManager.UpdateProjectActivity(temp, _project.Id);
            Assert.Contains(temp, _projectManager.Projects.Find(x => x.Id.Equals(_project.Id))?.ProjectActivities);

            temp.Description = "test activity 02";
            _projectManager.UpdateProjectActivity(temp, _project.Id);
            Assert.Contains(temp, _projectManager.Projects.Find(x => x.Id.Equals(_project.Id))?.ProjectActivities);

            _projectManager.DeleteProjectActivity(temp, _project);
            Assert.False(_projectManager.Projects.Find(x => x.Id.Equals(_project.Id))?.ProjectActivities.Contains(temp));
        }
    }
}