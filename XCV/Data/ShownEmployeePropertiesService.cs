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
    public class ShownEmployeePropertiesService : IShownEmployeePropertiesService
    {
        [Inject] private DatabaseUtils DatabaseUtils { get; set; }
        [Inject] private IEmployeeService EmployeeService { get; set; }
        /// <summary>
        /// Create new Instance of ShownEmployeePropertiesService
        /// </summary>
        /// <param name="databaseUtils"></param>
        /// <param name="employeeService"></param>
        public ShownEmployeePropertiesService(DatabaseUtils databaseUtils, IEmployeeService employeeService)
        {
            DatabaseUtils = databaseUtils;
            EmployeeService = employeeService;
        }

        /// <inheritdoc />
        public async Task<ShownEmployeeProperties?> GetShownEmployeeProperties(Guid shownEmployeePropertiesId)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            var shownEmployee =
                await connection.QueryFirstOrDefaultAsync<ShownEmployeeProperties>(
                    "Select Id, Employee_Id as employeeId, RateCardLevel, PlannedWeeklyHours, Offer_Id as OfferId, Discount, LastChanged from ShownEmployeeProperty where Id = @id",
                    new {id = shownEmployeePropertiesId});
            if (shownEmployee == null) return null;
            await SetSelectedExperience(shownEmployee);
            await SetExperience(shownEmployee);
            return shownEmployee;
        }

        /// <inheritdoc />
        public async Task<List<ShownEmployeeProperties>> GetAllShownEmployeeProperties()
        {
            using IDbConnection con = new SqlConnection(DatabaseUtils.ConnectionString);
            var result = await con.QueryAsync<ShownEmployeeProperties>(
                "SELECT Id, Employee_Id as employeeId, RateCardLevel, PlannedWeeklyHours, Offer_Id as OfferId, Discount, LastChanged FROM ShownEmployeeProperty");
            if (result == null) return new List<ShownEmployeeProperties>();
            var shownEmployeePropertiesEnumerable = result.ToList();
            foreach (ShownEmployeeProperties shownEmployeeProperties in shownEmployeePropertiesEnumerable)
            {
                await SetSelectedExperience(shownEmployeeProperties);
                await SetExperience(shownEmployeeProperties);
            }
            return shownEmployeePropertiesEnumerable.ToList();
        }

        /// <inheritdoc />
        public async Task<(DateTime?, DataBaseResult)> UpdateShownEmployeeProperties(ShownEmployeeProperties shownEmployeeProperties)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            
            IEnumerable<DateTime>? lastChanged;
            
            var result = await connection.QueryAsync<Guid>("Select Id from ShownEmployeeProperty where Id = @id",
                new {id = shownEmployeeProperties.Id});
            if (result != null && result.Any())
            {
                //Only here so DumyData can be updated
                if (shownEmployeeProperties.LastChanged == null)
                {
                    var cache = await connection.QueryFirstOrDefaultAsync<DateTime>("SELECT LastChanged FROM ShownEmployeeProperty WHERE Id = @id",
                        new {id = shownEmployeeProperties.Id});
                    shownEmployeeProperties.LastChanged = cache;
                }

                lastChanged = await connection.QueryAsync<DateTime>(
                    "Begin Transaction " +
                    "DECLARE @timestamp DATETIME=CURRENT_TIMESTAMP " +
                    "IF (SELECT LastChanged FROM ShownEmployeeProperty WHERE Id = @id) = @lastChanged "+
                            "BEGIN "+
                                "Update ShownEmployeeProperty  "+
                                "Set Employee_Id = @employeeId, RateCardLevel = @rateCardLevel, "+
                                "PlannedWeeklyHours = @plannedWeeklyHours, Offer_Id = @offerId, Discount = @discount, "+ 
                                "LastChanged = @timestamp "+
                            "where Id = @id "+
                            "Update Offer SET LastChanged = @timestamp "+
                            "WHERE Id = "+
                            "(SELECT Offer_Id FROM ShownEmployeeProperty WHERE Id = @id) "+
                    "SELECT LastChanged FROM ShownEmployeeProperty WHERE Id = @id "+
                    "END " +
                    "COMMIT",
                    new
                    {
                        employeeId = shownEmployeeProperties.EmployeeId,
                        rateCardLevel = shownEmployeeProperties.RateCardLevel,
                        id = shownEmployeeProperties.Id,
                        plannedWeeklyHours = shownEmployeeProperties.PlannedWeeklyHours,
                        offerId = shownEmployeeProperties.OfferId,
                        discount = shownEmployeeProperties.Discount,
                        lastChanged = shownEmployeeProperties.LastChanged
                    });
                
                if (lastChanged.FirstOrDefault() == new DateTime()) return (null, DataBaseResult.Failed);
                await connection.ExecuteAsync(
                    "Delete from ShownEmployeeProperty_HardSkill where ShownEmployeeProperty_Id = @id",
                    new {id = shownEmployeeProperties.Id});
                
                await connection.ExecuteAsync(
                    "Delete from ShownEmployeeProperty_SoftSkill where ShownEmployeeProperty_Id = @id",
                    new {id = shownEmployeeProperties.Id});
                
                await connection.ExecuteAsync(
                    "Delete from ShownEmployeeProperty_Field where ShownEmployeeProperty_Id = @id",
                    new {id = shownEmployeeProperties.Id});
                
                await connection.ExecuteAsync(
                    "Delete from ShownEmployeeProperty_Language where ShownEmployeeProperty_Id = @id",
                    new {id = shownEmployeeProperties.Id});
                
                await connection.ExecuteAsync(
                    "Delete from ShownEmployeeProperty_Role where ShownEmployeeProperty_Id = @id",
                    new {id = shownEmployeeProperties.Id});
                
                await connection.ExecuteAsync(
                    "Delete from ShownEmployeeProperty_Project where ShownEmployeeProperty_Id = @id",
                    new {id = shownEmployeeProperties.Id});

                await connection.ExecuteAsync(
                    "Delete from ShownEmployeeProperty_ProjectActivity where ShownEmployeeProperty_Id = @id",
                    new {id = shownEmployeeProperties.Id});
                
                await InsertUsedExperience(shownEmployeeProperties);

                return (lastChanged.FirstOrDefault(), DataBaseResult.Updated);
            }
            lastChanged = await connection.QueryAsync<DateTime>("Insert into ShownEmployeeProperty VALUES (@id, @employee_Id, @rateCardLevel, @plannedWeeklyHours, @offerId, @discount, CURRENT_TIMESTAMP )"+
                                          "SELECT LastChanged FROM ShownEmployeeProperty WHERE Id = @id ",
                new
                {
                    id = shownEmployeeProperties.Id,
                    employee_Id = shownEmployeeProperties.EmployeeId,
                    rateCardLevel = shownEmployeeProperties.RateCardLevel,
                    plannedWeeklyHours = shownEmployeeProperties.PlannedWeeklyHours,
                    offerId = shownEmployeeProperties.OfferId,
                    discount = shownEmployeeProperties.Discount
                });
               
            await InsertUsedExperience(shownEmployeeProperties);

            return (lastChanged.FirstOrDefault(), DataBaseResult.Inserted);
        }


        /// <inheritdoc />
        public async Task<DateTime?> DeleteShownEmployeeProperties(Guid shownEmployeePropertiesId)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            return await connection.QueryFirstOrDefaultAsync<DateTime>(
                "Begin Transaction " +
                // "If exists (Select * from ShownEmployeeProperty sh where sh.Id = @id) " +
                "Begin Update Offer set LastChanged = CURRENT_TIMESTAMP where Id in " +
                "(Select Offer_Id from ShownEmployeeProperty where Id = @id) End " +
                "SELECT o.LastChanged FROM Offer o INNER JOIN ShownEmployeeProperty sh ON  o.Id = sh.Offer_Id WHERE sh.Id = @id " +
                "Delete from ShownEmployeeProperty where Id = @id "+
                "commit "
                , new { id = shownEmployeePropertiesId}
            );
        }
        
        private async Task InsertUsedExperience(ShownEmployeeProperties shownEmployeeProperties)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            foreach (var hardSkill in shownEmployeeProperties.SelectedExperience.HardSkills)
            {
                await connection.ExecuteAsync(
                    "Insert into ShownEmployeeProperty_HardSkill values (@shownEmployeeProperty_Id, @hardSkill_Id, @hardSkill_Level)",
                    new
                    {
                        shownEmployeeProperty_Id = shownEmployeeProperties.Id, hardSkill_Id = hardSkill.Item1.Id,
                        hardSkill_Level = hardSkill.Item2
                    });
            }
            foreach (var softSkill in shownEmployeeProperties.SelectedExperience.SoftSkills)
            {
                await connection.ExecuteAsync(
                    "Insert into ShownEmployeeProperty_SoftSkill values (@shownEmployeeProperty_Id, @softSkill_Id)",
                    new
                    {
                        shownEmployeeProperty_Id = shownEmployeeProperties.Id, softSkill_Id = softSkill.Id
                    });
            }
            foreach (var field in shownEmployeeProperties.SelectedExperience.Fields)
            {
                await connection.ExecuteAsync(
                    "Insert into ShownEmployeeProperty_Field values (@shownEmployeeProperty_Id, @field_Id)",
                    new
                    {
                        shownEmployeeProperty_Id = shownEmployeeProperties.Id, field_Id = field.Id
                    });
            }
            foreach (var language in shownEmployeeProperties.SelectedExperience.Languages)
            {
                await connection.ExecuteAsync(
                    "Insert into ShownEmployeeProperty_Language values (@shownEmployeeProperty_Id, @language_Id, @language_Level)",
                    new
                    {
                        shownEmployeeProperty_Id = shownEmployeeProperties.Id, language_Id = language.Item1.Id, language_Level = language.Item2
                    });
            }
            foreach (var role in shownEmployeeProperties.SelectedExperience.Roles)
            {
                await connection.ExecuteAsync(
                    "Insert into ShownEmployeeProperty_Role values (@shownEmployeeProperty_Id, @role_Id)",
                    new
                    {
                        shownEmployeeProperty_Id = shownEmployeeProperties.Id, role_Id = role.Id,
                    });
            }
            foreach (var project in shownEmployeeProperties.ProjectIds)
            {
                await connection.ExecuteAsync(
                    "Insert into ShownEmployeeProperty_Project values (@shownEmployeeProperty_Id, @project_Id)",
                    new
                    {
                        shownEmployeeProperty_Id = shownEmployeeProperties.Id, project_Id = project,
                    });
            }

            foreach (var projectActivity in shownEmployeeProperties.ProjectActivityIds)
            {
                await connection.ExecuteAsync(
                    "Insert into ShownEmployeeProperty_ProjectActivity values (@shownEmployeeProperty_Id, @projectActivity_Id)",
                    new {shownEmployeeProperty_Id = shownEmployeeProperties.Id, projectActivity_Id = projectActivity});
            }
        }

        
        private async Task SetSelectedExperience(ShownEmployeeProperties shownEmployeeProperties)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            var fields = await connection.QueryAsync<Field>(
                "SELECT Id, FieldName as name FROM Field JOIN ShownEmployeeProperty_Field ON Field_Id = id WHERE ShownEmployeeProperty_Id = @id",
                new {id = shownEmployeeProperties.Id});
            var softSkills = await connection.QueryAsync<SoftSkill>(
                "SELECT Id, SoftSkillName as name FROM SoftSkill JOIN ShownEmployeeProperty_SoftSkill ON SoftSkill_Id = id WHERE ShownEmployeeProperty_Id = @id",
                new {id = shownEmployeeProperties.Id});
            var roles = await connection.QueryAsync<Role>(
                "SELECT Id, RoleName as name FROM Role JOIN ShownEmployeeProperty_Role ON Role_Id = id WHERE ShownEmployeeProperty_Id = @id",
                new {id = shownEmployeeProperties.Id});
            var projectIds = await connection.QueryAsync<Guid>(
                "SELECT Project_Id FROM ShownEmployeeProperty_Project WHERE ShownEmployeeProperty_Id = @id",
                new {id = shownEmployeeProperties.Id});
            var projectActivityIds = await connection.QueryAsync<Guid>(
                "Select ProjectActivity_Id from ShownEmployeeProperty_ProjectActivity where ShownEmployeeProperty_Id = @id",
                new {id = shownEmployeeProperties.Id});
            var hardSkills =
                await connection.QueryAsync<HardSkill, HardSkillLevel, (HardSkill, HardSkillLevel)>(
                    "SELECT h.Id, h.HardSkillName as name, h.HardSkillCategory ,e.HardSkill_Level as HardSkillLevel FROM HardSkill h JOIN ShownEmployeeProperty_HardSkill e ON e.HardSkill_Id = h.Id WHERE e.ShownEmployeeProperty_Id = @id",
                    (hardSkill, hardSkillLevel) => (hardSkill, hardSkillLevel),
                    new {id = shownEmployeeProperties.Id}, splitOn:"HardSkillLevel");
            var languages = 
                await connection.QueryAsync<Language, LanguageLevel, (Language, LanguageLevel)>(
                    "SELECT h.Id, h.LanguageName as name, e.Language_Level AS LanguageLevel FROM Language h JOIN ShownEmployeeProperty_Language e ON e.Language_Id = h.Id WHERE e.ShownEmployeeProperty_Id = @id",
                    (language, languageLevel) => (language, languageLevel),
                    new {id = shownEmployeeProperties.Id}, splitOn: "LanguageLevel");
            shownEmployeeProperties.SelectedExperience.Fields.AddRange(fields);
            shownEmployeeProperties.SelectedExperience.SoftSkills.AddRange(softSkills);
            shownEmployeeProperties.SelectedExperience.Roles.AddRange(roles);
            shownEmployeeProperties.SelectedExperience.HardSkills.AddRange(hardSkills);
            shownEmployeeProperties.SelectedExperience.Languages.AddRange(languages);
            shownEmployeeProperties.ProjectIds.AddRange(projectIds);
            shownEmployeeProperties.ProjectActivityIds.AddRange(projectActivityIds);
        }

        private async Task SetExperience(ShownEmployeeProperties shownEmployeeProperties)
        {
            shownEmployeeProperties.Experience =
                await EmployeeService.GetExperienceForEmployee(shownEmployeeProperties.EmployeeId);
        }
    }
}