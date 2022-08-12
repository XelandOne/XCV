using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace XCV.Entities
{
    /// <summary>
    /// Base class of all experience types. Every experience has a name.
    /// </summary>
    public abstract class Experience : IEquatable<Experience>
    {
        [Required]
        public string Name { get; set; }

        public Guid Id { get; } = Guid.NewGuid();

        public DateTime? LastChanged;

        public Experience(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public Experience(Guid id, string name) : this(name)
        {
            Id = id;
        }

        public bool Equals(Experience? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name && Id.Equals(other.Id);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Experience) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Id);
        }
    }
}