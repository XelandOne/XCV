using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using XCV.Entities;

namespace XCV.Pages
{
    public partial class ProjectOverview
    {
        private bool _showCommitDelete;
        private Project _current = new Project();
        private Guid? FieldId { get; set; }
        private List<Project> Projects { get; set; } = new();
        private bool _showAddProject { get; set; }
        /// <summary>
        /// Boolean value set true if someone deleted a project at the same time as someone else.
        /// </summary>
        [Parameter] public bool Failed { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await _projectManager.Load();
            await _experienceManager.Load();
            Projects = _projectManager.Projects;
            Projects.Sort();
        }
        
        /// <summary>
        /// If failed is true an someone else deleted the project und another one wants also to delete it then it shows a message and goes back to projectOverview.
        /// </summary>
        protected override async Task OnParametersSetAsync()
        {
            if (Failed)
            {
                await Task.Delay(3000);
                Failed = false;
                _navigationManager.NavigateTo("projectOverview/");
            }
        }

        /// <summary>
        /// adds Project and updates view
        /// </summary>
        private async Task AddProject()
        {
            if(FieldId == null) return;
            await _experienceManager.Load();
            
            _current.Field = _experienceManager.Fields.Find(f => f.Id == FieldId);
            Projects = _projectManager.Projects;
            await _projectManager.AddNewProject(_current);
            _showAddProject = false;
        }

        /// <summary>
        /// removes Project and updates view
        /// </summary>
        /// <param name="project">deleted project</param>
        private void DeleteProject(Project project)
        {
            _current = project;
            _showCommitDelete = true;
        }
        
        /// <summary>
        /// opens modal for adding a new Project
        /// </summary>
        private void ModalAddProject()
        {
            _current = new Project();
            _showAddProject = !_showAddProject;
        }
        
        /// <summary>
        /// close Modals and reset Parameters
        /// </summary>
        private void CloseModal()
        {
            _showAddProject = false;
            _showCommitDelete = false;
            _current = new Project();
        }

        private async Task OnCommitDelete()
        {
            _showCommitDelete = false;
            await _projectManager.DeleteProjects(_current);
            Projects = _projectManager.Projects;
            _current = new Project();
        }
    }
}