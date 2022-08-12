using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using XCV.Data;
using XCV.Entities;

namespace XCV.Services
{
    /// <summary>
    /// Manager for handling document configurations.
    /// </summary>
    public class DocumentConfigurationManager
    {
        [Inject] private IDocumentConfigurationService DocumentConfigurationService { get; set; }
        [Inject] private IOfferService OfferService { get; set; }

        public DocumentConfigurationManager(IDocumentConfigurationService documentConfigurationService,
            IOfferService offerService)
        {
            DocumentConfigurationService = documentConfigurationService;
            OfferService = offerService;
        }

        /// <summary>
        /// Get all document configurations of an offer.
        /// </summary>
        /// <param name="offerId">The guid of the offer.</param>
        /// <returns>All document configurations which belong to the offer.</returns>
        public async Task<IEnumerable<DocumentConfiguration>?> GetDocumentConfigurations(Guid offerId)
        {
            var offer = await OfferService.GetOffer(offerId);
            if (offer == null) return null;
            List<DocumentConfiguration> documentConfigurations = new();

            offer.DocumentConfigurations.ForEach(dc =>
            {
                var documentConfiguration = DocumentConfigurationService.GetDocumentConfiguration(dc).Result;
                if (documentConfiguration != null)
                {
                    documentConfigurations.Add(documentConfiguration);
                }
            });
            return documentConfigurations;
        }

        /// <summary>
        /// Get a single document configuration by its guid.
        /// </summary>
        /// <param name="documentConfigurationId"></param>
        /// <returns></returns>
        public async Task<DocumentConfiguration?> GetDocumentConfiguration(Guid documentConfigurationId)
        {
            return await DocumentConfigurationService.GetDocumentConfiguration(documentConfigurationId);
        }

        /// <summary>
        /// Updates a document configuration.
        /// </summary>
        /// <param name="documentConfiguration"></param>
        /// <returns></returns>
        public async Task<bool> UpdateDocumentConfiguration(DocumentConfiguration documentConfiguration)
        {
            return await DocumentConfigurationService.UpdateDocumentConfiguration(documentConfiguration);
        }

        /// <summary>
        /// Deletes a document configuration.
        /// </summary>
        /// <param name="documentConfigurationId">The guid of the document configuration.</param>
        /// <returns></returns>
        public async Task<bool> DeleteDocumentConfiguration(Guid documentConfigurationId)
        {
            return await DocumentConfigurationService.DeleteDocumentConfiguration(documentConfigurationId);
        }
    }
}