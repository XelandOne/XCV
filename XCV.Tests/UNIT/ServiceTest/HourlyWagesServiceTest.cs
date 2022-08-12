using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using XCV.Data;
using XCV.Entities.Enums;
using XCV.Services;

namespace XCV.Tests.UNIT.ServiceTest
{
    public class HourlyWagesServiceTest
    {
        private DatabaseUtils _databaseUtils;

        private IHourlyWagesService _hourlyWagesService;

        [OneTimeSetUp]
        public void SetUp()
        {
            var config = InitConfiguration();
            _databaseUtils = new DatabaseUtils(config);
            _hourlyWagesService = new HourlyWagesService(_databaseUtils);
            _databaseUtils.LoadTables();
        }
        
        private static IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder().AddJsonFile(Path.Combine(".","appsettings.Development.json")).Build();
            return config;
        }

        [Test]
        public async Task GetHourlyWage()
        {
            var priceRight = await _hourlyWagesService.GetHourlyWage(RateCardLevel.Level2);

            const double priceFalse = 100.00;

            await _hourlyWagesService.UpdateHourlyWage(RateCardLevel.Level2, priceFalse);

            var priceTest = await _hourlyWagesService.GetHourlyWage(RateCardLevel.Level2);

            if (priceRight != null) await _hourlyWagesService.UpdateHourlyWage(RateCardLevel.Level2, priceRight.Value);

            Assert.AreEqual(priceTest, priceFalse);

        }

        [Test]
        public async Task InsertAndDeleteTest()
        {
            var priceRight = await _hourlyWagesService.GetHourlyWage(RateCardLevel.Level4);

            var result = await _hourlyWagesService.DeleteHourlyWage(RateCardLevel.Level4);
            
            Assert.True(result);

            var getEmptyPrice = await _hourlyWagesService.GetHourlyWage(RateCardLevel.Level4);
            
            Assert.Null(getEmptyPrice);
            
            var insertion = priceRight != null && await _hourlyWagesService.UpdateHourlyWage(RateCardLevel.Level4, priceRight.Value);
            
            Assert.False(insertion);
        }
    }
}