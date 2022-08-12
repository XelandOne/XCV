using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Components;
using XCV.Entities;
using XCV.Entities.Enums;
using XCV.Services;

namespace XCV.Data
{
    /// <inheritdoc />
    public class ProjectService : IProjectService
    {
        [Inject] private IProjectActivityService ProjectActivityService { get; set; }

        [Inject] private DatabaseUtils DatabaseUtils { get; set; }

        /// <summary>
        /// Create new Instance of ProjectService
        /// </summary>
        /// <param name="databaseUtils"></param>
        public ProjectService(DatabaseUtils databaseUtils)
        {
            DatabaseUtils = databaseUtils;
            ProjectActivityService = new ProjectActivityService(DatabaseUtils);
        }

        /// <inheritdoc />
        public async Task<List<Project>> GetAllProjects()
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            List<Project> projects = new List<Project>();
            var projectIds = await connection.QueryAsync<Guid>("Select Id from Project");
            foreach (var projectId in projectIds)
            {
                var result = await connection.QueryAsync<Project, Field?, Project>(
                    "Select p.Id, p.Title, p.StartDate, p.EndDate, p.ProjectDescription, p.LastChanged, f.Id, f.FieldName as Name, f.LastChanged from (Project p Left outer join Field f on p.Field_Id = f.Id) where p.Id = @id",
                    (project, field) =>
                    {
                        if (field == null) return project;
                        project.Field = field;

                        return project;
                    }, new {id = projectId},
                    splitOn: "Id");
                if (result != null)
                {
                    projects.Add(result.First());
                }
            }

            foreach (var project in projects)
            {
                var projectActivities = await connection.QueryAsync<Guid>(
                    "Select Id from ProjectActivity where Project_Id = @projectId", new {projectId = project.Id}
                );
                foreach (var projectActivity in projectActivities)
                {
                    var activity = await ProjectActivityService.GetProjectActivity(projectActivity);
                    if (activity != null)
                    {
                        project.ProjectActivities.Add(activity);
                    }
                }

                var purposes = await connection.QueryAsync<string?>(
                    "Select Purpose from Project_Purpose where Project_Id = @project_Id",
                    new {project_Id = project.Id});
                foreach (var purpose in purposes)
                {
                    if (purpose != null) project.ProjectPurposes.Add(purpose);
                }
            }

            return projects;
        }

        /// <inheritdoc />
        public async Task<Project?> GetProject(Guid projectId)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            var result = await connection.QueryAsync<Project, Field?, Project>(
                "Select p.Id, p.Title, p.StartDate, p.EndDate, p.ProjectDescription, p.LastChanged, f.Id, f.FieldName as Name from Project p Left outer join Field f on p.Field_Id = f.Id where p.Id = @id",
                (project, field) =>
                {
                    if (field == null) return project;
                    project.Field = field;
                    return project;
                }, new {id = projectId},
                splitOn: "Id");
            var projectReady = result.FirstOrDefault();
            if (projectReady == null) return projectReady;
            var projectActivities = await connection.QueryAsync<Guid>(
                "Select Id from ProjectActivity where Project_Id = @project_Id", new {project_Id = projectId}
            );
            foreach (var projectActivity in projectActivities)
            {
                var activity = await ProjectActivityService.GetProjectActivity(projectActivity);
                if (activity != null)
                {
                    projectReady.ProjectActivities.Add(activity);
                }
            }

            var purposes = await connection.QueryAsync<string?>(
                "Select Purpose from Project_Purpose where Project_Id = @project_Id",
                new {project_Id = projectId});
            foreach (var purpose in purposes)
            {
                if (purpose != null) projectReady.ProjectPurposes.Add(purpose);
            }

            return projectReady;
        }

        /// <inheritdoc />
        public async Task<List<(Guid, string)>> GetProjectNames(List<Guid> projectIds)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);

            IEnumerable<(Guid, string)> result = await connection.QueryAsync<(Guid, string)>(
                "Select Id, Title from Project where Id IN @ids",
                new {ids = projectIds});

            return result.ToList();
        }

        /// <inheritdoc />
        public async Task<List<(Guid, string)>> GetAllProjectNames()
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);

            var result = await connection.QueryAsync<(Guid, string)>("Select Id, Title from Project");

            return result.ToList();
        }

        /// <inheritdoc />
        public async Task<(DateTime?, DataBaseResult)> UpdateProject(Project project)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            DateTime? lastChanged;
            var result =
                await connection.QueryAsync("Select Id from Project where Id = @id", new {id = project.Id});
            if (result != null && result.Any())
            {
                //Only here so DumyData can be updated
                if (project.LastChanged == null)
                {
                    var cache = await connection.QueryFirstOrDefaultAsync<DateTime>(
                        "SELECT LastChanged from Project where Id = @id",
                        new {id = project.Id});
                    project.LastChanged = cache;
                }

                lastChanged = await connection.QueryFirstOrDefaultAsync<DateTime>(
                    "If (SELECT LastChanged FROM Project where Id = @id) = @lastChanged " +
                    "Begin " +
                    "Update Project " +
                    "Set Title = @title, Field_Id = @field_Id, StartDate = @startDate, EndDate = @endDate, ProjectDescription = @projectDescription, LastChanged = CURRENT_TIMESTAMP " +
                    "where Id = @id " +
                    "Select LastChanged from Project where Id = @id " +
                    "END",
                    new
                    {
                        title = project.Title,
                        field_Id = project.Field?.Id,
                        startDate = project.StartDate,
                        endDate = project.EndDate,
                        projectDescription = project.ProjectDescription,
                        id = project.Id,
                        lastChanged = project.LastChanged
                    }
                );
                if (lastChanged == new DateTime()) return (null, DataBaseResult.Failed);
                var projectActivityIds =
                    await connection.QueryAsync<Guid>("Select Id from ProjectActivity where Project_Id = @id",
                        new {id = project.Id});
                List<ProjectActivity> projectActivities = new();
                foreach (var ids in projectActivityIds)
                {
                    var activity = await ProjectActivityService.GetProjectActivity(ids);
                    if (activity != null)
                    {
                        projectActivities.Add(activity);
                    }
                }

                foreach (var projectActivity in projectActivities.Where(projectActivity =>
                    !project.ProjectActivities.Contains(projectActivity)))
                {
                    await ProjectActivityService.DeleteProjectActivity(projectActivity.Id);
                }

                foreach (var projectActivity in project.ProjectActivities)
                {
                    await ProjectActivityService.UpdateProjectActivity(projectActivity, project.Id);
                }

                var purposes = await connection.QueryAsync<string>(
                    "Select Purpose from Project_Purpose where Project_Id = @project_Id",
                    new {project_Id = project.Id});

                var purposeList = purposes.ToList();
                foreach (var purpose in purposeList.Where(purpose => !project.ProjectPurposes.Contains(purpose)))
                {
                    await connection.ExecuteAsync(
                        "Delete from Project_Purpose where Project_Id = @project_Id and Purpose = @tempPurpose",
                        new {project_Id = project.Id, tempPurpose = purpose});
                }

                foreach (var purpose in project.ProjectPurposes.Where(purpose => !purposeList.Contains(purpose)))
                {
                    await connection.ExecuteAsync("Insert into Project_Purpose values (@project_Id, @tempPurpose)",
                        new {project_Id = project.Id, tempPurpose = purpose});
                }

                await connection.ExecuteAsync("Delete from Employee_Project where Project_Id = @projectId",
                    new {projectId = project.Id});
                
                HashSet<Guid> employeesAdded = new();
                foreach (var activity in project.ProjectActivities)
                {
                    foreach (var tempEmp in activity.GetEmployeeIds())
                    {
                        if (employeesAdded.Add(tempEmp))
                        {
                            await connection.ExecuteAsync("Insert into Employee_Project values (@empId, @project_Id)",
                                new {empId = tempEmp, project_Id = project.Id});
                        }
                    }
                }

                return (lastChanged, DataBaseResult.Updated);
            }

            lastChanged = await connection.QueryFirstOrDefaultAsync<DateTime>(
                "Insert into Project values (@id, @title, @field_Id, @startDate, @endDate, @projectDescription, CURRENT_TIMESTAMP) " +
                "Select LastChanged from Project where Id = @id", new
                {
                    id = project.Id,
                    Field_id = project.Field?.Id,
                    title = project.Title,
                    startDate = project.StartDate,
                    endDate = project.EndDate,
                    projectDescription = project.ProjectDescription
                });

            foreach (var projectActivity in project.ProjectActivities)
            {
                await ProjectActivityService.UpdateProjectActivity(projectActivity, project.Id);
            }

            foreach (var purpose in project.ProjectPurposes)
            {
                await connection.ExecuteAsync("Insert into Project_Purpose values (@project_Id, @tempPurpose)",
                    new {project_Id = project.Id, tempPurpose = purpose});
            }

            foreach (var activity in project.ProjectActivities)
            {
                foreach (var tempEmp in activity.GetEmployeeIds())
                {
                    var res = await connection.QueryAsync<Guid>(
                        "Select Employee_Id from Employee_Project where Employee_Id = @empId and Project_Id = @project_Id",
                        new {empId = tempEmp, project_Id = project.Id});
                    if (!res.Any())
                    {
                        await connection.ExecuteAsync("Insert into Employee_Project values (@empId, @project_Id)",
                            new {empId = tempEmp, project_Id = project.Id});
                    }
                }
            }

            return (lastChanged, DataBaseResult.Inserted);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteProject(Guid projectId)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            return (await connection.ExecuteAsync(
                "Begin Transaction " +
                "If exists (Select * from Project p left outer join ShownEmployeeProperty_Project sh on p.Id = sh.Project_Id where p.Id = @id) " +
                "Begin Update ShownEmployeeProperty set LastChanged = CURRENT_TIMESTAMP where Id in " +
                "(Select ShownEmployeeProperty_Id from ShownEmployeeProperty_Project where Project_Id = @id) End " +
                "If exists (Select * from Project p left outer join Employee_Project e on p.Id = e.Project_Id where p.Id = @id) " +
                "Begin Update Employee set LastChanged = CURRENT_TIMESTAMP where Id in " +
                "(Select Employee_Id from Employee_Project where Project_Id = @id) END " +
                "Delete from Project where Id = @id " +
                "commit", new {id = projectId}
            )) > 0;
        }
    }
}