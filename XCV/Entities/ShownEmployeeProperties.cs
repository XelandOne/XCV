using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Components;
using XCV.Data;
using XCV.Entities.Enums;
using XCV.Services;

namespace XCV.Entities
{
    /// <summary>
    /// ShownEmployeeProperties Entity to locally store, an update an ShownEmployeeProperties
    /// </summary>
    public class ShownEmployeeProperties : IEquatable<ShownEmployeeProperties>
    {
        public Guid Id { get; } = Guid.NewGuid();
        public Guid EmployeeId { get; }
        public RateCardLevel RateCardLevel { get; set; }
        /// <summary>
        /// The experiences of the employee.
        /// </summary>
        public UsedExperience Experience { get; set; } = new();
        /// <summary>
        /// The shown experiences for this configuration.
        /// </summary>
        public UsedExperience SelectedExperience { get; set; } = new();
        /// <summary>
        /// The shown projects for tis configuration.
        /// </summary>
        public List<Guid> ProjectIds { get; } = new();
        /// <summary>
        /// The shown project activities for this configuration.
        /// </summary>
        public List<Guid> ProjectActivityIds { get; } = new();
        /// <summary>
        /// The planned number of hours per week the employee will work for this project.
        /// </summary>
        public int? PlannedWeeklyHours { get; set; }
        
        public Guid OfferId { get; }
        /// <summary>
        /// Stores the discount of the original price for the employee in this offer.
        /// </summary>
        public double Discount { get; set; } = 0.0;
        public DateTime? LastChanged { get; set; }

        /// <summary>
        /// constructor copy an ShownEmployeeProperties
        /// </summary>
        /// <param name="shownEmployeeProperties"></param>
        /// <param name="offerId"></param>
        public ShownEmployeeProperties(ShownEmployeeProperties shownEmployeeProperties, Guid offerId)
        {
            EmployeeId = shownEmployeeProperties.EmployeeId;
            RateCardLevel = shownEmployeeProperties.RateCardLevel;
            Experience = shownEmployeeProperties.Experience;
            SelectedExperience = shownEmployeeProperties.SelectedExperience;
            shownEmployeeProperties.ProjectIds.ForEach(x => ProjectIds.Add(x));
            shownEmployeeProperties.ProjectActivityIds.ForEach(x => ProjectActivityIds.Add(x));
            PlannedWeeklyHours = shownEmployeeProperties.PlannedWeeklyHours;
            Discount = shownEmployeeProperties.Discount;
            OfferId = offerId;
        }
        
        //for dapper
        public ShownEmployeeProperties(Guid id, Guid employeeId, RateCardLevel rateCardLevel, Guid offerId)
        {
            Id = id;
            EmployeeId = employeeId;
            RateCardLevel = rateCardLevel;
            OfferId = offerId;
        }

        public ShownEmployeeProperties(Guid id, Guid employeeId, RateCardLevel rateCardLevel, int? plannedWeeklyHours, Guid offerId,
            double discount, DateTime lastChanged) :
            this(id, employeeId, rateCardLevel, offerId)
        {
            PlannedWeeklyHours = plannedWeeklyHours;
            Discount = discount;
            LastChanged = lastChanged;
        }

        public ShownEmployeeProperties(Employee employee, Guid offerId)
        {
            EmployeeId = employee.Id;
            RateCardLevel = employee.RateCardLevel;
            Experience = employee.Experience;
            SelectedExperience = new UsedExperience(employee.Experience);
            ProjectIds = employee.ProjectIds;
            OfferId = offerId;
        }

        public bool Equals(ShownEmployeeProperties? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id) && EmployeeId.Equals(other.EmployeeId) && RateCardLevel == other.RateCardLevel &&
                   Experience.Equals(other.Experience) && SelectedExperience.Equals(other.SelectedExperience) &&
                   ProjectIds.All(other.ProjectIds.Contains) && PlannedWeeklyHours.Equals(other.PlannedWeeklyHours) &&
                   Discount.Equals(other.Discount) && ProjectActivityIds.All(other.ProjectActivityIds.Contains);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ShownEmployeeProperties) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, EmployeeId, (int) RateCardLevel, Experience, SelectedExperience, ProjectIds);
        }
    }
}