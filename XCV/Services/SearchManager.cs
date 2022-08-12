using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using XCV.Entities;

namespace XCV.Services
{
    /// <summary>
    /// Manages the Search Page
    /// Preloads all the Employees and Experience
    /// provides a list of employees and experiences for the EmployeeSearch page
    /// Finds all the Employees with the selected experiences and provides them to the EmployeeSearchResult page
    /// </summary>
    public class SearchManager
    {
        [Inject] private ExperienceManager ExperienceManager { get; set; }
        [Inject] private EmployeeManager EmployeeManager { get; set; }
        /// <summary>
        /// Returns SelectedEmployees
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Guid> SelectedEmployees => _selectedEmployees;
        /// <summary>
        /// Returns SelectedExperience
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Guid> SelectedExperience => _selectedExperience;
        
        private readonly List<Guid> _selectedExperience;
        private readonly List<Guid> _selectedEmployees;
        private readonly List<(Employee, List<Guid>)> _foundEmployees;

        /// <summary>
        /// initializes the service and the lists
        /// </summary>
        /// <param name="experienceManager"></param>
        /// <param name="employeeManager"></param>
        public SearchManager(ExperienceManager experienceManager, EmployeeManager employeeManager)
        {
            ExperienceManager = experienceManager;
            EmployeeManager = employeeManager;

            _selectedExperience = new List<Guid>();
            _selectedEmployees = new List<Guid>();
            _foundEmployees = new List<(Employee, List<Guid>)>();
        }
        /// <summary>
        /// adds or removes ids from the SelectedEmployee List, according to the selected Employees on the EmployeeSearch page
        /// </summary>
        /// <param name="employee"></param>
        /// <param name="selected"></param>
        public void SelectEmployee(Guid employee, bool selected)
        {
            if (selected)
            {
                if (!_selectedEmployees.Contains(employee))
                    _selectedEmployees.Add(employee);
            }
            else
                _selectedEmployees.Remove(employee);
        }
        /// <summary>
        /// adds or removes ids from the SelectedExperience List, according to the selected Experiences on the EmployeeSearch page
        /// </summary>
        /// <param name="experience"></param>
        /// <param name="selected"></param>
        public void SelectExperience(Guid experience, bool selected)
        {
            if (selected)
            {
                if (!_selectedExperience.Contains(experience))
                    _selectedExperience.Add(experience);
            }
            else
                _selectedExperience.Remove(experience);
        }
        
        /// <summary>
        /// finds all employees with at least on of the selected experiences or one who was selected directly by name
        /// puts them into a list with all the experiences that match the ones they selected on their profiles
        /// </summary>
        /// <returns>List(Employee, List(Guid)) with found employee and their skills that match the selected ones</returns>
        public List<(Employee, List<Guid>)> GetSearchResult()
        {
            _foundEmployees.Clear();
            foreach (var employee in _selectedEmployees
                .Select(selectedEmployee => EmployeeManager.GetEmployee(selectedEmployee))
                .Where(employee => employee != null)) if (employee != null) _foundEmployees.Add((employee, new List<Guid>()));

            foreach (var experience in _selectedExperience)
            {
                if (EmployeeManager.Employees == null) continue;
                foreach (var employee in EmployeeManager.Employees)
                {
                    if (employee.Experience.Fields.Exists(x => x.Id == experience))
                    {
                        AddFoundEmployee(employee);
                        _foundEmployees.Find(x => x.Item1.Equals(employee)).Item2
                            .Add(ExperienceManager.Fields.Find(x => x.Id.Equals(experience))!.Id);
                    }
                    else if (employee.Experience.Roles.Exists(x => x.Id == experience))
                    {
                        AddFoundEmployee(employee);
                        _foundEmployees.Find(x => x.Item1.Equals(employee)).Item2
                            .Add(ExperienceManager.Roles.Find(x => x.Id.Equals(experience))!.Id);
                    }
                    else if (employee.Experience.Languages.Exists(x => x.Item1.Id == experience))
                    {
                        AddFoundEmployee(employee);
                        _foundEmployees.Find(x => x.Item1.Equals(employee)).Item2
                            .Add(ExperienceManager.Languages.Find(x => x.Id.Equals(experience))!.Id);
                    }
                    else if (employee.Experience.HardSkills.Exists(x => x.Item1.Id == experience))
                    {
                        AddFoundEmployee(employee);
                        _foundEmployees.Find(x => x.Item1.Equals(employee)).Item2
                            .Add(ExperienceManager.HardSkills.Find(x => x.Id.Equals(experience))!.Id);
                    }
                    else if (employee.Experience.SoftSkills.Exists(x => x.Id == experience))
                    {
                        AddFoundEmployee(employee);
                        _foundEmployees.Find(x => x.Item1.Equals(employee)).Item2
                            .Add(ExperienceManager.SoftSkills.Find(x => x.Id.Equals(experience))!.Id);
                    }
                }
            }
            return _foundEmployees;
        }
        
        /// <summary>
        /// clears selected Employee and Experience List
        /// </summary>
        public void RemoveSelected()
        {
            _selectedEmployees.Clear();
            _selectedExperience.Clear();
        }
        
        /// <summary>
        /// adds a new Employee to the FoundEmployee list
        /// </summary>
        /// <param name="employee"></param>
        private void AddFoundEmployee(Employee employee)
        {
            if (!_foundEmployees.Exists(x => x.Item1.Equals(employee)))
                _foundEmployees.Add((employee, new List<Guid>()));
        }
    }
}