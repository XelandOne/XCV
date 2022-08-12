using System;
using Moq;
using NUnit.Framework;
using XCV.Entities;
using XCV.Entities.Enums;
using XCV.Services;

namespace XCV.Tests.UNIT.ManagerTest
{
    public class SearchManagerTest
    {
        private SearchManager _searchManager;
        private Employee _employee;
        
        [OneTimeSetUp]
        public void SetUp()
        {
            // Init mocked services
            var mockEmployeeManager = new Mock<EmployeeManager>(null, null);
            var mockLoadedExperience = new Mock<ExperienceManager>(null);
            
            // Init sample Experiences in mocked LoadedExperience
            mockLoadedExperience.Object.Fields.Add(new Field(Guid.NewGuid(), "Bildung"));
            mockLoadedExperience.Object.Roles.Add(new Role(Guid.NewGuid(), "Product Owner"));
            mockLoadedExperience.Object.SoftSkills.Add(new SoftSkill(Guid.NewGuid(), "Impulsgeben"));
            mockLoadedExperience.Object.HardSkills.Add(new HardSkill(Guid.NewGuid(), "C"));
            mockLoadedExperience.Object.Languages.Add(new Language(Guid.NewGuid(), "Englisch"));
            
            // Init sample Employee
            _employee = new Employee(
                new Guid(),
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
            
            // Init sample Employees Experiences
            _employee.Experience.Fields.Add(mockLoadedExperience.Object.Fields[0]);
            _employee.Experience.Roles.Add(mockLoadedExperience.Object.Roles[0]);
            _employee.Experience.SoftSkills.Add(mockLoadedExperience.Object.SoftSkills[0]);
            _employee.Experience.HardSkills.Add((mockLoadedExperience.Object.HardSkills[0], HardSkillLevel.Expert));
            _employee.Experience.Languages.Add((mockLoadedExperience.Object.Languages[0], LanguageLevel.Fluent));
            
            // Init Service
            mockEmployeeManager.Object.Employees.Add(_employee);
            _searchManager = new SearchManager(mockLoadedExperience.Object, mockEmployeeManager.Object);
        }
        

        [Test]
        public void TestSearchEmployeeDirectly()
        {
            // Adds an Employee to the selectedEmployee variable in _employeeSearchService
            _searchManager.SelectEmployee(_employee.Id, true);
            // tries the find the Employee in the return value of the search
            Assert.IsTrue(_searchManager.GetSearchResult().Exists(x => x.Item1.Equals(_employee)));
        }
        
        [Test]
        public void TestSearchEmployeeByExperience()
        {
            // Adds one of each experience from a given Employee to the selectedExperience variable in _employeeSearchService
            _searchManager.SelectEmployee(_employee.Id, true);
            _searchManager.SelectExperience(_employee.Experience.Fields[0].Id, true);
            _searchManager.SelectExperience(_employee.Experience.Roles[0].Id, true);
            _searchManager.SelectExperience(_employee.Experience.SoftSkills[0].Id, true);
            _searchManager.SelectExperience(_employee.Experience.Languages[0].Item1.Id, true);
            _searchManager.SelectExperience(_employee.Experience.HardSkills[0].Item1.Id, true);

            // starts the search
            var result = _searchManager.GetSearchResult();

            // tries to find the Employee
            Assert.IsTrue(result.Exists(x => x.Item1.Id.Equals(_employee.Id)));

            var cache = result.Find(x => x.Item1.Equals(_employee)).Item2;

            // looks whether the Experiences where added to the found Employee
            Assert.Contains(_employee.Experience.Fields[0].Id, cache);
            Assert.Contains(_employee.Experience.Roles[0].Id, cache);
            Assert.Contains(_employee.Experience.SoftSkills[0].Id, cache);
            Assert.Contains(_employee.Experience.Languages[0].Item1.Id, cache);
            Assert.Contains(_employee.Experience.HardSkills[0].Item1.Id, cache);
        }
    }
}