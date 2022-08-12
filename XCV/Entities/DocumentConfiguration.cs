using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using XCV.Data;
using XCV.Services;

namespace XCV.Entities
{
    /// <summary>
    /// A document configuration represents everything needed to export a document.
    /// It contains a reference to an offer with shown employee properties and other configurations for what to display in the document.
    /// </summary>
    public class DocumentConfiguration : IEquatable<DocumentConfiguration>
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Title { get; set; }
        public DateTimeOffset CreationTime { get; }
        /// <summary>
        /// Sets if a cover sheet should be shown.
        /// </summary>
        public bool ShowCoverSheet { get; set; }
        /// <summary>
        /// Sets if a page with all required experiences of the offer should be shown.
        /// </summary>
        public bool ShowRequiredExperience { get; set; }
        /// <summary>
        /// Sets if a price calculation should be shown in the word sheet.
        /// </summary>
        public bool IncludePriceCalculation { get; set; }

        public Guid OfferId { get; }
        /// <summary>
        /// A list with all shown employee property ids which should be shown in the document
        /// </summary>
        public List<Guid> ShownEmployeePropertyIds { get; } = new();


        public DocumentConfiguration(string title, Offer offer)
        {
            CreationTime = DateTime.Now;
            Title = title;
            OfferId = offer.Id;
            offer.DocumentConfigurations.Add(Id);
        }

        public DocumentConfiguration(string title, bool showCoverSheet, bool showRequiredExperience,
            bool includePriceCalculation, Offer offer) : this(title, offer)
        {
            ShowCoverSheet = showCoverSheet;
            ShowRequiredExperience = showRequiredExperience;
            IncludePriceCalculation = includePriceCalculation;
        }

        public DocumentConfiguration(string title, bool showCoverSheet, bool showRequiredExperience,
            bool includePriceCalculation, Offer offer,
            List<Guid> shownEmployeePropertyIds) : this(title, showCoverSheet, showRequiredExperience,
            includePriceCalculation, offer)
        {
            ShownEmployeePropertyIds = shownEmployeePropertyIds;
        }

        //For Dapper
        public DocumentConfiguration(Guid id, string title, DateTimeOffset creationTime, bool showCoverSheet,
            bool showRequiredExperience, bool includePriceCalculation, Guid offerId)
        {
            Id = id;
            Title = title;
            CreationTime = creationTime;
            ShowCoverSheet = showCoverSheet;
            ShowRequiredExperience = showRequiredExperience;
            IncludePriceCalculation = includePriceCalculation;
            OfferId = offerId;
        }


        public bool Equals(DocumentConfiguration? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id) && Title == other.Title && CreationTime.Equals(other.CreationTime) &&
                   ShowCoverSheet == other.ShowCoverSheet && ShowRequiredExperience == other.ShowRequiredExperience &&
                   IncludePriceCalculation == other.IncludePriceCalculation &&
                   OfferId.Equals(other.OfferId) &&
                   ShownEmployeePropertyIds.All(other.ShownEmployeePropertyIds.Contains);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DocumentConfiguration) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Title, CreationTime, ShowCoverSheet, ShowRequiredExperience, OfferId,
                ShownEmployeePropertyIds);
        }
    }
}