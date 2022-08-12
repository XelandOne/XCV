using System;

namespace XCV.Entities
{
    /// <summary>
    /// Represents a soft skill.
    /// </summary>
    public class SoftSkill : Experience
    {
        public SoftSkill(string name) : base(name)
        {
            
        }

        public SoftSkill(Guid id, string name) : base(id, name)
        {
            
        }
        public SoftSkill(Guid id, string name, DateTime lastChanged) : base(id, name)
        {
            LastChanged = lastChanged;
        }
    }
}