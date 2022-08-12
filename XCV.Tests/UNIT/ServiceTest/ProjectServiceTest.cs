using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using XCV.Data;
using XCV.Entities;
using XCV.Entities.Enums;
using XCV.Services;

namespace XCV.Tests.ServiceTest
{
    public class ProjectServiceTest
    {
        private DatabaseUtils _databaseUtils;

        private IProjectService _projectService;

        private IEmployeeService _employeeService;

        private IProjectActivityService _projectActivityService;

        private IExperienceService _experienceService;


        [OneTimeSetUp]
        public void SetUp()
        {
            var config = InitConfiguration();
            this._databaseUtils = new DatabaseUtils(config);
            this._projectService = new ProjectService(_databaseUtils);
            this._projectActivityService = new ProjectActivityService(_databaseUtils);
            this._employeeService = new EmployeeService(_databaseUtils);
            this._experienceService = new ExperienceService(_databaseUtils);

            _databaseUtils.LoadTables();
        }

        private static IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder().AddJsonFile(Path.Combine(".", "appsettings.Development.json"))
                .Build();
            return config;
        }


        [Test]
        public async Task ProjectUpdateWithProjectActivityAndEmployeeIdsAndPurpose()
        {
            var project =
                new Project("XCV Projekt", DateTime.Now, null, "Programm zur Verwaltung von Angeboten");
            var projectActivityOne = new ProjectActivity("Liftdisplay einrichten");
            var employeeOne = new Employee(Authorizations.Admin, "Meiners", "Alexander", "Alex", DateTime.Now, 3, 4, 3, RateCardLevel.Level7, null);
            var employeeTwo = new Employee(Authorizations.Pleb, "Schmitz", "Thomas", "Tooom", DateTime.Now, 2, 1, 4, RateCardLevel.Level4, null);
            var purposeOne = "PurposeOne";
            var purposeTwo = "PurposeTwo";
            var employees = await _employeeService.GetAllEmployees();

            foreach (var employee in employees.Where(employee =>
                employee.UserName.Equals(employeeOne.UserName) || employee.UserName.Equals(employeeTwo.UserName)))
            {
                await _employeeService.DeleteEmployee(employee.Id);
            }

            projectActivityOne.AddEmployee(employeeOne.Id);
            projectActivityOne.AddEmployee(employeeTwo.Id);
            project.ProjectActivities.Add(projectActivityOne);
            project.ProjectPurposes.Add(purposeOne);
            project.ProjectPurposes.Add(purposeTwo);

            await _employeeService.UpdateEmployee(employeeOne);
            await _employeeService.UpdateEmployee(employeeTwo);
            await _projectService.UpdateProject(project);

            var projectTest = await _projectService.GetProject(project.Id);
            var projectActivityTestOne =
                await _projectActivityService.GetProjectActivity(projectActivityOne.Id);
            if (!projectActivityOne.Equals(projectActivityTestOne))
            {
                Assert.Fail("ProjectActivities were not properly inserted");
            }

            if (!project.Equals(projectTest))
            {
                Assert.Fail("Project was not properly inserted");
            }

            await _projectService.DeleteProject(project.Id);

            var projectList = await _projectService.GetAllProjects();

            foreach (var value in projectList)
            {
                if (value.Id.Equals(project.Id))
                {
                    Assert.Fail("Project was not properly deleted");
                }
            }

            await _employeeService.DeleteEmployee(employeeOne.Id);
            await _employeeService.DeleteEmployee(employeeTwo.Id);

            Assert.Pass("Project, ProjectActivities and Employees were properly inserted");
        }

        [Test]
        public async Task ProjectUpdateWithoutProjectActivityAndEndDate()
        {
            var project =
                new Project("ProjectWithoutProjectActivity", DateTime.Now, DateTime.Now,
                    "Programm zur Verwaltung von Angeboten");

            await _projectService.UpdateProject(project);

            var projectTest = await _projectService.GetProject(project.Id);

            if (!project.Equals(projectTest))
            {
                Assert.Fail("Project was not properly inserted");
            }

            await _projectService.DeleteProject(project.Id);
            
            var projectList = await _projectService.GetAllProjects();

            foreach (var value in projectList)
            {
                if (value.Id.Equals(project.Id))
                {
                    Assert.Fail("Project was not properly deleted");
                }
            }

            Assert.Pass("Project was properly inserted");
        }

        [Test]
        public async Task ProjectUpdateWithMoreProjectActivitiesWithoutEmployeeIds()
        {
            var project =
                new Project("ProjektWithMoreProjectActivities", DateTime.Now, null,
                    "Programm zur Verwaltung von Angeboten");
            var projectActivityOne = new ProjectActivity("Liftdisplay einrichten");
            var projectActivityTwo = new ProjectActivity("Gui einrichten");
            project.ProjectActivities.Add(projectActivityOne);
            project.ProjectActivities.Add(projectActivityTwo);

            await _projectService.UpdateProject(project);

            var projectTest = await _projectService.GetProject(project.Id);
            var projectActivityOneTest =
                await _projectActivityService.GetProjectActivity(projectActivityOne.Id);
            var projectActivityTwoTest =
                await _projectActivityService.GetProjectActivity(projectActivityTwo.Id);

            if (!(projectActivityOne.Equals(projectActivityOneTest) &&
                  projectActivityTwo.Equals(projectActivityTwoTest)))
            {
                Assert.Fail("ProjectActivities were not properly inserted");
            }

            if (!project.Equals(projectTest))
            {
                Assert.Fail("Project was not properly inserted");
            }

            await _projectService.DeleteProject(project.Id);
            
            var projectList = await _projectService.GetAllProjects();

            foreach (var value in projectList)
            {
                if (value.Id.Equals(project.Id))
                {
                    Assert.Fail("Project was not properly deleted");
                }
            }

            Assert.Pass("Project and ProjectActivity were properly inserted");
        }

        [Test]
        public async Task ProjectUpdateWithField()
        {
            var field = new Field("Fußball");

            var fields = await _experienceService.LoadFields();

            foreach (var skill in fields.Where(skill => field.Name == skill.Name))
            {
                await _experienceService.DeleteExperience(skill);
            }

            var project =
                new Project("ProjektWithMoreProjectActivities", field, DateTime.Now, null,
                    "Programm zur Verwaltung von Angeboten");
            await _experienceService.UpdateExperience(field);
            await _projectService.UpdateProject(project);

            var fieldTest = (Field) (await _experienceService.GetExperience(field.Id));
            var projectTest = await _projectService.GetProject(project.Id);

            if (!field.Equals(fieldTest))
            {
                Assert.Fail("Field was not properly inserted");
            }

            if (!project.Equals(projectTest))
            {
                Assert.Fail("Project was not properly inserted");
            }

            await _projectService.DeleteProject(project.Id);
            
            var projectList = await _projectService.GetAllProjects();

            foreach (var value in projectList.Where(value => value.Id.Equals(project.Id)))
            {
                Assert.Fail("Project was not properly deleted");
            }

            await _experienceService.DeleteExperience(field);

            Assert.Pass("Project and Field were properly inserted");
        }

        [Test]
        public async Task DeleteEmployeeFromProjectActivity()
        {
            var project =
                new Project("XCV Projekt", DateTime.Now, null, "Programm zur Verwaltung von Angeboten");
            var projectActivityOne = new ProjectActivity("Liftdisplay einrichten");
            var employeeOne = new Employee(Authorizations.Admin, "Meiners", "Alexander", "Alex", DateTime.Now, 3, 4, 3,
                RateCardLevel.Level7, null);
            var employeeTwo = new Employee(Authorizations.Pleb, "Schmitz", "Thomas", "Tooom", DateTime.Now, 2, 1, 4,
                RateCardLevel.Level4, null);

            var employees = await _employeeService.GetAllEmployees();

            foreach (var employee in employees.Where(employee =>
                employee.UserName.Equals(employeeOne.UserName) || employee.UserName.Equals(employeeTwo.UserName)))
            {
                await _employeeService.DeleteEmployee(employee.Id);
            }

            projectActivityOne.AddEmployee(employeeOne.Id);
            projectActivityOne.AddEmployee(employeeTwo.Id);
            project.ProjectActivities.Add(projectActivityOne);


            await _employeeService.UpdateEmployee(employeeOne);
            await _employeeService.UpdateEmployee(employeeTwo);
            await _projectService.UpdateProject(project);

            var projectActivityTestOne =
                await _projectActivityService.GetProjectActivity(projectActivityOne.Id);

            projectActivityOne.DeleteEmployee(employeeTwo.Id);

            await _projectActivityService.UpdateProjectActivity(projectActivityOne, project.Id);

            var projectActivity = await _projectActivityService.GetProjectActivity(projectActivityOne.Id);

            var projectTestTwo = await _projectService.GetProject(project.Id);

            if (projectActivity != null && !projectActivity.Equals(projectActivityTestOne))
            {
                Assert.Fail("Employee was not properly deleted");
            }

            if (projectTestTwo != null && !projectTestTwo.Equals(project))
            {
                Assert.Fail();
            }

            await _projectService.DeleteProject(project.Id);

            await _employeeService.DeleteEmployee(employeeOne.Id);

            await _employeeService.DeleteEmployee(employeeTwo.Id);

            Assert.Pass("Employee was properly deleted");
        }

        [Test]
        public async Task UpdateProjectWithDeletedProjectActivityAndDeletedPurpose()
        {
            var project =
                new Project("ProjektWithMoreProjectActivities", DateTime.Now, null,
                    "Programm zur Verwaltung von Angeboten");
            var projectActivityOne = new ProjectActivity("Liftdisplay einrichten");
            var projectActivityTwo = new ProjectActivity("Gui einrichten");
            var purposeOne = "Inserted";
            var purposeTwo = "Deleted";
            project.ProjectActivities.Add(projectActivityOne);
            project.ProjectActivities.Add(projectActivityTwo);
            project.ProjectPurposes.Add(purposeOne);
            project.ProjectPurposes.Add(purposeTwo);

            await _projectService.UpdateProject(project);

            var projectTestOne = await _projectService.GetProject(project.Id);

            Assert.AreEqual(project, projectTestOne);

            if (projectTestOne != null)
            {
                Assert.Contains(projectActivityOne, projectTestOne.ProjectActivities);

                Assert.Contains(projectActivityTwo, projectTestOne.ProjectActivities);

                project.ProjectActivities.Remove(projectActivityTwo);

                project.ProjectPurposes.Remove(purposeTwo);

                await _projectService.UpdateProject(project);

                var projectTestTwo = await _projectService.GetProject(project.Id);

                if (projectTestTwo != null)
                {
                    Assert.AreEqual(projectTestOne.ProjectPurposes.Count - 1, projectTestTwo.ProjectPurposes.Count);

                    await _projectService.DeleteProject(project.Id);

                    Assert.AreEqual(project, projectTestTwo);


                    Assert.AreEqual(projectTestOne.ProjectActivities.Count - 1,
                        projectTestTwo.ProjectActivities.Count);

                    Assert.Contains(projectActivityOne, projectTestTwo.ProjectActivities);

                    if (projectTestTwo.ProjectActivities.Contains(projectActivityTwo))
                    {
                        Assert.Fail("ProjectActivityTwo was not properly deleted");
                    }
                }
            }
        }

        [Test]
        public async Task GetAllProjectActivitiesFromOneProject()
        {
            var project =
                new Project("GetAllProjectActivity", DateTime.Now, null, "All projectActivities form one project");
            var projectActivityOne = new ProjectActivity("");
            var projectActivityTwo = new ProjectActivity("Gui einrichten");
            project.ProjectActivities.Add(projectActivityOne);
            project.ProjectActivities.Add(projectActivityTwo);
            var employeeOne = new Employee(Authorizations.Admin, "Meiners", "Alexander", "Alex", DateTime.Now, 3, 4, 3,
                RateCardLevel.Level7, null);
            var employeeTwo = new Employee(Authorizations.Pleb, "Schmitz", "Thomas", "Tooomm", DateTime.Now, 2, 1, 4,
                RateCardLevel.Level4, null);

            var employees = await _employeeService.GetAllEmployees();

            foreach (var employee in employees.Where(employee =>
                employee.UserName.Equals(employeeOne.UserName) || employee.UserName.Equals(employeeTwo.UserName)))
            {
                await _employeeService.DeleteEmployee(employee.Id);
            }

            projectActivityOne.AddEmployee(employeeOne.Id);
            projectActivityTwo.AddEmployee(employeeTwo.Id);
            await _employeeService.UpdateEmployee(employeeOne);
            await _employeeService.UpdateEmployee(employeeTwo);
            await _projectService.UpdateProject(project);
            var projectActivities = await _projectActivityService.GetAllProjectActivities(project.Id);
            foreach (var projectActivity in projectActivities)
            {
                if (!project.ProjectActivities.Contains(projectActivity))
                {
                    Assert.Fail("GetAllProjectActivities in ProjectActivityService has failed");
                }
            }

            foreach (var projectActivity in projectActivities)
            {
                if (projectActivity.Id == projectActivityOne.Id)
                {
                    Assert.AreEqual(projectActivity, projectActivityOne);
                }

                if (projectActivity.Id == projectActivityTwo.Id)
                {
                    Assert.AreEqual(projectActivity, projectActivityTwo);
                }
            }

            await _projectService.DeleteProject(project.Id);
            await _employeeService.DeleteEmployee(employeeOne.Id);
            await _employeeService.DeleteEmployee(employeeTwo.Id);
            Assert.Pass("GetAllProjectActivities in ProjectActivityService has succeeded");
        }

        [Test]
        public async Task GetProjectNamesTest_ShouldReturnProjects_WhenGivenList()
        {
            var projectOne = new Project("GetProjectNamesTestOneList", DateTime.Now, null, "should be returned");
            var projectTwo = new Project("GetProjectNamesTestTwoList", DateTime.Now, null, "should be returned, too");
            await _projectService.UpdateProject(projectOne);
            await _projectService.UpdateProject(projectTwo);
            var list = new List<Guid> {projectOne.Id, projectTwo.Id};

            var resultList = await _projectService.GetProjectNames(list);

            Assert.Contains((projectOne.Id, projectOne.Title), resultList);
            Assert.Contains((projectTwo.Id, projectTwo.Title), resultList);
            await _projectService.DeleteProject(projectOne.Id);
            await _projectService.DeleteProject(projectTwo.Id);
        }

        [Test]
        public async Task GetProjectNamesTest_ShouldReturnAllProject()
        {
            var projectOne = new Project("GetProjectNamesTestOne", DateTime.Now, null, "should be returned");
            var projectTwo = new Project("GetProjectNamesTestTwo", DateTime.Now, null, "should be returned, too");
            await _projectService.UpdateProject(projectOne);
            await _projectService.UpdateProject(projectTwo);
            var emptyList = new List<Guid>();
            var allProjects = await _projectService.GetAllProjects();

            var resultList = await _projectService.GetAllProjectNames();

            foreach (var project in allProjects)
            {
                var tuple = (project.Id, project.Title);
                Assert.Contains(tuple, resultList);
            }

            await _projectService.DeleteProject(projectOne.Id);
            await _projectService.DeleteProject(projectTwo.Id);
        }

        [Test]
        public async Task UpdateProjectActivityTest_ShouldInsert_WhenNotExits()
        {
            var projectOne = new Project("UpdateProjectActivityTest Project Insert", DateTime.Now, null, "");
            await _projectService.UpdateProject(projectOne);
            var projActivity = new ProjectActivity("UpdateProjectActivityTestInserted");
            var employeeOne = new Employee(Authorizations.Admin, "Meiners", "Alexander", "Alex", DateTime.Now, 3, 4, 3,
                RateCardLevel.Level7, null);
            var employeeTwo = new Employee(Authorizations.Pleb, "Schmitz", "Thomas", "Tooomm", DateTime.Now, 2, 1, 4,
                RateCardLevel.Level4, null);
            projActivity.AddEmployee(employeeOne.Id);
            projActivity.AddEmployee(employeeTwo.Id);
            await _employeeService.UpdateEmployee(employeeOne);
            await _employeeService.UpdateEmployee(employeeTwo);


            Assert.False((await _projectActivityService.UpdateProjectActivity(projActivity, projectOne.Id)));

            var result = await _projectActivityService.GetProjectActivity(projActivity.Id);
            Assert.AreEqual(projActivity, result);

            projActivity.GetEmployeeIds().ForEach(id => _employeeService.DeleteEmployee(id));
            await _projectService.DeleteProject(projectOne.Id);
            await _projectActivityService.DeleteProjectActivity(projActivity.Id);
        }

        [Test]
        public async Task UpdateProjectActivityTest_ShouldUpdate_WhenExits()
        {
            //arrange
            var projectOne = new Project("UpdateProjectActivityTest Project Update", DateTime.Now, null, "");
            await _projectService.UpdateProject(projectOne);
            var projActivity = new ProjectActivity("UpdateProjectActivityTest to be updated");
            var employeeOne = new Employee(Authorizations.Admin, "Meiners", "Alexander", "UpdateProjectActivityTestOne",
                DateTime.Now, 3, 4, 3, RateCardLevel.Level7, null);
            var employeeTwo = new Employee(Authorizations.Pleb, "Schmitz", "Thomas", "UpdateProjectActivityTestTwo",
                DateTime.Now, 2, 1, 4, RateCardLevel.Level4, null);
            var employeeThree = new Employee(Authorizations.Pleb, "Dieterle", "Daniel", "UpdateProjectActivityTestThree",
                DateTime.Now, 2, 1, 4, RateCardLevel.Level4, null);
            projActivity.AddEmployee(employeeOne.Id);
            projActivity.AddEmployee(employeeTwo.Id);
            await _employeeService.UpdateEmployee(employeeOne);
            await _employeeService.UpdateEmployee(employeeTwo);
            await _employeeService.UpdateEmployee(employeeThree);
            await _projectActivityService.UpdateProjectActivity(projActivity, projectOne.Id);
            //update
            projActivity.Description = "UpdateProjectActivityTest Updated";
            projActivity.RemoveEmployee(employeeTwo.Id);
            projActivity.AddEmployee(employeeThree.Id);

            //act / assert
            Assert.True((await _projectActivityService.UpdateProjectActivity(projActivity, projectOne.Id)));
                

            var result = await _projectActivityService.GetProjectActivity(projActivity.Id);
            Assert.AreEqual(projActivity, result);

            await _employeeService.DeleteEmployee(employeeOne.Id);
            await _employeeService.DeleteEmployee(employeeTwo.Id);
            await _employeeService.DeleteEmployee(employeeThree.Id);
            await _projectService.DeleteProject(projectOne.Id);
            await _projectActivityService.DeleteProjectActivity(projActivity.Id);
        }

        [Test]
        public async Task UpdateAndDeleteProjectWithPurposes()
        {
            var project = new Project("UpdateProjectActivityTest Project Update", DateTime.Now, null, "dd");
            var purposeOne = "PurposeOne";
            var purposeTwo = "PurposeTwo";
            var purposeThree = "PurposeThree";
            project.ProjectPurposes.Add(purposeOne);
            project.ProjectPurposes.Add(purposeTwo);

            await _projectService.UpdateProject(project);

            project.ProjectPurposes.Remove(purposeTwo);
            
            var lastChange = await _projectService.UpdateProject(project);
            project.LastChanged = lastChange.Item1;

            var projectTestOne = await _projectService.GetProject(project.Id);

            if (projectTestOne != null) Assert.AreEqual(project.ProjectPurposes.Count, projectTestOne.ProjectPurposes.Count);

            project.ProjectPurposes.Add(purposeThree);
            lastChange = await _projectService.UpdateProject(project);
            project.LastChanged = lastChange.Item1;
            
            var projectTestTwo = await _projectService.GetProject(project.Id);

            if (projectTestTwo != null)
                Assert.AreEqual(project.ProjectPurposes.Count, projectTestTwo.ProjectPurposes.Count);

            await _projectService.DeleteProject(project.Id);
        }

        [Test]
        public async Task GetAllProjectWithProjectActivitiesAndPurposesAndEmployeeId()
        {
            var dbProjects = await _projectService.GetAllProjects();
            foreach (var project in dbProjects)
            {
                await _projectService.DeleteProject(project.Id);
            }
            var projectOne = new Project("UpdateProjectActivityTest Project Update", DateTime.Now, null, "dd");
            var projectTwo = new Project("UpdateProjectActivityTest Project Update", DateTime.Now, null, "dd");
            var projectActivityOne = new ProjectActivity("");
            var projectActivityTwo = new ProjectActivity("");
            var projectActivityThree = new ProjectActivity("");
            var projectActivityFour = new ProjectActivity("");
            projectOne.ProjectActivities.Add(projectActivityOne);
            projectOne.ProjectActivities.Add(projectActivityTwo);
            projectTwo.ProjectActivities.Add(projectActivityThree);
            projectTwo.ProjectActivities.Add(projectActivityFour);
            var purposeOne = "PurposeOne";
            var purposeTwo = "PurposeTwo";
            var purposeThree = "PurposeThree";
            var purposeFour = "PurposeFour";
            projectOne.ProjectPurposes.Add(purposeOne);
            projectOne.ProjectPurposes.Add(purposeTwo);
            projectTwo.ProjectPurposes.Add(purposeThree);
            projectTwo.ProjectPurposes.Add(purposeFour);
            var employeeOne = new Employee(Authorizations.Admin, "Meiners", "Alexander", "UpdateProjectActivityTestOne",
                DateTime.Now, 3, 4, 3, RateCardLevel.Level7, null);
            var employeeTwo = new Employee(Authorizations.Pleb, "Schmitz", "Thomas", "UpdateProjectActivityTestTwo",
                DateTime.Now, 2, 1, 4, RateCardLevel.Level4, null);
            await _employeeService.UpdateEmployee(employeeOne);
            await _employeeService.UpdateEmployee(employeeTwo);
            
            projectActivityOne.AddEmployee(employeeOne.Id);
            projectActivityThree.AddEmployee(employeeTwo.Id);
            
            await _projectService.UpdateProject(projectOne);
            await _projectService.UpdateProject(projectTwo);

            var testProjects = await _projectService.GetAllProjects();
            
            Assert.True(testProjects.Contains(projectOne));
            Assert.True(testProjects.Contains(projectTwo));

            foreach (var project in testProjects)
            {
                if (project.Id.Equals(projectOne.Id))
                {
                    Assert.AreEqual(project, projectOne);
                }

                if (project.Id.Equals(projectTwo.Id))
                {
                    Assert.AreEqual(project, projectTwo);
                }
            }

            await _projectService.DeleteProject(projectOne.Id);
            await _projectService.DeleteProject(projectTwo.Id);
            await _employeeService.DeleteEmployee(employeeOne.Id);
            await _employeeService.DeleteEmployee(employeeTwo.Id);
            
            foreach (var project in dbProjects)
            {
                await _projectService.UpdateProject(project);
            }
        }

        [Test]
        public async Task GetProjectWhichDoesntExist()
        {
            var projectOne = new Project("UpdateProjectActivityTest Project Update", DateTime.Now, null, "dd");
            var projectTest = await _projectService.GetProject(projectOne.Id);
            Assert.IsNull(projectTest);
        }

        [Test]
        public async Task DeleteProjectActivityWithUpdateFromProject()
        {
            var projectOne = new Project("DeleteProjectActivities", DateTime.Now, null, "Test");
            var projectActivityOne = new ProjectActivity("e");
            var projectActivityTwo = new ProjectActivity("r");
            var projectActivityThree = new ProjectActivity("s");
            var projectActivityFour = new ProjectActivity("p");
            var employeeOne = new Employee(Authorizations.Admin, "Meiners", "Alexander", "e",
                DateTime.Now, 3, 4, 3, RateCardLevel.Level7, null);
            var employeeTwo = new Employee(Authorizations.Pleb, "Schmitz", "Thomas", "f",
                DateTime.Now, 2, 1, 4, RateCardLevel.Level4, null);
            var employeeThree = new Employee(Authorizations.Admin, "dds", "sdfs", "g",
                DateTime.Now, 3, 4, 3, RateCardLevel.Level7, null);
            var employeeFour = new Employee(Authorizations.Pleb, "dsfs", "Thsdfd", "h",
                DateTime.Now, 2, 1, 4, RateCardLevel.Level4, null);
            await _employeeService.UpdateEmployee(employeeOne);
            await _employeeService.UpdateEmployee(employeeTwo);
            await _employeeService.UpdateEmployee(employeeThree);
            await _employeeService.UpdateEmployee(employeeFour);
            projectOne.ProjectActivities.Add(projectActivityOne);
            projectOne.ProjectActivities.Add(projectActivityTwo);
            projectOne.ProjectActivities.Add(projectActivityThree);
            projectOne.ProjectActivities.Add(projectActivityFour);
            projectActivityOne.AddEmployee(employeeOne.Id);
            projectActivityOne.AddEmployee(employeeTwo.Id);
            projectActivityOne.AddEmployee(employeeThree.Id);
            projectActivityOne.AddEmployee(employeeFour.Id);

            await _projectService.UpdateProject(projectOne);

            projectOne.ProjectActivities.Remove(projectActivityOne);
            projectOne.ProjectActivities.Remove(projectActivityTwo);
            projectOne.ProjectActivities.Remove(projectActivityThree);
            projectOne.ProjectActivities.Remove(projectActivityFour);

            await _projectService.UpdateProject(projectOne);

            var projectTest = await _projectService.GetProject(projectOne.Id);

            await _projectService.DeleteProject(projectOne.Id);
            await _employeeService.DeleteEmployee(employeeOne.Id);
            await _employeeService.DeleteEmployee(employeeTwo.Id);
            await _employeeService.DeleteEmployee(employeeThree.Id);
            await _employeeService.DeleteEmployee(employeeFour.Id);
            
            Assert.AreEqual(projectOne, projectTest);
        }
    }
}