using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Components;
using XCV.Entities;
using XCV.Entities.Enums;
using XCV.InputModels;
using Field = XCV.Entities.Field;

namespace XCV.Pages
{
    public partial class ProjectDetailEdit
    {
        [Parameter] public Guid Id { get; set; }
        /// <summary>
        /// Boolean value set true if someone updated a project at the same time as someone else.
        /// </summary>
        [Parameter] public bool Failed { get; set; }
        
        private Project? Project { get; set; }
        /// <summary>
        /// The copy of the project which will be processed on the page.
        /// </summary>
        private Project? ProjectCopy { get; set; } = new();
        private ActivityModel? _activityModel { get; set; }
        private PurposeModel? _purposeModel { get; set; }
        /// <summary>
        /// For showing the activityModal for adding an activity
        /// </summary>
        private bool AddActivityShow { get; set; }
        /// <summary>
        /// For showing the purposeModel for adding a purpose
        /// </summary>
        private bool AddPurposeShow { get; set; }
        /// <summary>
        /// For showing the the activityModel for updating an activity
        /// </summary>
        private bool AddActivityChange { get; set; }
        private ProjectActivity ProjectActivityInput;
        private List<Employee> _employeesInProject { get; set; }
        private List<ProjectActivity> _activities { get; set; }
       
        /// <summary>
        /// The copy of the projectActivityList which will be processed on the page.
        /// </summary>
        private List<ProjectActivity> _activitiesCopy { get; set; }
        private Guid? FieldId { get; set; }
        /// <summary>
        /// The copy of the fieldId
        /// </summary>
        private Guid? FieldIdCopy { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Project = new Project("", DateTime.Now, DateTime.Now, "");
            _employeesInProject = new List<Employee>();
            _activities = new List<ProjectActivity>();
            _activitiesCopy = _activities;
            FieldId = null;
            _activityModel = null;
            AddActivityShow = false;
            await _projectManager.Load();
            await _employeeManager.Load();
            await _experienceManager.Load();

            Project = await _projectManager.GetProject(Id);
            ProjectCopy = await _projectManager.GetProject(Id);
            

            if (Project != null && Project.Field != null)
            {
                FieldId = Project.Field.Id;
                FieldIdCopy = Project.Field.Id;
                _employeesInProject = await _employeeManager.GetEmployeesInProject(Project.Id);
            }

            if (ProjectCopy != null)
            {
                _activitiesCopy = await _projectManager.GetProjectActivities(ProjectCopy.Id);
            }

            if (ProjectCopy == null)
            {
                _navigationManager.NavigateTo("ProjectOverview/" + true, true);
            }
        }

        /// <summary>
        /// If failed is true an someone else updated the project und another one wants also to delete it then it shows a message and goes back to projectOverview.
        /// </summary>
        protected override async Task OnParametersSetAsync()
        {
            if (Failed)
            {
                await Task.Delay(5000);
                Failed = false;
                _navigationManager.NavigateTo("/projectdetailedit/" + Project?.Id);
            }
        }

        /// <summary>
        /// submits the update for a Project
        /// </summary>
        private async Task OnSubmit()
        {
            Project = ProjectCopy;
            FieldId = FieldIdCopy;
            _activities = _activitiesCopy;
            if (Project == null )
            {
                _navigationManager.NavigateTo("ProjectOverview/" + true, true);
                return;
            }

            Project.Field = _experienceManager.Fields.Find(x => x.Id.Equals(FieldId));
            Project.ProjectActivities.Clear();
            _activities.ForEach(x => Project.ProjectActivities.Add(x));

            var result = await _projectManager.UpdateProject(Project);
            if (result == DataBaseResult.Inserted)
                _navigationManager.NavigateTo("ProjectOverview/" + true, true);
            if (result == DataBaseResult.Failed)
                _navigationManager.NavigateTo("/projectdetailedit/" + Project.Id + "/" + true, true);

            _navigationManager.NavigateTo("/projectdetail/" + Project?.Id);
            
        }

        /// <summary>
        /// opens modal for adding a project activity
        /// </summary>
        private void ModalAddActivity()
        {
            AddActivityShow = !AddActivityShow;
            _activityModel = new ActivityModel();
        }

        /// <summary>
        /// opens modal for updating a project activity
        /// </summary>
        /// <param name="projectActivity">updated project activity</param>
        private void ModalChangeActivity(ProjectActivity projectActivity)
        {
            ProjectActivityInput = projectActivity;
            AddActivityChange = !AddActivityChange;
        }

        /// <summary>
        /// adds project activity and updates view
        /// </summary>
        private void AddActivity()
        {
            if (_activityModel != null && ProjectCopy != null)
            {
                _activitiesCopy.Add(new ProjectActivity(_activityModel.Name));
            }

            CloseActivityModal();
        }

        /// <summary>
        /// adds purpose and updates view
        /// </summary>
        private void AddPurpose()
        {
            if (_purposeModel != null && ProjectCopy != null)
            {
                ProjectCopy.ProjectPurposes.Add(_purposeModel.Name);
            }

            ClosePurposeModal();
        }

        /// <summary>
        /// removes project activity and updates view
        /// </summary>
        /// <param name="projectActivity">removed project activity</param>
        private void RemoveActivity(ProjectActivity projectActivity)
        {
            _activitiesCopy.Remove(projectActivity);
        }

        /// <summary>
        /// removes purpose and updates view
        /// </summary>
        /// <param name="purpose">removed purpose</param>
        private void RemovePurpose(string purpose)
        {
            ProjectCopy?.ProjectPurposes.Remove(purpose);
        }

        /// <summary>
        /// close Modal and reset Parameters
        /// </summary>
        private void CloseActivityModal()
        {
            AddActivityShow = false;
            AddActivityChange = false;
            _activityModel = null;
        }

        /// <summary>
        /// updates a project activity and updates view
        /// </summary>
        private void ChangeActivity()
        {
            foreach (var activity in _activitiesCopy)
            {
                if (activity.Id.Equals(ProjectActivityInput.Id))
                {
                    if (_activityModel != null)
                    {
                        activity.Description = _activityModel.Name;
                    }
                }
            }

            CloseActivityModal();
        }

        /// <summary>
        /// close Modal and reset Parameters
        /// </summary>
        private void ClosePurposeModal()
        {
            AddPurposeShow = false;
            _purposeModel = null;
        }

        /// <summary>
        /// opens modal for adding a purpose
        /// </summary>
        private void ModalAddPurpose()
        {
            AddPurposeShow = true;
            _purposeModel = new PurposeModel();
        }
    }
}