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
    /// Scoped service to save Experience data
    /// Contains all functions for getting, updating, editing or deleting experience data
    /// All insert and update functions return a DataBaseResult enum,
    /// so the UI can react appropriately to synchronization errors
    /// </summary>
    public class ExperienceManager
    {
        [Inject] private IExperienceService ExperienceService { get; set; }
        /// <summary>
        /// List of all the fields (derived from database)
        /// </summary>
        public List<Field> Fields { get; private set; } = new();
        /// <summary>
        /// List of all the roles (derived from database)
        /// </summary>
        public List<Role> Roles { get; private set; } = new();
        /// <summary>
        /// List of all soft skills (derived from database)
        /// </summary>
        public List<SoftSkill> SoftSkills { get; private set; } = new();
        /// <summary>
        /// List of all hard skills (derived from database)
        /// </summary>
        public List<HardSkill> HardSkills { get; private set; } = new();
        /// <summary>
        /// List of all languages (derived from database)
        /// </summary>
        public List<Language> Languages { get; private set; } = new();
        private bool _loaded;

        /// <summary>
        /// Initializes injected services
        /// </summary>
        /// <param name="experienceService"></param>
        public ExperienceManager(IExperienceService experienceService)
        {
            ExperienceService = experienceService;
        }

        /// <summary>
        /// checks whether there is data on on the database
        /// if not, loads test data into the database
        /// </summary>
        /// <returns>true if one of the five experience has at least one item in the database, false otherwise</returns>
        public async Task<bool> Load()
        {
            return await LoadExperiences();
        }

        /// <summary>
        /// saves the different experiences into the separate lists
        /// </summary>
        /// <returns>true if one of the five experience has at least one item in the database, false otherwise</returns>
        private async Task<bool> LoadExperiences()
        {
            if (_loaded) return _loaded;
            Fields = await ExperienceService.LoadFields();
            Roles = await ExperienceService.LoadRoles();
            SoftSkills = await ExperienceService.LoadSoftSkills();
            HardSkills = await ExperienceService.LoadHardSkills();
            Languages = await ExperienceService.LoadLanguages();
            if (Fields.Count <= 0 && Roles.Count <= 0 && SoftSkills.Count <= 0 && HardSkills.Count <= 0 &&
                Languages.Count <= 0) return _loaded;
            _loaded = true;
            SortList();
            return _loaded;
        }

        /// <summary>
        /// sort all experience by name
        /// </summary>
        private void SortList()
        {
            Fields = Fields.OrderBy(x => x.Name).ToList();
            Roles = Roles.OrderBy(x => x.Name).ToList();
            SoftSkills = SoftSkills.OrderBy(x => x.Name).ToList();
            HardSkills = HardSkills.OrderBy(x => x.Name).ToList();
            Languages = Languages.OrderBy(x => x.Name).ToList();
        }

        
        /// <summary>
        /// Adds new Experience to the correct list
        /// </summary>
        /// <param name="experience"></param>
        /// <returns>inserted if added into the database, failed or updated otherwise</returns>
        public async Task<DataBaseResult> InsertExperience(Experience experience)
        {
            Experience? experienceTemp = null;
            switch (experience)
            {
                case Field field:
                {
                    if (Fields.Exists(x => x.Id.Equals(field.Id)) || Fields.Exists(x => x.Name.Equals(field.Name, StringComparison.OrdinalIgnoreCase)))
                        return DataBaseResult.Failed;
                    Fields.Add(field);
                    experienceTemp = field;
                    break;
                }
                case Role role:
                {
                    if (Roles.Exists(x => x.Id.Equals(role.Id)) || Roles.Exists(x => x.Name.Equals(role.Name, StringComparison.OrdinalIgnoreCase)))
                        return DataBaseResult.Failed;
                    Roles.Add(role);
                    experienceTemp = role;
                    break;
                }
                case Language language:
                {
                    if (Languages.Exists(x => x.Id.Equals(language.Id)) ||
                        Languages.Exists(x => x.Name.Equals(language.Name, StringComparison.OrdinalIgnoreCase))) return DataBaseResult.Failed;
                    Languages.Add(language);
                    experienceTemp = language;
                    break;
                }
                case SoftSkill softSkill:
                {
                    if (SoftSkills.Exists(x => x.Id.Equals(softSkill.Id)) ||
                        SoftSkills.Exists(x => x.Name.Equals(softSkill.Name, StringComparison.OrdinalIgnoreCase))) return DataBaseResult.Failed;
                    SoftSkills.Add(softSkill);
                    experienceTemp = softSkill;
                    break;
                }
                case HardSkill hardSkill:
                {
                    if (HardSkills.Exists(x => x.Id.Equals(hardSkill.Id)) ||
                        HardSkills.Exists(x => x.Name.Equals(hardSkill.Name, StringComparison.OrdinalIgnoreCase))) return DataBaseResult.Failed;
                    HardSkills.Add(hardSkill);
                    experienceTemp = hardSkill;
                    break;
                }
            }

            SortList();
            if (experienceTemp == null) return DataBaseResult.Failed;
            var (dateTime, dataBaseResult) = await ExperienceService.UpdateExperience(experience);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            experienceTemp.LastChanged = dateTime;
            return dataBaseResult;
        }

        /// <summary>
        /// updates a given Experience
        /// </summary>
        /// <param name="experience"></param>
        /// <returns>updated if experience was updated in the database, failed or updated otherwise</returns>
        public async Task<DataBaseResult> UpdateExperience(Experience experience)
        {
            Experience? experienceTemp = null;
            switch (experience)
            {
                case Field field:
                {
                    var fieldRemove = Fields.Find(x => x.Id.Equals(field.Id));
                    if (fieldRemove != null)
                        Fields.Remove(fieldRemove);
                    Fields.Add(field);
                    experienceTemp = field;
                    break;
                }
                case Role role:
                {
                    var roleRemove = Roles.Find(x => x.Id.Equals(role.Id));
                    if (roleRemove != null)
                        Roles.Remove(roleRemove);
                    Roles.Add(role);
                    experienceTemp = role;
                    break;
                }
                case Language language:
                {
                    var languageRemove = Languages.Find(x => x.Id.Equals(language.Id));
                    if (languageRemove != null)
                        Languages.Remove(languageRemove);
                    Languages.Add(language);
                    experienceTemp = language;
                    break;
                }
                case SoftSkill softSkill:
                {
                    var softSkillRemove = SoftSkills.Find(x => x.Id.Equals(softSkill.Id));
                    if (softSkillRemove != null)
                        SoftSkills.Remove(softSkillRemove);
                    SoftSkills.Add(softSkill);
                    experienceTemp = softSkill;
                    break;
                }
                case HardSkill hardSkill:
                {
                    var hardSkillRemove = HardSkills.Find(x => x.Id.Equals(hardSkill.Id));
                    if (hardSkillRemove != null)
                        HardSkills.Remove(hardSkillRemove);
                    HardSkills.Add(hardSkill);
                    experienceTemp = hardSkill;
                    break;
                }
            }

            SortList();
            if (experienceTemp == null) return DataBaseResult.Failed;
            var (dateTime, dataBaseResult) = await ExperienceService.UpdateExperience(experience);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            if (dataBaseResult == DataBaseResult.Inserted) await DeleteExperience(experience);
            experienceTemp.LastChanged = dateTime;
            return dataBaseResult;
        }

        /// <summary>
        /// deletes a giving Experience
        /// </summary>
        /// <param name="experience"></param>
        /// <returns></returns>
        public async Task DeleteExperience(Experience experience)
        {
            switch (experience)
            {
                case Field field:
                    Fields.Remove(field);
                    break;
                case Role role:
                    Roles.Remove(role);
                    break;
                case Language language:
                    Languages.Remove(language);
                    break;
                case SoftSkill softSkill:
                    SoftSkills.Remove(softSkill);
                    break;
                case HardSkill hardSkill:
                    HardSkills.Remove(hardSkill);
                    break;
            }

            await ExperienceService.DeleteExperience(experience);
        }
    }
}