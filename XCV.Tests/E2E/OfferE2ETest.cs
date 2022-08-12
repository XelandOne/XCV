using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using XCV.Data;
using XCV.Entities;
using XCV.Entities.Enums;
using XCV.Services;

namespace XCV.Tests.E2E
{
    [TestFixture(typeof(FirefoxDriver))]
    [TestFixture(typeof(ChromeDriver))]
    public class OfferE2ETest<TWebDriver> where TWebDriver : IWebDriver, new()
    {
        private IWebDriver? _driver;

        private readonly TimeSpan _timeSpan = TimeSpan.FromSeconds(40);

        private IHost? _programHost;

        private readonly bool _isPipelineActive = Environment.MachineName.Contains("runner");

        private DatabaseUtils _databaseUtils;

        private IOfferService _offerService;

        private IShownEmployeePropertiesService _shownEmployeePropertiesService;

        private IEmployeeService _employeeService;
        
        private Guid _employeeId = Guid.NewGuid();

        [OneTimeSetUp]
        public void StartProgram()
        {
            _programHost = Program.CreateHostBuilder(Array.Empty<string>()).Build();
            _programHost.Start();
            _databaseUtils = new DatabaseUtils(InitConfiguration());
            _databaseUtils.LoadTables();
            _employeeService = new EmployeeService(_databaseUtils);
            _shownEmployeePropertiesService = new ShownEmployeePropertiesService(_databaseUtils, _employeeService);
            _offerService = new OfferService(_databaseUtils, _shownEmployeePropertiesService);
        }

        private static IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder().AddJsonFile(Path.Combine(".", "appsettings.Development.json"))
                .Build();
            return config;
        }

        [SetUp]
        public void SetUp()
        {
            try
            {
                if (_isPipelineActive && typeof(TWebDriver) == typeof(ChromeDriver))
                {
                    return;
                }
                _driver = SeleniumHelper.GetWebDriver<TWebDriver>();

                _employeeService.UpdateEmployee(new Employee(_employeeId, Authorizations.SalesAdmin, "Sales", "Admin", "testAccountOfferInsert",DateTime.Now,
                    1, 1, 1, RateCardLevel.Level1, null));
                
                SeleniumHelper.Login(_driver, "testAccountOfferInsert");

                _driver.Navigate().GoToUrl("https://localhost:5001/OfferOverview");
            }
            catch (Exception e)
            {
                _driver?.Close();
                _driver?.Dispose();
                throw;
            }
        }

        [Test]
        [TestCase("Softwareprojekt", Description = "Normal Title for Offer")]
        [TestCase("firma123!", Description = "Title for Offer with alphabetical characters")]
        public void CreateOffer(string offerTitle)
        {
            if (_isPipelineActive && typeof(TWebDriver) == typeof(ChromeDriver))
            {
                Assert.Pass();
            }

            var wait = new WebDriverWait(_driver, _timeSpan);

            wait.Until(driver => driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div/div[1]/span/button")));
            var newOfferButton = _driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div/div[1]/span/button"));
            newOfferButton.Click();

            var drive = _driver.Url;

            Assert.True(_driver.Url.EndsWith("/OfferOverview"));

            
            wait.Until(driver => driver.FindElement(By.XPath("//*[@id='newOffer']")));
            var webElementTwo = _driver.FindElement(By.XPath("//*[@id='newOffer']"));
            webElementTwo.SendKeys(offerTitle);
            wait.Until(driver => driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div/div/li/div/div/button[1]")));
            var webElementThree =
                _driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div/div/li/div/div/button[1]"));
            webElementThree.Click();

            Assert.True(_driver.Url.EndsWith("/OfferOverview"));
        }

        /*[Test]
        public async Task AddEmployeeToOffer()
        {
            if (_isPipelineActive && typeof(TWebDriver) == typeof(ChromeDriver))
            {
                Assert.Pass();
            }

            var employee = new Employee(Authorization.Pleb, "Max", "Mustermann", "muster", DateTime.Now, 3, 4, 3,
                RateCardLevel.Level4, null);
            var employees = await _employeeService.GetAllEmployees();
            foreach (var tempEmployee in employees)
            {
                if (tempEmployee.UserName == employee.UserName)
                {
                    await _employeeService.DeleteEmployee(tempEmployee.Id);
                }
            }
            await _employeeService.UpdateEmployee(employee);

            var offers = await _offerService.GetAllOffers();

            foreach (var offer in offers)
            {
                await _offerService.DeleteOffer(offer.Id);
            }

            var offerOne = new Offer("Test");
            await _offerService.UpdateOffer(offerOne);

            var wait = new WebDriverWait(_driver, _timeSpan);

            _driver.Navigate().GoToUrl("https://localhost:5001/OfferOverview/Offer/" + offerOne.Id);

            wait.Until(driver => driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div/div[1]/span/a/button")));
            var editButton  = _driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div/div[1]/span/a/button"));
            editButton.Click();
            wait.Until(driver =>
                driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div/div[2]/div/div[1]/div/div/div[2]/a")));
            var offerEmployeePlusButton =
                _driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div/div[2]/div/div[1]/div/div/div[2]/a"));
            offerEmployeePlusButton.Click();

            Assert.True(_driver.Url.EndsWith("/EmployeeSearch/" + offerOne.Id));

            wait.Until(driver => driver.FindElement(By.XPath("//*[@id='muster']")));
            var checkBoxEmployee = _driver.FindElement(By.XPath("//*[@id='muster']"));
            checkBoxEmployee.Click();

            wait.Until(driver => driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div/div/div/div[1]/h4/a")));
            var startSearchButton = _driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div/div/div/div[1]/h4/a"));
            startSearchButton.Click();
            Assert.True(_driver.Url.EndsWith("/EmployeeSearch/Results/" + offerOne.Id));

            wait.Until(driver => driver.FindElement(By.XPath("//*[@id='muster']")));
            var checkBoxEmployeeAdd = _driver.FindElement(By.XPath("//*[@id='muster']"));
            checkBoxEmployeeAdd.Click();

            wait.Until(driver => driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div/div/div/div[1]/h4/a")));
            var addEmployeeToOfferButton = _driver.FindElement(By.XPath("/html/body/div[1]/div[2]/div/div/div/div[1]/h4/a"));
            addEmployeeToOfferButton.Click();
            Assert.True(_driver.Url.EndsWith("/OfferOverview/Offer/" + offerOne.Id));

            await _offerService.DeleteOffer(offerOne.Id);

            foreach (var offer in offers)
            {
                await _offerService.UpdateOffer(offer);
            }

            await _employeeService.DeleteEmployee(employee.Id);
        }*/

        [TearDown]
        public void TearDown()
        {
            _employeeService.DeleteEmployee(_employeeId);
            _driver?.Close();
            _driver?.Dispose();
        }

        [OneTimeTearDown]
        public async Task ClosedProgram()
        {
            if (_programHost != null) await _programHost.StopAsync();
            using IDbConnection connection = new SqlConnection(_databaseUtils.ConnectionString);
            try
            {
                await connection.ExecuteAsync("Delete from Offer where Title = 'firma123!' or Title = 'Softwareprojekt'");
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}