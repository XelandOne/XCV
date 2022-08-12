using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using XCV.Data;
using XCV.Entities;
using XCV.Entities.Enums;

namespace XCV.Services
{
    /// <summary>
    /// Connection between Database-Services and UI
    /// Scoped service to save Projects data
    /// Contains all functions for getting, updating, editing or deleting project data
    /// All insert and update functions return a DataBaseResult enum,
    /// so the UI can react appropriately to synchronization errors
    /// </summary>
    public class ProjectManager
    {
        [Inject] private IProjectService ProjectService { get; set; }
        [Inject] private IProjectActivityService ProjectActivityService { get; set; }

        public List<Project> Projects { get; private set; } = new();

        private bool _loaded;

        /// <summary>
        /// Initializes the injected service
        /// </summary>
        /// <param name="projectService"></param>
        /// <param name="projectActivityService"></param>
        public ProjectManager(IProjectService projectService, IProjectActivityService projectActivityService)
        {
            ProjectService = projectService;
            ProjectActivityService = projectActivityService;
        }

        /// <summary>
        /// needs to be called in the OnInitializedAsync() function
        /// </summary>
        /// <returns>true if there is at least one employee in the database</returns>
        public async Task<bool> Load()
        {
            if (_loaded) return _loaded;
            Projects = await ProjectService.GetAllProjects();
            if (Projects.Count > 0) _loaded = true;
            Projects = Projects.OrderByDescending(x => x.LastChanged).ToList();
            return _loaded;
        }

        /// <summary>
        /// updates the title of an project and updates the database
        /// </summary>
        /// <param name="project"></param>
        /// <param name="newTitle"></param>
        /// <returns>updated if added into the database, failed or inserted otherwise</returns>
        public async Task<DataBaseResult> UpdateProjectTitle(Project project, string newTitle)
        {
            var projectNew = Projects.Find(x => x.Id.Equals(project.Id));
            if (projectNew == null) return DataBaseResult.Failed;
            projectNew.Title = newTitle;
            var (dateTime, dataBaseResult) = await ProjectService.UpdateProject(projectNew);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            if (dataBaseResult == DataBaseResult.Inserted)
            {
                await ProjectService.DeleteProject(projectNew.Id);
                return DataBaseResult.Inserted;
            }

            projectNew.LastChanged = dateTime;
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// get the project from the Projects list with projectId
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns>Project</returns>
        public async Task<Project?> GetProject(Guid projectId)
        {
            return await ProjectService.GetProject(projectId);
        }

        /// <summary>
        /// update the Field of an project and updates the database
        /// </summary>
        /// <param name="project"></param>
        /// <param name="field"></param>
        /// <returns>updated if added into the database, failed or inserted otherwise</returns>
        public async Task<DataBaseResult> UpdateField(Project project, Field field)
        {
            var projectNew = Projects.Find(x => x.Id.Equals(project.Id));
            if (projectNew == null) return DataBaseResult.Failed;
            projectNew.Field = field;
            var (dateTime, dataBaseResult) = await ProjectService.UpdateProject(projectNew);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            if (dataBaseResult == DataBaseResult.Inserted)
            {
                await ProjectService.DeleteProject(projectNew.Id);
                return DataBaseResult.Inserted;
            }

            projectNew.LastChanged = dateTime;
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// update the start and end date of an project and updates the database
        /// </summary>
        /// <param name="project"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns>updated if added into the database, failed or inserted otherwise</returns>
        public async Task<DataBaseResult> UpdateProjectTimeSpan(Project project, DateTime startDate, DateTime endDate)
        {
            var projectNew = Projects.Find(x => x.Id.Equals(project.Id));
            if (projectNew == null) return DataBaseResult.Failed;
            projectNew.StartDate = startDate;
            projectNew.EndDate = endDate;
            var (dateTime, dataBaseResult) = await ProjectService.UpdateProject(projectNew);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            if (dataBaseResult == DataBaseResult.Inserted)
            {
                await ProjectService.DeleteProject(projectNew.Id);
                return DataBaseResult.Inserted;
            }

            projectNew.LastChanged = dateTime;
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// updates the description of an project and updates the database
        /// </summary>
        /// <param name="project"></param>
        /// <param name="description"></param>
        /// /// <returns>updated if added into the database, failed or inserted otherwise</returns>
        public async Task<DataBaseResult> UpdateProjectDescription(Project project, string description)
        {
            var projectNew = Projects.Find(x => x.Id.Equals(project.Id));
            if (projectNew == null) return DataBaseResult.Failed;
            projectNew.ProjectDescription = description;
            var (dateTime, dataBaseResult) = await ProjectService.UpdateProject(projectNew);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            if (dataBaseResult == DataBaseResult.Inserted)
            {
                await ProjectService.DeleteProject(projectNew.Id);
                return DataBaseResult.Inserted;
            }

            projectNew.LastChanged = dateTime;
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// Updates an project.
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        /// /// <returns>updated if added into the database, failed or inserted otherwise</returns>
        public async Task<DataBaseResult> UpdateProject(Project project)
        {
            var oldProject = Projects.Find(x => x.Id.Equals(project.Id));
            if (oldProject == null) return DataBaseResult.Failed;
            Projects.Remove(oldProject);
            Projects.Add(project);
            
            var (lastChanged, dataBaseResult) = await ProjectService.UpdateProject(project);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            if (dataBaseResult == DataBaseResult.Inserted)
            {
                await ProjectService.DeleteProject(project.Id);
                return DataBaseResult.Inserted;
            }
            project.LastChanged = lastChanged;
            return DataBaseResult.Updated;
        }
        
        /// <summary>
        /// adds a new project to the project property and inserts it into the database
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        /// <returns>inserted if added into the database, failed or updated otherwise</returns>
        public async Task<DataBaseResult> AddNewProject(Project project)
        {
            if (Projects.Contains(project))
                Projects.Remove(project);
            Projects.Add(project);
            Projects = Projects.OrderByDescending(x => x.LastChanged).ToList();
            var (dateTime, dataBaseResult) = await ProjectService.UpdateProject(project);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            project.LastChanged = dateTime;
            return DataBaseResult.Inserted;
        }

        /// <summary>
        /// removes a project from the Project property and the database
        /// </summary>
        /// <param name="project"></param>
        public async Task DeleteProjects(Project project)
        {
            Projects.Remove(project);
            await ProjectService.DeleteProject(project.Id);
        }

        /// <summary>
        /// returns a list of projectId with their names
        /// should be used in employeeProfiles
        /// </summary>
        /// <param name="projectIds"></param>
        /// <returns>List(Guid, string) of project from the param projectIds list</returns>
        public async Task<List<(Guid, string)>> GetProjectNames(List<Guid> projectIds)
        {
            return await ProjectService.GetProjectNames(projectIds);
        }

        /// <summary>
        /// returns a list of all project ids with their names.
        /// </summary>
        /// <returns>List(Guid, string) of all the project names</returns>
        public async Task<List<(Guid, string)>> GetAllProjectNames()
        {
            return await ProjectService.GetAllProjectNames();
        }

        /// <summary>
        /// get a list of the projects projectActivities
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns>List(ProjectActivity) get all the projectActivities from the Project with projectId</returns>
        public async Task<List<ProjectActivity>> GetProjectActivities(Guid projectId)
        {
            return await ProjectActivityService.GetAllProjectActivities(projectId);
        }
        
        /// <summary>
        /// get a list of the projects projectActivities
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns>List(string) of all the project purposes from a project with projectId</returns>
        public List<string> GetProjectPurposes(Guid projectId)
        {
            return Projects.Find(x => x.Id == projectId)?.ProjectPurposes ?? new List<string>();
        }

        /// <summary>
        /// Updates a ProjectActivity
        /// </summary>
        /// <param name="projectActivity"></param>
        /// <param name="projectId"></param>
        /// <returns>updated if added into the database, failed or inserted otherwise</returns>
        public async Task<DataBaseResult> UpdateProjectActivity(ProjectActivity projectActivity, Guid projectId)
        {
            var project = Projects.Find(x => x.Id.Equals(projectId)) ?? await ProjectService.GetProject(projectId);
            if (project == null) return DataBaseResult.Failed;
            if (project.ProjectActivities.Contains(projectActivity))
                project.ProjectActivities.Remove(projectActivity);
            project.ProjectActivities.Add(projectActivity);

            var (dateTime, dataBaseResult) = await ProjectService.UpdateProject(project);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            if (dataBaseResult == DataBaseResult.Inserted)
            {
                await ProjectService.DeleteProject(project.Id);
                return DataBaseResult.Inserted;
            }

            project.LastChanged = dateTime;
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// Deletes a ProjectActivity
        /// </summary>
        /// <param name="project"></param>
        /// <param name="projectActivity"></param>
        /// <returns>updated if added into the database, failed or inserted otherwise</returns>
        public async Task<DataBaseResult> DeleteProjectActivity(ProjectActivity projectActivity, Project project)
        {
            var projectNew = Projects.Find(x => x.Id.Equals(project.Id));
            if (projectNew == null) return DataBaseResult.Failed;
            projectNew.ProjectActivities.Remove(projectActivity);
            var (dateTime, dataBaseResult) = await ProjectService.UpdateProject(projectNew);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            if (dataBaseResult == DataBaseResult.Inserted)
            {
                await ProjectService.DeleteProject(project.Id);
                return DataBaseResult.Inserted;
            }

            projectNew.LastChanged = dateTime;
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// adds a project purpose
        /// </summary>
        /// <param name="project"></param>
        /// <param name="purpose"></param>
        /// <returns>updated if added into the database, failed or inserted otherwise</returns>
        public async Task<DataBaseResult> AddProjectPurpose(Project project, string purpose)
        {
            var projectNew = Projects.Find(x => x.Id.Equals(project.Id));
            if (projectNew == null) return DataBaseResult.Failed;
            if (!projectNew.ProjectPurposes.Contains(purpose))
                projectNew.ProjectPurposes.Add(purpose);
            var (dateTime, dataBaseResult) = await ProjectService.UpdateProject(projectNew);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            if (dataBaseResult == DataBaseResult.Inserted)
            {
                await ProjectService.DeleteProject(projectNew.Id);
                return DataBaseResult.Inserted;
            }

            projectNew.LastChanged = dateTime;
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// Deletes a project purpose
        /// </summary>
        /// <param name="project"></param>
        /// <param name="purpose"></param>
        /// <returns>updated if added into the database, failed or inserted otherwise</returns>
        public async Task<DataBaseResult> RemoveProjectPurpose(Project project, string purpose)
        {
            var projectNew = Projects.Find(x => x.Id.Equals(project.Id));
            if (projectNew == null) return DataBaseResult.Failed;
            projectNew.ProjectPurposes.Remove(purpose);
            var (dateTime, dataBaseResult) = await ProjectService.UpdateProject(projectNew);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            if (dataBaseResult == DataBaseResult.Inserted)
            {
                await ProjectService.DeleteProject(projectNew.Id);
                return DataBaseResult.Inserted;
            }

            projectNew.LastChanged = dateTime;
            return DataBaseResult.Updated;
        }
    }
}