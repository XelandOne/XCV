using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.RazorPages;
using XCV.Entities;

namespace XCV.Pages
{
    public partial class ProjectDetail
    {
        [Parameter] public string Id { get; set; }
        private Project? Project { get; set; }
        /// <summary>
        /// A dictionary of projectActivities with belonging employees
        /// </summary>
        private Dictionary<ProjectActivity, List<Employee>> _activities { get; set; }
        private List<Employee> _employees { get; set; }
        
        
        protected override async Task OnInitializedAsync()
        {
            Project = new Project("", new Field(""), DateTime.Now, DateTime.Now, "");

            _employees = new List<Employee>();
            _activities = new Dictionary<ProjectActivity, List<Employee>>();
            await _projectManager.Load();
            await _employeeManager.Load();


            if (!Id.Equals("-1"))
            {
                Project = _projectManager.Projects.Find(x => Equals(x.Id.ToString(), Id));
            }

            if (Project != null)
            {
                _employees = await _employeeManager.GetEmployeesInProject(Project.Id); 
            }
            
            if (Project == null) return;

            foreach (var activity in await _projectManager.GetProjectActivities(Project.Id))
            {
                _activities.Add(activity, new List<Employee>());
                foreach (var employeeId in activity.GetEmployeeIds())
                {
                    var employee = _employeeManager.GetEmployee(employeeId);
                    if (employee != null) _activities[activity].Add(employee);
                }
            }
        }
    }
}