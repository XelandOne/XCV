using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using XCV.Data;
using XCV.Entities;
using XCV.Entities.Enums;

namespace XCV.Services
{
    /// <summary>
    /// Generate Dummy Data into the Database for tests
    /// </summary>
    public class FillDummyData
    {
        [Inject] private IEmployeeService EmployeeService { get; set; }
        [Inject] private ExperienceManager ExperienceManager { get; set; }
        [Inject] private IProjectService ProjectService { get; set; }
        [Inject] private IOfferService OfferService { get; set; }
        [Inject] private FillExperienceTable FillExperienceTable { get; set; }
        
        private readonly string _pathTo =  Path.Combine(".", "Files", "dummyData.json");
        private readonly JObject _data;
        private readonly List<Project> _projects = new ();
        private readonly List<Employee> _employees = new ();
        private readonly Random _rnd = new();

        public FillDummyData(IEmployeeService employeeService, ExperienceManager experienceManager, IProjectService projectService, IOfferService offerService, FillExperienceTable fillExperienceTable)
        {
            EmployeeService = employeeService;
            ExperienceManager = experienceManager;
            ProjectService = projectService;
            OfferService = offerService;
            FillExperienceTable = fillExperienceTable;
            _data = JObject.Load(new JsonTextReader(new StreamReader(_pathTo)));
        }

        private JToken? GetValue(string dataKey)
        {
            foreach (var (key, value) in _data) if (key.Equals(dataKey)) return value;
            return null;
        }
        
        public async Task GenerateDummyData()
        {
            if (!_data.HasValues) return;
            if (!await ExperienceManager.Load())
                await Task.Run(FillExperienceTable.Fill);
            await ExperienceManager.Load();
            await DummyProjects(GetValue("Projects"));
            await DummyEmployees(GetValue("Employees"));
            await DummyOffers(GetValue("Offers"));
        }
        
        DateTime RandomDay()
        {
            var start = new DateTime(2012, 1, 1);
            var range = (DateTime.Today - start).Days;           
            return start.AddDays(_rnd.Next(range));
        }
        
        private async Task DummyProjects(JToken? projects)
        {
            if (projects == null) return;
            foreach (var project in projects)
            {
                if (!project.HasValues) continue;
                Project temp = new
                (
                    Guid.Parse(project.First!["Id"]!.ToString()),
                    project.First!["Title"]!.ToString(),
                    DateTime.ParseExact(project.First["StartDate"]!.ToString(), "dd/MM/yyyy HH:mm:ss",
                        new CultureInfo("en-US")),
                    DateTime.ParseExact(project.First["EndDate"]!.ToString(), "dd/MM/yyyy HH:mm:ss",
                        new CultureInfo("en-US")),
                    project.First!["ProjectDescription"]!.ToString()
                ) {Field = ExperienceManager.Fields[_rnd.Next(ExperienceManager.Fields.Count)]};

                var date1 = RandomDay();
                var date2 = RandomDay();
                if (date1 < date2)
                {
                    temp.StartDate = date1;
                    temp.EndDate = date2;
                } 
                else if (date1 > date2)
                {
                    temp.StartDate = date2;
                    temp.EndDate = date1;
                }
                
                
                foreach (var purpose in project.First["Purpose"]!)
                {
                    if (temp.ProjectPurposes.Contains(purpose.ToString()))
                        continue;
                    temp.ProjectPurposes.Add(purpose.ToString());
                }
                
                foreach (var projectActivity in project.First["ProjectActivities"]!)
                {
                    if (temp.ProjectActivities.Exists(x => x.Description.Equals(projectActivity.ToString())))
                        continue;
                    ProjectActivity cache = new (projectActivity.ToString());
                    temp.ProjectActivities.Add(cache);
                }
                var result = await ProjectService.UpdateProject(temp);
                temp.LastChanged = result.Item1;
                _projects.Add(temp);
            }
        }

        private async Task<byte[]?> LoadProfilePicture(Guid employeeId)
        {
            string path =  Path.Combine(".", "wwwroot", "images", employeeId.ToString().ToUpper()+".jpg");
            if (!File.Exists(path)) return null;
            Image img = await Image.LoadAsync(path);
            img.Mutate(i => i.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Crop,
                    Size = new Size(512, 512)
                })
            );
            await using var memoryStreamSave = new MemoryStream();
            await img.SaveAsync(memoryStreamSave, JpegFormat.Instance);
            return memoryStreamSave.ToArray();
        }
        
        private async Task DummyEmployees(JToken? employees)
        {
            if (employees == null) return;
            var rolesIndex = 0;
            foreach (var employee in employees)
            {
                if (!employee.HasValues) continue;
                Employee temp = new
                (
                    Guid.Parse(employee.First!["Id"]!.ToString()),
                    (Authorizations) int.Parse(employee.First["Authorizations"]!.ToString()),
                    employee.First["Surname"]!.ToString(),
                    employee.First["Firstname"]!.ToString(),
                    employee.First["Username"]!.ToString(),
                    DateTime.ParseExact(employee.First["EmployedSince"]!.ToString(),"dd/MM/yyyy HH:mm:ss", new CultureInfo("en-US")), 
                    int.Parse(employee.First["WorkExperience"]!.ToString()),
                    int.Parse(employee.First["ScientificAssistant"]!.ToString()),
                    int.Parse(employee.First["StudentAssistant"]!.ToString()),
                    (RateCardLevel) int.Parse(employee.First["RateCardLevel"]!.ToString()),
                    null
                );

                temp.ProfilePicture = await LoadProfilePicture(temp.Id);
                
                for (var i = 0; i < 5; i++)
                {
                    var next = _rnd.Next(_projects.Count);
                    var projectId = _projects[next].Id;
                    if (!temp.ProjectIds.Contains(projectId))
                    {
                        temp.ProjectIds.Add(projectId);
                    }
                }
                
                temp = GenerateRandomExperience(temp, rolesIndex++);
                _employees.Add(temp);
                await EmployeeService.UpdateEmployee(temp);
                foreach (var projectId in temp.ProjectIds)
                {
                    var project = _projects.Find(x => x.Id.Equals(projectId));
                    if (project == null) continue;
                    for (var index = 0; index < 3; index++)
                    {
                        var activity = project.ProjectActivities[_rnd.Next(project.ProjectActivities.Count)];
                        activity.AddEmployee(temp.Id);
                    }

                    var result = await ProjectService.UpdateProject(project);
                    project.LastChanged = result.Item1;
                }
            }
        }

        private async Task DummyOffers(JToken? offers)
        {
            if (offers == null) return;
            foreach (var offer in offers)
            {
                if (!offer.HasValues) continue;
                Offer temp = new
                (
                    Guid.Parse(offer.First!["Id"]!.ToString()),
                    offer.First!["Title"]!.ToString(), 
                    DateTime.ParseExact(offer.First["StartDate"]!.ToString(), "dd/MM/yyyy HH:mm:ss", new CultureInfo("en-US")),
                    DateTime.ParseExact(offer.First["EndDate"]!.ToString(), "dd/MM/yyyy HH:mm:ss", new CultureInfo("en-US"))
                );
                temp = GenerateRandomExperience(temp);

                var date1 = RandomDay();
                var date2 = RandomDay();
                if (date1 < date2)
                {
                    temp.StartDate = date1;
                    temp.EndDate = date2;
                } 
                else if (date1 > date2)
                {
                    temp.StartDate = date2;
                    temp.EndDate = date1;
                }
                
                foreach (var shownEmployeeProperty in offer.First["ShownEmployeeProperties"]!)
                {
                    var employee = _employees.Find(x =>
                        x.Id.Equals(Guid.Parse(shownEmployeeProperty.First!["EmployeeId"]!.ToString())));
                    if (employee == null) continue;
                    ShownEmployeeProperties shortEmployee = new
                    (
                        Guid.Parse(shownEmployeeProperty.First!["Id"]!.ToString()), 
                        employee.Id,
                        employee.RateCardLevel,
                        Guid.Parse(offer.First!["Id"]!.ToString())
                    ) {SelectedExperience = employee.Experience};
                    employee.ProjectIds.ForEach(x => shortEmployee.ProjectIds.Add(x));
                    foreach (var project in _projects)
                    {
                        if (employee.ProjectIds.Contains(project.Id))
                        {
                            foreach (var projectProjectActivity in project.ProjectActivities)
                            {
                                if (projectProjectActivity.GetEmployeeIds().Contains(employee.Id))
                                    shortEmployee.ProjectActivityIds.Add(projectProjectActivity.Id);
                            }
                        }
                    }

                    shortEmployee.PlannedWeeklyHours = _rnd.Next(10, 40);
                    shortEmployee.Discount = _rnd.NextDouble()/2.0;
                    temp.ShortEmployees.Add(shortEmployee);
                }
                
                await OfferService.UpdateOffer(temp);
            }
        }
        
        private Employee GenerateRandomExperience(Employee temp, int rolesIndex)
        {
            Role role = ExperienceManager.Roles[rolesIndex % ExperienceManager.Roles.Count];
            if (!temp.Experience.Roles.Contains(role))
                temp.Experience.Roles.Add(role);
            
            for (var i = 0; i < 5; i++)
            {
                Field field = ExperienceManager.Fields[_rnd.Next(ExperienceManager.Fields.Count)];
                if (!temp.Experience.Fields.Contains(field))
                    temp.Experience.Fields.Add(field);

                Language language = ExperienceManager.Languages[_rnd.Next(ExperienceManager.Languages.Count)];
                if (!temp.Experience.Languages.Exists(x => x.Item1.Equals(language)))
                    temp.Experience.Languages.Add((language, (LanguageLevel) _rnd.Next(1, 6)));
            }
            for (var i = 0; i < 20; i++)
            {
                if (i % 2 == 0)
                {
                    SoftSkill softSkill = ExperienceManager.SoftSkills[_rnd.Next(ExperienceManager.SoftSkills.Count)];
                    if (!temp.Experience.SoftSkills.Contains(softSkill))
                        temp.Experience.SoftSkills.Add(softSkill);
                }
                HardSkill hardSkill = ExperienceManager.HardSkills[_rnd.Next(ExperienceManager.HardSkills.Count)];
                if (!temp.Experience.HardSkills.Exists(x => x.Item1.Equals(hardSkill)))
                    temp.Experience.HardSkills.Add((hardSkill, (HardSkillLevel) _rnd.Next(1, 5)));
            }

            return temp;
        }

        private Offer GenerateRandomExperience(Offer temp)
        {
            Role role = ExperienceManager.Roles[_rnd.Next(ExperienceManager.Roles.Count)];
            if (!temp.Experience.Roles.Contains(role))
                temp.Experience.Roles.Add(role);
            
            for (var i = 0; i < 5; i++)
            {
                Field field = ExperienceManager.Fields[_rnd.Next(ExperienceManager.Fields.Count)];
                if (!temp.Experience.Fields.Contains(field))
                    temp.Experience.Fields.Add(field);

                Language language = ExperienceManager.Languages[_rnd.Next(ExperienceManager.Languages.Count)];
                if (!temp.Experience.Languages.Exists(x => x.Item1.Equals(language)))
                    temp.Experience.Languages.Add((language, (LanguageLevel) _rnd.Next(1, 6)));
            }
            
            var cache = temp.Experience.Languages[0];
            var newLanguage = new Language(cache.Item1.Id, cache.Item1.Name);
            temp.Experience.Languages.Remove(cache);
            temp.Experience.Languages.Add((newLanguage, LanguageLevel.Fluent));
            
            for (var i = 0; i < 20; i++)
            {
                if (i % 2 == 0)
                {
                    SoftSkill softSkill = ExperienceManager.SoftSkills[_rnd.Next(ExperienceManager.SoftSkills.Count)];
                    if (!temp.Experience.SoftSkills.Contains(softSkill))
                        temp.Experience.SoftSkills.Add(softSkill);
                }
                HardSkill hardSkill = ExperienceManager.HardSkills[_rnd.Next(ExperienceManager.HardSkills.Count)];
                if (!temp.Experience.HardSkills.Exists(x => x.Item1.Equals(hardSkill)))
                    temp.Experience.HardSkills.Add((hardSkill, (HardSkillLevel) _rnd.Next(1, 5)));
            }

            return temp;
        }
    }
}