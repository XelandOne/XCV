using System.Collections.Generic;
using System.Threading.Tasks;
using XCV.Entities.Enums;

namespace XCV.Data
{
    /// <inheritdoc />
    public class DummyHourlyWagesService : IHourlyWagesService
    {
        private readonly Dictionary<RateCardLevel, double> _hourlyWages = new Dictionary<RateCardLevel, double>()
        {
            {RateCardLevel.Level1, 80.00},
            {RateCardLevel.Level2, 95.00},
            {RateCardLevel.Level3, 105.00},
            {RateCardLevel.Level4, 120.00},
            {RateCardLevel.Level5, 135.00},
            {RateCardLevel.Level6, 155.00},
            {RateCardLevel.Level7, 195.00},
            {RateCardLevel.Level8, 260.00}
        };

        /// <inheritdoc />
        public async Task<double?> GetHourlyWage(RateCardLevel rateCardLevel)
        {
            return await Task.FromResult(_hourlyWages[rateCardLevel]);
        }

        /// <inheritdoc />
        public async Task<bool> UpdateHourlyWage(RateCardLevel rateCardLevel, double wage)
        {
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteHourlyWage(RateCardLevel rateCardLevel)
        {
            return true;
        }
    }
}