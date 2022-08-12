using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XCV.Entities;
using XCV.Entities.Enums;

namespace XCV.Data
{
    /// <summary>
    /// Interface for handling exchange with the database for the Experiences (Language, Role, SoftSkill or HardSkill).
    /// </summary>
    public interface IExperienceService
    {
        /// <summary>
        /// Gets all HardSkills from the database and returns it within a list.
        /// </summary>
        /// <returns>List with HardSkills or an empty list</returns>
        public Task<List<HardSkill>> LoadHardSkills();

        /// <summary>
        /// Gets all SoftSkills from the database and returns it within a list.
        /// </summary>
        /// <returns>List with SoftSkills or an empty list</returns>
        public Task<List<SoftSkill>> LoadSoftSkills();

        /// <summary>
        /// Gets all Roles from the database and returns it within a list.
        /// </summary>
        /// <returns>List with Roles or an empty list</returns>
        public Task<List<Field>> LoadFields();

        /// <summary>
        /// Get all Fields from the database and returns it within a list.
        /// </summary>
        /// <returns>List with Fields or an empty list</returns>
        public Task<List<Role>> LoadRoles();

        
        /// <summary>
        /// Get all Languages from the database and returns it within a list.
        /// </summary>
        /// <returns>List with Language or an empty list</returns>
        public Task<List<Language>> LoadLanguages();
        
        /// <summary>
        /// Gets an experience out of the database with given Id.
        /// Experience can be from DataType HardSkillCategory, Language, Role, SoftSkill or HardSkill.
        /// </summary>
        /// <param name="experienceId">The Id of the experience from a type given above</param>
        /// <returns>Returns an experience from a type given above or null if the experience doesnt exist in the database</returns>
        public Task<Experience?> GetExperience(Guid experienceId);

        /// <summary>
        /// Updates the given Experience or inserts the Experience in the database.
        /// Experience can be from DataType Language, Role, SoftSkill or HardSkill.
        /// </summary>
        /// <param name="experience">The experience from a type given above</param>
        /// <returns>Returns the time when it was updated or inserted. It also returns Failed, Updated or Inserted from the enum DatabaseResult</returns>
        public Task<(DateTime?, DataBaseResult)> UpdateExperience(Experience experience);
        /// <summary>
        /// Deletes the given Experience from the database.
        /// Experience can be from DataType Language, Role, SoftSkill or HardSkill.
        /// Updates all lastChanged columns of tables which are connected to this experience.
        /// </summary>
        /// <param name="experience">The experience from a type given above</param>
        /// <returns>True if the Experience was deleted</returns>
        public Task<bool> DeleteExperience(Experience experience);
    }
}