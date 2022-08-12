using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using XCV.InputModels;
using XCV.Services;

namespace XCV.Pages
{
    public partial class Index
    {
        [Inject] private AuthenticationStateProvider _authenticationStateProvider { get; set; }
        [CascadingParameter] private Task<AuthenticationState> authenticationStateTask { get; set; }
        [Inject] private NavigationManager _navigationManager { get; set; }
        [Inject] private ExperienceManager ExperienceManager { get; set; }
        [Inject] public FillDummyData FillDummyData { get; set; }
        private bool _disableLoadDummy = false;
        private bool _disableLoadData = false;


        private SignInModel _signInModel = new();

        private async Task OnDummyData()
        {
            _disableLoadDummy = true;
            _disableLoadData = true;
            await FillDummyData.GenerateDummyData();
        }
        
        private async Task OnDatabase()
        {
            _disableLoadData = true;
            await ExperienceManager.Load();
        }
        

        private async Task OnLogInSubmit()
        {
            var loginSuccess = await (_authenticationStateProvider as CustomAuthStateProvider)!.Login(_signInModel.Login);

            if (!loginSuccess)
            {
                _navigationManager.NavigateTo("/LoginFailed");
                return;
            }
            
            _navigationManager.NavigateTo("/employeeprofile");
        }

        private void CreateNewProfile()
        {
            _navigationManager.NavigateTo("/EmployeeProfileCreation");
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var authState = await authenticationStateTask;
                var user = authState.User;

                if (user.Identity != null && user.Identity.IsAuthenticated)
                {
                    _navigationManager.NavigateTo("/employeeprofile");
                }
            }
        }
    }
}