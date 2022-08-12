using System;
using Moq;
using NUnit.Framework;
using XCV.Data;
using XCV.Entities;
using XCV.Entities.Enums;
using XCV.Services;

namespace XCV.Tests.UNIT.ManagerTest
{
    public class OfferManagerTest
    {
        private OfferManager _offerManager;
        private Mock<ExperienceManager> _mockExperienceManager;
        private Guid _projectId;
        
        [OneTimeSetUp]
        public void SetUp()
        {
            // Init mocked services
            var mockOfferService = new Mock<OfferService>(null, null);
            var mockShownEmployeePropertiesService = new Mock<ShownEmployeePropertiesService>(null, null);
            var mockProjectActivityService = new Mock<ProjectActivityService>(null);
            Mock<EmployeeManager> mockEmployeeManager = new Mock<EmployeeManager>(null, null);
            _mockExperienceManager = new Mock<ExperienceManager>(null);
            
            // Init sample Experiences in mocked LoadedExperience
            _mockExperienceManager.Object.Fields.Add(new Field(Guid.NewGuid(), "Bildung"));
            _mockExperienceManager.Object.Roles.Add(new Role(Guid.NewGuid(), "Product Owner"));
            _mockExperienceManager.Object.SoftSkills.Add(new SoftSkill(Guid.NewGuid(), "Impulsgeben"));
            _mockExperienceManager.Object.HardSkills.Add(new HardSkill(Guid.NewGuid(), "C"));
            _mockExperienceManager.Object.Languages.Add(new Language(Guid.NewGuid(), "Englisch"));

            // Init sample Employee
            Employee employee = new Employee(
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
            employee.Experience.Fields.Add(_mockExperienceManager.Object.Fields[0]);
            employee.Experience.Roles.Add(_mockExperienceManager.Object.Roles[0]);
            employee.Experience.SoftSkills.Add(_mockExperienceManager.Object.SoftSkills[0]);
            employee.Experience.HardSkills.Add((_mockExperienceManager.Object.HardSkills[0], HardSkillLevel.Expert));
            employee.Experience.Languages.Add((_mockExperienceManager.Object.Languages[0], LanguageLevel.Fluent));
            _projectId = Guid.NewGuid();
            employee.ProjectIds.Add(_projectId);
            
            mockEmployeeManager.Object.Employees.Add(employee);
            
            // Init Service
            _offerManager = new OfferManager(mockOfferService.Object, mockShownEmployeePropertiesService.Object, mockEmployeeManager.Object, mockProjectActivityService.Object);
            
            // Init sample offer
            var offer = new Offer(Guid.NewGuid(), "Angebot 01");
            offer.Experience.Fields.Add(_mockExperienceManager.Object.Fields[0]);
            offer.Experience.Roles.Add(_mockExperienceManager.Object.Roles[0]);
            offer.Experience.SoftSkills.Add(_mockExperienceManager.Object.SoftSkills[0]);
            offer.Experience.HardSkills.Add((_mockExperienceManager.Object.HardSkills[0], HardSkillLevel.Expert));
            offer.Experience.Languages.Add((_mockExperienceManager.Object.Languages[0], LanguageLevel.Fluent));
            var shortEmployees = new ShownEmployeeProperties(Guid.NewGuid(), employee.Id, employee.RateCardLevel, offer.Id) {Experience = employee.Experience};
            shortEmployees.SelectedExperience = shortEmployees.Experience;
            shortEmployees.SelectedExperience = new UsedExperience(shortEmployees.Experience);
            offer.ShortEmployees.Add(shortEmployees);
            _offerManager.Offers.Add(offer);
        }

        [Test]
        public void TestRemoveAddExperience()
        {
            var offer = _offerManager.Offers[0];
            
            var field = _mockExperienceManager.Object.Fields[0];
            _offerManager.RemoveExperience(field, offer.Id);
            Assert.False(offer.Experience.Fields.Contains(field));
            _offerManager.AddExperience(field, offer.Id);
            Assert.Contains(field, offer.Experience.Fields);
            
            var role = _mockExperienceManager.Object.Roles[0];
            _offerManager.RemoveExperience(role, offer.Id);
            Assert.False(offer.Experience.Roles.Contains(role));
            _offerManager.AddExperience(role, offer.Id);
            Assert.Contains(role, offer.Experience.Roles);
            
            var softSkill = _mockExperienceManager.Object.SoftSkills[0];
            _offerManager.RemoveExperience(softSkill, offer.Id);
            Assert.False(offer.Experience.SoftSkills.Contains(softSkill));
            _offerManager.AddExperience(softSkill, offer.Id);
            Assert.Contains(softSkill, offer.Experience.SoftSkills);
            
            var hardSkill = _mockExperienceManager.Object.HardSkills[0];
            _offerManager.RemoveExperience(hardSkill, offer.Id);
            Assert.False(offer.Experience.HardSkills.Exists(x => x.Item1.Equals(hardSkill)));
            _offerManager.UpdateHardSkill((hardSkill, HardSkillLevel.Expert), offer.Id);
            Assert.True(offer.Experience.HardSkills.Exists(x => x.Item1.Equals(hardSkill)));
            _offerManager.UpdateHardSkill((hardSkill, HardSkillLevel.HobbyUse), offer.Id);
            Assert.True(offer.Experience.HardSkills.Find(x => x.Item1.Equals(hardSkill)).Item2.Equals(HardSkillLevel.HobbyUse));
            
            var language = _mockExperienceManager.Object.Languages[0];
            _offerManager.RemoveExperience(language, offer.Id);
            Assert.False(offer.Experience.Languages.Exists(x => x.Item1.Equals(language)));
            _offerManager.UpdateLanguage((language, LanguageLevel.Fluent), offer.Id);
            Assert.True(offer.Experience.Languages.Exists(x => x.Item1.Equals(language)));
            _offerManager.UpdateLanguage((language, LanguageLevel.Beginner), offer.Id);
            Assert.True(offer.Experience.Languages.Find(x => x.Item1.Equals(language)).Item2.Equals(LanguageLevel.Beginner));
        }

        [Test]
        public void TestRemoveAddSelectedExperience()
        {
            var offer = _offerManager.Offers[0];
            var employee = offer.ShortEmployees[0];

            var field = _mockExperienceManager.Object.Fields[0];
            _offerManager.RemoveSelectedExperience(field, offer.Id, employee.Id);
            Assert.False(employee.SelectedExperience.Fields.Contains(field));
            _offerManager.AddSelectedExperience(field, offer.Id, employee.Id);
            Assert.Contains(field, employee.SelectedExperience.Fields);

            var role = _mockExperienceManager.Object.Roles[0];
            _offerManager.RemoveSelectedExperience(role, offer.Id, employee.Id);
            Assert.False(employee.SelectedExperience.Roles.Contains(role));
            _offerManager.AddSelectedExperience(role, offer.Id, employee.Id);
            Assert.Contains(role, employee.SelectedExperience.Roles);

            var softSkill = _mockExperienceManager.Object.SoftSkills[0];
            _offerManager.RemoveSelectedExperience(softSkill, offer.Id, employee.Id);
            Assert.False(employee.SelectedExperience.SoftSkills.Contains(softSkill));
            _offerManager.AddSelectedExperience(softSkill, offer.Id, employee.Id);
            Assert.Contains(softSkill, employee.SelectedExperience.SoftSkills);

            var hardSkill = _mockExperienceManager.Object.HardSkills[0];
            _offerManager.RemoveSelectedExperience(hardSkill, offer.Id, employee.Id);
            Assert.False(employee.SelectedExperience.HardSkills.Exists(x => x.Item1.Equals(hardSkill)));
            _offerManager.AddSelectedHardSkill((hardSkill, HardSkillLevel.Expert), offer.Id, employee.Id);
            Assert.True(employee.SelectedExperience.HardSkills.Exists(x => x.Item1.Equals(hardSkill)));

            var language = _mockExperienceManager.Object.Languages[0];
            _offerManager.RemoveSelectedExperience(language, offer.Id, employee.Id);
            Assert.True(!employee.SelectedExperience.Languages.Exists(x => x.Item1.Equals(language)));
            _offerManager.AddSelectedLanguage((language, LanguageLevel.Fluent), offer.Id, employee.Id);
            Assert.True(employee.SelectedExperience.Languages.Exists(x => x.Item1.Equals(language)));
        }

        [Test]
        public void TestRemoveAddProject()
        {
            var offer = _offerManager.Offers[0];
            var employee = offer.ShortEmployees[0];

            _offerManager.RemoveProjects(_projectId, offer.Id, employee.Id);
            Assert.False(employee.ProjectIds.Contains(_projectId));
            _offerManager.AddProjects(_projectId, offer.Id, employee.Id);
            Assert.Contains(_projectId, employee.ProjectIds);
            
        }
        
        [Test]
        public void TestUpdateRateCardLevel()
        {
            var offer = _offerManager.Offers[0];
            var employee = offer.ShortEmployees[0];
            
            _offerManager.UpdateRateCardLevel(RateCardLevel.Level5, offer.Id, employee.Id);
            Assert.AreEqual(RateCardLevel.Level5, employee.RateCardLevel);
        }

        [Test]
        public void RemoveAddShownEmployeeProperties()
        {
            var cache = new Employee(
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
            _offerManager.AddShownEmployeeProperties(cache, _offerManager.Offers[0].Id);
            Assert.AreEqual(cache.Id, _offerManager.Offers[0].ShortEmployees.Find(x => x.EmployeeId.Equals(cache.Id))?.EmployeeId);
            
            var shortEmployee = _offerManager.Offers[0].ShortEmployees.Find(x => x.EmployeeId.Equals(cache.Id));
            if (shortEmployee == null){ Assert.Fail(); return;}
            _offerManager.RemoveShownEmployeeProperties(shortEmployee.Id, _offerManager.Offers[0].Id);
            Assert.False(_offerManager.Offers[0].ShortEmployees.Contains(shortEmployee));
        }
    }
}