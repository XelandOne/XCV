using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using XCV.Data;
using XCV.Entities;
using XCV.Entities.Enums;
using XCV.Services;

namespace XCV.Tests.UNIT.ManagerTest
{
    public class EmployeeManagerTest
    {
        private EmployeeManager _employeeManager;
        private Employee _employee;
        private Project _project;
        private List<ProjectActivity> _projectActivities;
        private UsedExperience _experience;

        [OneTimeSetUp]
        public void SetUp()
        {
            // Init mocked services
            var mockEmployeeService = new Mock<IEmployeeService>();
            var mockProjectManager = new Mock<ProjectManager>(null, null);
            
            // Init sample Employee
            _employee = new Employee(
                Guid.NewGuid(),
                Authorizations.Admin,
                "Eli",
                "Davis",
                "DavisEli",
                DateTime.Now, 
                1,
                2,
                3,
                RateCardLevel.Level7,
                null
            );
            
            _experience = new UsedExperience();
            _experience.Fields.Add(new Field(Guid.NewGuid(), "Bildung"));
            _experience.Roles.Add(new Role(Guid.NewGuid(), "Product Owner"));
            _experience.SoftSkills.Add(new SoftSkill(Guid.NewGuid(), "Impulsgeben"));
            _experience.HardSkills.Add((new HardSkill(Guid.NewGuid(), "C"), HardSkillLevel.Expert));
            _experience.Languages.Add((new Language(Guid.NewGuid(), "Englisch"), LanguageLevel.Advanced));

            _employee.Experience = new UsedExperience(_experience);

            _projectActivities = new List<ProjectActivity>
            {
                new ("activity 1"),
                new ("activity 2"),
                new ("activity 3")
            };
            _projectActivities[0].AddEmployee(_employee.Id);
            _projectActivities[1].AddEmployee(_employee.Id);
            _projectActivities[2].AddEmployee(_employee.Id);

            _project = new Project(
                "Project 01",
                new Field(Guid.NewGuid(), "IT"),
                DateTime.Now,
                DateTime.Now, 
                "Test"
            );

            _projectActivities.ForEach(x => _project.ProjectActivities.Add(x));
            
            _employee.ProjectIds.Add(_project.Id);
            
            // Init Service
            _employeeManager = new EmployeeManager(mockEmployeeService.Object, mockProjectManager.Object);

            _employeeManager.AddNewEmployee(_employee);
            _employeeManager.LoginEmployee(_employee.UserName);
        }

        [Test]
        public void TestRemoveAddProjectActivity()
        {
            _employeeManager.RemoveProjectActivity(_projectActivities, _projectActivities[0].Id, _project.Id);
            Assert.True(!_projectActivities[0].GetEmployeeIds().Contains(_employee.Id));
            _employeeManager.AddProjectActivity(_projectActivities, _projectActivities[0].Id, _project.Id);
            Assert.Contains(_employee.Id, _projectActivities[0].GetEmployeeIds());
        }

        [Test]
        public void TestChangeEmployeeData()
        {
            var currentEmployee = _employeeManager.CurrentEmployee;
            if (currentEmployee == null) return;
            var employee = _employeeManager.GetEmployee(currentEmployee.Id);
            if (currentEmployee == null || employee == null) Assert.Fail();

            _employeeManager.UpdateName("Jessica", "Morales");
            Assert.AreEqual("Jessica", currentEmployee?.FirstName);
            Assert.AreEqual("Morales", currentEmployee?.SurName);
            Assert.AreEqual("Jessica", employee?.FirstName);
            Assert.AreEqual("Morales", employee?.SurName);

            var time = DateTime.Now;
            _employeeManager.UpdateRelevantWorkExperience(time, 4, 5, 6);
            Assert.AreEqual(time, currentEmployee?.EmployedSince);
            Assert.AreEqual(4, currentEmployee?.WorkExperience);
            Assert.AreEqual(5, currentEmployee?.ScientificAssistant);
            Assert.AreEqual(6, currentEmployee?.StudentAssistant);
            Assert.AreEqual(time, employee?.EmployedSince);
            Assert.AreEqual(4, employee?.WorkExperience);
            Assert.AreEqual(5, employee?.ScientificAssistant);
            Assert.AreEqual(6, employee?.StudentAssistant);
            
            _employeeManager.UpdateRateCardLevel(RateCardLevel.Level4);
            Assert.AreEqual(RateCardLevel.Level4, currentEmployee?.RateCardLevel);
            Assert.AreEqual(RateCardLevel.Level4, employee?.RateCardLevel);
            _employeeManager.UpdateProfilePicture(new byte[4]);
            Assert.AreEqual(new byte[4], currentEmployee?.ProfilePicture);
            Assert.AreEqual(new byte[4], employee?.ProfilePicture);
            
            _employeeManager.RemoveProjectIdFromCurrentEmployee(_project.Id);
            Assert.False(currentEmployee?.ProjectIds.Contains(_project.Id));
            Assert.False(employee?.ProjectIds.Contains(_project.Id));
            _employeeManager.AddProjectIdToCurrentEmployee(_project.Id);
            Assert.Contains(_project.Id, currentEmployee?.ProjectIds);
            Assert.Contains(_project.Id, employee?.ProjectIds);
        }

        [Test]
        public void TestAddProjectId()
        {
            var employeeTest = new Employee(
                Guid.NewGuid(),
                Authorizations.Admin,
                "Test",
                "Case",
                "TestCase",
                DateTime.Now,
                1,
                2,
                3,
                RateCardLevel.Level7,
                null
            );

            var projectTest = _project = new Project(
                "Project 01",
                new Field(Guid.NewGuid(), "IT"),
                DateTime.Now,
                DateTime.Now, 
                "Test"
            );
            employeeTest.ProjectIds.Add(projectTest.Id);
            
            _employeeManager.AddProjectId(employeeTest, projectTest.Id);
            Assert.AreEqual(1, employeeTest.ProjectIds.Count);
            _employeeManager.RemoveProjectId(employeeTest, projectTest.Id);
            Assert.AreEqual(0, employeeTest.ProjectIds.Count);
            _employeeManager.AddProjectId(employeeTest, projectTest.Id);
            Assert.Contains(projectTest.Id, employeeTest.ProjectIds);
        }

        [Test]
        public void TestRemoveAddExperience()
        {
            var currentEmployee = _employeeManager.CurrentEmployee;
            if (currentEmployee == null) return;
            var employee = _employeeManager.GetEmployee(currentEmployee.Id);
            if (currentEmployee == null || employee == null) Assert.Fail();
            
            _employeeManager.RemoveExperience(_experience.Fields[0]);
            Assert.False(currentEmployee?.Experience.Fields.Contains(_experience.Fields[0]));
            Assert.False(employee?.Experience.Fields.Contains(_experience.Fields[0]));
            _employeeManager.AddExperience(_experience.Fields[0]);
            Assert.Contains(_experience.Fields[0], currentEmployee?.Experience.Fields);
            Assert.Contains(_experience.Fields[0], employee?.Experience.Fields);
            
            _employeeManager.RemoveExperience(_experience.Roles[0]);
            Assert.False(currentEmployee?.Experience.Roles.Contains(_experience.Roles[0]));
            Assert.False(employee?.Experience.Roles.Contains(_experience.Roles[0]));
            _employeeManager.AddExperience(_experience.Roles[0]);
            Assert.Contains(_experience.Roles[0], currentEmployee?.Experience.Roles);
            Assert.Contains(_experience.Roles[0], employee?.Experience.Roles);
            
            _employeeManager.RemoveExperience(_experience.SoftSkills[0]);
            Assert.False(currentEmployee?.Experience.SoftSkills.Contains(_experience.SoftSkills[0]));
            Assert.False(employee?.Experience.SoftSkills.Contains(_experience.SoftSkills[0]));
            _employeeManager.AddExperience(_experience.SoftSkills[0]);
            Assert.Contains(_experience.SoftSkills[0], currentEmployee?.Experience.SoftSkills);
            Assert.Contains(_experience.SoftSkills[0], employee?.Experience.SoftSkills);
            
            _employeeManager.RemoveExperience(_experience.HardSkills[0].Item1);
            Assert.False(currentEmployee?.Experience.HardSkills.Contains(_experience.HardSkills[0]));
            Assert.False(employee?.Experience.HardSkills.Contains(_experience.HardSkills[0]));
            _employeeManager.UpdateHardSkill(_experience.HardSkills[0]);
            Assert.Contains(_experience.HardSkills[0], currentEmployee?.Experience.HardSkills);
            Assert.Contains(_experience.HardSkills[0], employee?.Experience.HardSkills);
            _employeeManager.UpdateHardSkill((_experience.HardSkills[0].Item1, HardSkillLevel.ProductiveUse));
            Assert.Contains((_experience.HardSkills[0].Item1, HardSkillLevel.ProductiveUse), currentEmployee?.Experience.HardSkills);
            Assert.Contains((_experience.HardSkills[0].Item1, HardSkillLevel.ProductiveUse), employee?.Experience.HardSkills);
            
            _employeeManager.RemoveExperience(_experience.Languages[0].Item1);
            Assert.False(currentEmployee?.Experience.Languages.Contains(_experience.Languages[0]));
            Assert.False(employee?.Experience.Languages.Contains(_experience.Languages[0]));
            _employeeManager.UpdateLanguage(_experience.Languages[0]);
            Assert.Contains(_experience.Languages[0], currentEmployee?.Experience.Languages);
            Assert.Contains(_experience.Languages[0], employee?.Experience.Languages);
            _employeeManager.UpdateLanguage((_experience.Languages[0].Item1, LanguageLevel.Intermediate));
            Assert.Contains((_experience.Languages[0].Item1, LanguageLevel.Intermediate), currentEmployee?.Experience.Languages);
            Assert.Contains((_experience.Languages[0].Item1, LanguageLevel.Intermediate), employee?.Experience.Languages);
        }
    }
}