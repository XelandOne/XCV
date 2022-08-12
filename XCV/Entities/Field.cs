using System;

namespace XCV.Entities
{
    /// <summary>
    /// A field of work.
    /// </summary>
    public class Field : Experience
    {
        public Field(string name) : base(name)
        {
            
        }

        public Field(Guid id, string name) : base(id, name)
        {
            
        }
        public Field(Guid id, string name, DateTime lastChanged) : base(id, name)
        {
            LastChanged = lastChanged;
        }
    }
}