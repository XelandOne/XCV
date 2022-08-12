using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using XCV.Entities;
using XCV.Entities.Enums;

namespace XCV.Services
{
    /// <summary>
    /// Handles authorisation of the logged in employee.
    /// </summary>
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        [Inject] private EmployeeManager EmployeeManager { get; set; }
        
        private ProtectedLocalStorage _protectedLocalStorage;
        private NavigationManager _navigationManager;
        

        public CustomAuthStateProvider(ProtectedLocalStorage protectedLocalStorage, NavigationManager navigationManager, EmployeeManager employeeManager)
        {
            _protectedLocalStorage = protectedLocalStorage;
            _navigationManager = navigationManager;
            EmployeeManager = employeeManager;
        }
        
        
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var signedInEmployee = EmployeeManager.CurrentEmployee;
            if (signedInEmployee == null)
            {
                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            }

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, signedInEmployee.Authorizations.ToString()),
            }, "username");

            var user = new ClaimsPrincipal(identity);

            return Task.FromResult(new AuthenticationState(user));
        }

        
        /// <summary>
        /// Login of the employee with the given username.
        /// </summary>
        /// <param name="username">The user name of the employee</param>
        /// <returns>False if the username does not exist</returns>
        public async Task<bool> Login(string username)
        {
            var loginSuccess = await EmployeeManager.LoginEmployee(username);

            await _protectedLocalStorage.SetAsync("username", username);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return loginSuccess;
        }
        
        /// <summary>
        /// Logout of the currently logged in employee.
        /// </summary>
        /// <returns></returns>
        public async Task Logout()
        {
            EmployeeManager.LogoutEmployee();
            await _protectedLocalStorage.DeleteAsync("username");
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            _navigationManager.NavigateTo("/");
        }
    }
}