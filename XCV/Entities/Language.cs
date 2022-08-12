using System;

namespace XCV.Entities
{
    /// <summary>
    /// An object for storing a language.
    /// </summary>
    public class Language : Experience
    {
        public Language(string name) : base(name)
        {
            
        }

        public Language(Guid id, string name) : base(id, name)
        {
            
        }
        public Language(Guid id, string name, DateTime lastChanged) : base(id, name)
        {
            LastChanged = lastChanged;
        }
    }
}