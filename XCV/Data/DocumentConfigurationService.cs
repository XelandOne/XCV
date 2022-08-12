using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Components;
using XCV.Entities;
using XCV.Services;

namespace XCV.Data
{
    /// <inheritdoc />
    public class DocumentConfigurationService : IDocumentConfigurationService
    {
        [Inject] private DatabaseUtils DatabaseUtils { get; set; }
        /// <summary>
        /// Create new Instance of DocumentConfigurationService
        /// </summary>
        /// <param name="databaseUtils"></param>
        public DocumentConfigurationService(DatabaseUtils databaseUtils)
        {
            DatabaseUtils = databaseUtils;
        }

        /// <inheritdoc />
        public async Task<DocumentConfiguration?> GetDocumentConfiguration(Guid documentConfigurationId)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            DocumentConfiguration? documentConfiguration = null;
            var result = await connection.QueryAsync<DocumentConfiguration, Guid?, DocumentConfiguration>(
                "Select d.Id, d.Title, d.CreationTime, d.ShowCoverSheet, d.ShowRequiredExperience, d.IncludePriceCalculation, d.Offer_Id as OfferId, ds.ShownEmployeeProperty_Id from DocumentConfigurations d Left outer join DocumentConfigurations_ShownEmployeeProperties ds on d.Id = ds.Documents_Id where d.Id = @id",
                (documentConfig, shownEmployeeId) =>
                {
                    documentConfiguration ??= documentConfig;

                    if (shownEmployeeId.HasValue)
                    {
                        documentConfiguration.ShownEmployeePropertyIds.Add(shownEmployeeId.Value);
                    }


                    return documentConfiguration;
                }, new {id = documentConfigurationId},
                splitOn: "ShownEmployeeProperty_Id");

            return result.FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<bool> UpdateDocumentConfiguration(DocumentConfiguration documentConfiguration)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            var result = await connection.QueryAsync<Guid>("Select Id from DocumentConfigurations where Id = @id",
                new {id = documentConfiguration.Id});
            if (result != null && result.Any())
            {
                await connection.ExecuteAsync(
                    "Update DocumentConfigurations set Title = @title, CreationTime = @creationTime, ShowCoverSheet = @showCoverSheet, ShowRequiredExperience = @showRequiredExperience, IncludePriceCalculation = @includePriceCalculation where Id = @id",
                    new
                    {
                        title = documentConfiguration.Title, creationTime = documentConfiguration.CreationTime,
                        showCoverSheet = documentConfiguration.ShowCoverSheet,
                        showRequiredExperience = documentConfiguration.ShowRequiredExperience,
                        includePriceCalculation = documentConfiguration.IncludePriceCalculation,
                        id = documentConfiguration.Id
                    });

                var documentShownEmployeeIds = await connection.QueryAsync<Guid>(
                    "Select ShownEmployeeProperty_Id from DocumentConfigurations_ShownEmployeeProperties where Documents_Id = @id",
                    new {id = documentConfiguration.Id});
                var shownEmployeeIds = documentShownEmployeeIds as Guid[] ?? documentShownEmployeeIds.ToArray();
                foreach (var ids in shownEmployeeIds)
                {
                    if (!documentConfiguration.ShownEmployeePropertyIds.Contains(ids))
                    {
                        await connection.ExecuteAsync(
                            "Delete from DocumentConfigurations_ShownEmployeeProperties where ShownEmployeeProperty_Id = @id",
                            new {id = ids});
                    }
                }

                foreach (var ids in documentConfiguration.ShownEmployeePropertyIds.Where(ids =>
                    !shownEmployeeIds.Contains(ids)))
                {
                    await connection.ExecuteAsync(
                        "Insert into DocumentConfigurations_ShownEmployeeProperties values (@documents_Id, @shownEmployeeProperty_Id)",
                        new {documents_Id = documentConfiguration.Id, shownEmployeeProperty_Id = ids});
                }

                return true;
            }

            await connection.ExecuteAsync(
                "Insert into DocumentConfigurations values (@id, @title, @creationTime, @showCoverSheet, @showRequiredExperience, @includePriceCalculation, @offer_Id)",
                new
                {
                    id = documentConfiguration.Id, title = documentConfiguration.Title,
                    creationTime = documentConfiguration.CreationTime,
                    showCoverSheet = documentConfiguration.ShowCoverSheet,
                    showRequiredExperience = documentConfiguration.ShowRequiredExperience,
                    includePriceCalculation = documentConfiguration.IncludePriceCalculation,
                    offer_Id = documentConfiguration.OfferId
                });

            foreach (var ids in documentConfiguration.ShownEmployeePropertyIds)
            {
                await connection.ExecuteAsync(
                    "Insert into DocumentConfigurations_ShownEmployeeProperties values (@documents_Id, @shownEmployeeProperty_Id)",
                    new {documents_Id = documentConfiguration.Id, shownEmployeeProperty_Id = ids});
            }

            return false;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteDocumentConfiguration(Guid documentConfigurationId)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            //first delete all entries in DocumentConfigurations_ShownEmployeeProperties
            await connection.ExecuteAsync(
                "DELETE FROM DocumentConfigurations_ShownEmployeeProperties WHERE Documents_Id = @id",
                new {id = documentConfigurationId});
            return (await connection.ExecuteAsync("Delete from DocumentConfigurations where Id = @id",
                new {id = documentConfigurationId})) > 0;
        }
    }
}