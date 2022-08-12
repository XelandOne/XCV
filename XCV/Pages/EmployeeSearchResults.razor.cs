using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using XCV.Entities;
using XCV.Services;

namespace XCV.Pages
{
    public partial class EmployeeSearchResults
    {
        private bool _showModal;
        private OfferModel _model = new OfferModel();
        /// <summary>
        /// String value regarding the searched Filter Term
        /// </summary>
        private string? Filter { get; set; }
        /// <summary>
        /// Boolean value set true if searched info equals the displayed skill
        /// </summary>
        private bool Hid { get; set; }
        [Parameter] public Guid Id { get; set; }
        /// <summary>
        /// Boolean value set true if search has been started out of offer
        /// </summary>
        [Parameter] public bool Direct { get; set; }
        /// <summary>
        /// List of employees found by initial search 
        /// </summary>
        [Parameter] public List<(Employee, List<Guid>)> EmployeeSelection { get; set; } = new();
        /// <summary>
        /// List of employees selected to add to offer/create new offer  
        /// </summary>
        [Parameter] public List<Guid> SelectedEmployees { get; set; } = new();
        /// <summary>
        /// String value for creation of new offer
        /// </summary>
        private string? Offer { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (Direct)
            {
                _searchManager.RemoveSelected();
                var offer = _offerManager.Offers.Find(x => x.Id.Equals(Id));
                if (offer != null)
                {
                    offer.Experience.Fields.ForEach(x => _searchManager.SelectExperience(x.Id, true));
                    offer.Experience.Languages.ForEach(x => _searchManager.SelectExperience(x.Item1.Id, true));
                    offer.Experience.Roles.ForEach(x => _searchManager.SelectExperience(x.Id, true));
                    offer.Experience.HardSkills.ForEach(x => _searchManager.SelectExperience(x.Item1.Id, true));
                    offer.Experience.SoftSkills.ForEach(x => _searchManager.SelectExperience(x.Id, true));
                }
            }

            await _employeeManager.Load();
            EmployeeSelection = _searchManager.GetSearchResult();
            
            EmployeeSelection.ForEach(x => x.Item2.Sort());
            EmployeeSelection.Sort((a, b) => b.Item2.Count - a.Item2.Count);
        }

        private bool IsVisible(string s)
        {
            return string.IsNullOrEmpty(Filter) || s.Contains(Filter, StringComparison.OrdinalIgnoreCase);
        }

        private void UpdateSelected(Guid employee, object checkedValue)
        {
            if ((bool) checkedValue)
            {
                if (!SelectedEmployees.Contains(employee))
                    SelectedEmployees.Add(employee);
            }
            else
            {
                SelectedEmployees.Remove(employee);
            }
        }

        private async Task SaveOffer()
        {
            var newOffer = _model.title.Trim();
            Offer offer = new (newOffer);
            await _offerManager.UpdateOffer(offer);

            //var of = _offerManager.Offers.Find(o => o.Title.Equals(newOffer));
            //if (of != null)
            //{
            await AddToOffer(offer.Id);
                //_navManager.NavigateTo("OfferOverview/Offer/" + offer.Id);
            //}
        }

        private async Task AddToOffer(Guid offerId)
        {
            foreach (var cache in SelectedEmployees)
            {
                await _offerManager.AddShownEmployeeProperties(EmployeeSelection.Find(x => x.Item1.Id.Equals(cache)).Item1, offerId);
            }
            _navManager.NavigateTo("OfferOverview/Offer/" + offerId, true);
        }

        private void CloseModal()
        {
            _showModal = false;
        }

        private class OfferModel
        {
            [Required(ErrorMessage = "Titel ist erforderlich")]
            [StringLength(175, ErrorMessage = "Titel darf nur 175 Zeichen lang sein")]
            public string title { get; set; }
        }
    }
}