using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using OpenXmlPowerTools;
using XCV.Data;
using XCV.Entities;
using XCV.Entities.Enums;
using XCV.Services;

namespace XCV.Pages
{
    public partial class OfferDetailEdit
    {
        [Inject] public ExperienceManager ExperienceManager { get; set; }
        [Parameter] public Guid Id { get; set; }
        [Parameter] public Offer? Offer { get; set; }
        /// <summary>
        /// A dictionary of employees (Guid) with list of belonging projects(Guid, Name)
        /// </summary>
        [Parameter] public Dictionary<Guid, List<(Guid, string)>?> Projects { get; set; } = new();
        /// <summary>
        /// Boolean value set true if someone changed, updated or deleted a project at the same time as someone else.
        /// </summary>
        [Parameter] public bool Failed { get; set; }
        /// <summary>
        /// String value regarding the searched Filter Term
        /// </summary>
        private string? Filter { get; set; }
        private string? Title { get; set; }
        private DateTime? StartDate { get; set; }
        private DateTime? EndDate { get; set; }
        
        /// <summary>
        /// Boolean value (OnInitialized: true) set false if user wants to create a new offer, and set true again if offer is created
        /// </summary>
        private bool Hid { get; set; }

        /// <inheritdoc />
        protected override async Task OnInitializedAsync()
        {
            await _offerManager.Load();
            var temp = _offerManager.GetOffer(Id);
            if (temp != null)
            {
                Offer = temp;
                foreach (var shownEmployeeProperties in Offer.ShortEmployees)
                {
                    await _employeeManager.LoadEmployee(shownEmployeeProperties.EmployeeId);
                }
            }
            await _experienceManager.Load();
            await _projectManager.Load();
            
            if (Offer != null)
            {
                
                
                foreach (var employee in Offer.ShortEmployees)
                {
                    var emp = _employeeManager.GetEmployee(employee.EmployeeId);
                
                    if (emp != null)
                    {
                        List<(Guid, string)> l = new();
                        foreach (var pro in emp.ProjectIds)
                        {
                            var p = _projectManager.Projects.Find(p => p.Id.Equals(pro));
                            if (p != null)
                            {
                                var title = p.Title;
                                if (title != null)
                                {
                                    l.Add((pro,title));
                                }
                            }
                        }
                        if (!Projects.ContainsKey(employee.EmployeeId))
                            Projects.Add(employee.EmployeeId, l);
                    }
                }
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            if (Failed)
            {
                await Task.Delay(5000);
                Failed = false;
                _navigationManager.NavigateTo("/OfferOverview/Offer/edit/"+Offer.Id);
            }
        }

        private async Task DeleteOffer()
        {
            if (Offer != null)
            {
                await _offerManager.DeleteOffer(Offer);
                _navigationManager.NavigateTo("OfferOverview");
            }
            
        }
        
        private async Task Enter(KeyboardEventArgs e)
        {
            if (e.Code is "Enter" or "NumpadEnter")
            {
                await Task.Delay(1000);
                await ChangeTitle();
            }
        }
        
        private async Task ChangeTitle()
        {
            if (Title != null && Offer != null && Title.Length<101)
            {
                Offer.Title = Title;
                var result = await _offerManager.UpdateOffer(Offer);
                if (result == DataBaseResult.Inserted)
                    _navigationManager.NavigateTo("OfferOverview/"+true, true);
                if (result == DataBaseResult.Failed)
                    _navigationManager.NavigateTo("/OfferOverview/Offer/edit/"+Offer.Id+"/"+true, true);
            }
        }

        private async Task ChangeDates()
        {
            if (Offer == null) return;
            if (StartDate != null && EndDate != null && StartDate.Value < EndDate.Value)
            {
                Offer.StartDate = StartDate;
                Offer.EndDate = EndDate;
                await _offerManager.UpdateOffer(Offer);
            } else if (StartDate != null)
            {
                Offer.StartDate = StartDate;
                Offer.EndDate = null;
                await _offerManager.UpdateOffer(Offer);
            } else if (EndDate != null)
            {
                Offer.EndDate = EndDate;
                Offer.StartDate = null;
                await _offerManager.UpdateOffer(Offer);
            }
        }
        
        private async Task CopyOffer()
        {
            if (Offer != null)
            {
                await _offerManager.CopyOffer(Offer);
                // string title = Offer.Title + " Kopie";
                // Offer Copy = new Offer(title, Offer.StartDate, Offer.EndDate);
                // Copy.Experience = Offer.Experience;
                // foreach (var employee in Offer.ShortEmployees)
                // {
                //     Copy.ShortEmployees.Add(employee);
                // }
                // foreach (var config in Offer.DocumentConfigurations)
                // {
                //     Copy.DocumentConfigurations.Add(config);
                // }
                //
                // _offerManager.UpdateOffer(Copy);
                
                _navigationManager.NavigateTo("OfferOverview");
            }
            
        }
        private async Task SelectHardskill((HardSkill, HardSkillLevel) hardskill, Guid id, object checkedValue)
        {
            if (Offer == null) return;
            
            if ((bool) checkedValue)
            {
                var result = await _offerManager.AddSelectedHardSkill(hardskill, Offer.Id, id);
                if (result == DataBaseResult.Inserted)
                    _navigationManager.NavigateTo("OfferOverview/"+true, true);
                if (result == DataBaseResult.Failed)
                    _navigationManager.NavigateTo("/OfferOverview/Offer/edit/"+Offer.Id+"/"+true, true);
            }
            else
            {
                var result = await _offerManager.RemoveSelectedExperience(hardskill.Item1, Offer.Id, id);
                if (result == DataBaseResult.Inserted);
                _navigationManager.NavigateTo("OfferOverview/"+true, true);
                if (result == DataBaseResult.Failed);
                _navigationManager.NavigateTo("/OfferOverview/Offer/edit/"+Offer.Id+"/"+true, true);
            }
        }
        
        private async Task SelectSoftskill(SoftSkill softskill, Guid id, object checkedValue)
        {
            if (Offer == null) return ;
            if ((bool) checkedValue)
            {
                var result = await _offerManager.AddSelectedExperience(softskill, Offer.Id, id);
                if (result == DataBaseResult.Inserted)
                    _navigationManager.NavigateTo("OfferOverview/"+true, true);
                if (result == DataBaseResult.Failed)
                    _navigationManager.NavigateTo("/OfferOverview/Offer/edit/"+Offer.Id+"/"+true, true);
            }
            else
            {
                var result = await _offerManager.RemoveSelectedExperience(softskill, Offer.Id, id);
                if (result == DataBaseResult.Inserted)
                    _navigationManager.NavigateTo("OfferOverview/"+true, true);
                if (result == DataBaseResult.Failed)
                    _navigationManager.NavigateTo("/OfferOverview/Offer/edit/"+Offer.Id+"/"+true, true);
            }
        }
        
        private async Task SelectLanguage((Language, LanguageLevel) language, Guid id, object checkedValue)
        {
            if (Offer == null) return ;
            if ((bool) checkedValue)
            {
                var result = await _offerManager.AddSelectedLanguage(language, Offer.Id, id);
                if (result == DataBaseResult.Inserted)
                    _navigationManager.NavigateTo("OfferOverview/"+true, true);
                if (result == DataBaseResult.Failed)
                    _navigationManager.NavigateTo("/OfferOverview/Offer/edit/"+Offer.Id+"/"+true, true);
            }
            else
            {
                var result = await _offerManager.RemoveSelectedExperience(language.Item1, Offer.Id, id);
                if (result == DataBaseResult.Inserted)
                    _navigationManager.NavigateTo("OfferOverview/"+true, true);
                if (result == DataBaseResult.Failed)
                    _navigationManager.NavigateTo("/OfferOverview/Offer/edit/"+Offer.Id+"/"+true, true);
            }
        }
        
        private async Task SelectProject(Guid projectId, Guid id, object checkedValue)
        {
            if (Offer == null) return ;
            if ((bool) checkedValue)
            {
                var result = await _offerManager.AddProjects(projectId, Offer.Id, id);
                if (result == DataBaseResult.Inserted)
                    _navigationManager.NavigateTo("OfferOverview/"+true, true);
                if (result == DataBaseResult.Failed)
                    _navigationManager.NavigateTo("/OfferOverview/Offer/edit/"+Offer.Id+"/"+true, true);
            }
            else
            {
                var result = await _offerManager.RemoveProjects(projectId, Offer.Id, id);
                if (result == DataBaseResult.Inserted)
                    _navigationManager.NavigateTo("OfferOverview/"+true, true);
                if (result == DataBaseResult.Failed)
                    _navigationManager.NavigateTo("/OfferOverview/Offer/edit/"+Offer.Id+"/"+true, true);
            }
        }
        
        private async Task SelectField(Field field, Guid id, object checkedValue)
        {
            if (Offer == null) return ;
            if ((bool) checkedValue )
            {
                var result = await _offerManager.AddSelectedExperience(field, Offer.Id, id);
                if (result == DataBaseResult.Inserted)
                    _navigationManager.NavigateTo("OfferOverview/"+true, true);
                if (result == DataBaseResult.Failed)
                    _navigationManager.NavigateTo("/OfferOverview/Offer/edit/"+Offer.Id+"/"+true, true);
            }
            else if (Offer != null)
            {
                var result = await _offerManager.RemoveSelectedExperience(field, Offer.Id, id);
                if (result == DataBaseResult.Inserted)
                    _navigationManager.NavigateTo("OfferOverview/"+true, true);
                if (result == DataBaseResult.Failed)
                    _navigationManager.NavigateTo("/OfferOverview/Offer/edit/"+Offer.Id+"/"+true, true);
            }
        }
        
        private async Task SelectHardskillOffer((HardSkill, HardSkillLevel) hardskill, object checkedValue)
        {
            if (Offer == null) return ;
            if ((bool) checkedValue)
            {
                var result = await _offerManager.UpdateHardSkill(hardskill, Offer.Id);
                if (result == DataBaseResult.Inserted)
                    _navigationManager.NavigateTo("OfferOverview/"+true, true);
                    
                if (result == DataBaseResult.Failed)
                    _navigationManager.NavigateTo("/OfferOverview/Offer/edit/"+Offer.Id+"/"+true, true);
            }
        }

        private async Task SelectSoftskillOffer(SoftSkill softskill, object checkedValue)
        {
            if (Offer == null) return;
            if ((bool) checkedValue)
            {
                var result = await _offerManager.AddExperience(softskill, Offer.Id);
                if (result  == DataBaseResult.Inserted)
                    _navigationManager.NavigateTo("OfferOverview/"+true, true);
                if (result  == DataBaseResult.Failed)
                    _navigationManager.NavigateTo("/OfferOverview/Offer/edit/"+Offer.Id+"/"+true, true);
            }
        }
        
        private async Task SelectFieldOffer(Field field, object checkedValue)
        {
            if (Offer == null) return;
            if ((bool) checkedValue)
            {
                var result = await _offerManager.AddExperience(field, Offer.Id);
                if (result == DataBaseResult.Inserted)
                    _navigationManager.NavigateTo("OfferOverview/"+true, true);
                if (result == DataBaseResult.Failed)
                    _navigationManager.NavigateTo("/OfferOverview/Offer/edit/"+Offer.Id+"/"+true, true);
            }
        }
        
        private async Task SelectRoleOffer(Role role, object checkedValue)
        {
            if (Offer == null) return;
            if ((bool) checkedValue)
            {
                var result = await _offerManager.AddExperience(role, Offer.Id);
                if (result == DataBaseResult.Inserted)
                    _navigationManager.NavigateTo("OfferOverview/"+true, true);
                if (result == DataBaseResult.Failed)
                    _navigationManager.NavigateTo("/OfferOverview/Offer/edit/"+Offer.Id+"/"+true, true);
            }
        }
        
        private async Task SelectLanguageOffer((Language, LanguageLevel) language,object checkedValue)
        {
            if (Offer == null) return;
            if ((bool) checkedValue)
            {
                var result = await _offerManager.UpdateLanguage(language, Offer.Id);
                if (result == DataBaseResult.Inserted)
                    _navigationManager.NavigateTo("OfferOverview/"+true, true);
                if (result == DataBaseResult.Failed)
                    _navigationManager.NavigateTo("/OfferOverview/Offer/edit/"+Offer.Id+"/"+true, true);
            }
        }

        
        private async Task SelectRateCardLevel(ChangeEventArgs e, Guid id)
        {
            if (Offer == null) return ;
            RateCardLevel r = Enum.Parse<RateCardLevel>((string) e.Value);
            var result = await _offerManager.UpdateRateCardLevel(r, Offer.Id, id);
            if (result == DataBaseResult.Inserted)
                _navigationManager.NavigateTo("OfferOverview/"+true, true);
            if (result == DataBaseResult.Failed)
                _navigationManager.NavigateTo("/OfferOverview/Offer/edit/"+Offer.Id+"/"+true, true);
        }
        
        private async Task SelectPlannedWeeklyHours(ChangeEventArgs e, Guid id)
        {
            if (Offer == null) return ;
            int p = int.Parse((string) e.Value);
            var result = await _offerManager.UpdatePlannedWeeklyHours(p, Offer.Id, id);
            if (result == DataBaseResult.Inserted)
                _navigationManager.NavigateTo("OfferOverview/"+true, true);
            if (result == DataBaseResult.Failed)
                _navigationManager.NavigateTo("/OfferOverview/Offer/edit/"+Offer.Id+"/"+true, true);
        }
        
        private async Task SelectDiscount(ChangeEventArgs e, Guid id)
        {
            if (Offer == null) return ;
            int i = int.Parse((string) e.Value);
            float p = i;
            p = p / 100;
            var result = await _offerManager.UpdateDiscount(p, Offer.Id, id);
            if (result == DataBaseResult.Inserted)
                _navigationManager.NavigateTo("OfferOverview/"+true, true);
            if (result == DataBaseResult.Failed)
                _navigationManager.NavigateTo("/OfferOverview/Offer/edit/"+Offer.Id+"/"+true, true);
        }
        
        private async Task RemoveProjects(Guid projectId, Guid offerId, Guid employeeId)
        {
            if (Offer == null) return ;
            var result = await _offerManager.RemoveProjects(projectId, offerId, employeeId);
            if (result == DataBaseResult.Inserted)
                _navigationManager.NavigateTo("OfferOverview/"+true, true);
            if (result == DataBaseResult.Failed)
                _navigationManager.NavigateTo("/OfferOverview/Offer/edit/"+Offer.Id+"/"+true, true);
        }
        
        private async Task RemoveSelectedExperience(Experience experience, Guid offerId, Guid employeeId)
        {
            if (Offer == null) return ;
            var result = await _offerManager.RemoveSelectedExperience(experience, offerId, employeeId);
            if (result == DataBaseResult.Inserted)
                _navigationManager.NavigateTo("OfferOverview/"+true, true);
            if (result == DataBaseResult.Failed)
                _navigationManager.NavigateTo("/OfferOverview/Offer/edit/"+Offer.Id+"/"+true, true);
        }
        
        private async Task RemoveShownEmployeeProperties(Guid employeeId, Guid offerId)
        {
            if (Offer == null) return ;
            var result = await _offerManager.RemoveShownEmployeeProperties(employeeId, offerId);
            if (result == DataBaseResult.Inserted)
                _navigationManager.NavigateTo("OfferOverview/"+true, true);
            if (result == DataBaseResult.Failed)
                _navigationManager.NavigateTo("/OfferOverview/Offer/edit/"+Offer.Id+"/"+true, true);
        }
        
        private async Task RemoveExperience(Experience experience, Guid offerId)
        {
            if (Offer == null) return ;
            var result = await _offerManager.RemoveExperience(experience, offerId);
            if (result == DataBaseResult.Inserted)
                _navigationManager.NavigateTo("OfferOverview/"+true, true);
            if (result == DataBaseResult.Failed)
                _navigationManager.NavigateTo("/OfferOverview/Offer/edit/"+Offer.Id+"/"+true, true);
        }

        private bool IsVisible(string s)
        {
            return string.IsNullOrEmpty(Filter) || s.Contains(Filter, StringComparison.OrdinalIgnoreCase);
        }
    }
}
