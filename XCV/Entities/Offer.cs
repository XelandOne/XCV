using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XCV.Entities
{
    /// <summary>
    /// Offer Entity to locally store, an update an offer
    /// </summary>
    public class Offer : IEquatable<Offer>
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Title { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public UsedExperience Experience { get; set; } = new();

        /// <summary>
        /// The configured employees for the offer.
        /// </summary>
        public List<ShownEmployeeProperties> ShortEmployees { get; } = new();

        /// <summary>
        /// The document configurations which belong to this offer.
        /// </summary>
        public List<Guid> DocumentConfigurations { get; } = new();

        public DateTime? LastChanged { get; set; }

        public Offer(string title)
        {
            Title = title;
        }

        //Constructor for Dapper
        public Offer(Guid id, string title) : this(title)
        {
            Id = id;
        }

        public Offer(string title, DateTime? startDate, DateTime? endDate) : this(title)
        {
            StartDate = startDate;
            EndDate = endDate;
        }

        //Constructor for Dapper
        public Offer(Guid id, string title, DateTime? startDate, DateTime? endDate, DateTime lastChanged) : this(title,
            startDate, endDate)
        {
            Id = id;
            LastChanged = lastChanged;
        }

        //Constructor for DummyData
        public Offer(Guid id, string title, DateTime? startDate, DateTime? endDate) : this(title, startDate, endDate)
        {
            Id = id;
        }

        public Offer(string title, DateTime? startDate, DateTime? endDate, List<Employee> employees) : this(title,
            startDate, endDate)
        {
            employees.ForEach(e => ShortEmployees.Add(new ShownEmployeeProperties(e, this.Id)));
        }

        public void AddEmployee(Employee employee)
        {
            ShortEmployees.Add(new ShownEmployeeProperties(employee, this.Id));
        }

        public bool Equals(Offer? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id) && Title == other.Title && Experience.Equals(other.Experience) &&
                   ShortEmployees.All(other.ShortEmployees.Contains) &&
                   DocumentConfigurations.All(other.DocumentConfigurations.Contains);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Offer) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Title, Experience, ShortEmployees);
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder = stringBuilder.Append("Id: " + Id);
            stringBuilder = stringBuilder.Append(" Title: " + Title);
            stringBuilder = stringBuilder.Append(Experience.ToString());
            stringBuilder = stringBuilder.Append(ShortEmployees.ToString());
            return stringBuilder.ToString();
        }
    }
}