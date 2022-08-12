using System;
using System.Threading.Tasks;
using XCV.Entities;

namespace XCV.Data
{
    /// <summary>
    /// Interface for handling exchange with the database for the documentConfigurations.
    /// </summary>
    public interface IDocumentConfigurationService
    {
        /// <summary>
        /// Gets the DocumentConfiguration with the given documentConfigurationId.
        /// </summary>
        /// <param name="documentConfigurationId">The Id of a documentConfiguration</param>
        /// <returns>Returns an object of DocumentConfiguration or null if it doesnt exist</returns>
        public Task<DocumentConfiguration?> GetDocumentConfiguration(Guid documentConfigurationId);

        /// <summary>
        /// Updates the given DocumentConfiguration or inserts the DocumentConfiguration in the database.
        /// </summary>
        /// <param name="documentConfiguration">An object of documentConfiguration</param>
        /// <returns>True if the DocumentConfiguration was updated or False if the DocumentConfiguration was inserted</returns>
        public Task<bool> UpdateDocumentConfiguration(DocumentConfiguration documentConfiguration);

        /// <summary>
        /// Deletes the given DocumentConfiguration from the database.
        /// </summary>
        /// <param name="documentConfigurationId">The Id of a documentConfiguration</param>
        /// <returns>True if the DocumentConfiguration was deleted</returns>
        public Task<bool> DeleteDocumentConfiguration(Guid documentConfigurationId);
    }
}