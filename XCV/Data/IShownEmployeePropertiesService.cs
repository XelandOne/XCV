using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XCV.Entities;
using XCV.Entities.Enums;

namespace XCV.Data
{
    /// <summary>
    /// Interface for handling exchange with the database for the ShownEmployeePropertyServices.
    /// </summary>
    public interface IShownEmployeePropertiesService
    {
        /// <summary>
        /// Gets the ShownEmployeeProperties with the given Id from the database.
        /// Returns null if ShownEmployeeProperties does not exist.
        /// </summary>
        /// <param name="shownEmployeePropertiesId">The shownEmployeePropertiesId of a shownEmployeeProperty</param>
        /// <returns>Object from Type ShownEmployeeProperties or null if it doesnt exists</returns>
        public Task<ShownEmployeeProperties?> GetShownEmployeeProperties(Guid shownEmployeePropertiesId);
        /// <summary>
        /// Get all ShownEmployeeProperties from the database and returns it within a list.
        /// </summary>
        /// <returns>List of ShownEmployeeProperties or an empty list</returns>
        public Task<List<ShownEmployeeProperties>> GetAllShownEmployeeProperties();
        /// <summary>
        /// Updates the given shownEmployeeProperties or inserts the shownEmployeeProperties in the database,
        /// if the shownEmployeeProperties does not exist. Does check for last changed.
        /// </summary>
        /// <param name="shownEmployeeProperties">An shownEmployeeProperties object</param>
        /// <returns>Tuple where item 1 is the timestamp of the moment the given offer was last changed and
        /// item 2 indicates, whether the Update was successful or not</returns>
        public Task<(DateTime?, DataBaseResult)> UpdateShownEmployeeProperties(ShownEmployeeProperties shownEmployeeProperties);

        /// <summary>
        /// Deletes the given ShownEmployeeProperties from the database and updates all lastChanged column which are connected with ShownEmployeeProperty.
        /// </summary>
        /// <param name="shownEmployeePropertiesId">The shownEmployeePropertiesId of a shownEmployeeProperty</param>
        /// <returns>True if the ShownEmployeeProperties was deleted</returns>
        public Task<DateTime?> DeleteShownEmployeeProperties(Guid shownEmployeePropertiesId);
    }
}