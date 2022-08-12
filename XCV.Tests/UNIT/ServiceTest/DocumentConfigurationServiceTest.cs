using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
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
    public class DocumentConfigurationServiceTest
    {

        private DatabaseUtils _databaseUtils;

        private IDocumentConfigurationService _documentConfigurationService;

        private IOfferService _offerService;

        private IShownEmployeePropertiesService _shownEmployeePropertiesService;

        private IEmployeeService _employeeService;

        [OneTimeSetUp]
        public void SetUp()
        {
            var config = InitConfiguration();
            _databaseUtils = new DatabaseUtils(config);
            _documentConfigurationService = new DocumentConfigurationService(_databaseUtils);
            _employeeService = new EmployeeService(_databaseUtils);
            _shownEmployeePropertiesService = new ShownEmployeePropertiesService(_databaseUtils, _employeeService);
            _offerService = new OfferService(_databaseUtils, _shownEmployeePropertiesService);
            _databaseUtils.LoadTables();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            using IDbConnection connection = new SqlConnection(_databaseUtils.ConnectionString);
            connection.Execute("DELETE FROM Employee WHERE Surname LIKE '%DocumentTest%' OR Firstname LIKE '%DocumentTest%'");
            connection.Execute("DELETE FROM Offer WHERE Title LIKE '%DocumentTest%'");
            connection.Execute("DELETE FROM DocumentConfigurations WHERE Title LIKE '%DocumentTest%'");
        }
        
        private static IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder().AddJsonFile(Path.Combine(".","appsettings.Development.json")).Build();
            return config;
            
            
        }

        [Test]
        public async Task InsertDocumentTest()
        {
            //arrange
            var offer = new Offer("UpdateDocumentTest");
            
            var employee = new Employee(Authorizations.Sales, "Inserted", "InsertDocumentTest", "InsertDocumentTest", DateTime.Now,
                15, 1, 1, RateCardLevel.Level3, null);
            var shownEmployeeProperty = new ShownEmployeeProperties(employee, offer.Id);
            var employeeIds = new List<Guid> {shownEmployeeProperty.Id};
            var documentConfiguration = new DocumentConfiguration("InsertDocumentTest", false, false, false, offer, employeeIds);
            
            //act
            await _offerService.UpdateOffer(offer);
            await _employeeService.UpdateEmployee(employee);
            await _shownEmployeePropertiesService.UpdateShownEmployeeProperties(shownEmployeeProperty);
            await _documentConfigurationService.UpdateDocumentConfiguration(documentConfiguration);
            var documentConfigurationTest =
                await _documentConfigurationService.GetDocumentConfiguration(documentConfiguration.Id);
            
            //assert
            Assert.AreEqual(documentConfiguration, documentConfigurationTest);
        }

        [Test]
        public async Task DeleteDocumentTest()
        {
            //arrange
            var offer = new Offer("DeleteDocumentTest");
            var employee = new Employee(Authorizations.Sales, "tobedeleted", "DeleteDocumentTest", "DeleteDocumentTest", DateTime.Now,
                15, 1, 1, RateCardLevel.Level3, null);
            var shownEmployeeProperty = new ShownEmployeeProperties(employee, offer.Id);
            var employeeIds = new List<Guid> {shownEmployeeProperty.Id};
            var documentConfiguration = new DocumentConfiguration("DeleteDocumentTest", false, false, false, offer, employeeIds);
            
            //act
            await _offerService.UpdateOffer(offer);
            await _employeeService.UpdateEmployee(employee);
            await _shownEmployeePropertiesService.UpdateShownEmployeeProperties(shownEmployeeProperty);
            await _documentConfigurationService.UpdateDocumentConfiguration(documentConfiguration);
            await _documentConfigurationService.DeleteDocumentConfiguration(documentConfiguration.Id);
            
            //assert
            Assert.Null(await _documentConfigurationService.GetDocumentConfiguration(documentConfiguration.Id));
        }

        [Test]
        public async Task UpdateDocumentTest()
        {
            //arrange
            var offer = new Offer("UpdateDocumentTest");
            
            var employeeOne = new Employee(Authorizations.Sales, "One", "UpdateDocumentTest", "UpdateDocumentTestOne", DateTime.Now,
                15, 1, 1, RateCardLevel.Level3, null);
            var employeeTwo = new Employee(Authorizations.Sales, "Two", "UpdateDocumentTest", "UpdateDocumentTestTwo", DateTime.Now,
                15, 1, 1, RateCardLevel.Level3, null);
            var shownEmployeePropertyOne = new ShownEmployeeProperties(employeeOne, offer.Id);
            var shownEmployeePropertyTwo = new ShownEmployeeProperties(employeeTwo, offer.Id);
            var employeeIds = new List<Guid> {shownEmployeePropertyOne.Id};
            var documentConfiguration = new DocumentConfiguration("InsertDocumentTest", false, false, false, offer, employeeIds);
            
            //act
            await _offerService.UpdateOffer(offer);
            await _employeeService.UpdateEmployee(employeeOne);
            await _employeeService.UpdateEmployee(employeeTwo);
            await _shownEmployeePropertiesService.UpdateShownEmployeeProperties(shownEmployeePropertyOne);
            await _shownEmployeePropertiesService.UpdateShownEmployeeProperties(shownEmployeePropertyTwo);
            //update one (insertion)
            await _documentConfigurationService.UpdateDocumentConfiguration(documentConfiguration);
            //changes
            documentConfiguration.ShowCoverSheet = true;
            documentConfiguration.ShownEmployeePropertyIds.Remove(shownEmployeePropertyOne.Id);
            documentConfiguration.ShownEmployeePropertyIds.Add(shownEmployeePropertyTwo.Id);
            //update two (update)
            await _documentConfigurationService.UpdateDocumentConfiguration(documentConfiguration);
            
            //assert
            Assert.AreEqual(documentConfiguration, await _documentConfigurationService.GetDocumentConfiguration(documentConfiguration.Id));

        }
    }
}