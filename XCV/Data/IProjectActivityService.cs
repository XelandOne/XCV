using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XCV.Entities;
using XCV.Entities.Enums;

namespace XCV.Data
{
    /// <summary>
    /// Interface for handling exchange with the database for the ProjectActivities.
    /// </summary>
    public interface IProjectActivityService
    {
        /// <summary>
        /// Get all ProjectActivities from one project from the database and return it within a list.
        /// </summary>
        /// <param name="projectId">The projectId of the project which projectActivities will be loaded</param>
        /// <returns>List of ProjectActivities or an empty list</returns>
        public Task<List<ProjectActivity>> GetAllProjectActivities(Guid projectId);

        /// <summary>
        /// Gets the ProjectActivity with the given Id from the database.
        /// </summary>
        /// <param name="projectActivityId">The projectActivityId from the projectActivity</param>
        /// <returns>Object from Type ProjectActivity or null if it doesnt exist</returns>
        public Task<ProjectActivity?> GetProjectActivity(Guid projectActivityId);

        /// <summary>
        /// Updates the given ProjectActivity or inserts the ProjectActivity in the database.
        /// </summary>
        /// <param name="projectActivity">The projectActivity which will be updated</param>
        /// <param name="projectId">The projectId of the project which contains this projectActivity</param>
        /// <returns>True if the ProjectActivity was updated or false if the projectActivity was inserted</returns>
        public Task<bool> UpdateProjectActivity(ProjectActivity projectActivity, Guid projectId);

        /// <summary>
        /// Deletes the given ProjectActivity from the database.
        /// </summary>
        /// <param name="projectActivityId">The projectActivityId of the projectActivity</param>
        /// <returns>True if the ProjectActivity was deleted</returns>
        public Task<bool> DeleteProjectActivity(Guid projectActivityId);
    }
}