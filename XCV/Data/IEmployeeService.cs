using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XCV.Entities;
using XCV.Entities.Enums;

namespace XCV.Data
{
    /// <summary>
    /// Interface for handling exchange with the database for the employees.
    /// </summary>
    public interface IEmployeeService
    {
        /// <summary>
        /// Gets the Employee with the given Id from the database. Returns null if employee does not exist.
        /// </summary>
        /// <param name="employeeId">The employeeId of an employee</param>
        /// <returns>Object from Type employee or null if it doesnt exist</returns>
        public Task<Employee?> GetEmployee(Guid employeeId);
        /// <summary>
        /// Gets the Employee with the given Username from the database. Returns null if employee does not exist.
        /// </summary>
        /// <param name="username">The username of an employee</param>
        /// <returns>Object from Type employee or null if it doesnt exist</returns>
        public Task<Employee?> GetEmployee(string username);
        /// <summary>
        /// Get all Employees from the database and returns it within a list. 
        /// </summary>
        /// <returns>List of employees (empty if none were found)</returns>
        public Task<List<Employee>> GetAllEmployees();
        /// <summary>
        /// Returns an instance of UsedExperience with all experiences associated with given ID from the Database.
        /// If no experiences were found the lists will be empty. Can be called even though there is no Employee with this ID in the Database.
        /// </summary>
        /// <param name="employeeId">The employeeId of an employee</param>
        /// <returns>UsedExperience instance</returns>
        public Task<UsedExperience> GetExperienceForEmployee(Guid employeeId);

        
        /// <summary>
        /// Updates the given Employee or inserts the Employee in the database,
        /// if the Employee does not exist. 
        /// </summary>
        /// <param name="employee">An employee object</param>
        /// <returns>Returns the time when it was updated or inserted. It also returns Failed, Updated or Inserted from the enum DatabaseResult</returns>
        public Task<(DateTime?, DataBaseResult)> UpdateEmployee(Employee employee);
        /// <summary>
        /// Deletes the given Employee from the database.
        /// </summary>
        /// <param name="employeeId">The employeeId of an employee</param>
        /// <returns>True if the employee was deleted</returns>
        public Task<bool> DeleteEmployee(Guid employeeId);
        /// <summary>
        /// Returns the first- and lastname of the employee with given Id, if there is an entry. Otherwise returns null. 
        /// </summary>
        /// <param name="employeeId">The employeeId of an employee</param>
        /// <returns>(Firstname, Surname) or null</returns>
        public Task<(string, string)?> GetName(Guid employeeId);
        /// <summary>
        /// Returns a list of all firstnames and surnames in the Database in an arbitrary order.
        /// </summary>
        /// <returns>List of Tuple (Firstname, Surname)</returns>
        public Task<Dictionary<Guid, (string, string)>> GetAllNames();
        /// <summary>
        /// Returns all employee involved in project given by projectId. List will be empty when there is no project with
        /// given ID or there are no employees in relation to the project
        /// </summary>
        /// <param name="projectId">The projectId of a project</param>
        /// <returns>List of Employees or an empty list if no employees are existing in the project</returns>
        public Task<List<Employee>> GetEmployeesInProject(Guid projectId);
    }
}