using System.Threading.Tasks;
using XCV.Entities.Enums;

namespace XCV.Data
{
    /// <summary>
    /// Interface for handling exchange with the database for the Wages of the RateCardLevels.
    /// </summary>
    public interface IHourlyWagesService
    {
        /// <summary>
        /// Returns the hourly wage of an employee with the given rate card level.
        /// </summary>
        /// <param name="rateCardLevel">The rate card level of the employee.</param>
        /// <returns>The hourly wage in Euros or null if doesnt exist</returns>
        public Task<double?> GetHourlyWage(RateCardLevel rateCardLevel);
        /// <summary>
        /// Updates (or inserts if there was no entry) the hourly wage of given rate card level
        /// </summary>
        /// <param name="rateCardLevel">The rateCardLevel which will be updated</param>
        /// <param name="wage">The new wage of a rateCardLevel</param>
        /// <returns>true if updated, false if inserted</returns>
        public Task<bool> UpdateHourlyWage(RateCardLevel rateCardLevel, double wage);
        /// <summary>
        /// Deletes hourly wage from database if there was an entry with given rate card level
        /// </summary>
        /// <param name="rateCardLevel">The rateCardLevel which wage will be deleted</param>
        /// <returns>true when an entry was deleted</returns>
        public Task<bool> DeleteHourlyWage(RateCardLevel rateCardLevel);
    }
}