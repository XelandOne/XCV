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
    public class ExperienceService : IExperienceService
    {
        [Inject] private DatabaseUtils DatabaseUtils { get; set; }
        /// <summary>
        /// Create new Instance of ExperienceService
        /// </summary>
        /// <param name="databaseUtils"></param>
        public ExperienceService(DatabaseUtils databaseUtils)
        {
            DatabaseUtils = databaseUtils;
        }

        /// <inheritdoc />
        public async Task<List<HardSkill>> LoadHardSkills()
        {
            using IDbConnection connection = new SqlConnection(
                DatabaseUtils.ConnectionString);
            var result = await connection.QueryAsync<HardSkill>(
                "SELECT Id, HardSkillName as Name, HardSkillCategory, LastChanged FROM HardSkill");
            return result.AsList();
        }

        /// <inheritdoc />
        public async Task<List<SoftSkill>> LoadSoftSkills()
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            var result =
                await connection.QueryAsync<SoftSkill>("Select Id, SoftSkillName as Name, LastChanged from SoftSkill");
            return result.AsList();
        }

        /// <inheritdoc />
        public async Task<List<Role>> LoadRoles()
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            var result = await connection.QueryAsync<Role>("Select Id, RoleName as Name, LastChanged from Role");
            return result.AsList();
        }

        /// <inheritdoc />
        public async Task<List<Field>> LoadFields()
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            var result = await connection.QueryAsync<Field>("Select Id, FieldName as Name, LastChanged from Field");
            return result.AsList();
        }

        /// <inheritdoc />
        public async Task<List<Language>> LoadLanguages()
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            var result =
                await connection.QueryAsync<Language>("Select Id, LanguageName as Name, LastChanged from Language");
            return result.AsList();
        }

        /// <inheritdoc />
        public async Task<Experience?> GetExperience(Guid experienceId)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            var softSkill =
                await connection.QueryFirstOrDefaultAsync<SoftSkill?>(
                    "Select Id, SoftSkillName as Name, LastChanged from SoftSkill where Id = @id",
                    new {id = experienceId});
            if (softSkill != null)
            {
                return softSkill;
            }

            var hardSkill =
                await connection.QueryFirstOrDefaultAsync<HardSkill?>(
                    "Select Id, HardSkillName as Name, HardSkillCategory, LastChanged from HardSkill where Id = @id",
                    new {id = experienceId});
            if (hardSkill != null)
            {
                return hardSkill;
            }

            var field = await connection.QueryFirstOrDefaultAsync<Field?>(
                "Select Id, FieldName as Name, LastChanged from Field where Id = @id",
                new {id = experienceId});
            if (field != null)
            {
                return field;
            }

            var role = await connection.QueryFirstOrDefaultAsync<Role?>(
                "Select Id, RoleName as Name, LastChanged from Role where Id = @id", new {id = experienceId});
            if (role != null)
            {
                return role;
            }

            var language =
                await connection.QueryFirstOrDefaultAsync<Language?>(
                    "Select Id, LanguageName as Name, LastChanged from Language where Id = @id",
                    new {id = experienceId});
            return language ?? null;
        }

        /// <inheritdoc />
        public async Task<(DateTime?, DataBaseResult)> UpdateExperience(Experience experience)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            DateTime? lastChanged;
            var result = await connection.QueryAsync<Guid>(
                $"Select Id from {experience.GetType().Name} where Id = @experienceId",
                new {experienceId = experience.Id});
            var experienceName =
                await connection.QueryAsync<string>(
                    $"Select {experience.GetType().Name}Name from {experience.GetType().Name} where {experience.GetType().Name}Name = @experienceName",
                    new {experienceName = experience.Name});
            if (experienceName.Any())
            {
                return (null, DataBaseResult.Failed);
            }
            if (experience.GetType() == typeof(HardSkill))
            {
                HardSkill hardSkill = (HardSkill) experience;
                if (result != null && result.Any())
                {
                    //Only here so DumyData can be updated
                    if (hardSkill.LastChanged == null)
                    {
                        var cache = await connection.QueryAsync<DateTime>(
                            $"Select LastChanged from {experience.GetType().Name} where Id = @id",
                            new {id = hardSkill.Id});
                        hardSkill.LastChanged = cache.FirstOrDefault();
                    }

                    lastChanged = await connection.QueryFirstOrDefaultAsync<DateTime>(
                        "IF (Select LastChanged from " + experience.GetType().Name +
                        " where Id = @id) = @lastChanged " +
                        "Begin " +
                        "Update " + experience.GetType().Name +
                        " Set " + experience.GetType().Name +
                        "Name = @experienceName, HardSkillCategory = @hardSkillCategory, LastChanged = CURRENT_TIMESTAMP " +
                        "Where Id = @id " +
                        "Select LastChanged from " + experience.GetType().Name + " where Id = @id END", new
                        {
                            experienceName = experience.Name, id = experience.Id,
                            hardSkillCategory = hardSkill.HardSkillCategory, lastChanged = hardSkill.LastChanged
                        }
                    );
                    if (lastChanged == new DateTime()) return (null, DataBaseResult.Failed);
                    return (lastChanged, DataBaseResult.Updated);
                }

                lastChanged = await connection.QueryFirstOrDefaultAsync<DateTime>(
                    "Insert into " + experience.GetType().Name +
                    " values (@id, @experienceName, @hardSkillCategory, CURRENT_TIMESTAMP) " +
                    "Select LastChanged from " + experience.GetType().Name + " where Id = @id", new
                    {
                        id = experience.Id, experiencename = experience.Name,
                        hardSkillCategory = hardSkill.HardSkillCategory, lastChanged = hardSkill.LastChanged
                    }
                );

                return (lastChanged, DataBaseResult.Inserted);
            }

            if (result != null && result.Any())
            {
                if (experience.LastChanged == null)
                {
                    var cache = await connection.QueryAsync<DateTime>(
                        $"Select LastChanged from {experience.GetType().Name} where Id = @id",
                        new {id = experience.Id});
                    experience.LastChanged = cache.FirstOrDefault();
                }

                lastChanged = await connection.QueryFirstOrDefaultAsync<DateTime>(
                    "IF (Select LastChanged from " + experience.GetType().Name +
                    " where Id = @id) = @lastChanged " +
                    "Begin " +
                    "Update " + experience.GetType().Name +
                    " Set " + experience.GetType().Name +
                    "Name = @experienceName, LastChanged = CURRENT_TIMESTAMP " +
                    "Where Id = @id " +
                    "Select LastChanged from " + experience.GetType().Name + " where Id = @id END", new
                    {
                        experienceName = experience.Name, id = experience.Id,
                        lastChanged = experience.LastChanged
                    }
                );
                return lastChanged == new DateTime()
                    ? (null, DataBaseResult.Failed)
                    : (lastChanged, DataBaseResult.Updated);
            }

            lastChanged = await connection.QueryFirstOrDefaultAsync<DateTime>(
                "Insert into " + experience.GetType().Name +
                " values (@id, @experienceName, CURRENT_TIMESTAMP) " +
                "Select LastChanged from " + experience.GetType().Name + " where Id = @id", new
                {
                    id = experience.Id, experiencename = experience.Name,
                    lastChanged = experience.LastChanged
                }
            );
            return (lastChanged, DataBaseResult.Inserted);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteExperience(Experience experience)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            return (await connection.ExecuteAsync(
                "BEGIN Transaction " +
                "IF exists (Select * from " + experience.GetType().Name +
                " f left outer join ShownEmployeeProperty_" + experience.GetType().Name + " sh on f.Id = sh." +
                experience.GetType().Name + "_Id where f.Id = @id) " +
                "Begin update ShownEmployeeProperty set LastChanged = CURRENT_TIMESTAMP where Id in " +
                "(Select ShownEmployeeProperty_Id from ShownEmployeeProperty_" + experience.GetType().Name + " where " +
                experience.GetType().Name + "_Id = @id)  " +
                "If exists (Select * from ShownEmployeeProperty where Id in " +
                "(Select ShownEmployeeProperty_Id from "+ experience.GetType().Name + " f left outer join ShownEmployeeProperty_"+ experience.GetType().Name + " sh on f.Id = sh." + experience.GetType().Name+ "_Id where f.Id = @id)) " +
                "Begin Update Offer set LastChanged = CURRENT_TIMESTAMP where Id in " +
                "(Select ShownEmployeeProperty_Id from "+ experience.GetType().Name + " f left outer join ShownEmployeeProperty_"+ experience.GetType().Name + " sh on f.Id = sh."+ experience.GetType().Name +"_Id where f.Id = @id) END " +
                "END " + 
                "IF exists (Select * from " + experience.GetType().Name +
                " f left outer join Offer_" + experience.GetType().Name + " o on f.Id = o." +
                experience.GetType().Name + "_Id where f.Id = @id) " +
                "Begin update Offer set LastChanged = CURRENT_TIMESTAMP where Id in " +
                "(Select Offer_Id from Offer_" + experience.GetType().Name + " where " +
                experience.GetType().Name + "_Id = @id) END " +
                "IF exists (Select * from " + experience.GetType().Name +
                " f left outer join Employee_" + experience.GetType().Name + " e on f.Id = e." +
                experience.GetType().Name + "_Id where f.Id = @id) " +
                "Begin update Employee set LastChanged = CURRENT_TIMESTAMP where Id in " +
                "(Select Employee_Id from Employee_" + experience.GetType().Name + " where " +
                experience.GetType().Name + "_Id = @id) END " +
                "Delete from " + experience.GetType().Name + " where Id = @id " +
                "commit"
                , new {id = experience.Id}
            )) > 0;
        }
    }
}