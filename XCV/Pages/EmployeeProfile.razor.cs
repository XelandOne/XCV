using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XCV.Entities;
using XCV.Services;

namespace XCV.Pages
{
    public partial class EmployeeProfile
    {
        private Employee? _employee;
        private List<(Guid, string)>? _employeeProjects;
        private readonly Dictionary<Guid, List<ProjectActivity>> _projectActivitiesForProject = new ();
        
        protected override async Task OnInitializedAsync()
        {
            _employee = _employeeManager.CurrentEmployee;
            _employeeProjects = await _employeeManager.GetProjectsOfEmployee();
            await _projectManager.Load();

            await UpdateProjectActivitiesForProject();
        }
        
        private async Task OnSignOutClick()
        {
            await (_authenticationStateProvider as CustomAuthStateProvider)!.Logout();
        }
        
        /// <summary>
        /// updates view
        /// </summary>
        private async Task UpdateProjectActivitiesForProject()
        {
            _projectActivitiesForProject.Clear();
            if (_employeeProjects != null)
                foreach (var (item1, _) in _employeeProjects)
                {
                    _projectActivitiesForProject.Add(item1,
                        await _projectManager.GetProjectActivities(item1));
                }
        }
    }
}