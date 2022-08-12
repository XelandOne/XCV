using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Components;
using XCV.Entities;
using XCV.Entities.Enums;
using XCV.Services;

namespace XCV.Data
{
    /// <inheritdoc />
    public class EmployeeService : IEmployeeService
    {
        [Inject] private DatabaseUtils DatabaseUtils { get; set; }
        /// <summary>
        /// Constructor for EmployeeService
        /// </summary>
        /// <param name="databaseUtils"></param>
        public EmployeeService(DatabaseUtils databaseUtils)
        {
            DatabaseUtils = databaseUtils;
        }

        /// <inheritdoc />
        public async Task<Employee?> GetEmployee(Guid employeeId)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            var result = await connection.QueryFirstOrDefaultAsync<Employee>(
                "SELECT Id, Authorizations, Surname, Firstname, Username, EmployedSince, WorkExperience, ScientificAssistant, StudentAssistant, RateCardLevel, ProfilePicture, LastChanged FROM Employee WHERE Id = @id",
                new {id = employeeId});
            Employee returnEmployee = result;
            if (returnEmployee == null) return null;
            //call by reference
            await SetUsedExperience(returnEmployee);
            return returnEmployee;
        }

        /// <inheritdoc />
        public async Task<Employee?> GetEmployee(string username)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            var result = connection.QueryFirstOrDefault<Employee?>(
                "SELECT Id, Authorizations, Surname, Firstname, Username, EmployedSince, WorkExperience, ScientificAssistant, StudentAssistant, RateCardLevel, ProfilePicture, LastChanged FROM Employee WHERE Username = @userName",
                new {userName = username});

            //Employee returnEmployee = result.FirstOrDefault();
            if (result == null) return null;
            //call by reference
            await SetUsedExperience(result);
            return result;
        }

        /// <inheritdoc />
        public async Task<List<Employee>> GetAllEmployees()
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            var employees = await connection.QueryAsync<Employee>(
                "SELECT Id, Authorizations, Surname, Firstname, Username, EmployedSince, WorkExperience, ScientificAssistant, StudentAssistant, RateCardLevel, ProfilePicture, LastChanged FROM Employee");
            if (employees == null) return new List<Employee>();

            foreach (var employee in employees)
            {
                //call by reference
                await SetUsedExperience(employee);
            }

            return employees.ToList();
        }

        /// <inheritdoc />
        public async Task<UsedExperience> GetExperienceForEmployee(Guid employeeId)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);

            //get all exp
            var fields = await connection.QueryAsync<Field>(
                "SELECT f.Id, f.FieldName as name FROM Field f JOIN Employee_Field ef ON ef.Field_Id = f.Id WHERE ef.Employee_Id = @id",
                new {id = employeeId});
            var roles = await connection.QueryAsync<Role>(
                "SELECT r.Id, r.RoleName as name FROM Role r JOIN Employee_Role er ON er.Role_Id = r.Id WHERE er.Employee_Id = @id",
                new {id = employeeId});
            var softSkills = await connection.QueryAsync<SoftSkill>(
                "SELECT f.Id, f.SoftSkillName as name  FROM SoftSkill f JOIN Employee_SoftSkill ef ON ef.SoftSkill_Id = f.Id WHERE ef.Employee_Id = @id",
                new {id = employeeId});
            var hardSkills =
                await connection.QueryAsync<HardSkill, HardSkillLevel, (HardSkill, Entities.Enums.HardSkillLevel)>(
                    "SELECT h.Id, h.HardSkillName as name, h.HardSkillCategory,e.HardSkill_Level FROM HardSkill h JOIN Employee_HardSkill e ON e.HardSkill_Id = h.Id WHERE e.Employee_Id = @id",
                    (harSkill, hardSkillLevel) => (harSkill, hardSkillLevel),
                    new {id = employeeId},
                    splitOn: "HardSkill_Level");
            var languages =
                await connection.QueryAsync<Language, LanguageLevel, (Language, Entities.Enums.LanguageLevel)>(
                    "SELECT h.Id, h.LanguageName as name, e.Language_Level FROM Language h JOIN Employee_Language e ON e.Language_Id = h.Id WHERE e.Employee_Id = @id",
                    (language, languageLevel) => (language, languageLevel),
                    new {id = employeeId},
                    splitOn: "Language_Level");

            //set them
            var usedExp = new UsedExperience();
            usedExp.Fields.AddRange(fields.ToList());
            usedExp.Roles.AddRange(roles.ToList());
            usedExp.SoftSkills.AddRange(softSkills.ToList());
            usedExp.HardSkills.AddRange(hardSkills.ToList());
            usedExp.Languages.AddRange(languages.ToList());
            return usedExp;
        }

        /// <inheritdoc />
        public async Task<(DateTime?, DataBaseResult)> UpdateEmployee(Employee employee)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);

            DateTime? lastChanged;
            
            var result = await connection.QueryAsync<Guid>("Select Id from Employee where Id = @id",
                new {id = employee.Id});
            if (result != null && result.Any())
            {
                //Only here so DumyData can be updated
                if (employee.LastChanged == null)
                {
                    var cache = await connection.QueryAsync<DateTime>("Select LastChanged from Employee where Id = @id",
                        new {id = employee.Id});
                    employee.LastChanged = cache.FirstOrDefault();
                }

                lastChanged =  await connection.QueryFirstOrDefaultAsync<DateTime>(
                "IF (SELECT LastChanged FROM Employee WHERE Id = @id) = @lastChanged " +
                        "BEGIN " +
                            "Update Employee " +
                            "Set Surname = @surname, Firstname = @firstname, EmployedSince = @employedSince, " +
                                "WorkExperience = @workExperience, ScientificAssistant = @scientificAssistant, " +
                                "StudentAssistant = @studentAssistant, RateCardLevel = @rateCardLevel, " +
                                "ProfilePicture = @profilePicture, Authorizations = @authorizations, "+
                                "LastChanged = CURRENT_TIMESTAMP " + 
                            "WHERE Id = @id " +
                            "SELECT LastChanged FROM Employee WHERE Id = @id " +
                        "END",
                new
                {
                    id = employee.Id,
                    surname = employee.SurName,
                    firstname = employee.FirstName,
                    employedSince = employee.EmployedSince.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    workExperience = employee.WorkExperience,
                    scientificAssistant = employee.ScientificAssistant,
                    studentAssistant = employee.StudentAssistant,
                    rateCardLevel = employee.RateCardLevel,
                    profilePicture = employee.ProfilePicture,
                    authorizations = employee.Authorizations,
                    lastChanged = employee.LastChanged
                });

                if (lastChanged == new DateTime()) return (lastChanged, DataBaseResult.Failed);
                await connection.ExecuteAsync("DELETE FROM Employee_Field WHERE Employee_Id = @id",
                    new {id = employee.Id});
                await connection.ExecuteAsync("DELETE FROM Employee_HardSkill WHERE Employee_Id = @id",
                    new {id = employee.Id});
                await connection.ExecuteAsync("DELETE FROM Employee_SoftSkill WHERE Employee_Id = @id",
                    new {id = employee.Id});
                await connection.ExecuteAsync("DELETE FROM Employee_Role WHERE Employee_Id = @id",
                    new {id = employee.Id});
                await connection.ExecuteAsync("DELETE FROM Employee_Project WHERE Employee_Id = @id",
                    new {id = employee.Id});
                await connection.ExecuteAsync("DELETE FROM Employee_Language WHERE Employee_Id = @id",
                    new {id = employee.Id});
                await InsertRelationTables(employee);
                return (lastChanged, DataBaseResult.Updated);
            }

            lastChanged = await connection.QueryFirstOrDefaultAsync<DateTime>(
                "Insert into Employee " +
                "values (@Id, @Surname, @Firstname, @Username, @EmployedSince, " +
                "@WorkExperience, @ScientificAssistant, @StudentAssistant, " +
                " @RateCardLevel, @ProfilePicture, @Authorizations, CURRENT_TIMESTAMP) " +
                "SELECT LastChanged FROM Employee WHERE Id = @id",
                new
                {
                    id = employee.Id,
                    Surname = employee.SurName,
                    Firstname = employee.FirstName,
                    Username = employee.UserName,
                    EmployedSince = employee.EmployedSince.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    WorkExperience = employee.WorkExperience,
                    ScientificAssistant = employee.ScientificAssistant,
                    StudentAssistant = employee.StudentAssistant,
                    RateCardLevel = employee.RateCardLevel,
                    ProfilePicture = employee.ProfilePicture,
                    Authorizations = employee.Authorizations,
                });
            await InsertRelationTables(employee);
            return (lastChanged, DataBaseResult.Inserted);
        }
        

        private async Task InsertRelationTables(Employee employee)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            foreach (var field in employee.Experience.Fields)
            {
                await connection.ExecuteAsync("INSERT INTO Employee_Field VALUES (@employeeId, @fieldId)",
                    new {employeeId = employee.Id, fieldId = field.Id});
            }

            foreach (var role in employee.Experience.Roles)
            {
                await connection.ExecuteAsync("INSERT INTO Employee_Role VALUES (@employeeId, @roleId)",
                    new {employeeId = employee.Id, roleId = role.Id});
            }

            foreach (var softSkill in employee.Experience.SoftSkills)
            {
                await connection.ExecuteAsync("INSERT INTO Employee_SoftSkill VALUES (@employeeId, @softSkillId)",
                    new {employeeId = employee.Id, softSkillId = softSkill.Id});
            }

            foreach (var (language, languageLevel) in employee.Experience.Languages)
            {
                await connection.ExecuteAsync("INSERT INTO Employee_Language VALUES (@employeeId, @langId, @langLevel)",
                    new {employeeId = employee.Id, langId = language.Id, langLevel = languageLevel});
            }

            foreach (var (hardSkill, hardSkillLevel) in employee.Experience.HardSkills)
            {
                await connection.ExecuteAsync(
                    "INSERT INTO Employee_HardSkill VALUES (@employeeId, @hardSkillId, @hardLevel)",
                    new {employeeId = employee.Id, hardSkillId = hardSkill.Id, hardLevel = hardSkillLevel});
            }

            foreach (var projectId in employee.ProjectIds)
            {
                await connection.ExecuteAsync("INSERT INTO Employee_Project VALUES (@employeeId, @projId)",
                    new {employeeId = employee.Id, projId = projectId});
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeleteEmployee(Guid employeeId)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            return (await connection.ExecuteAsync("Delete from Employee where Id = @id",
                new {id = employeeId.ToString()})) > 0;
        }

        /// <inheritdoc />
        public async Task<(string, string)?> GetName(Guid employeeId)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            var result =
                await connection.QueryFirstOrDefaultAsync<(string, string)>(
                    "SELECT Firstname, Surname FROM Employee WHERE Id = @id", new {id = employeeId});

            return (result.Equals(default(ValueTuple<string, string>))) ? null : result;
        }

        /// <inheritdoc />
        public async Task<Dictionary<Guid, (string, string)>> GetAllNames()
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            var dictionary = new Dictionary<Guid, (string, string)>();
            var result =
                await connection.QueryAsync<Guid, string, string, (string, string)>(
                    "SELECT Id, Firstname, Surname FROM Employee",
                    (guid, firstname, lastname) =>
                    {
                        if (!dictionary.TryGetValue(guid, out var tuple))
                        {
                            tuple = (firstname, lastname);
                            dictionary.Add(guid, tuple);
                        }

                        return tuple;
                    }, splitOn: "Id, Firstname, Surname");
            return dictionary;
        }

        /// <inheritdoc />
        public async Task<List<Employee>> GetEmployeesInProject(Guid projectId)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            var result = await connection.QueryAsync<Employee>(
                @"SELECT DISTINCT E.Id, E.Authorizations, E.Surname, E.Firstname, E.Username, E.EmployedSince, E.WorkExperience, E.ScientificAssistant, E.StudentAssistant, E.RateCardLevel, E.ProfilePicture, E.LastChanged
                    FROM Employee E 
                        join Employee_Project EP on E.Id = EP.Employee_Id
                    WHERE EP.Project_Id = @project_Id", new {project_Id = projectId});
            if (result == null || !result.Any()) return new List<Employee>();
            return result.ToList();
        }

        private async Task SetUsedExperience(Employee employee)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            var fields = await connection.QueryAsync<Field>(
                "SELECT f.Id, f.FieldName as name, f.LastChanged FROM Field f JOIN Employee_Field ef ON ef.Field_Id = f.Id WHERE ef.Employee_Id = @id",
                new {id = employee.Id});
            var roles = await connection.QueryAsync<Role>(
                "SELECT r.Id, r.RoleName as name, r.LastChanged FROM Role r JOIN Employee_Role er ON er.Role_Id = r.Id WHERE er.Employee_Id = @id",
                new {id = employee.Id});
            var softSkills = await connection.QueryAsync<SoftSkill>(
                "SELECT f.Id, f.SoftSkillName as name, f.LastChanged  FROM SoftSkill f JOIN Employee_SoftSkill ef ON ef.SoftSkill_Id = f.Id WHERE ef.Employee_Id = @id",
                new {id = employee.Id});
            var hardSkills =
                await connection.QueryAsync<HardSkill, HardSkillLevel, (HardSkill, Entities.Enums.HardSkillLevel)>(
                    "SELECT h.Id, h.HardSkillName as name, h.HardSkillCategory, h.LastChanged, e.HardSkill_Level FROM HardSkill h JOIN Employee_HardSkill e ON e.HardSkill_Id = h.Id WHERE e.Employee_Id = @id",
                    (harSkill, hardSkillLevel) => (harSkill, hardSkillLevel),
                    new {id = employee.Id},
                    splitOn: "HardSkill_Level");
            var languages =
                await connection.QueryAsync<Language, LanguageLevel, (Language, Entities.Enums.LanguageLevel)>(
                    "SELECT h.Id, h.LanguageName as name, h.LastChanged, e.Language_Level FROM Language h JOIN Employee_Language e ON e.Language_Id = h.Id WHERE e.Employee_Id = @id",
                    (language, languageLevel) => (language, languageLevel),
                    new {id = employee.Id},
                    splitOn: "Language_Level");
            var projectIds = await connection.QueryAsync<Guid>(
                "SELECT Project_Id FROM Employee_Project WHERE Employee_Id = @id",
                new {id = employee.Id});
            employee.Experience.Fields.AddRange(fields.ToList());
            employee.Experience.Roles.AddRange(roles.ToList());
            employee.Experience.SoftSkills.AddRange(softSkills.ToList());
            employee.Experience.HardSkills.AddRange(hardSkills.ToList());
            employee.Experience.Languages.AddRange(languages.ToList());
            employee.ProjectIds.AddRange(projectIds.ToList());
        }
    }
}