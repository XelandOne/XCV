using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using XCV.Data;
using XCV.Entities;
using XCV.Services;

namespace XCV.Pages
{
    public partial class OfferOverview
    {
        [Inject] public OfferManager OfferManager { get; set; }
        [Inject] public EmployeeManager EmployeeManager { get; set; }
        [Parameter] public Guid? Id { get; set; }
        private string? Offer { get; set; }
        [Parameter] public List<Offer> Offers { get; set; } = new();
        /// <summary>
        /// Boolean value set true if someone changed, updated or deleted a project at the same time as someone else.
        /// </summary>
        [Parameter] public bool Failed { get; set; }
        /// <summary>
        /// Boolean value (OnInitialized: true) set false if user wants to create a new offer, and set true again if offer is created
        /// </summary>
        private bool Hid { get; set; }
        /// <summary>
        /// A dictionary of employees (Guid) with list of belonging names(firstname, lastname)
        /// </summary>
        private Dictionary<Guid, (string, string)> EmployeeNames = new();
        
        /// <inheritdoc />
        protected override async Task OnInitializedAsync()
        {
            Hid = true;
            await OfferManager.Load();
            EmployeeNames = await EmployeeManager.GetAllNames();
            Offers = OfferManager.Offers;
            Offers = Offers.OrderByDescending(x => x.LastChanged).ToList();
        }
        
        protected override async Task OnParametersSetAsync()
        {
            if (Failed)
            {
                await Task.Delay(3000);
                Failed = false;
                _navigationManager.NavigateTo("OfferOverview/");
            }
        }


        public bool CreateOffer()
        {
            Hid = false;
            return true;
        }

        public async Task SaveOffer(string? newOffer)
        {
            if (newOffer == null) return;
            Offer offer = new Offer(newOffer);
            await OfferManager.UpdateOffer(offer);
            Offer = null;
            Hid = true;
            _navigationManager.NavigateTo("OfferOverview", true);
        }
    }
}