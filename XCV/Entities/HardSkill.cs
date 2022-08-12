using System;
using System.Collections.Generic;

namespace XCV.Entities
{
    /// <summary>
    /// A hard skill with the hard skill category it belongs to.
    /// </summary>
    public class HardSkill : Experience, IEquatable<HardSkill>
    {
        public string HardSkillCategory { get; set; }

        public HardSkill(Guid id, string name) : base(id, name)
        {
            
        }

        public HardSkill(Guid id, string name, string hardSkillCategory) : base(id, name)
        {
            HardSkillCategory = hardSkillCategory;
        }
        public HardSkill(string name, string hardSkillCategory) : base(name)
        {
            HardSkillCategory = hardSkillCategory;
        }
        
        public HardSkill(Guid id, string name, string hardSkillCategory, DateTime lastChanged) : base(id, name)
        {
            HardSkillCategory = hardSkillCategory;
            LastChanged = lastChanged;
        }

        public bool Equals(HardSkill? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && HardSkillCategory == other.HardSkillCategory;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((HardSkill) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), HardSkillCategory);
        }
    }
}