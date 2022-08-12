using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Components;
using XCV.Entities;
using XCV.Services;

namespace XCV.Data
{
    /// <inheritdoc />
    public class ProjectActivityService : IProjectActivityService
    {
        [Inject] private DatabaseUtils DatabaseUtils { get; set; }
        /// <summary>
        /// Creates new Instance of ProjectActivityService
        /// </summary>
        /// <param name="databaseUtils"></param>
        public ProjectActivityService(DatabaseUtils databaseUtils)
        {
            DatabaseUtils = databaseUtils;
        }

        /// <inheritdoc />
        public async Task<List<ProjectActivity>> GetAllProjectActivities(Guid projectId)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            List<ProjectActivity> projectActivities = new List<ProjectActivity>();
            var projectActivityIds = await connection.QueryAsync<Guid>(
                "Select Id from ProjectActivity where Project_Id = @project_Id", new {project_Id = projectId});
            foreach (var projectActivityId in projectActivityIds)
            {
                var activity = await GetProjectActivity(projectActivityId);
                if (activity != null)
                {
                    projectActivities.Add(activity);
                }
            }

            return projectActivities;
            
        }

        /// <inheritdoc />
        public async Task<ProjectActivity?> GetProjectActivity(Guid projectActivityId)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            ProjectActivity? projectActivity = null;
            var result = await connection.QueryAsync<ProjectActivity, Guid?, ProjectActivity>(
                "Select p.Id, p.Description, pe.Employee_Id from ProjectActivity p left outer join ProjectActivities_Employee pe on p.Id = pe.ProjectActivity_Id where p.Id = @id",
                (projectActivityTemp, employeeId) =>
                {
                    projectActivity ??= projectActivityTemp;

                    if (employeeId.HasValue)
                    {
                        projectActivity.AddEmployee(employeeId.Value);
                    }

                    return projectActivity;
                }, new {id = projectActivityId},
                splitOn: "Employee_Id");

            return result.FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<bool> UpdateProjectActivity(ProjectActivity projectActivity,
            Guid projectId)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            var result = await connection.QueryAsync<Guid>("Select Id from ProjectActivity where Id = @id",
                new {id = projectActivity.Id});
            if (result != null && result.Any())
            {
                await connection.ExecuteAsync("Update ProjectActivity Set Description = @description where Id = @id",
                    new {description = projectActivity.Description, id = projectActivity.Id});
                var employeeIds = await connection.QueryAsync<Guid?>(
                    "Select Employee_Id from ProjectActivities_Employee where Project_Id = @project_Id and ProjectActivity_Id = @projectActivity_Id",
                    new {project_Id = projectId, projectActivity_Id = projectActivity.Id});
                var enumerable = employeeIds.ToList();
                foreach (var ids in enumerable)
                {
                    if (!ids.HasValue) continue;
                    if (!projectActivity.GetEmployeeIds().Contains(ids.Value))
                    {
                        await connection.ExecuteAsync(
                            "Delete from ProjectActivities_Employee where Project_Id = @project_Id and ProjectActivity_Id = @projectActivity_Id and Employee_Id = @employee_Id",
                            new {project_Id = projectId, projectActivity_Id = projectActivity.Id, employee_Id = (Guid) ids});
                    }
                }

                foreach (var ids in projectActivity.GetEmployeeIds().Where(ids => !enumerable.Contains(ids)))
                {
                    await connection.ExecuteAsync(
                        "Insert into ProjectActivities_Employee values (@project_Id, @projectActivity_Id, @employee_Id)",
                        new {project_Id = projectId, projectActivity_Id = projectActivity.Id, employee_Id = ids});
                }

                return true;
            }

            await connection.ExecuteAsync(
                "Insert into ProjectActivity values (@id, @ActivityDescription, @project_Id)",
                new
                {
                    id = projectActivity.Id, ActivityDescription = projectActivity.Description,
                    project_Id = projectId
                });
            foreach (var employeeIds in projectActivity.GetEmployeeIds())
            {
                await connection.ExecuteAsync(
                    "Insert into ProjectActivities_Employee values (@project_Id, @projectActivity_Id, @employee_Id)",
                    new
                    {
                        project_Id = projectId, projectActivity_Id = projectActivity.Id,
                        employee_Id = employeeIds
                    });
            }

            return false;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteProjectActivity(Guid projectActivityId)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            return (await connection.ExecuteAsync("Delete from ProjectActivity where Id = @id",
                new {id = projectActivityId})) > 0;
        }
    }
}