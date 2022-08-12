using System;

namespace XCV.Entities
{
    /// <summary>
    /// A role in a project of an employee.
    /// </summary>
    public class Role : Experience
    {
        public Role(string name) : base(name)
        {
        }

        public Role(Guid id, string name) : base(id, name)
        {
            
        }
        public Role(Guid id, string name, DateTime lastChanged) : base(id, name)
        {
            LastChanged = lastChanged;
        }
    }
}