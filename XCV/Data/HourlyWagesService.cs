using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Components;
using XCV.Entities.Enums;
using XCV.Services;

namespace XCV.Data
{
    /// <inheritdoc />
    public class HourlyWagesService : IHourlyWagesService
    {

        [Inject] private DatabaseUtils DatabaseUtils { get; set; }
        /// <summary>
        /// Create new Instance of HourlyWagesService
        /// </summary>
        /// <param name="databaseUtils"></param>
        public HourlyWagesService(DatabaseUtils databaseUtils)
        {
            DatabaseUtils = databaseUtils;
        }

        /// <inheritdoc />
        public async Task<double?> GetHourlyWage(RateCardLevel rateCardLevel)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            var result = await connection.QueryFirstOrDefaultAsync<double?>(
                "Select Wage from HourlyWages where RateCardLevel = @rateCardLevels",
                new {rateCardLevels = rateCardLevel});
            return result;
        }

        /// <inheritdoc />
        public async Task<bool> UpdateHourlyWage(RateCardLevel rateCardLevel, double wage)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            var result = await connection.QueryAsync<RateCardLevel>(
                "Select RateCardLevel from HourlyWages where RateCardLevel = @rateCardLevels",
                new {rateCardLevels = rateCardLevel});
            if (result.Any())
            {
                await connection.ExecuteAsync(
                    "Update HourlyWages set Wage = @wages where RateCardLevel = @rateCardLevels",
                    new {wages = wage, rateCardLevels = rateCardLevel});
                return true;
            }

            await connection.ExecuteAsync("Insert into HourlyWages values (@rateCardLevels, @wages)",
                new {rateCardLevels = rateCardLevel, wages = wage});
            return false;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteHourlyWage(RateCardLevel rateCardLevel)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            return (await connection.ExecuteAsync("Delete from HourlyWages where RateCardLevel = @rateCardLevels",
                new {rateCardLevels = rateCardLevel})) > 0;
        }
    }
}