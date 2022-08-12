using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using XCV.Entities.Enums;
using XCV.ValidationAttributes;

namespace XCV.Entities
{
    /// <summary>
    /// An employee of the company with details like a name, a profile picture and his experiences.
    /// </summary>
    public class Employee : IEquatable<Employee>
    {
        private const string InvalidNameCharactersErrorMessage = "Der Name darf keine Sonderzeichen beinhalten.";
        private const string InvalidNameLengthsErrorMessage = "Der Name darf nicht länger als {1} Zeichen sein.";
        private const string InvalidYearsErrorMessage = "Die Anzahl an Jahren muss zwischen {1} und {2} liegen.";
        
        public Guid Id { get; } = Guid.NewGuid();

        [Required]
        [StringLength(30, ErrorMessage = InvalidNameLengthsErrorMessage)]
        [RegularExpression("^\\D*$", ErrorMessage = InvalidNameCharactersErrorMessage)]
        [SqlInjectionValidation]
        public string FirstName { get; set; }
        [Required]
        [StringLength(30, ErrorMessage = InvalidNameLengthsErrorMessage)]
        [RegularExpression("^\\D*$", ErrorMessage = InvalidNameCharactersErrorMessage)]
        public string SurName { get; set; }
        [Required]
        [StringLength(30, ErrorMessage = InvalidNameLengthsErrorMessage)]
        public string UserName { get; }
        [Required] [DateTimeRange] public DateTime EmployedSince { get; set; }

        /// <summary>
        /// Number of years worked outside of the company.
        /// </summary>
        [Required]
        [Range(0, 100, ErrorMessage = InvalidYearsErrorMessage)]
        public int WorkExperience { get; set; }
        /// <summary>
        /// Work experience as a scientific assistant.
        /// </summary>
        [Required(ErrorMessage = InvalidYearsErrorMessage)]
        [Range(0, 100, ErrorMessage = InvalidYearsErrorMessage)]
        public int ScientificAssistant { get; set; }
        /// <summary>
        /// Work experience as a student assistant.
        /// </summary>
        [Required]
        [Range(0, 100, ErrorMessage = InvalidYearsErrorMessage)]
        public int StudentAssistant { get; set; }
        [Required] public Authorizations Authorizations { get; set; }
        [Required] public RateCardLevel RateCardLevel { get; set; }
        public byte[]? ProfilePicture { get; set; }
        /// <summary>
        /// All experiences of the employee.
        /// </summary>
        public UsedExperience Experience { get; set; } = new();
        /// <summary>
        /// The projects the employee worked on.
        /// </summary>
        public List<Guid> ProjectIds { get; } = new();
        public DateTime? LastChanged { get; set; }

        ///cosntructor to copy an Employee
        public Employee(Employee employee)
        {
            Id = employee.Id;
            FirstName = employee.FirstName;
            SurName = employee.SurName;
            UserName = employee.UserName;
            EmployedSince = employee.EmployedSince;
            WorkExperience = employee.WorkExperience;
            ScientificAssistant = employee.ScientificAssistant;
            StudentAssistant = employee.StudentAssistant;
            Authorizations = employee.Authorizations;
            RateCardLevel = employee.RateCardLevel;
            if (employee.ProfilePicture != null) ProfilePicture = employee.ProfilePicture.ToArray();
            Experience = new UsedExperience(employee.Experience);
            ProjectIds = employee.ProjectIds.ToList();
            LastChanged = employee.LastChanged;
        }
        
        

        public Employee(Authorizations authorizations, string surName, string firstName, string userName,
            DateTime employedSince, int workExperience, int scientificAssistant, int studentAssistant,
            RateCardLevel rateCardLevel, byte[]? profilePicture)
        {
            FirstName = firstName;
            SurName = surName;
            Authorizations = authorizations;
            UserName = userName;
            EmployedSince = employedSince.Date;
            WorkExperience = workExperience;
            ScientificAssistant = scientificAssistant;
            StudentAssistant = studentAssistant;
            RateCardLevel = rateCardLevel;
            ProfilePicture = profilePicture;
        }

        // Constructor for FillDummyData
        public Employee(Guid id, Authorizations authorizationses, string surName, string firstName, string userName,
            DateTime employedSince, int workExperience, int scientificAssistant, int studentAssistant,
            RateCardLevel rateCardLevel, byte[]? profilePicture) : this(authorizationses, surName, firstName, userName,
            employedSince, workExperience,
            scientificAssistant, studentAssistant, rateCardLevel, profilePicture)
        {
            Id = id;
        }
        
        // Constructor for dapper
        /*public Employee(Guid id, Authorizations authorizationses, string surName, string firstName, string userName,
            DateTime employedSince, int workExperience, int scientificAssistant, int studentAssistant,
            RateCardLevel rateCardLevel, byte[]? profilePicture, DateTime lastChanged) : this(authorizationses, surName, firstName, userName,
            employedSince, workExperience,
            scientificAssistant, studentAssistant, rateCardLevel, profilePicture)
        {
            Id = id;
            LastChanged = lastChanged;
        }*/
        
        public Employee(Guid id, Authorizations authorizations, string surname, string firstname, string username,
            DateTime employedSince, int workExperience, int scientificAssistant, int studentAssistant,
            RateCardLevel rateCardLevel, byte[]? profilePicture, DateTime lastChanged) : this(authorizations, surname, firstname, username,
            employedSince, workExperience,
            scientificAssistant, studentAssistant, rateCardLevel, profilePicture)
        {
            Id = id;
            LastChanged = lastChanged;
        }

        // Constructor for dapper
        /*public Employee(Guid id, string surName, string firstName, string userName,
            DateTime employedSince, int workExperience, int scientificAssistant, int studentAssistant,
            byte[]? profilePicture, DateTime lastChanged)
        {
            Id = id;
            FirstName = firstName;
            SurName = surName;
            UserName = userName;
            EmployedSince = employedSince.Date;
            WorkExperience = workExperience;
            ScientificAssistant = scientificAssistant;
            StudentAssistant = studentAssistant;
            ProfilePicture = profilePicture;
            LastChanged = lastChanged;
        }*/

        /// <summary>
        /// Gets the years of work experience of the employee.
        /// </summary>
        /// <returns>Years of work experience</returns>
        public int CalcRelevantWorkExperience()
        {
            double yearsInCurrentCompany = DateTime.Now.Date.Subtract(EmployedSince).Days / 365.0;
            if (yearsInCurrentCompany < 0)
                yearsInCurrentCompany = 0;
            
            return (int) Math.Round(yearsInCurrentCompany + WorkExperience +
                          0.5 * ScientificAssistant +
                          0.3 * StudentAssistant);
        }

        public bool Equals(Employee? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id) && FirstName == other.FirstName && SurName == other.SurName &&
                   UserName == other.UserName && EmployedSince.Equals(other.EmployedSince) &&
                   WorkExperience == other.WorkExperience && ScientificAssistant == other.ScientificAssistant &&
                   StudentAssistant == other.StudentAssistant && Authorizations == other.Authorizations &&
                   RateCardLevel == other.RateCardLevel &&
                   (ProfilePicture?.Equals(other.ProfilePicture) ?? other.ProfilePicture == null) &&
                   Experience.Equals(other.Experience) && ProjectIds.All(other.ProjectIds.Contains);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Employee) obj);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Id);
            hashCode.Add(FirstName);
            hashCode.Add(SurName);
            hashCode.Add(UserName);
            hashCode.Add(EmployedSince);
            hashCode.Add(WorkExperience);
            hashCode.Add(ScientificAssistant);
            hashCode.Add(StudentAssistant);
            hashCode.Add((int) Authorizations);
            hashCode.Add((int) RateCardLevel);
            hashCode.Add(ProfilePicture);
            hashCode.Add(Experience);
            hashCode.Add(ProjectIds);
            return hashCode.ToHashCode();
        }
    }
}