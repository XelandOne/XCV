using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Components;
using XCV.Entities;


namespace XCV.Pages
{
    public partial class DocumentConfigurationOverview
    {
        [Parameter] public Guid Id { get; set; }
        [Parameter] public Offer? Offer { get; set; }
        [Parameter] public List<DocumentConfiguration> Configurations { get; set; } = new();
        
        /// <summary>
        /// A dictionary of employees (Guid) with list of belonging names(firstname, lastname)
        /// </summary>
        private Dictionary<Guid, (string, string)> _employeeNames = new();
        private string? Title { get; set; }
        
        /// <summary>
        /// Boolean value (OnInitialized: true) set false if user wants to create a new configuration, and set true again if configuration is created
        /// </summary>
        private bool Hid { get; set; }
        
        /// <inheritdoc />
        protected override async Task OnInitializedAsync()
        {
            Hid = true;

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

            _employeeNames = await _employeeManager.GetAllNames();

            if (Offer != null)
            {
                var configs = Offer.DocumentConfigurations;
                //var configs =  await _documentConfigurationManager.GetDocumentConfigurations(Offer.Id);
                foreach (var config in configs)
                {
                    var configuration = await _documentConfigurationManager.GetDocumentConfiguration(config);
                    if (configuration != null)
                    {
                        Configurations.Add(configuration);
                    }
                }
            }
        }
        
        private void CreateConfiguration()
        {
            Hid = false;
        }

        private async Task SaveConfiguration(string? title)
        {
            if (title == null) return;
            if (Offer != null)
            {
                DocumentConfiguration configuration = new DocumentConfiguration(title, Offer);
                Configurations.Add(configuration);
                await _documentConfigurationManager.UpdateDocumentConfiguration(configuration);
            }

            Title = "";

            Hid = true;
        }
    }
}