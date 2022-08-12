using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using XCV.Data;
using XCV.Entities;
using XCV.InputModels;
using XCV.Services;

namespace XCV.Pages
{
    public partial class EmployeeProfileCreation
    {
        private EmployeeCreationModel _employeeCreationModel = new();

        private bool _usernameAlreadyExists;

        private async Task OnCreationSubmit()
        {
            var usernameAlreadyExists = await UsernameAlreadyExists();
            if (usernameAlreadyExists)
            {
                _usernameAlreadyExists = true;
                StateHasChanged();
                return;
            }


            if (_employeeCreationModel.Authorization != null && _employeeCreationModel.EmployedSince != null &&
                _employeeCreationModel.WorkExperience != null && _employeeCreationModel.ScientificAssistant != null &&
                _employeeCreationModel.StudentAssistant != null && _employeeCreationModel.RateCardLevel != null)
            {
                var employee = new Employee(_employeeCreationModel.Authorization.Value,
                    _employeeCreationModel.SurName,
                    _employeeCreationModel.FirstName, _employeeCreationModel.UserName,
                    _employeeCreationModel.EmployedSince.Value,
                    _employeeCreationModel.WorkExperience.Value,
                    _employeeCreationModel.ScientificAssistant.Value,
                    _employeeCreationModel.StudentAssistant.Value,
                    _employeeCreationModel.RateCardLevel.Value,
                    _employeeCreationModel.ProfilePicture);
                await _employeeManager.AddNewEmployee(employee);

                await (_authenticationStateProvider as CustomAuthStateProvider)!.Login(employee.UserName);
            }

            _navigationManager.NavigateTo("/employeeprofile/edit");
        }

        private async Task<bool> UsernameAlreadyExists()
        {
            var username = _employeeCreationModel.UserName;
            var employee = await _employeeService.GetEmployee(username);

            var usernameAlreadyExists = employee != null;
            return usernameAlreadyExists;
        }
    }
}