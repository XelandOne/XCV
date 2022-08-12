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
    public class OfferService : IOfferService
    {
        [Inject] private DatabaseUtils _databaseUtils { get; set; }
        private readonly IShownEmployeePropertiesService _shownEmployeePropertiesService;
        /// <summary>
        /// Creates an Instance of OfferService
        /// </summary>
        /// <param name="databaseUtils"></param>
        /// <param name="shownEmployeePropertiesService"></param>
        public OfferService(DatabaseUtils databaseUtils, IShownEmployeePropertiesService shownEmployeePropertiesService)
        {
            _databaseUtils = databaseUtils;
            _shownEmployeePropertiesService = shownEmployeePropertiesService;
        }

        /// <inheritdoc />
        public async Task<Offer?> GetOffer(Guid offerId)
        {
            using IDbConnection con = new SqlConnection(_databaseUtils.ConnectionString);
            var offer = await con.QueryFirstOrDefaultAsync<Offer>(
                "SELECT Id, Title, StartDate, EndDate, LastChanged FROM Offer WHERE Id = @id",
                new {id = offerId.ToString()});
            if (offer == null) return null;
            await SetUsedExperience(offer);
            //add DocumentConfigs
            var documentsIds = await con.QueryAsync<Guid>(
                "Select Id from DocumentConfigurations where Offer_Id = @offer_Id",
                new {offer_Id = offerId});
            foreach (var id in documentsIds)
            {
                offer.DocumentConfigurations.Add(id);
            }
            //get and add ShortEmployees
            var shortEmployeeIds =
                await con.QueryAsync<Guid>("Select Id from ShownEmployeeProperty where Offer_Id = @offer_Id",
                    new {offer_Id = offerId});
            foreach (var id in shortEmployeeIds)
            {
                var shortEmployee = await _shownEmployeePropertiesService.GetShownEmployeeProperties(id);
                if (shortEmployee != null)
                {
                    offer.ShortEmployees.Add(shortEmployee);
                }
            }

            return offer;
        }

        /// <inheritdoc />
        public async Task<List<Offer>> GetAllOffers()
        {
            using IDbConnection con = new SqlConnection(_databaseUtils.ConnectionString);
            var result = await con.QueryAsync<Offer>(
                "SELECT Id, Title, StartDate, EndDate, LastChanged FROM Offer");
            if (result == null) return new List<Offer>();
            var offerList = result.ToList();
            foreach (Offer offer in offerList)
            {
                await SetUsedExperience(offer);
            }

            foreach (var offer in offerList)
            {
                //add DocumentConfigs
                var documentsIds = await con.QueryAsync<Guid>(
                    "Select Id from DocumentConfigurations where Offer_Id = @offer_Id",
                    new {offer_Id = offer.Id});
                foreach (var id in documentsIds)
                {
                    offer.DocumentConfigurations.Add(id);
                }
                var shortEmployeeIds =
                    await con.QueryAsync<Guid>("Select Id from ShownEmployeeProperty where Offer_Id = @offer_Id",
                        new {offer_Id = offer.Id});
                //get and add ShortEmployees
                foreach (var id in shortEmployeeIds)
                {
                    var shortEmployee = await _shownEmployeePropertiesService.GetShownEmployeeProperties(id);
                    if (shortEmployee != null)
                    {
                        offer.ShortEmployees.Add(shortEmployee);
                    }
                }
            }

            return offerList.ToList();
        }

        /// <inheritdoc />
        public async Task<(DateTime?, DataBaseResult)> UpdateOffer(Offer offer)
        {
            using IDbConnection con = new SqlConnection(_databaseUtils.ConnectionString);

            DateTime? lastChanged;

            var result = await con.QueryAsync<Guid>("SELECT Id FROM Offer WHERE Id = @id",
                new {id = offer.Id});
            if (result != null && result.Any())
            {
                //Only here so DumyData can be updated
                if (offer.LastChanged == null)
                {
                    var cache = await con.QueryAsync<DateTime>("SELECT LastChanged FROM Offer WHERE Id = @id",
                        new {id = offer.Id});
                    offer.LastChanged = cache.FirstOrDefault();
                }

                lastChanged = await con.QueryFirstOrDefaultAsync<DateTime>(
                    "IF (SELECT LastChanged FROM Offer WHERE Id = @id) = @lastChanged " +
                    "BEGIN " +
                    "UPDATE Offer " +
                    "SET Title = @title, StartDate = @startDate, EndDate = @endDate, LastChanged = CURRENT_TIMESTAMP " +
                    "WHERE Id = @id " +
                    "SELECT LastChanged FROM Offer WHERE Id = @id "+
                    "END",
                    new
                    {
                        id = offer.Id, title = offer.Title, startDate = offer.StartDate, endDate = offer.EndDate,
                        lastChanged = offer.LastChanged
                    });

                if (lastChanged == new DateTime()) return (null, DataBaseResult.Failed);
                await con.ExecuteAsync("DELETE FROM Offer_Field WHERE Offer_Id = @id",
                    new {id = offer.Id});
                await con.ExecuteAsync("DELETE FROM Offer_HardSkill WHERE Offer_Id = @id",
                    new {id = offer.Id});
                await con.ExecuteAsync("DELETE FROM Offer_SoftSkill WHERE Offer_Id = @id",
                    new {id = offer.Id});
                await con.ExecuteAsync("DELETE FROM Offer_Role WHERE Offer_Id = @id",
                    new {id = offer.Id});
                await con.ExecuteAsync("DELETE FROM ShownEmployeeProperty WHERE Offer_Id = @id",
                    new {id = offer.Id});
                await con.ExecuteAsync("DELETE FROM Offer_Language WHERE Offer_Id = @id",
                    new {id = offer.Id});
                await InsertRelationTables(offer);
                return (lastChanged, DataBaseResult.Updated);
            }

            lastChanged = await con.QueryFirstOrDefaultAsync<DateTime>(
                "INSERT INTO Offer VALUES (@id, @title, @startDate, @endDate, CURRENT_TIMESTAMP) " +
                "SELECT LastChanged FROM OFFER WHERE Id = @id",
                new {id = offer.Id, title = offer.Title, startDate = offer.StartDate, endDate = offer.EndDate});
            await InsertRelationTables(offer);
            return (lastChanged, DataBaseResult.Inserted);
        }

        private async Task InsertRelationTables(Offer offer)
        {
            using IDbConnection connection = new SqlConnection(_databaseUtils.ConnectionString);
            foreach (var field in offer.Experience.Fields)
            {
                await connection.ExecuteAsync("INSERT INTO Offer_Field VALUES (@offerId, @fieldId)",
                    new {offerId = offer.Id, fieldId = field.Id});
            }

            foreach (var role in offer.Experience.Roles)
            {
                await connection.ExecuteAsync("INSERT INTO Offer_Role VALUES (@offerId, @roleId)",
                    new {offerId = offer.Id, roleId = role.Id});
            }

            foreach (var softSkill in offer.Experience.SoftSkills)
            {
                await connection.ExecuteAsync("INSERT INTO Offer_SoftSkill VALUES (@offerId, @softSkillId)",
                    new {offerId = offer.Id, softSkillId = softSkill.Id});
            }

            foreach (var (language, languageLevel) in offer.Experience.Languages)
            {
                await connection.ExecuteAsync("INSERT INTO Offer_Language VALUES (@offerId, @langId, @langLevel)",
                    new {offerId = offer.Id, langId = language.Id, langLevel = languageLevel});
            }

            foreach (var (hardSkill, hardSkillLevel) in offer.Experience.HardSkills)
            {
                await connection.ExecuteAsync("INSERT INTO Offer_HardSkill VALUES (@offerId, @hardSkillId, @hardLevel)",
                    new {offerId = offer.Id, hardSkillId = hardSkill.Id, hardLevel = hardSkillLevel});
            }

            foreach (var shortEmployee in offer.ShortEmployees)
            {
                var result = await _shownEmployeePropertiesService.UpdateShownEmployeeProperties(shortEmployee);
                shortEmployee.LastChanged = result.Item1;
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeleteOffer(Guid offerId)
        {
            using IDbConnection connection = new SqlConnection(_databaseUtils.ConnectionString);
            return (await connection.ExecuteAsync("Delete from Offer where Id = @id", new {id = offerId.ToString()})) >
                   0;
        }

        private async Task SetUsedExperience(Offer offer)
        {
            using IDbConnection con = new SqlConnection(_databaseUtils.ConnectionString);
            var fields =
                await con.QueryAsync<Field>(
                    "SELECT Id, FieldName as name FROM Field JOIN Offer_Field ON Field_Id = Field.Id WHERE Offer_Id = @id",
                    new {id = offer.Id});
            var softSkills =
                await con.QueryAsync<SoftSkill>(
                    "SELECT Id, SoftSkillName as name FROM SoftSkill JOIN Offer_SoftSkill ON SoftSkill_Id = SoftSkill.Id  WHERE Offer_Id = @id",
                    new {id = offer.Id});
            var roles =
                await con.QueryAsync<Role>(
                    "SELECT Id, RoleName as name FROM Role JOIN Offer_Role ON Role_Id = Role.Id WHERE Offer_Id = @id",
                    new {id = offer.Id});
            var hardSkills =
                await con.QueryAsync<HardSkill, HardSkillLevel, (HardSkill, HardSkillLevel)>(
                    "SELECT h.Id, h.HardSkillName as name, h.HardSkillCategory, e.HardSkill_Level as HardSkillLevel FROM HardSkill h JOIN Offer_HardSkill e ON e.HardSkill_Id = h.Id WHERE e.Offer_Id = @id",
                    (hardSkill, hardSkillLevel) => (hardSkill, hardSkillLevel),
                    new {id = offer.Id}, splitOn: "HardSkillLevel");
            var languages =
                await con.QueryAsync<Language, LanguageLevel, (Language, Entities.Enums.LanguageLevel)>(
                    "SELECT h.Id, h.LanguageName as name, e.Language_Level as LanguageLevel FROM Language h JOIN Offer_Language e ON e.Language_Id = h.Id WHERE e.Offer_Id = @id",
                    (language, languageLevel) => (language, languageLevel),
                    new {id = offer.Id},
                    splitOn: "LanguageLevel");
            var shortIds =
                await con.QueryAsync<Guid>(
                    "SELECT Id FROM ShownEmployeeProperty WHERE Offer_Id = @id",
                    new {id = offer.Id});
            offer.Experience.Fields.AddRange(fields);
            offer.Experience.SoftSkills.AddRange(softSkills);
            offer.Experience.Roles.AddRange(roles);
            offer.Experience.HardSkills.AddRange(hardSkills);
            offer.Experience.Languages.AddRange(languages);
        }
    }
}