using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using XCV.Entities.Enums;
using XCV.Services;

namespace XCV.Entities
{
    /// <summary>
    /// Entity to manage all the experiences in on place
    /// </summary>
    public class UsedExperience : IEquatable<UsedExperience>
    {
        public List<Field> Fields { get; }
        public List<Role> Roles { get; }
        public List<SoftSkill> SoftSkills { get; }
        public List<(HardSkill, HardSkillLevel)> HardSkills { get; }
        public List<(Language, LanguageLevel)> Languages { get; }
        

        public UsedExperience()
        {
            Fields = new List<Field>();
            Roles = new List<Role>();
            SoftSkills = new List<SoftSkill>();
            HardSkills = new List<(HardSkill, HardSkillLevel)>();
            Languages = new List<(Language, LanguageLevel)>();
        }

        /// <summary>
        /// constructor to make a copy of an offer
        /// </summary>
        /// <param name="temp"></param>
        public UsedExperience(UsedExperience temp)
        {
            Fields = temp.Fields.ToList();
            Roles = temp.Roles.ToList();
            SoftSkills = temp.SoftSkills.ToList();
            HardSkills = temp.HardSkills.ToList();
            Languages = temp.Languages.ToList();
        }

        public Experience? GetSkillByGuid(Guid id)
        {
            if (Fields.Exists(x => x.Id.Equals(id)))
                return Fields.Find(x => x.Id.Equals(id));
            if (Roles.Exists(x => x.Id.Equals(id)))
                return Roles.Find(x => x.Id.Equals(id));
            if (SoftSkills.Exists(x => x.Id.Equals(id)))
                return SoftSkills.Find(x => x.Id.Equals(id));
            if (Languages.Exists(x => x.Item1.Id.Equals(id)))
                return Languages.Find(x => x.Item1.Id.Equals(id)).Item1;
            if (HardSkills.Exists(x => x.Item1.Id.Equals(id)))
                return HardSkills.Find(x => x.Item1.Id.Equals(id)).Item1;
            return null;
        }
        
        public bool Equals(UsedExperience? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Fields.All(other.Fields.Contains) && Roles.All(other.Roles.Contains) &&
                   SoftSkills.All(other.SoftSkills.Contains) && HardSkills.All(other.HardSkills.Contains) &&
                   Languages.All(other.Languages.Contains);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((UsedExperience) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Fields, Roles, SoftSkills, HardSkills, Languages);
        }
    }
}