using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

namespace XCV.Entities
{
    /// <summary>
    /// A project activity belongs to a project and has a description of what was done. It has a reference to all employees which worked on this project activity.
    /// </summary>
    public class ProjectActivity : IEquatable<ProjectActivity>
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Description { get; set; }

        private readonly List<Guid> _employeeIds = new List<Guid>();

        public ProjectActivity(string description)
        {
            Description = description;
        }
        
        public ProjectActivity(){}
        
        public ProjectActivity(Guid id, string description) : this(description)
        {
            
        }

        public void AddEmployee(Guid employeeId)
        {
            if (!_employeeIds.Contains(employeeId))
                _employeeIds.Add(employeeId);
        }
        
        public void RemoveEmployee(Guid employeeId)
        {
            _employeeIds.Remove(employeeId);
        }

        public void DeleteEmployee(Guid employeeId)
        {
            _employeeIds.Remove(employeeId);
        }

        /// <summary>
        /// Returns EmployeeIds
        /// </summary>
        /// <returns></returns>
        public List<Guid> GetEmployeeIds()
        {
            return _employeeIds;
        }

        public bool Equals(ProjectActivity? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _employeeIds.All(other._employeeIds.Contains) && Id.Equals(other.Id) && Description == other.Description;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ProjectActivity) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_employeeIds, Id, Description);
        }
    }
}