using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using XCV.Entities;
using XCV.Entities.Enums;

namespace XCV.Pages
{
    public partial class EmployeeProfileEdit
    {
        [Parameter] public Employee? CurrentEmployeeCopy { get; set; }
        private List<(Guid, string)>? EmployeeProjects { get; set; } = new List<(Guid, string)>();
        private bool AddRoleShow { get; set; }
        private bool AddLanguageShow { get; set; }
        private bool AddFieldShow { get; set; }
        private bool AddHardSkillShow { get; set; }
        private bool AddSoftSkillShow { get; set; }
        private bool AddProjectShow { get; set; }
        private bool AddActivityShow { get; set; }
        private Guid RoleSelectedId { get; set; }
        private Guid LanguageSelectedId { get; set; }
        private LanguageLevel LanguageLevelSelected { get; set; } = LanguageLevel.Native;
        private Guid FieldSelectedId { get; set; }
        private Guid HardSkillSelectedId { get; set; }
        private HardSkillLevel HardSkillLevelSelected { get; set; } = HardSkillLevel.Expert;
        private Guid SoftSkillSelectedId { get; set; }
        private Guid ProjectSelectedId { get; set; }
        private Guid ProjectActivitySelectedId { get; set; }
        private Project? ProjectForProjectActivity { get; set; }
        private List<ProjectActivity> ProjectActivities { get; set; } = new List<ProjectActivity>();

        private Dictionary<Guid, List<ProjectActivity>> ProjectActivitiesForProject =
            new Dictionary<Guid, List<ProjectActivity>>();
        private List<(Guid, string)> _allProjectNames;
        private List<(Guid, string)> _projectNames;
        

        private bool _profileImageToLarge;
        private const double MaxFileSizeMb = 5;

        protected override async Task OnInitializedAsync()
        {
            CurrentEmployeeCopy = null;
            ProjectForProjectActivity = null;
            await _employeeManager.Load();
            await _experienceManager.Load();
            await _projectManager.Load();
            _allProjectNames = await _projectManager.GetAllProjectNames();
            
            var currentEmployee = _employeeManager.CurrentEmployee;
            if (currentEmployee != null)
            {
                CurrentEmployeeCopy = new Employee(currentEmployee);
                //CurrentEmployeeCopy = currentEmployee;
                _projectNames = await GetProjects();
                EmployeeProjects = await _employeeManager.GetProjectsOfEmployee();
            }
            await UpdateProjectActivitiesForProject();
        }

        private async Task OnProfileImageUpload(InputFileChangeEventArgs e)
        {
            _profileImageToLarge = e.File.Size >= MaxFileSizeMb * 1000000;
            if (_profileImageToLarge)
            {
                return;
            }

            await using var memoryStream = new MemoryStream();
            var image = await Image.LoadAsync(e.File.OpenReadStream((long) MaxFileSizeMb * 1000000));
            //image.Mutate(i => i.Resize(512, 512));
            image.Mutate(i => i.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Crop,
                    Size = new Size(512, 512)
                })
            );

            await using var memoryStreamSave = new MemoryStream();
            await image.SaveAsync(memoryStreamSave, PngFormat.Instance);

            if (CurrentEmployeeCopy != null) CurrentEmployeeCopy.ProfilePicture = memoryStreamSave.ToArray();
            StateHasChanged();
        }

        /// <summary>
        /// updates the employeeProfile and submits all changes
        /// </summary>
        private async Task OnValidSubmit()
        {
            if (CurrentEmployeeCopy != null) await _employeeService.UpdateEmployee(CurrentEmployeeCopy);
            await _employeeManager.RefreshLoggedInEmployee();

            _navigationManager.NavigateTo("/employeeprofile");
        }

        /// <summary>
        /// opens modal for adding a role
        /// </summary>
        private void ModalAddRole()
        {
            AddRoleShow = !AddRoleShow;
        }

        /// <summary>
        /// opens modal for adding a language
        /// </summary>
        private void ModalAddLanguage()
        {
            AddLanguageShow = !AddLanguageShow;
        }

        /// <summary>
        /// opens modal for adding a field
        /// </summary>
        private void ModalAddField()
        {
            AddFieldShow = !AddFieldShow;
        }

        /// <summary>
        /// opens modal for adding a hardskill
        /// </summary>
        private void ModalAddHardSkill()
        {
            AddHardSkillShow = !AddHardSkillShow;
        }

        /// <summary>
        /// opens modal for adding a softskill
        /// </summary>
        private void ModalAddSoftSkill()
        {
            AddSoftSkillShow = !AddSoftSkillShow;
        }

        /// <summary>
        /// opens modal for adding a project
        /// </summary>
        private void ModalAddProject()
        {
            AddProjectShow = !AddProjectShow;
        }

        /// <summary>
        /// opens modal for adding a project activity for a project
        /// </summary>
        /// <param name="projectId">project to which the project activity is added</param>
        private async Task ModalAddActivity(Guid projectId)
        {
            ProjectForProjectActivity = await _projectManager.GetProject(projectId);
            UpdateProjectActivities();
            AddActivityShow = !AddActivityShow;
        }

        /// <summary>
        /// closes modals and resets parameters
        /// </summary>
        private void CloseModal()
        {
            AddRoleShow = false;
            AddLanguageShow = false;
            AddFieldShow = false;
            AddHardSkillShow = false;
            AddSoftSkillShow = false;
            AddProjectShow = false;
            AddActivityShow = false;

            RoleSelectedId = new Guid();
            LanguageSelectedId = new Guid();
            FieldSelectedId = new Guid();
            HardSkillSelectedId = new Guid();
            SoftSkillSelectedId = new Guid();
            ProjectSelectedId = new Guid();
            ProjectActivitySelectedId = new Guid();
            StateHasChanged();
            //await UpdateProjectActivitiesForProject();
            StateHasChanged();
            //await UpdateProjects();
            //StateHasChanged();
            //CurrentEmployeeCopy = await _employeeManager.RefreshLoggedInEmployee();
            //_navigationManager.NavigateTo("/employeeprofile/edit", forceLoad: true);
            //UpdateProjects();
        }

        /// <summary>
        /// adds role and updates view
        /// </summary>
        private void AddRole()
        {
            var roleResult = _experienceManager.Roles.Find(x => x.Id.Equals(RoleSelectedId));
            if (CurrentEmployeeCopy != null && roleResult != null)
                _employeeManager.AddExperience(CurrentEmployeeCopy, roleResult);
            CloseModal();
        }

        /// <summary>
        /// removes role and updates view
        /// </summary>
        /// <param name="role">removed role</param>
        private void RemoveRole(Role role)
        {
            if (CurrentEmployeeCopy != null) _employeeManager.RemoveExperience(CurrentEmployeeCopy, role);
            //_navigationManager.NavigateTo("/employeeprofile/edit", forceLoad: true);
        }

        /// <summary>
        /// adds language and updates view
        /// </summary>
        private void AddLanguage()
        {
            var language = _experienceManager.Languages.Find(x => x.Id.Equals(LanguageSelectedId));
            if (CurrentEmployeeCopy != null)
                if (language != null)
                    _employeeManager.UpdateLanguage(CurrentEmployeeCopy, (language, LanguageLevelSelected));
            CloseModal();
        }

        /// <summary>
        /// removes language and updates view
        /// </summary>
        /// <param name="language">removed language</param>
        private void RemoveLanguage(Language language)
        {
            if (CurrentEmployeeCopy != null) _employeeManager.RemoveExperience(CurrentEmployeeCopy, language);

            //_navigationManager.NavigateTo("/employeeprofile/edit", forceLoad: true);
        }

        /// <summary>
        /// adds field and updates view
        /// </summary>
        private void AddField()
        {
            if (CurrentEmployeeCopy != null)
            {
                var fieldResult = _experienceManager.Fields.Find(x => x.Id.Equals(FieldSelectedId));
                if (fieldResult != null)
                    _employeeManager.AddExperience(CurrentEmployeeCopy,
                        fieldResult);
            }

            CloseModal();
        }

        /// <summary>
        /// removes field and updates view
        /// </summary>
        /// <param name="field">removed field</param>
        private void RemoveField(Field field)
        {
            if (CurrentEmployeeCopy != null) _employeeManager.RemoveExperience(CurrentEmployeeCopy, field);
            //_navigationManager.NavigateTo("/employeeprofile/edit", forceLoad: true);
        }

        /// <summary>
        /// adds hardskill and updates view
        /// </summary>
        private void AddHardSkill()
        {
            var hardSkill = _experienceManager.HardSkills.Find(h => h.Id.Equals(HardSkillSelectedId));
            if (CurrentEmployeeCopy != null && hardSkill != null)
                _employeeManager.UpdateHardSkill(CurrentEmployeeCopy, (hardSkill, HardSkillLevelSelected));
            CloseModal();
        }

        /// <summary>
        /// removes hardskill and updates view
        /// </summary>
        /// <param name="hardSkill">removed hardskill</param>
        private void RemoveHardSkill(HardSkill hardSkill)
        {
            if (CurrentEmployeeCopy != null) _employeeManager.RemoveExperience(CurrentEmployeeCopy, hardSkill);
        }

        /// <summary>
        /// adds softskill and updates view
        /// </summary>
        private void AddSoftSkill()
        {
            var softSkill = _experienceManager.SoftSkills.Find(x => x.Id.Equals(SoftSkillSelectedId));
            if (CurrentEmployeeCopy != null && softSkill != null)
            {
                _employeeManager.AddExperience(CurrentEmployeeCopy,
                    softSkill);
            }

            CloseModal();
        }

        /// <summary>
        /// removed softskill and updates view
        /// </summary>
        /// <param name="softSkill"></param>
        private void RemoveSoftSkill(SoftSkill softSkill)
        {
            if (CurrentEmployeeCopy != null) _employeeManager.RemoveExperience(CurrentEmployeeCopy, softSkill);
            //_navigationManager.NavigateTo("/employeeprofile/edit", forceLoad: true);
        }

        /// <summary>
        /// adds project and updates view
        /// </summary>
        private async Task AddProject()
        {
            CurrentEmployeeCopy?.ProjectIds.Add(ProjectSelectedId);
            _projectNames = await GetProjects();
            await UpdateProjectActivitiesForProject();
            CloseModal();
        }

        /// <summary>
        /// removes project and project activities of this project and updates view
        /// </summary>
        /// <param name="projectId"></param>
        private async Task RemoveProject(Guid projectId)
        {
            CurrentEmployeeCopy?.ProjectIds.Remove(projectId);
            
            var project = await _projectManager.GetProject(projectId);
            if (project == null)
            {
                _navigationManager.NavigateTo("/employeeprofile/edit", true);
                return;
            }
            var activities = await _employeeManager.GetAllProjectActivities(projectId);
            if (activities == null) return;
            foreach (var projectProjectActivity in project.ProjectActivities.Where(projectProjectActivity => CurrentEmployeeCopy != null && projectProjectActivity.GetEmployeeIds().Contains(CurrentEmployeeCopy.Id)))
            {
                await _employeeManager.RemoveProjectActivity(activities, projectProjectActivity.Id, projectId);
            }
            
            _projectNames = await GetProjects();
            StateHasChanged();
        }

        /// <summary>
        /// adds project activity and updates view
        /// </summary>
        private async void AddActivity()
        {
            if (ProjectForProjectActivity == null) return;
            if(ProjectActivitiesForProject[ProjectForProjectActivity.Id].Exists(x => x.Id.Equals(ProjectActivitySelectedId)))
            {
                await _employeeManager.AddProjectActivity(ProjectForProjectActivity.ProjectActivities, ProjectActivitySelectedId,
                                ProjectForProjectActivity.Id);
                await UpdateProjectActivitiesForProject();
            }
            CloseModal();
        }

        /// <summary>
        /// removes project activity and updates view
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="activity"></param>
        private async void RemoveActivity(Guid projectId, ProjectActivity activity)
        {
            var project = await _projectManager.GetProject(projectId);
            if (project == null)
            {
                _navigationManager.NavigateTo("/employeeprofile/edit", true);
                return;
            }
            await _employeeManager.RemoveProjectActivity(project.ProjectActivities, activity.Id,
                projectId);
            await UpdateProjectActivitiesForProject();
            StateHasChanged();
        }

        /// <summary>
        /// gets the project activities of a project
        /// </summary>
        private void UpdateProjectActivities()
        {
            ProjectActivities.Clear();
            if (ProjectForProjectActivity == null)
            {
                _navigationManager.NavigateTo("/employeeprofile/edit", true);
                return;
            }
            foreach (var activity in ProjectForProjectActivity.ProjectActivities)
            {
                ProjectActivities.Add(activity);
            }
        }

        /// <summary>
        /// gets the project activities of the current employee copy
        /// </summary>
        private async Task UpdateProjectActivitiesForProject()
        {
            ProjectActivitiesForProject.Clear();
            foreach (var project in _projectNames)
            {
                ProjectActivitiesForProject.Add(project.Item1, await _projectManager.GetProjectActivities(project.Item1));
            }
        }

        /// <summary>
        /// Gets the project ids and names of the current employee copy.
        /// </summary>
        /// <returns></returns>
        private async Task<List<(Guid, string)>> GetProjects()
        {
            if (CurrentEmployeeCopy != null)
                return await _projectManager.GetProjectNames(CurrentEmployeeCopy.ProjectIds);
            return new List<(Guid, string)>();
        }

        /// <summary>
        /// Removes the profile image of the current employee.
        /// </summary>
        private void RemoveProfileImage()
        {
            if (CurrentEmployeeCopy != null) CurrentEmployeeCopy.ProfilePicture = null;
        }
    }
}