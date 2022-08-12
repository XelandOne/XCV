using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XCV.Entities;
using XCV.Entities.Enums;

namespace XCV.Data
{
    /// <summary>
    /// Interface for handling exchange with the database for the projects.
    /// </summary>
    public interface IProjectService
    {
        /// <summary>
        /// Get all Projects from the database and returns it within a list.
        /// </summary>
        /// <returns>List of Projects or an empty list</returns>
        public Task<List<Project>> GetAllProjects();

        /// <summary>
        /// Gets the names of all projects and its ids.
        /// </summary>
        /// <returns>Returns a tuple with the guids and names of all projects.</returns>
        public Task<List<(Guid, string)>> GetAllProjectNames();


        /// <summary>
        /// Gets the Project with the given Id from the database.
        /// </summary>
        /// <param name="projectId">The projectId of the project which is loaded from the database</param>
        /// <returns>Object from Type Project or null if it doesnt exist</returns>
        public Task<Project?> GetProject(Guid projectId);


        /// <summary>
        /// Gets the names and the ids of the provided projectIds.
        /// </summary>
        /// <param name="projectIds">All projectIds which names are pulled out of the database</param>
        /// <returns>Returns all projectNames with guids of the provided projectIds in a list with suitable projectId</returns>
        public Task<List<(Guid, string)>> GetProjectNames(List<Guid> projectIds);

        /// <summary>
        /// Updates the given Project or inserts the Project in the database.
        /// </summary>
        /// <param name="project">Project object which will be updated or inserted</param>
        /// <returns>Returns the time when it was updated or inserted. It also returns Failed, Updated or Inserted from the enum DatabaseResult</returns>
        public Task<(DateTime?, DataBaseResult)> UpdateProject(Project project);

        /// <summary>
        /// Deletes the given Project from the database and all attributes from this project and Updates the lastChanged column of ShownEmployeeProperty and Employee which are connected with that project.
        /// </summary>
        /// <param name="projectId">ProjectId of the project which will be deleted</param>
        /// <returns>True if the Project was deleted</returns>
        public Task<bool> DeleteProject(Guid projectId);
    }
}