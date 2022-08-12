using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using XCV.Entities;

namespace XCV.Pages
{
    public partial class EmployeeSearch
    {
        [Parameter] public Guid Id { get; set; }
        /// <summary>
        /// String value regarding the searched Filter Term
        /// </summary>
        private string? Filter { get; set; }
        /// <summary>
        /// String value regarding the selected Dropdown Menupoint 
        /// </summary>
        private string? Dropdown { get; set; }
        /// <summary>
        /// Boolean value set true if searched info equals the displayed skill
        /// </summary>
        private bool Hid { get; set; }
        
        protected override async Task OnInitializedAsync()
        {
            Dropdown = "Alles";
            await _experienceManager.Load();
            await _employeeManager.Load();
        }
        
        private bool IsVisible(string s)
        {
            return string.IsNullOrEmpty(Filter) || s.Contains(Filter, StringComparison.OrdinalIgnoreCase);
        }

        private void SelectEmployee(Guid employee, object checkedValue)
        {
            _searchManager.SelectEmployee(employee, (bool) checkedValue);
        }

        private void SelectExperience(Guid experience, object checkedValue)
        {
            _searchManager.SelectExperience(experience, (bool) checkedValue);
        }
        
        protected override void OnInitialized()
        {
            _searchManager.RemoveSelected();
        }
    }
}