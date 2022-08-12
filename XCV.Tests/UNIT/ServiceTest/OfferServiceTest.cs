using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using XCV.Data;
using XCV.Entities;
using XCV.Entities.Enums;
using XCV.Services;

namespace XCV.Tests.UNIT.ServiceTest
{
    public  class OfferServiceBase
    {
        private IOfferService _offerService;
        private DatabaseUtils _databaseUtils;
        private IShownEmployeePropertiesService _shownEmployeePropertiesService;
        private IEmployeeService _employeeService;
        private IDocumentConfigurationService _documentConfigurationService;

        private static IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder().AddJsonFile(Path.Combine(".", "appsettings.Development.json"))
                .Build();
            return config;
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _databaseUtils = new DatabaseUtils(InitConfiguration());
            _employeeService = new EmployeeService(_databaseUtils);
            _shownEmployeePropertiesService = new ShownEmployeePropertiesService(_databaseUtils, _employeeService);
            _documentConfigurationService = new DocumentConfigurationService(_databaseUtils);
            _offerService = new OfferService(_databaseUtils, _shownEmployeePropertiesService);

            _databaseUtils.LoadTables();

        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            using IDbConnection connection = new SqlConnection(_databaseUtils.ConnectionString);
            connection.Execute("DELETE FROM Offer WHERE Title LIKE '!TESTING!%'");
            connection.Execute("DELETE FROM Field WHERE FieldName LIKE 'testFieldOffer%'");
            connection.Execute("DELETE FROM Role WHERE RoleName LIKE 'testRoleOffer%'");
            connection.Execute("DELETE FROM SoftSkill WHERE SoftSkillName LIKE 'testSoftSkillOffer%'");
            connection.Execute("DELETE FROM HardSkill WHERE HardSkillName LIKE 'testHardSkillOffer%'");
            connection.Execute("DELETE FROM Language WHERE LanguageName LIKE 'testLanguageOffer%'");
            connection.Execute("DELETE FROM Employee WHERE Username LIKE '%Offer%Test%'");
        }

        protected async Task<UsedExperience> GetUsedExperience(string testname)
        {
            using IDbConnection connection = new SqlConnection(_databaseUtils.ConnectionString);
            var usedExp = new UsedExperience();
            usedExp.Fields.Add(
                await connection.QueryFirstAsync<Field>(
                    "IF NOT EXISTS (SELECT * FROM Field WHERE FieldName = @name) BEGIN INSERT INTO Field VALUES (@id, @name, CURRENT_TIMESTAMP) END SELECT Id, FieldName AS name, lastChanged FROM Field WHERE FieldName = @name",
                    new {id = Guid.NewGuid(), name = "testFieldOffer" + testname}));
            usedExp.Roles.Add(
                await connection.QueryFirstAsync<Role>(
                    "IF NOT EXISTS (SELECT * FROM Role WHERE RoleName = @name) BEGIN INSERT INTO Role VALUES (@id, @name, CURRENT_TIMESTAMP) END SELECT Id, RoleName AS name, lastChanged FROM Role WHERE RoleName = @name",
                    new {id = Guid.NewGuid(), name = "testRoleOffer" + testname}));
            usedExp.SoftSkills.Add(
                await connection.QueryFirstAsync<SoftSkill>(
                    "IF NOT EXISTS (SELECT * FROM SoftSkill WHERE SoftSkillName = @name) BEGIN INSERT INTO SoftSkill VALUES (@id, @name, CURRENT_TIMESTAMP) END SELECT Id, SoftSkillName AS name, lastChanged FROM SoftSkill WHERE SoftSkillName = @name",
                    new {id = Guid.NewGuid(), name = "testSoftSkillOffer" + testname}));
            usedExp.HardSkills.Add(Tuple.Create(await connection.QueryFirstAsync<HardSkill>(
                    "IF NOT EXISTS (SELECT * FROM HardSkill WHERE HardSkillName = @name) BEGIN INSERT INTO HardSkill VALUES (@id, @name, @category, CURRENT_TIMESTAMP) END SELECT Id, HardSkillName as name, HardSkillCategory, lastChanged FROM HardSkill WHERE HardSkillName = @name",
                    new {id = Guid.NewGuid(), name = "testHardSkillOffer" + testname, category = "test"}),
                HardSkillLevel.Expert).ToValueTuple());
            usedExp.Languages.Add(Tuple.Create(await connection.QueryFirstAsync<Language>(
                    "IF NOT EXISTS (SELECT * FROM Language WHERE LanguageName = @name) BEGIN INSERT INTO Language VALUES (@id, @name, CURRENT_TIMESTAMP) END SELECT Id, LanguageName as name, lastChanged FROM Language WHERE LanguageName = @name",
                    new {id = Guid.NewGuid(), name = "testLanguageOffer" + testname}),
                LanguageLevel.Advanced).ToValueTuple());
            return usedExp;
        }

        [Test]
        public async Task GetOfferTest_ShouldReturnNull_WhenNotExits()
        {
            Assert.Null(await _offerService.GetOffer(Guid.NewGuid()));
        }

        [Test]
        public async Task GetOfferTest_ShouldReturnOffer_WhenExits()
        {
            var offer = new Offer("!TESTING!Get");
            offer.Experience = await GetUsedExperience("Get");
            await _offerService.UpdateOffer(offer);
            Thread.Sleep(10);
            var result = await _offerService.GetOffer(offer.Id);
            Assert.NotNull(result);
            Assert.AreEqual(offer, result);
        }

        [Test]
        public async Task UpdateOfferTest_ShouldInsertOffer_WhenNotExits()
        {
            var offer = new Offer("!TESTING!Insert");
            offer.Experience = await GetUsedExperience("UpdateInsert");
            var result = await _offerService.UpdateOffer(offer);
            Assert.True(result.Item2 == DataBaseResult.Inserted);
            using IDbConnection connection = new SqlConnection(_databaseUtils.ConnectionString);
            var insertedEmployee =
                await connection.QueryFirstOrDefaultAsync("SELECT Id FROM Offer WHERE Id = @id",
                    new {id = offer.Id});
            Assert.NotNull(insertedEmployee);
        }

        [Test]
        public async Task UpdateOfferTest_ShouldUpdateOffer_WhenNotExits()
        {
            var offer = new Offer("!TESTING!toBeUpdated");
            offer.Experience = await GetUsedExperience("UpdateUpdate");
            await _offerService.UpdateOffer(offer);
            offer.Title = "!TESTING!Updated";
            var result = await _offerService.UpdateOffer(offer);
            Assert.True(result.Item2 == DataBaseResult.Updated);
            using IDbConnection connection = new SqlConnection(_databaseUtils.ConnectionString);
            var title =
                await connection.QueryFirstOrDefaultAsync<string>("SELECT Title FROM Offer WHERE Id = @id",
                    new {id = offer.Id});
            Assert.AreEqual(offer.Title, title);
        }

        [Test]
        public async Task GetAllOffersTest_ShouldReturnAllOffers()
        {
            var offer1 = new Offer("!TESTING!offer1");
            var offer2 = new Offer("!TESTING!offer2");
            offer1.Experience = await GetUsedExperience("GetAllOne");
            ;
            offer2.Experience = await GetUsedExperience("GetAllTwo");
            ;
            await _offerService.UpdateOffer(offer1);
            await _offerService.UpdateOffer(offer2);

            var list = await _offerService.GetAllOffers();

            Assert.True(list.Any());
            Assert.Contains(offer1, list);
            Assert.Contains(offer2, list);
        }


        [Test]
        public async Task DeleteOfferTest_ShouldReturnFalse_WhenNotExits()
        {
            Assert.False(await _offerService.DeleteOffer(Guid.NewGuid()));
        }

        [Test]
        public async Task InsertOfferWithShortEmployees()
        {
            var employee1 = new Employee(Authorizations.Admin, "SurName", "FirstName", "OfferInsertTestShort",
                DateTime.Now, 1, 2, 3, RateCardLevel.Level3, null);
            await _employeeService.UpdateEmployee(employee1);

            var offer = new Offer("Test Title", null, null, new List<Employee>() {employee1});
            await _offerService.UpdateOffer(offer);

            var offerTest = await _offerService.GetOffer(offer.Id);

            var employeeTest = await _employeeService.GetEmployee(employee1.Id);

            if (!offer.Equals(offerTest))
            {
                Assert.Fail("Offer was not properly inserted");
            }

            if (!employee1.Equals(employeeTest))
            {
                Assert.Fail("Employee was not properly inserted");
            }

            await _offerService.DeleteOffer(offer.Id);

            await _employeeService.DeleteEmployee(employee1.Id);

            Assert.Pass("Offer and Employee were properly inserted");
        }

        [Test]
        public async Task GetAllOffersWhenEmpty()
        {
            var offers = await _offerService.GetAllOffers();

            foreach (var offer in offers)
            {
                await _offerService.DeleteOffer(offer.Id);
            }

            var offersEmpty = await _offerService.GetAllOffers();

            foreach (var offer in offers)
            {
                await _offerService.UpdateOffer(offer);
            }

            Assert.IsEmpty(offersEmpty);
        }

        [Test]
        public async Task UpdateOfferWithStartAndEndDate()
        {
            var offer = new Offer("OfferWithStartAndEndDate", DateTime.Now, DateTime.Now);

            await _offerService.UpdateOffer(offer);

            var offerTest = await _offerService.GetOffer(offer.Id);

            await _offerService.DeleteOffer(offer.Id);

            Assert.AreEqual(offer, offerTest);
        }

        [Test]
        public async Task GetOfferWithDocumentIds()
        {
            var offer = new Offer("GetOfferWithDocumentIds");
            var documentOne = new DocumentConfiguration("GetOfferTest", false, false, false, offer);
            offer.DocumentConfigurations.Add(documentOne.Id);
            await _offerService.UpdateOffer(offer);
            await _documentConfigurationService.UpdateDocumentConfiguration(documentOne);
            var offerTest = await _offerService.GetOffer(offer.Id);

            await _offerService.DeleteOffer(offer.Id);
            
            Assert.AreEqual(offer, offerTest);
        }

        [Test]
        public async Task GetAllOfferWithDocumentIds()
        {
            var offersDataBase = await _offerService.GetAllOffers();

            foreach (var offer in offersDataBase)
            {
                await _offerService.DeleteOffer(offer.Id);
            }
            
            var offerOne = new Offer("GetAllOfferOne");
            var offerTwo = new Offer("GetAllOfferTwo");
            var documentOne = new DocumentConfiguration("GetAllOfferTestOne", false, false, false, offerOne);
            var documentTwo = new DocumentConfiguration("GetAllOfferTestTwo", false, false, false, offerOne);
            var documentThree = new DocumentConfiguration("GetAllOfferTestThree", false, false, false, offerTwo);
            var documentFour = new DocumentConfiguration("GetAllOfferTestFour", false, false, false, offerTwo);
            offerOne.DocumentConfigurations.Add(documentOne.Id);
            offerOne.DocumentConfigurations.Add(documentTwo.Id);
            offerTwo.DocumentConfigurations.Add(documentThree.Id);
            offerTwo.DocumentConfigurations.Add(documentFour.Id);

            await _offerService.UpdateOffer(offerOne);
            await _offerService.UpdateOffer(offerTwo);
            await _documentConfigurationService.UpdateDocumentConfiguration(documentOne);
            await _documentConfigurationService.UpdateDocumentConfiguration(documentTwo);
            await _documentConfigurationService.UpdateDocumentConfiguration(documentThree);
            await _documentConfigurationService.UpdateDocumentConfiguration(documentFour);

            var offerTest = await _offerService.GetAllOffers();

            foreach (var offer in offerTest)
            {
                await _offerService.DeleteOffer(offer.Id);
            }

            foreach (var offer in offersDataBase)
            {
                await _offerService.UpdateOffer(offer);
            }
            
            Assert.Contains(offerOne, offerTest);
            Assert.Contains(offerTwo, offerTest);

        }
    }
}
