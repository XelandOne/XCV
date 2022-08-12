using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using XCV.Data;
using XCV.Entities;
using XCV.Services;

namespace XCV.Pages
{
    public partial class OfferDetail
    {
        [Parameter] public Guid Id { get; set; }
        [Parameter] public Offer? Offer { get; set; }
        /// <summary>
        /// A dictionary of projects (Guid) with belonging projectActivities
        /// </summary>
        [Parameter] public Dictionary<Guid, List<ProjectActivity>?> ProjectActivities { get; set; } = new();
        
        /// <inheritdoc />
        protected override async Task OnInitializedAsync()
        {
            await _offerManager.Load();
            var temp = _offerManager.GetOffer(Id);
            if (temp != null)
            {
                Offer = temp;
                foreach (var shownEmployeeProperties in Offer.ShortEmployees)
                {
                    await _employeeManager.LoadEmployee(shownEmployeeProperties.EmployeeId);

                    foreach (var proj in shownEmployeeProperties.ProjectIds)
                    {
                        var projectactivitiy = await _projectManager.GetProjectActivities(proj);
                        if (projectactivitiy != null)
                        {
                            ProjectActivities[shownEmployeeProperties.Id] =  projectactivitiy;
                        }
                    }
                    
                }
            }
            await _projectManager.Load();
            
        }

        private bool EmployeesLoaded()
        {
            var loaded = true;
            if (Offer == null) return false;
            foreach (var shownEmployeeProperties in Offer.ShortEmployees.Where(shownEmployeeProperties =>
                !_employeeManager.Employees.Exists(x => x.Id.Equals(shownEmployeeProperties.EmployeeId))))
            {
                loaded = false;
            }

            return loaded;
        }
    }
}