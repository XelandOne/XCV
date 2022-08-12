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
    /// Scoped service to save Offers data
    /// Contains all functions for getting, updating, editing or deleting offer data
    /// All insert and update functions return a DataBaseResult enum,
    /// so the UI can react appropriately to synchronization errors
    /// </summary>
    public class OfferManager
    {
        [Inject] private IOfferService OfferService { get; set; }
        [Inject] private IShownEmployeePropertiesService ShownEmployeePropertiesService { get; set; }
        [Inject] private EmployeeManager EmployeeManager { get; set; }
        [Inject] private IProjectActivityService ProjectActivityService { get; set; }
        public List<Offer> Offers { get; private set; } = new();
        private bool _loaded;

        public OfferManager(IOfferService offerService, IShownEmployeePropertiesService shownEmployeePropertiesService,
            EmployeeManager employeeManager, IProjectActivityService projectActivityService)
        {
            OfferService = offerService;
            ShownEmployeePropertiesService = shownEmployeePropertiesService;
            EmployeeManager = employeeManager;
            ProjectActivityService = projectActivityService;
        }

        /// <summary>
        /// gets all the Offers and saves them in the Offers list
        /// </summary>
        /// <returns>true if at least one offer was loaded otherwise returns false</returns>
        public async Task<bool> Load()
        {
            if (_loaded) return _loaded;
            Offers = await OfferService.GetAllOffers();
            
            if (Offers.Count <= 0) return _loaded;
            _loaded = true;
            return _loaded;
        }

        /// <summary>
        /// adds new ShownEmployeeProperties the the database and the offer with the offerId
        /// </summary>
        /// <param name="employee"></param>
        /// <param name="offerId"></param>
        /// <returns>updated if nothing went wrong, inserted or failed otherwise</returns>
        public async Task<DataBaseResult> AddShownEmployeeProperties(Employee employee, Guid offerId)
        {
            var offer = Offers.Find(x => x.Id.Equals(offerId));
            if (offer == null) return DataBaseResult.Failed;
            var shortEmployee = new ShownEmployeeProperties(employee, offer.Id);
            if (!offer.ShortEmployees.Exists(x => x.EmployeeId.Equals(employee.Id)))
                offer.ShortEmployees.Add(shortEmployee);
            var (dateTime, dataBaseResult) = await OfferService.UpdateOffer(offer);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;

            if (dataBaseResult == DataBaseResult.Inserted)
            {
                await OfferService.DeleteOffer(offer.Id);
                return DataBaseResult.Inserted;
            }
            offer.LastChanged = dateTime;
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// removes ShownEmployeeProperties from the database and the offer with the offerId
        /// </summary>
        /// <param name="shortEmployeeId"></param>
        /// <param name="offerId"></param>
        /// <returns>updated if nothing went wrong, inserted or failed otherwise</returns>
        public async Task<DataBaseResult> RemoveShownEmployeeProperties(Guid shortEmployeeId, Guid offerId)
        {
            var offer = Offers.Find(x => x.Id.Equals(offerId));
            if (offer == null) return DataBaseResult.Failed;
            var shortEmployee = offer.ShortEmployees.Find(x => x.Id.Equals(shortEmployeeId));
            if (shortEmployee == null) return DataBaseResult.Failed;
            offer.ShortEmployees.Remove(shortEmployee);
            var (dateTime, dataBaseResult) = await OfferService.UpdateOffer(offer);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            if (dataBaseResult == DataBaseResult.Inserted)
            {
                await OfferService.DeleteOffer(offer.Id);
                return DataBaseResult.Inserted;
            }
            offer.LastChanged = dateTime;
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// get offer from Offers list with offerId
        /// </summary>
        /// <param name="offerId"></param>
        /// <returns>Offer with offerId</returns>
        public Offer? GetOffer(Guid offerId)
        {
            return Offers.Find(x => x.Id.Equals(offerId));
        }

        /// <summary>
        /// copies an offer
        /// </summary>
        /// <param name="offer"></param>
        /// <returns>a true copy of a giving offer</returns>
        public async Task<Offer?> CopyOffer(Offer offer)
        {
            var count = Offers.Count(cache => cache.Title.Contains(offer.Title));
            var temp = new Offer(offer.Title + " Kopie " + count, offer.StartDate, offer.EndDate)
                {Experience = new UsedExperience(offer.Experience)};

            foreach (var shortEmployee in offer.ShortEmployees.Select(shownEmployeeProperties =>
                new ShownEmployeeProperties(shownEmployeeProperties, temp.Id)))
            {
                temp.ShortEmployees.Add(shortEmployee);
            }

            offer.DocumentConfigurations.ForEach(x => temp.DocumentConfigurations.Add(x));
            await OfferService.UpdateOffer(temp);

            var copy = await OfferService.GetOffer(temp.Id);
            if (copy == null) return null;
            Offers.Add(copy);
            return copy;
        }

        /// <summary>
        /// update a full offer in the Offers list and the database
        /// </summary>
        /// <param name="offer"></param>
        /// <returns>updated if nothing went wrong, inserted or failed otherwise</returns>
        public async Task<DataBaseResult> UpdateOffer(Offer offer)
        {
            Offers.Remove(offer);
            Offers.Add(offer);
            var (dateTime, dataBaseResult) = await OfferService.UpdateOffer(offer);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            offer.LastChanged = dateTime;
            return dataBaseResult;
        }

        /// <summary>
        /// deletes an offer
        /// </summary>
        /// <param name="offer"></param>
        /// <returns>true if offer was deleted, false otherwise</returns>
        public async Task<bool> DeleteOffer(Offer offer)
        {
            Offers.Remove(offer);
            return await OfferService.DeleteOffer(offer.Id);
        }

        /// <summary>
        /// Removes experience from offer
        /// </summary>
        /// <param name="experience"></param>
        /// <param name="offerId"></param>
        /// <returns>updated if nothing went wrong, inserted or failed otherwise</returns>
        public async Task<DataBaseResult> RemoveExperience(Experience experience, Guid offerId)
        {
            var offer = Offers.Find(x => x.Id.Equals(offerId));
            if (offer == null) return DataBaseResult.Failed;
            switch (experience)
            {
                case Role role:
                    offer.Experience.Roles.Remove(role);
                    break;
                case Field field:
                    offer.Experience.Fields.Remove(field);
                    break;
                case SoftSkill softSkill:
                    offer.Experience.SoftSkills.Remove(softSkill);
                    break;
                case HardSkill:
                    offer.Experience.HardSkills.Remove(
                        offer.Experience.HardSkills.Find(x => x.Item1.Id.Equals(experience.Id)));
                    break;
                case Language:
                    offer.Experience.Languages.Remove(
                        offer.Experience.Languages.Find(x => x.Item1.Id.Equals(experience.Id)));
                    break;
            }

            var (dateTime, dataBaseResult) = await OfferService.UpdateOffer(offer);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            if (dataBaseResult == DataBaseResult.Inserted)
            {
                await OfferService.DeleteOffer(offer.Id);
                return DataBaseResult.Inserted;
            }

            offer.LastChanged = dateTime;
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// adds an Role, Field or SoftSkill to an Offer
        /// </summary>
        /// <param name="experience"></param>
        /// <param name="offerId"></param>
        /// <returns>updated if nothing went wrong, inserted or failed otherwise</returns>
        public async Task<DataBaseResult> AddExperience(Experience experience, Guid offerId)
        {
            var offer = Offers.Find(x => x.Id.Equals(offerId));
            if (offer == null) return DataBaseResult.Failed;
            switch (experience)
            {
                case Role role:
                    if (!offer.Experience.Roles.Contains(role))
                        offer.Experience.Roles.Add(role);
                    break;
                case Field field:
                    if (!offer.Experience.Fields.Contains(field))
                        offer.Experience.Fields.Add(field);
                    break;
                case SoftSkill softSkill:
                    if (!offer.Experience.SoftSkills.Contains(softSkill))
                        offer.Experience.SoftSkills.Add(softSkill);
                    break;
            }

            var (dateTime, dataBaseResult) = await OfferService.UpdateOffer(offer);

            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            if (dataBaseResult == DataBaseResult.Inserted)
            {
                await OfferService.DeleteOffer(offer.Id);
                return DataBaseResult.Inserted;
            }

            offer.LastChanged = dateTime;
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// updates a HardSkill with Level of an Offer
        /// </summary>
        /// <param name="hardSkill"></param>
        /// <param name="offerId"></param>
        /// <returns>updated if nothing went wrong, inserted or failed otherwise</returns>
        public async Task<DataBaseResult> UpdateHardSkill((HardSkill, HardSkillLevel) hardSkill, Guid offerId)
        {
            var offer = Offers.Find(x => x.Id.Equals(offerId));
            if (offer == null) return DataBaseResult.Failed;
            if (offer.Experience.HardSkills.Exists(x => x.Item1.Equals(hardSkill.Item1)))
                offer.Experience.HardSkills.Remove(
                    offer.Experience.HardSkills.Find(x => x.Item1.Equals(hardSkill.Item1)));
            offer.Experience.HardSkills.Add(hardSkill);

            var (dateTime, dataBaseResult) = await OfferService.UpdateOffer(offer);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            if (dataBaseResult == DataBaseResult.Inserted)
            {
                await OfferService.DeleteOffer(offer.Id);
                return DataBaseResult.Inserted;
            }

            offer.LastChanged = dateTime;
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// updates a Language with Level of an Offer
        /// </summary>
        /// <param name="language"></param>
        /// <param name="offerId"></param>
        /// <returns>updated if nothing went wrong, inserted or failed otherwise</returns>
        public async Task<DataBaseResult> UpdateLanguage((Language, LanguageLevel) language, Guid offerId)
        {
            var offer = Offers.Find(x => x.Id.Equals(offerId));
            if (offer == null) return DataBaseResult.Failed;
            if (offer.Experience.Languages.Exists(x => x.Item1.Equals(language.Item1)))
                offer.Experience.Languages.Remove(offer.Experience.Languages.Find(x => x.Item1.Equals(language.Item1)));
            offer.Experience.Languages.Add(language);

            var (dateTime, dataBaseResult) = await OfferService.UpdateOffer(offer);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            if (dataBaseResult == DataBaseResult.Inserted)
            {
                await OfferService.DeleteOffer(offer.Id);
                return DataBaseResult.Inserted;
            }

            offer.LastChanged = dateTime;
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// Removes Experience from shownEmployeeProperties selectedExperience
        /// </summary>
        /// <param name="experience"></param>
        /// <param name="offerId"></param>
        /// <param name="shownEmployeePropertiesId"></param>
        /// <returns>updated if nothing went wrong, inserted or failed otherwise</returns>
        public async Task<DataBaseResult> RemoveSelectedExperience(Experience experience, Guid offerId,
            Guid shownEmployeePropertiesId)
        {
            var offer = Offers.Find(x => x.Id.Equals(offerId));
            if (offer == null) return DataBaseResult.Failed;
            var cache = offer.ShortEmployees
                .Find(x => x.Id.Equals(shownEmployeePropertiesId));
            if (cache == null) return DataBaseResult.Failed;
            switch (experience)
            {
                case Role role:
                    cache.SelectedExperience.Roles.Remove(role);
                    break;
                case Field field:
                    cache.SelectedExperience.Fields.Remove(field);
                    break;
                case SoftSkill softSkill:
                    cache.SelectedExperience.SoftSkills.Remove(softSkill);
                    break;
                case HardSkill:
                    cache.SelectedExperience.HardSkills.Remove(
                        cache.SelectedExperience.HardSkills.Find(x => x.Item1.Id.Equals(experience.Id)));
                    break;
                case Language:
                    cache.SelectedExperience.Languages.Remove(
                        cache.SelectedExperience.Languages.Find(x => x.Item1.Id.Equals(experience.Id)));
                    break;
            }

            var (dateTime, dataBaseResult) = await ShownEmployeePropertiesService.UpdateShownEmployeeProperties(cache);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            if (dataBaseResult == DataBaseResult.Inserted)
            {
                await ShownEmployeePropertiesService.DeleteShownEmployeeProperties(cache.Id);
                return DataBaseResult.Inserted;
            }

            offer.LastChanged = dateTime;
            cache.LastChanged = dateTime;
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// Adds an Role, Field or SoftSkill to shownEmployeeProperties selectedExperience
        /// </summary>
        /// <param name="experience"></param>
        /// <param name="offerId"></param>
        /// <param name="shownEmployeePropertiesId"></param>
        /// <returns>updated if nothing went wrong, inserted or failed otherwise</returns>
        public async Task<DataBaseResult> AddSelectedExperience(Experience experience, Guid offerId,
            Guid shownEmployeePropertiesId)
        {
            var offer = Offers.Find(x => x.Id.Equals(offerId));
            if (offer == null) return DataBaseResult.Failed;
            var cache = offer.ShortEmployees
                .Find(x => x.Id.Equals(shownEmployeePropertiesId));
            if (cache == null) return DataBaseResult.Failed;
            
            switch (experience)
            {
                case Role role:
                    if (cache.Experience.Roles.Contains(role) &&
                        !cache.SelectedExperience.Roles.Contains(role))
                        cache.SelectedExperience.Roles.Add(role);
                    break;
                case Field field:
                    if (cache.Experience.Fields.Contains(field) &&
                        !cache.SelectedExperience.Fields.Contains(field))
                        cache.SelectedExperience.Fields.Add(field);
                    break;
                case SoftSkill softSkill:
                    if (cache.Experience.SoftSkills.Contains(softSkill) &&
                        !cache.SelectedExperience.SoftSkills.Contains(softSkill))
                        cache.SelectedExperience.SoftSkills.Add(softSkill);
                    break;
            }

            var (dateTime, dataBaseResult) = await ShownEmployeePropertiesService.UpdateShownEmployeeProperties(cache);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            if (dataBaseResult == DataBaseResult.Inserted)
            {
                await ShownEmployeePropertiesService.DeleteShownEmployeeProperties(cache.Id);
                return DataBaseResult.Inserted;
            }

            offer.LastChanged = dateTime;
            cache.LastChanged = dateTime;
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// Adds an HardSkill and Level to shownEmployeeProperties selectedExperience
        /// </summary>
        /// <param name="hardSkill"></param>
        /// <param name="offerId"></param>
        /// <param name="shownEmployeePropertiesId"></param>
        /// <returns>updated if nothing went wrong, inserted or failed otherwise</returns>
        public async Task<DataBaseResult> AddSelectedHardSkill((HardSkill, HardSkillLevel) hardSkill, Guid offerId,
            Guid shownEmployeePropertiesId)
        {
            var offer = Offers.Find(x => x.Id.Equals(offerId));
            if (offer == null) return DataBaseResult.Failed;
            
            var cache = offer.ShortEmployees
                .Find(x => x.Id.Equals(shownEmployeePropertiesId));
            if (cache == null) return DataBaseResult.Failed;
            
            if (!cache.Experience.HardSkills.Contains(hardSkill) ||
                cache.SelectedExperience.HardSkills.Contains(hardSkill)) return DataBaseResult.Failed;
            cache.SelectedExperience.HardSkills.Add(hardSkill);
            
            var (dateTime, dataBaseResult) = await ShownEmployeePropertiesService.UpdateShownEmployeeProperties(cache);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            if (dataBaseResult == DataBaseResult.Inserted)
            {
                await ShownEmployeePropertiesService.DeleteShownEmployeeProperties(cache.Id);
                return DataBaseResult.Inserted;
            }

            offer.LastChanged = dateTime;
            cache.LastChanged = dateTime;
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// Adds an Language and Level to shownEmployeeProperties selectedExperience
        /// </summary>
        /// <param name="language"></param>
        /// <param name="offerId"></param>
        /// <param name="shownEmployeePropertiesId"></param>
        /// <returns>updated if nothing went wrong, inserted or failed otherwise</returns>
        public async Task<DataBaseResult> AddSelectedLanguage((Language, LanguageLevel) language, Guid offerId,
            Guid shownEmployeePropertiesId)
        {
            var offer = Offers.Find(x => x.Id.Equals(offerId));
            if (offer == null) return DataBaseResult.Failed;
            
            var cache = offer.ShortEmployees
                .Find(x => x.Id.Equals(shownEmployeePropertiesId));
            if (cache == null) return DataBaseResult.Failed;
            
            if (!cache.Experience.Languages.Contains(language) ||
                cache.SelectedExperience.Languages.Contains(language)) return DataBaseResult.Failed;
            cache.SelectedExperience.Languages.Add(language);
            
            var (dateTime, dataBaseResult) = await ShownEmployeePropertiesService.UpdateShownEmployeeProperties(cache);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            if (dataBaseResult == DataBaseResult.Inserted)
            {
                await ShownEmployeePropertiesService.DeleteShownEmployeeProperties(cache.Id);
                return DataBaseResult.Inserted;
            }

            offer.LastChanged = dateTime;
            cache.LastChanged = dateTime;
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// removes projects from shownEmployeeProperties
        /// </summary>
        /// <param name="projectsId"></param>
        /// <param name="offerId"></param>
        /// <param name="shownEmployeePropertiesId"></param>
        /// <returns>updated if nothing went wrong, inserted or failed otherwise</returns>
        public async Task<DataBaseResult> RemoveProjects(Guid projectsId, Guid offerId, Guid shownEmployeePropertiesId)
        {
            var offer = Offers.Find(x => x.Id.Equals(offerId));
            if (offer == null) return DataBaseResult.Failed;
            var cache = offer.ShortEmployees
                .Find(x => x.Id.Equals(shownEmployeePropertiesId));
            if (cache == null) return DataBaseResult.Failed;
            cache.ProjectIds.Remove(projectsId);
            var (dateTime, dataBaseResult) = await ShownEmployeePropertiesService.UpdateShownEmployeeProperties(cache);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            if (dataBaseResult == DataBaseResult.Inserted)
            {
                await ShownEmployeePropertiesService.DeleteShownEmployeeProperties(cache.Id);
                return DataBaseResult.Inserted;
            }

            offer.LastChanged = dateTime;
            cache.LastChanged = dateTime;
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// adds a project to shownEmployeeProperties
        /// </summary>
        /// <param name="projectsId"></param>
        /// <param name="offerId"></param>
        /// <param name="shownEmployeePropertiesId"></param>
        /// <returns>updated if nothing went wrong, inserted or failed otherwise</returns>
        public async Task<DataBaseResult> AddProjects(Guid projectsId, Guid offerId, Guid shownEmployeePropertiesId)
        {
            var offer = Offers.Find(x => x.Id.Equals(offerId));
            if (offer == null) return DataBaseResult.Failed;
            var cache = offer.ShortEmployees
                .Find(x => x.Id.Equals(shownEmployeePropertiesId));
            if (cache == null) return DataBaseResult.Failed;
            var employee = EmployeeManager.GetEmployee(cache.EmployeeId);
            if (employee == null || !employee.ProjectIds.Contains(projectsId)) return DataBaseResult.Failed;
            if (cache.ProjectIds.Contains(projectsId)) return DataBaseResult.Failed;
            cache.ProjectIds.Add(projectsId);
            var (dateTime, dataBaseResult) = await ShownEmployeePropertiesService.UpdateShownEmployeeProperties(cache);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            if (dataBaseResult == DataBaseResult.Inserted)
            {
                await ShownEmployeePropertiesService.DeleteShownEmployeeProperties(cache.Id);
                return DataBaseResult.Inserted;
            }

            offer.LastChanged = dateTime;
            cache.LastChanged = dateTime;
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// updates shownEmployeeProperties PlannedWeeklyHours
        /// </summary>
        /// <param name="plannedWeeklyHours"></param>
        /// <param name="offerId"></param>
        /// <param name="shownEmployeePropertiesId"></param>
        /// <returns>updated if nothing went wrong, inserted or failed otherwise</returns>
        public async Task<DataBaseResult> UpdatePlannedWeeklyHours(int plannedWeeklyHours, Guid offerId,
            Guid shownEmployeePropertiesId)
        {
            var offer = Offers.Find(x => x.Id.Equals(offerId));
            if (offer == null) return DataBaseResult.Failed;
            var cache = offer.ShortEmployees
                .Find(x => x.Id.Equals(shownEmployeePropertiesId));
            if (cache == null) return DataBaseResult.Failed;
            cache.PlannedWeeklyHours = plannedWeeklyHours;
            var (dateTime, dataBaseResult) = await ShownEmployeePropertiesService.UpdateShownEmployeeProperties(cache);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            if (dataBaseResult == DataBaseResult.Inserted)
            {
                await ShownEmployeePropertiesService.DeleteShownEmployeeProperties(cache.Id);
                return DataBaseResult.Inserted;
            }

            offer.LastChanged = dateTime;
            cache.LastChanged = dateTime;
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// updates shownEmployeeProperties Discount
        /// </summary>
        /// <param name="discount"></param>
        /// <param name="offerId"></param>
        /// <param name="shownEmployeePropertiesId"></param>
        /// <returns>updated if nothing went wrong, inserted or failed otherwise</returns>
        public async Task<DataBaseResult> UpdateDiscount(float discount, Guid offerId,
            Guid shownEmployeePropertiesId)
        {
            var offer = Offers.Find(x => x.Id.Equals(offerId));
            if (offer == null) return DataBaseResult.Failed;
            var cache = offer.ShortEmployees
                .Find(x => x.Id.Equals(shownEmployeePropertiesId));
            if (cache == null) return DataBaseResult.Failed;
            cache.Discount = discount;
            var (dateTime, dataBaseResult) = await ShownEmployeePropertiesService.UpdateShownEmployeeProperties(cache);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            if (dataBaseResult == DataBaseResult.Inserted)
            {
                await ShownEmployeePropertiesService.DeleteShownEmployeeProperties(cache.Id);
                return DataBaseResult.Inserted;
            }
            offer.LastChanged = dateTime;
            cache.LastChanged = dateTime;
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// updates shownEmployeeProperties RateCardLevel
        /// </summary>
        /// <param name="rateCardLevel"></param>
        /// <param name="offerId"></param>
        /// <param name="shownEmployeePropertiesId"></param>
        /// <returns>updated if nothing went wrong, inserted or failed otherwise</returns>
        public async Task<DataBaseResult> UpdateRateCardLevel(RateCardLevel rateCardLevel, Guid offerId,
            Guid shownEmployeePropertiesId)
        {
            var offer = Offers.Find(x => x.Id.Equals(offerId));
            if (offer == null) return DataBaseResult.Failed;
            var cache = offer.ShortEmployees
                .Find(x => x.Id.Equals(shownEmployeePropertiesId));
            if (cache == null) return DataBaseResult.Failed;
            cache.RateCardLevel = rateCardLevel;
            var (dateTime, dataBaseResult) = await ShownEmployeePropertiesService.UpdateShownEmployeeProperties(cache);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            if (dataBaseResult == DataBaseResult.Inserted)
            {
                await ShownEmployeePropertiesService.DeleteShownEmployeeProperties(cache.Id);
                return DataBaseResult.Inserted;
            }

            offer.LastChanged = dateTime;
            cache.LastChanged = dateTime;
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// Updates a ProjectActivity
        /// </summary>
        /// <param name="projectActivityId"></param>
        /// <param name="offerId"></param>
        /// <param name="shownEmployeePropertiesId"></param>
        /// <returns>updated if nothing went wrong, inserted or failed otherwise</returns>
        public async Task<DataBaseResult> AddProjectActivity(Guid projectActivityId, Guid offerId,
            Guid shownEmployeePropertiesId)
        {
            var offer = Offers.Find(x => x.Id.Equals(offerId));
            if (offer == null) return DataBaseResult.Failed;
            var cache = offer.ShortEmployees
                .Find(x => x.Id.Equals(shownEmployeePropertiesId));
            if (cache == null) return DataBaseResult.Failed;
            if (!cache.ProjectActivityIds.Contains(projectActivityId))
                cache.ProjectActivityIds.Add(projectActivityId);
            var (dateTime, dataBaseResult) = await ShownEmployeePropertiesService.UpdateShownEmployeeProperties(cache);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            if (dataBaseResult == DataBaseResult.Inserted)
            {
                await ShownEmployeePropertiesService.DeleteShownEmployeeProperties(cache.Id);
                return DataBaseResult.Inserted;
            }

            offer.LastChanged = dateTime;
            cache.LastChanged = dateTime;
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// Deletes a ProjectActivity
        /// </summary>
        /// <param name="projectActivityId"></param>
        /// <param name="offerId"></param>
        /// <param name="shownEmployeePropertiesId"></param>
        /// <returns>updated if nothing went wrong, inserted or failed otherwise</returns>
        public async Task<DataBaseResult> DeleteProjectActivity(Guid projectActivityId, Guid offerId,
            Guid shownEmployeePropertiesId)
        {
            var offer = Offers.Find(x => x.Id.Equals(offerId));
            if (offer == null) return DataBaseResult.Failed;
            var cache = Offers.Find(x => x.Id.Equals(offerId))?.ShortEmployees
                .Find(x => x.Id.Equals(shownEmployeePropertiesId));
            if (cache == null) return DataBaseResult.Failed;

            cache.ProjectActivityIds.Remove(projectActivityId);
            var (dateTime, dataBaseResult) = await ShownEmployeePropertiesService.UpdateShownEmployeeProperties(cache);
            if (dataBaseResult == DataBaseResult.Failed) return DataBaseResult.Failed;
            if (dataBaseResult == DataBaseResult.Inserted)
            {
                await ShownEmployeePropertiesService.DeleteShownEmployeeProperties(cache.Id);
                return DataBaseResult.Inserted;
            }

            offer.LastChanged = dateTime;
            cache.LastChanged = dateTime;
            return DataBaseResult.Updated;
        }

        /// <summary>
        /// needs Employee ID not shownEmployeePropertyId
        /// returns a list of all the ProjectActivities
        /// </summary>
        /// <param name="projectActivityIds"></param>
        /// <param name="employeeId"></param>
        /// <returns>List(ProjectActivity?)</returns>
        public async Task<List<ProjectActivity?>> GetAllProjectActivities(IEnumerable<Guid> projectActivityIds,
            Guid employeeId)
        {
            List<ProjectActivity?> projectActivities = new();
            foreach (var projectActivityId in projectActivityIds)
            {
                var projectActivity = await ProjectActivityService.GetProjectActivity(projectActivityId);
                if (projectActivity == null) continue;
                if (projectActivity.GetEmployeeIds().Contains(employeeId))
                    projectActivities.Add(projectActivity);
            }

            return projectActivities;
        }

        /// <summary>
        /// checks whether the employee has selected an activity
        /// </summary>
        /// <param name="projectActivityId"></param>
        /// <param name="shownEmployeeProperties"></param>
        /// <returns>true if ShownEmployeeProperty Has the ProjectActivity with projectActivityId</returns>
        public bool CheckShownEmployeePropertyHasProjectActivities(Guid projectActivityId,
            ShownEmployeeProperties shownEmployeeProperties)
        {
            return shownEmployeeProperties.ProjectActivityIds.Contains(projectActivityId);
        }
    }
}