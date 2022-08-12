using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XCV.Entities;
using XCV.Entities.Enums;

namespace XCV.Data
{
    /// <summary>
    /// Interface for handling exchange with the database for the Offers.
    /// </summary>
    public interface IOfferService
    {
        /// <summary>
        /// Gets the Offer with the given Id from the database. Returns null if Offer does not exist.
        /// </summary>
        /// <param name="offerId">The offerId of an offer</param>
        /// <returns>Object from Type Offer or null</returns>
        public Task<Offer?> GetOffer(Guid offerId);
        /// <summary>
        /// Get all Offers from the database and returns it within a list.
        /// </summary>
        /// <returns>List of Offers or empty list if no offer exist</returns>
        public Task<List<Offer>> GetAllOffers();
        /// <summary>
        /// Updates the given Offer or inserts the Offer in the database,
        /// if the Offer does not exist. Does check for last changed.
        /// </summary>
        /// <param name="offer">An offer object</param>
        /// <returns>Tuple where item 1 is the timestamp of the moment the given offer was last changed and
        /// item 2 indicates, whether the Update was successful or not</returns>
        public Task<(DateTime?, DataBaseResult)> UpdateOffer(Offer offer);
        
        
        /// <summary>
        /// Deletes the given Offer from the database.
        /// </summary>
        /// <param name="offerId">The offerId of an offer</param>
        /// <returns>True if the Offer was deleted</returns>
        public Task<bool> DeleteOffer(Guid offerId);
    }
}