using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using XCV.Data;
using XCV.Entities;
using XCV.Entities.Enums;
using XCV.Services;

namespace XCV.Tests.UNIT.ServiceTest
{
    public class ShownEmployeePropertiesServiceBase
    {
         protected DatabaseUtils DatabaseUtils { get; set; }
         protected IShownEmployeePropertiesService ShownEmployeePropertiesService { get; set; }
         protected IEmployeeService EmployeeService { get; set; }
          protected IProjectService ProjectService { get; set; }
          
          protected IOfferService OfferService { get; set; }
        
        
        private static IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder().AddJsonFile(Path.Combine(".","appsettings.Development.json")).Build();
            return config;
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            this.DatabaseUtils = new DatabaseUtils(InitConfiguration());
            EmployeeService = new EmployeeService(DatabaseUtils);
            ProjectService = new ProjectService(DatabaseUtils);
            this.ShownEmployeePropertiesService = new Data.ShownEmployeePropertiesService(DatabaseUtils, EmployeeService);
            OfferService = new OfferService(DatabaseUtils, ShownEmployeePropertiesService);
            DatabaseUtils.LoadTables();
            
        } 
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            connection.Execute("DELETE FROM Employee WHERE Surname LIKE '%Test%' OR Firstname LIKE '%Test%'");
            connection.Execute("DELETE FROM Field WHERE FieldName LIKE 'testFieldShown%'");
            connection.Execute("DELETE FROM Role WHERE RoleName LIKE 'testRoleShown%'");
            connection.Execute("DELETE FROM SoftSkill WHERE SoftSkillName LIKE 'testSoftSkillShown%'");
            connection.Execute("DELETE FROM HardSkill WHERE HardSkillName LIKE 'testHardSkillShown%'");
            connection.Execute("DELETE FROM Language WHERE LanguageName LIKE 'testLanguageShown%'");
        }
        
        protected async Task<UsedExperience> GetUsedExperience(string testname)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString); 
            var UsedExp = new UsedExperience();
            UsedExp.Fields.Add(
                await connection.QueryFirstAsync<Field>(
                    "IF NOT EXISTS (SELECT * FROM Field WHERE FieldName = @name) BEGIN INSERT INTO Field VALUES (@id, @name, CURRENT_TIMESTAMP) END SELECT Id, FieldName AS name, lastChanged FROM Field WHERE FieldName = @name",
                    new {id = Guid.NewGuid(), name = "testFieldShown'" + testname}));
            UsedExp.Roles.Add(
                await connection.QueryFirstAsync<Role>(
                    "IF NOT EXISTS (SELECT * FROM Role WHERE RoleName = @name) BEGIN INSERT INTO Role VALUES (@id, @name, CURRENT_TIMESTAMP) END SELECT Id, RoleName AS name, lastChanged FROM Role WHERE RoleName = @name",
                    new {id = Guid.NewGuid(), name = "testRoleShown'" + testname}));
            UsedExp.SoftSkills.Add(
                await connection.QueryFirstAsync<SoftSkill>(
                    "IF NOT EXISTS (SELECT * FROM SoftSkill WHERE SoftSkillName = @name) BEGIN INSERT INTO SoftSkill VALUES (@id, @name, CURRENT_TIMESTAMP) END SELECT Id, SoftSkillName AS name, lastChanged FROM SoftSkill WHERE SoftSkillName = @name",
                    new {id = Guid.NewGuid(), name = "testSoftSkillShown'" + testname}));
            UsedExp.HardSkills.Add(Tuple.Create(await connection.QueryFirstAsync<HardSkill>(
                    "IF NOT EXISTS (SELECT * FROM HardSkill WHERE HardSkillName = @name) BEGIN INSERT INTO HardSkill VALUES (@id, @name, @category, CURRENT_TIMESTAMP) END SELECT Id, HardSkillName as name, HardSkillCategory, lastChanged FROM HardSkill WHERE HardSkillName = @name",
                    new {id = Guid.NewGuid(), name = "testHardSkillShown'" + testname, category = "test"}),
                HardSkillLevel.Expert).ToValueTuple());
            UsedExp.Languages.Add(Tuple.Create(await connection.QueryFirstAsync<Language>(
                    "IF NOT EXISTS (SELECT * FROM Language WHERE LanguageName = @name) BEGIN INSERT INTO Language VALUES (@id, @name, CURRENT_TIMESTAMP) END SELECT Id, LanguageName as name, lastChanged FROM Language WHERE LanguageName = @name",
                    new {id = Guid.NewGuid(), name = "testLanguageShown'" + testname}),
                LanguageLevel.Advanced).ToValueTuple());
            return UsedExp;
        }
    }
    
    
    [TestFixture]
    public class ShownEmployeePropertiesServiceTest : ShownEmployeePropertiesServiceBase
    {
        [Test]
        public async Task GetShownEmployeePropertiesTest_ShouldReturnEmployee_WhenExits()
        {
            var offer = new Offer("TestOffer");
            //arrange
            var employee = new Employee(Authorizations.Sales, "Get", "GetEmployeeTest", "GetEmployeeTestUserOne", DateTime.Now,
                15, 1, 1, RateCardLevel.Level3, null);
            employee.Experience = await GetUsedExperience("Get");
            var shortEmployee = new ShownEmployeeProperties(employee, offer.Id)
            {
                SelectedExperience = await GetUsedExperience("GetShown"),
                PlannedWeeklyHours = 40,
                Discount = 0.2
            };
            await OfferService.UpdateOffer(offer);
            await EmployeeService.UpdateEmployee(employee);
            var lastChanged = await ShownEmployeePropertiesService.UpdateShownEmployeeProperties(shortEmployee);
            Thread.Sleep(10);
            if (lastChanged.Item2 == DataBaseResult.Inserted)
                shortEmployee.LastChanged = lastChanged.Item1;
            //act
            var result = await ShownEmployeePropertiesService.GetShownEmployeeProperties(shortEmployee.Id);
            await OfferService.DeleteOffer(offer.Id);
            //assert
            Assert.AreEqual(shortEmployee, result);
        }

        [Test]
        public async Task GetShownEmployeePropertiesTest_ShouldReturnNull_WhenNotExits()
        {
            Assert.Null(await ShownEmployeePropertiesService.GetShownEmployeeProperties(Guid.NewGuid()));
        }
        
        [Test]
        public async Task GetShownAllEmployeePropertiesTest_ShouldReturnEmployees_WhenExits()
        {
            var offer = new Offer("Test2");
            //arrange
            var employee = new Employee(Authorizations.Sales, "first", "GetAllEmployeeTest", "GetAllEmployeeTestUserTwo", DateTime.Now,
                15, 1, 1, RateCardLevel.Level3, null) {Experience = await GetUsedExperience("GetAllOne")};
            var employee2 = new Employee(Authorizations.Sales, "second", "GetAllEmployeeTest", "usernameTwo", DateTime.Now,
                15, 1, 1, RateCardLevel.Level3, null) {Experience = await GetUsedExperience("GetAllTwo")};
            var shortEmployee = new ShownEmployeeProperties(employee, offer.Id) {SelectedExperience = await GetUsedExperience("GetAllOneShown"), PlannedWeeklyHours = 40};
            var shortEmployee2 = new ShownEmployeeProperties(employee, offer.Id) {SelectedExperience = await GetUsedExperience("GetAllTwoShown"), Discount = 0.1};
            await OfferService.UpdateOffer(offer);
            await EmployeeService.UpdateEmployee(employee);
            await EmployeeService.UpdateEmployee(employee2);
            await ShownEmployeePropertiesService.UpdateShownEmployeeProperties(shortEmployee);
            await ShownEmployeePropertiesService.UpdateShownEmployeeProperties(shortEmployee2);
            //act
            var result = await ShownEmployeePropertiesService.GetAllShownEmployeeProperties();
            await OfferService.DeleteOffer(offer.Id);
            //assert
            Assert.True(result.Contains(shortEmployee));
            Assert.True(result.Contains(shortEmployee2));
        }

        [Test]
        public async Task UpdateShownEmployeeProperty_ShouldReturnFalse_WhenNotExits()
        {
            var offer = new Offer("Test3");
            var employee = new Employee(Authorizations.Sales, "false", "UpdateEmployeeTest", "UpdateEmployeeTestUser", DateTime.Now,
                15, 1, 1, RateCardLevel.Level3, null) {Experience = await GetUsedExperience("UpdateInsert")};
            var shortEmployee = new ShownEmployeeProperties(employee, offer.Id) {SelectedExperience = await GetUsedExperience("UpdateInsertShown")};
            await EmployeeService.UpdateEmployee(employee);
            await OfferService.UpdateOffer(offer);
            var result = await ShownEmployeePropertiesService.UpdateShownEmployeeProperties(shortEmployee);
            await OfferService.DeleteOffer(offer.Id);
            Assert.True(result.Item2 == DataBaseResult.Inserted);
        }
        
        [Test]
        public async Task UpdateShownEmployeeProperty_ShouldReturnTrue_WhenNotExits()
        {
            var offer = new Offer("Test4");
            var employee = new Employee(Authorizations.Sales, "false", "UpdateEmployeeTest", "UpdateEmployeeTestUserTwo", DateTime.Now,
                15, 1, 1, RateCardLevel.Level3, null) {Experience = await GetUsedExperience("UpdateUpdate")};
            var shortEmployee = new ShownEmployeeProperties(employee, offer.Id) {SelectedExperience = await GetUsedExperience("UpdateUpdateShown")};
            await EmployeeService.UpdateEmployee(employee);
            await OfferService.UpdateOffer(offer);
            Thread.Sleep(10);
            var lastChanged = await ShownEmployeePropertiesService.UpdateShownEmployeeProperties(shortEmployee);
            if (lastChanged.Item2 == DataBaseResult.Inserted)
                shortEmployee.LastChanged = lastChanged.Item1;
            shortEmployee.RateCardLevel = RateCardLevel.Level1;
            var result = await ShownEmployeePropertiesService.UpdateShownEmployeeProperties(shortEmployee);
            Assert.True(result.Item2 == DataBaseResult.Updated);
            
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            var rcl = await connection.QueryFirstOrDefaultAsync<RateCardLevel>(
                "SELECT RateCardLevel FROM ShownEmployeeProperty WHERE Id = @id", new {id = shortEmployee.Id});
            await OfferService.DeleteOffer(offer.Id);
            Assert.AreEqual(RateCardLevel.Level1, rcl);
        }
        
        [Test]
        public async Task DeleteOfferTest_ShouldReturnFalse_WhenNotExits()
        {
            var result = await ShownEmployeePropertiesService.DeleteShownEmployeeProperties(Guid.NewGuid());
            Assert.False(result != null && result != new DateTime());
        }

        [Test]
        public async Task UpdateShownEmployeePropertyWithProjectAndProjectActivities()
        {
            var offer = new Offer("Test5");
            var employee = new Employee(Authorizations.Sales, "ShownEmployeeWithProject", "UpdateEmployeeTestProject", "ShownEmployeeWithProjectUser", DateTime.Now,
                15, 1, 1, RateCardLevel.Level3, null);
            var project =
                new Project("ForShownEmployeeTest", DateTime.Now, null, "For Testing");
            var projectActivity = new ProjectActivity("UpdateShortEmployeeTest");
            project.ProjectActivities.Add(projectActivity);
            employee.ProjectIds.Add(project.Id);

            var shownEmployeeProperties = new ShownEmployeeProperties(employee, offer.Id);
            
            shownEmployeeProperties.ProjectActivityIds.Add(projectActivity.Id);

            await OfferService.UpdateOffer(offer);
            
            await ProjectService.UpdateProject(project);
            
            await EmployeeService.UpdateEmployee(employee);

            await ShownEmployeePropertiesService.UpdateShownEmployeeProperties(shownEmployeeProperties);

            var employeeTest = await EmployeeService.GetEmployee(employee.Id);

            var projectTest = await ProjectService.GetProject(project.Id);

            var shownEmployeePropertiesTest =
                await ShownEmployeePropertiesService.GetShownEmployeeProperties(shownEmployeeProperties.Id);

            await OfferService.DeleteOffer(offer.Id);
            
            await ShownEmployeePropertiesService.DeleteShownEmployeeProperties(shownEmployeeProperties.Id);

            await EmployeeService.DeleteEmployee(project.Id);
                
            await ProjectService.DeleteProject(project.Id);

            Assert.AreEqual(employee, employeeTest);

            Assert.AreEqual(project, projectTest);

            Assert.AreEqual(shownEmployeeProperties, shownEmployeePropertiesTest);
        }
    }
}