using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using XCV.Data;
using XCV.Entities;
using XCV.Entities.Enums;

namespace XCV.Services
{
    /// <summary>
    /// Connection between Database-Services and UI
    /// Scoped service to save Employee data
    /// Contains all functions for getting, updating, editing or deleting employee data
    /// All insert and update functions return a DataBaseResult enum,
    /// so the UI can react appropriately to synchronization errors
    /// </summary>
    public class EmployeeManager
    {
        [Inject] private IEmployeeService EmployeeService { get; set; }
        [Inject] private ProjectManager ProjectManager { get; set; }
        /// <summary>
        /// A list with all employees.
        /// </summary>
        public List<Employee> Employees { get; private set; } = new();
        /// <summary>
        /// The currently logged in employee.
        /// </summary>
        public Employee? CurrentEmployee { get; private set; }
        private bool _loaded;

        /// <summary>
        /// Initializes the injected services
        /// </summary>
        /// <param name="employeeService"></param>
        /// <param name="projectManager"></param>
        public EmployeeManager(IEmployeeService employeeService, ProjectManager projectManager)
        {
            EmployeeService = employeeService;
            ProjectManager = projectManager;
        }

        
        /// <summary>
        /// find an Employee in the preloaded Employee list
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns>employee with employeeId</returns>
        public Employee? GetEmployee(Guid employeeId)
        {
            return Employees.Find(x => x.Id.Equals(employeeId));
        }

        /// <summary>
        /// Loads an Individual employee if not already preloaded in the Employee list
        /// </summary>
        /// <param name="employeeId"></param>
        public async Task LoadEmployee(Guid employeeId)
        {
            if (!Employees.Exists(x => x.Id.Equals(employeeId)))
            {
                var temp = await EmployeeService.GetEmployee(employeeId);
                if (temp != null)
                    Employees.Add(temp);
            }
        }

        /// <summary>
        /// gets all the Employees and saves them in the Employees list
        /// </summary>
        /// <returns>true if at least one employee was loaded otherwise returns false</returns>
        public async Task<bool> Load()
        {
            if (_loaded) return _loaded;
            Employees = await EmployeeService.GetAllEmployees();
            if (Employees.Count <= 0) return _loaded;
            _loaded = true;
            Employees.Sort((x, y) => string.Compare(x.FirstName, y.FirstName, StringComparison.Ordinal));
            return _loaded;
        }

        /// <summary>
        /// loads only the ids and names of the employees,
        /// to make it more efficient when only the names are needed
        /// </summary>
        /// <returns>Dictionary(Guid, (string, string)) with all the employees ids, firstnames and surnames</returns>
        public async Task<Dictionary<Guid, (string, string)>> GetAllNames()
        {
            return await EmployeeService.GetAllNames();
        }

        /// <summary>
        /// adds an new employee to the database and employee list
        /// </summary>
        /// <param name="employee"></param>
        /// <returns>inserted if employee was correctly inserted into the database, failed or updated otherwise</returns>
        public async Task<DataBaseResult> AddNewEmployee(Employee employee)
        {
            if (Employees.Contains(employee))
                Employees.Remove(employee);
            var (dateTime, dataBaseResult) = await EmployeeService.UpdateEmployee(employee);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            employee.LastChanged = dateTime;
            Employees.Add(employee);
            Employees.Sort((x, y) => string.Compare(x.FirstName, y.FirstName, StringComparison.Ordinal));
            return DataBaseResult.Inserted;
        }

        /// <summary>
        /// Login of the employee with the given username.
        /// </summary>
        /// <param name="username">The username of the employee</param>
        /// <returns>False if the employee with the given username does not exist</returns>
        public async Task<bool> LoginEmployee(string username)
        {
            CurrentEmployee = await EmployeeService.GetEmployee(username) ??
                              Employees.Find(x => x.UserName.Equals(username));
            //DummyEmployeeLogin(username);
            return CurrentEmployee != null;
        }

        /// <summary>
        /// Performs a refresh of the current employee and returns him.
        /// </summary>
        /// <returns>The refreshed current employee</returns>
        public async Task<Employee?> RefreshLoggedInEmployee()
        {
            if (CurrentEmployee != null)
            {
                CurrentEmployee = await EmployeeService.GetEmployee(CurrentEmployee.Id) ??
                                  Employees.Find(x => x.Id.Equals(CurrentEmployee.Id));
                if (CurrentEmployee != null) UpdateEmployee(CurrentEmployee);
            }

            return CurrentEmployee;
        }

        /// <summary>
        /// Logout of the currently logged in employee.
        /// </summary>
        public void LogoutEmployee()
        {
            CurrentEmployee = null;
        }

        /// <summary>
        /// returns the the names of the employees project
        /// </summary>
        /// <returns>List(Guid, string) with all the project of the currentEmployee</returns>
        public async Task<List<(Guid, string)>?> GetProjectsOfEmployee()
        {
            if (CurrentEmployee?.ProjectIds != null)
                return await ProjectManager.GetProjectNames(CurrentEmployee.ProjectIds);
            return null;
        }

        /// <summary>
        /// returns all project names and ids.
        /// </summary>
        /// <returns>List(Guid, string) of all existing project</returns>
        public async Task<List<(Guid, string)>?> GetAllProjects()
        {
            return await ProjectManager.GetAllProjectNames();
        }

        /// <summary>
        /// returns a list of all the ProjectActivities
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns>List(ProjectActivity) of all existing ProjectActivities</returns>
        public async Task<List<ProjectActivity>?> GetAllProjectActivities(Guid projectId)
        {
            var project = await ProjectManager.GetProject(projectId);
            return project?.ProjectActivities;
        }

        /// <summary>
        /// checks whether the employee has selected an activity
        /// </summary>
        /// <param name="projectActivity"></param>
        /// <returns>true if employee has ProjectActivity, false otherwise</returns>
        public bool CheckEmployeeHasProjectActivities(ProjectActivity projectActivity)
        {
            return CurrentEmployee != null && projectActivity.GetEmployeeIds().Contains(CurrentEmployee.Id);
        }

        /// <summary>
        /// removes the employeeId from the projectActivity employeeId List 
        /// </summary>
        /// <param name="projectActivity"></param>
        /// <param name="projectActivityId"></param>
        /// <param name="projectId"></param>
        /// <returns>updated if removed from the database, failed or inserted otherwise</returns>
        public async Task<DataBaseResult> RemoveProjectActivity(List<ProjectActivity> projectActivity,
            Guid projectActivityId, Guid projectId)
        {
            if (CurrentEmployee == null) return DataBaseResult.Failed;
            var activity = projectActivity.Find(x => x.Id.Equals(projectActivityId));
            if (activity == null) return DataBaseResult.Failed;
            activity.RemoveEmployee(CurrentEmployee.Id);
            var result = await ProjectManager.UpdateProjectActivity(activity, projectId);
            return result == DataBaseResult.Failed ? DataBaseResult.Failed : DataBaseResult.Updated;
        }

        /// <summary>
        /// adds the employeeId to the projectActivity employeeId List 
        /// </summary>
        /// <param name="projectActivity"></param>
        /// <param name="projectActivityId"></param>
        /// <param name="projectId"></param>
        /// <returns>updated if added into the database, failed or inserted otherwise</returns>
        public async Task<DataBaseResult> AddProjectActivity(List<ProjectActivity> projectActivity,
            Guid projectActivityId, Guid projectId)
        {
            if (CurrentEmployee == null) return DataBaseResult.Failed;
            var activity = projectActivity.Find(x => x.Id.Equals(projectActivityId));
            if (activity == null) return DataBaseResult.Failed;
            activity.AddEmployee(CurrentEmployee.Id);
            var result = await ProjectManager.UpdateProjectActivity(activity, projectId);
            return result == DataBaseResult.Failed ? DataBaseResult.Failed : DataBaseResult.Updated;
        }

        /// <summary>
        /// Updates the currentEmployee in the Employee List
        /// </summary>
        /// <param name="newEmployee"></param>
        private void UpdateEmployee(Employee newEmployee)
        {
            var oldEmployee = Employees.Find(x => x.Id.Equals(newEmployee.Id));
            if (oldEmployee != null)
                Employees.Remove(oldEmployee);
            Employees.Add(newEmployee);
        }

        /// <summary>
        /// Update the firstName of an Employee
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="surName"></param>
        /// <returns>update if added into the database, failed or inserted otherwise</returns>
        public async Task<DataBaseResult> UpdateName(string firstName, string surName)
        {
            if (CurrentEmployee == null) return DataBaseResult.Failed;
            var copy = new Employee(CurrentEmployee);

            CurrentEmployee.FirstName = firstName;
            CurrentEmployee.SurName = surName;

            var (dateTime, dataBaseResult) = await EmployeeService.UpdateEmployee(CurrentEmployee);
            if (dataBaseResult == DataBaseResult.Failed)
            {
                CurrentEmployee = copy;
                return DataBaseResult.Failed;
            }

            CurrentEmployee.LastChanged = dateTime;
            UpdateEmployee(CurrentEmployee);
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// Updates the four values for the RelevantWorkExperience for an Employee
        /// </summary>
        /// <param name="employedSince"></param>
        /// <param name="workExperience"></param>
        /// <param name="scientificAssistant"></param>
        /// <param name="studentAssistant"></param>
        /// <returns>updated if added into the database, failed or inserted otherwise</returns>
        public async Task<DataBaseResult> UpdateRelevantWorkExperience(DateTime employedSince, int workExperience,
            int scientificAssistant, int studentAssistant)
        {
            if (CurrentEmployee == null) return DataBaseResult.Failed;

            var copy = new Employee(CurrentEmployee);
            CurrentEmployee.EmployedSince = employedSince;
            CurrentEmployee.WorkExperience = workExperience;
            CurrentEmployee.ScientificAssistant = scientificAssistant;
            CurrentEmployee.StudentAssistant = studentAssistant;

            var (dateTime, dataBaseResult) = await EmployeeService.UpdateEmployee(CurrentEmployee);
            if (dataBaseResult == DataBaseResult.Failed)
            {
                CurrentEmployee = copy;
                return DataBaseResult.Failed;
            }

            CurrentEmployee.LastChanged = dateTime;
            UpdateEmployee(CurrentEmployee);
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// Update the RateCardLevel of an Employee
        /// </summary>
        /// <param name="rateCardLevel"></param>
        /// <returns>updated if added into the database, failed or inserted otherwise</returns>
        public async Task<DataBaseResult> UpdateRateCardLevel(RateCardLevel rateCardLevel)
        {
            if (CurrentEmployee == null) return DataBaseResult.Failed;
            var copy = new Employee(CurrentEmployee);
            CurrentEmployee.RateCardLevel = rateCardLevel;
            var (dateTime, dataBaseResult) = await EmployeeService.UpdateEmployee(CurrentEmployee);
            if (dataBaseResult == DataBaseResult.Failed)
            {
                CurrentEmployee = copy;
                return DataBaseResult.Failed;
            }

            CurrentEmployee.LastChanged = dateTime;
            UpdateEmployee(CurrentEmployee);
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// Update the ProfilePicture of an Employee
        /// </summary>
        /// <param name="profilePicture"></param>
        /// <returns>updated if added into the database, failed or inserted otherwise</returns>
        public async Task<DataBaseResult> UpdateProfilePicture(byte[] profilePicture)
        {
            if (CurrentEmployee == null) return DataBaseResult.Failed;
            var copy = new Employee(CurrentEmployee);
            CurrentEmployee.ProfilePicture = profilePicture;
            var (dateTime, dataBaseResult) = await EmployeeService.UpdateEmployee(CurrentEmployee);
            if (dataBaseResult == DataBaseResult.Failed)
            {
                CurrentEmployee = copy;
                return DataBaseResult.Failed;
            }

            CurrentEmployee.LastChanged = dateTime;
            UpdateEmployee(CurrentEmployee);
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// adds a projectId to the currentEmployee
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns>updated if added into the database, failed or inserted otherwise</returns>
        public async Task<DataBaseResult> AddProjectIdToCurrentEmployee(Guid projectId)
        {
            if (CurrentEmployee == null) return DataBaseResult.Failed;
            if (CurrentEmployee.ProjectIds.Contains(projectId)) return DataBaseResult.Failed;
            var copy = new Employee(CurrentEmployee);
            CurrentEmployee.ProjectIds.Add(projectId);
            var (dateTime, dataBaseResult) = await EmployeeService.UpdateEmployee(CurrentEmployee);
            if (dataBaseResult == DataBaseResult.Failed)
            {
                CurrentEmployee = copy;
                return DataBaseResult.Failed;
            }

            CurrentEmployee.LastChanged = dateTime;
            UpdateEmployee(CurrentEmployee);
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// removes an projectId form the currentEmployee
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns>updated if added into the database, failed or inserted otherwise</returns>
        public async Task<DataBaseResult> RemoveProjectIdFromCurrentEmployee(Guid projectId)
        {
            if (CurrentEmployee == null) return DataBaseResult.Failed;
            var copy = new Employee(CurrentEmployee);
            CurrentEmployee.ProjectIds.Remove(projectId);
            var (dateTime, dataBaseResult) = await EmployeeService.UpdateEmployee(CurrentEmployee);
            if (dataBaseResult == DataBaseResult.Failed)
            {
                CurrentEmployee = copy;
                return DataBaseResult.Failed;
            }

            CurrentEmployee.LastChanged = dateTime;
            UpdateEmployee(CurrentEmployee);
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// removes an Experience from the Employee
        /// </summary>
        /// <param name="experience"></param>
        /// <returns>updated if added into the database, failed or inserted otherwise</returns>
        public async Task<DataBaseResult> RemoveExperience(Experience experience)
        {
            if (CurrentEmployee == null) return DataBaseResult.Failed;
            var copy = new Employee(CurrentEmployee);
            switch (experience)
            {
                case Role role:
                    CurrentEmployee.Experience.Roles.Remove(role);
                    break;
                case Field field:
                    CurrentEmployee.Experience.Fields.Remove(field);
                    break;
                case SoftSkill softSkill:
                    CurrentEmployee.Experience.SoftSkills.Remove(softSkill);
                    break;
                case HardSkill:
                    CurrentEmployee.Experience.HardSkills.Remove(
                        CurrentEmployee.Experience.HardSkills.Find(x => x.Item1.Id.Equals(experience.Id)));
                    break;
                case Language:
                    CurrentEmployee.Experience.Languages.Remove(
                        CurrentEmployee.Experience.Languages.Find(x => x.Item1.Id.Equals(experience.Id)));
                    break;
            }

            var (dateTime, dataBaseResult) = await EmployeeService.UpdateEmployee(CurrentEmployee);
            if (dataBaseResult == DataBaseResult.Failed)
            {
                CurrentEmployee = copy;
                return DataBaseResult.Failed;
            }

            CurrentEmployee.LastChanged = dateTime;
            UpdateEmployee(CurrentEmployee);
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// removes an Experience from the Employee
        /// </summary>
        /// <param name="employee"></param>
        /// <param name="experience"></param>
        public void RemoveExperience(Employee employee, Experience experience)
        {
            switch (experience)
            {
                case Role role:
                    employee.Experience.Roles.Remove(role);
                    break;
                case Field field:
                    employee.Experience.Fields.Remove(field);
                    break;
                case SoftSkill softSkill:
                    employee.Experience.SoftSkills.Remove(softSkill);
                    break;
                case HardSkill:
                    employee.Experience.HardSkills.Remove(
                        employee.Experience.HardSkills.Find(x => x.Item1.Id.Equals(experience.Id)));
                    break;
                case Language:
                    employee.Experience.Languages.Remove(
                        employee.Experience.Languages.Find(x => x.Item1.Id.Equals(experience.Id)));
                    break;
            }
        }

        /// <summary>
        /// adds a Role, Field or SoftSkill to an Employee
        /// </summary>
        /// <param name="experience"></param>
        /// <returns>updated if added into the database, failed or inserted otherwise</returns>
        public async Task<DataBaseResult> AddExperience(Experience experience)
        {
            if (CurrentEmployee == null) return DataBaseResult.Failed;
            var copy = new Employee(CurrentEmployee);
            switch (experience)
            {
                case Role role:
                    if (!CurrentEmployee.Experience.Roles.Contains(role))
                        CurrentEmployee.Experience.Roles.Add(role);
                    break;
                case Field field:
                    if (!CurrentEmployee.Experience.Fields.Contains(field))
                        CurrentEmployee.Experience.Fields.Add(field);
                    break;
                case SoftSkill softSkill:
                    if (!CurrentEmployee.Experience.SoftSkills.Contains(softSkill))
                        CurrentEmployee.Experience.SoftSkills.Add(softSkill);
                    break;
            }

            var (dateTime, dataBaseResult) = await EmployeeService.UpdateEmployee(CurrentEmployee);
            if (dataBaseResult == DataBaseResult.Failed)
            {
                CurrentEmployee = copy;
                return DataBaseResult.Failed;
            }

            CurrentEmployee.LastChanged = dateTime;
            UpdateEmployee(CurrentEmployee);
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// adds a Role, Field or SoftSkill to an Employee
        /// </summary>
        /// <param name="employee"></param>
        /// <param name="experience"></param>
        public void AddExperience(Employee employee, Experience experience)
        {
            switch (experience)
            {
                case Role role:
                    if (!employee.Experience.Roles.Contains(role))
                        employee.Experience.Roles.Add(role);
                    break;
                case Field field:
                    if (!employee.Experience.Fields.Contains(field))
                        employee.Experience.Fields.Add(field);
                    break;
                case SoftSkill softSkill:
                    if (!employee.Experience.SoftSkills.Contains(softSkill))
                        employee.Experience.SoftSkills.Add(softSkill);
                    break;
            }
        }

        /// <summary>
        /// updates a HardSkill with level of an Employee
        /// </summary>
        /// <param name="hardSkill"></param>
        /// <returns>updated if added into the database, failed or inserted otherwise</returns>
        public async Task<DataBaseResult> UpdateHardSkill((HardSkill, HardSkillLevel) hardSkill)
        {
            if (CurrentEmployee == null) return DataBaseResult.Failed;
            var copy = new Employee(CurrentEmployee);
            var oldHardSkillTuple =
                CurrentEmployee.Experience.HardSkills.Find(h => h.Item1.Id.Equals(hardSkill.Item1.Id));
            if (oldHardSkillTuple.Item1 != null!)
            {
                CurrentEmployee.Experience.HardSkills.Remove(oldHardSkillTuple);
            }

            CurrentEmployee.Experience.HardSkills.Add(hardSkill);

            var (dateTime, dataBaseResult) = await EmployeeService.UpdateEmployee(CurrentEmployee);
            if (dataBaseResult == DataBaseResult.Failed)
            {
                CurrentEmployee = copy;
                return DataBaseResult.Failed;
            }

            CurrentEmployee.LastChanged = dateTime;
            UpdateEmployee(CurrentEmployee);
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// updates a HardSkill with level of an Employee
        /// </summary>
        /// <param name="employee"></param>
        /// <param name="hardSkill"></param>
        public void UpdateHardSkill(Employee employee, (HardSkill, HardSkillLevel) hardSkill)
        {
            var oldHardSkillTuple = 
                employee.Experience.HardSkills.Find(h => h.Item1.Id.Equals(hardSkill.Item1.Id));
            if (oldHardSkillTuple.Item1 != null!)
            {
                employee.Experience.HardSkills.Remove(oldHardSkillTuple);
            }

            employee.Experience.HardSkills.Add(hardSkill);
        }

        /// <summary>
        /// updates a Language with level of an Employee
        /// </summary>
        /// <param name="language"></param>
        /// <returns>updated if added into the database, failed or inserted otherwise</returns>
        public async Task<DataBaseResult> UpdateLanguage((Language, LanguageLevel) language)
        {
            if (CurrentEmployee == null) return DataBaseResult.Failed;
            var copy = new Employee(CurrentEmployee);
            var oldLanguageTuple = CurrentEmployee.Experience.Languages.Find(l => l.Item1.Equals(language.Item1));
            if (oldLanguageTuple.Item1 != null!)
            {
                CurrentEmployee.Experience.Languages.Remove(oldLanguageTuple);
            }

            CurrentEmployee.Experience.Languages.Add(language);

            var (dateTime, dataBaseResult) = await EmployeeService.UpdateEmployee(CurrentEmployee);
            if (dataBaseResult == DataBaseResult.Failed)
            {
                CurrentEmployee = copy;
                return DataBaseResult.Failed;
            }

            CurrentEmployee.LastChanged = dateTime;
            UpdateEmployee(CurrentEmployee);
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// updates a Language with level of an Employee
        /// </summary>
        /// <param name="employee"></param>
        /// <param name="language"></param>
        /// <returns>updated if added into the database, failed or inserted otherwise</returns>
        public void UpdateLanguage(Employee employee, (Language, LanguageLevel) language)
        {
            var oldLanguageTuple = employee.Experience.Languages.Find(l => l.Item1.Equals(language.Item1));
            if (oldLanguageTuple.Item1 != null!)
            {
                employee.Experience.Languages.Remove(oldLanguageTuple);
            }

            employee.Experience.Languages.Add(language);
        }

        /// <summary>
        /// Gets all the Employees from the database who work on the project
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns>List(Employee) with all the employees in an project with projectId</returns>
        public async Task<List<Employee>> GetEmployeesInProject(Guid projectId)
        {
            return await EmployeeService.GetEmployeesInProject(projectId);
        }


        /// <summary>
        /// Adds the projectId to the given Employee and to the
        /// EmployeeList in the EmployeeManager if they are loaded, if not,
        /// only updates the Database
        /// </summary>
        /// <param name="employee"></param>
        /// <param name="projectId"></param>
        /// <returns>updated if added into the database, failed or inserted otherwise</returns>
        public async Task<DataBaseResult> AddProjectId(Employee employee, Guid projectId)
        {
            if (employee.ProjectIds.Contains(projectId)) return DataBaseResult.Failed;
            employee.ProjectIds.Add(projectId);
            if (CurrentEmployee != null && CurrentEmployee.Equals(employee))
            {
                return await AddProjectIdToCurrentEmployee(projectId);
            }

            var temp = Employees.Find(x => x.Id.Equals(employee.Id));
            if (temp != null)
            {
                if (temp.ProjectIds.Contains(projectId)) return DataBaseResult.Failed;
                temp.ProjectIds.Add(projectId);
                var (item1, item2) = await EmployeeService.UpdateEmployee(temp);
                if (item2 == DataBaseResult.Failed) return DataBaseResult.Failed;
                temp.LastChanged = item1;
                UpdateEmployee(temp);
                return DataBaseResult.Updated;
            }

            var (dateTime, dataBaseResult) = await EmployeeService.UpdateEmployee(employee);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            employee.LastChanged = dateTime;
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// Deletes the projectId from the given Employee and from the
        /// EmployeeList in the EmployeeManager if they are loaded, if not,
        /// only updates the Database
        /// </summary>
        /// <param name="employee"></param>
        /// <param name="projectId"></param>
        /// <returns>updated if added into the database, failed or inserted otherwise</returns>
        public async Task<DataBaseResult> RemoveProjectId(Employee employee, Guid projectId)
        {
            employee.ProjectIds.Remove(projectId);
            if (CurrentEmployee != null && CurrentEmployee.Equals(employee))
            {
                return await RemoveProjectIdFromCurrentEmployee(projectId);
            }

            var temp = Employees.Find(x => x.Id.Equals(employee.Id));
            if (temp != null)
            {
                temp.ProjectIds.Remove(projectId);
                var (item1, item2) = await EmployeeService.UpdateEmployee(temp);
                if (item2 == DataBaseResult.Failed) return DataBaseResult.Failed;
                temp.LastChanged = item1;
                UpdateEmployee(temp);
                return DataBaseResult.Updated;
            }

            var (dateTime, dataBaseResult) = await EmployeeService.UpdateEmployee(employee);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            employee.LastChanged = dateTime;
            return DataBaseResult.Updated;
        }
    }
}