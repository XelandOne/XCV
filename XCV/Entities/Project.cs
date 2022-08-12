using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using XCV.ValidationAttributes;

namespace XCV.Entities
{
    /// <summary>
    /// A project has a title and contains multiple project activities. It has a start date and an optional end date.
    /// </summary>
    public class Project : IEquatable<Project>, IComparable<Project>
    {
        private const string InvalidTextLengthErrorMessage = "Der Text darf nicht länger als {1} Zeichen sein.";
        private const string InvalidEndDateErrorMessage = "Das Enddatum kann nicht vor dem Startdatum liegen.";
        public Guid Id { get; } = Guid.NewGuid();

        public List<ProjectActivity> ProjectActivities { get; } = new List<ProjectActivity>();
        
        [Required(ErrorMessage = "Titel wird benötigt.")]
        [StringLength(100, ErrorMessage = InvalidTextLengthErrorMessage)]
        public string Title { get; set; }
        
        // [Required(ErrorMessage = "Bitte wähle eine Branche aus.")]
        public Field? Field { get; set; }
        
        [Required(ErrorMessage = "Bitte gebe ein Startdatum an.")]
        [DateTimeRange]
        [DateSmallerThan("EndDate", ErrorMessage = InvalidEndDateErrorMessage)]
        public DateTime StartDate { get; set; } = DateTime.Now;
        [DateGreaterThan("StartDate", ErrorMessage = InvalidEndDateErrorMessage)]
        public DateTime? EndDate { get; set; }
        
        [StringLength(1000, ErrorMessage = InvalidTextLengthErrorMessage)]
        public string ProjectDescription { get; set; }
        
        public DateTime? LastChanged { get; set; }

        public List<string> ProjectPurposes { get; } = new List<string>();

        public Project () {}
        
        public Project(string title, DateTime startDate, DateTime? endDate,
            string projectDescription)
        {
            Title = title;
            StartDate = startDate.Date;
            if (endDate.HasValue)
            {
                EndDate = endDate.Value.Date;
            }

            ProjectDescription = projectDescription;
        }

        public Project(string title, Field field, DateTime startDate, DateTime? endDate,
            string projectDescription) : this(title, startDate, endDate, projectDescription)
        {
            Field = field;
        }


        public Project(Guid id, string title, DateTime startDate, DateTime? endDate, string projectDescription) : this(
            title, startDate, endDate, projectDescription)
        {
            Id = id;
        }
        //For dapper
        public Project(Guid id, string title,  DateTime startDate, DateTime? endDate,
            string projectDescription, DateTime lastChanged) : this(id, title,  startDate, endDate, projectDescription)
        {
            LastChanged = lastChanged;
        }

        public bool Equals(Project? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id) && ProjectActivities.All(other.ProjectActivities.Contains) &&
                   Title == other.Title && Equals(Field, other.Field) && StartDate.Equals(other.StartDate) &&
                   Nullable.Equals(EndDate, other.EndDate) && ProjectDescription == other.ProjectDescription &&
                   ProjectPurposes.All(other.ProjectPurposes.Contains);
        }

        public int CompareTo(Project? other)
        {
            return other == null ? 1 : StartDate.CompareTo(other.StartDate) * -1;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Project) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, ProjectActivities, Title, Field, StartDate, EndDate, ProjectDescription);
        }
    }
}