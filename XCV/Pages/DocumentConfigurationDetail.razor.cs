using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using XCV.Entities;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using XCV.Entities.Enums;

namespace XCV.Pages
{
    public partial class DocumentConfigurationDetail
    {
        [Parameter] public Guid OfferId { get; set; }
        [Parameter] public Guid ConfigurationId { get; set; }
        [Parameter] public Offer? Offer { get; set; }
        [Parameter] public DocumentConfiguration? Configuration { get; set; }
        private string? Title { get; set; }
        
        /// <inheritdoc />
        protected override async Task OnInitializedAsync()
        {
            await _offerManager.Load();
            var temp = _offerManager.GetOffer(OfferId);
            if (temp != null)
            {
                Offer = temp;
                foreach (var shownEmployeeProperties in Offer.ShortEmployees)
                {
                    await _employeeManager.LoadEmployee(shownEmployeeProperties.EmployeeId);
                }
            }

            Configuration = await _documentConfigurationManager.GetDocumentConfiguration(ConfigurationId);
        }
        
        private async Task ToggleEmployee(Guid employeeId, bool newValue)
        {
            if (Configuration == null) return;
            
            if (newValue && !Configuration.ShownEmployeePropertyIds.Contains(employeeId))
            {
                Configuration.ShownEmployeePropertyIds.Add(employeeId);
                await _documentConfigurationManager.UpdateDocumentConfiguration(Configuration);
            }
            else if (!newValue)
            {
                Configuration.ShownEmployeePropertyIds.Remove(employeeId);
                await _documentConfigurationManager.UpdateDocumentConfiguration(Configuration);
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
            if (Title != null && Configuration != null && Title.Length<101)
            {
                Configuration.Title = Title;
                await _documentConfigurationManager.UpdateDocumentConfiguration(Configuration);
            }
        }

        private async Task SelectCoverSheet(bool select)
        {
            if (Configuration == null) return;
            
            if (select)
            {
                Configuration.ShowCoverSheet = true;
                await _documentConfigurationManager.UpdateDocumentConfiguration(Configuration);
            }
            else
            {
                Configuration.ShowCoverSheet = false;
                await _documentConfigurationManager.UpdateDocumentConfiguration(Configuration);
            }
        }

        private async Task SelectPriceCalculation(bool select)
        {
            if (Configuration == null) return;
            
            if (select)
            {
                Configuration.IncludePriceCalculation = true;
                await _documentConfigurationManager.UpdateDocumentConfiguration(Configuration);
            }
            else
            {
                Configuration.IncludePriceCalculation = false;
                await _documentConfigurationManager.UpdateDocumentConfiguration(Configuration);
            }
        }

        private async Task SelectRequiredExperience(bool select)
        {
            if (Configuration == null) return;
            
            if (select)
            {
                Configuration.ShowRequiredExperience = true;
                await _documentConfigurationManager.UpdateDocumentConfiguration(Configuration);
            }
            else
            {
                Configuration.ShowRequiredExperience = false;
                await _documentConfigurationManager.UpdateDocumentConfiguration(Configuration);
            }
        }
        private async Task CopyConfiguration()
        {
            if (Configuration != null && Offer != null)
            {
                 string title = Configuration.Title + " Kopie";
                 DocumentConfiguration copy = new DocumentConfiguration(title, Configuration.ShowCoverSheet, Configuration.ShowRequiredExperience, Configuration.IncludePriceCalculation, Offer, Configuration.ShownEmployeePropertyIds);

                 await _documentConfigurationManager.UpdateDocumentConfiguration(copy);

                 _navigationManager.NavigateTo("/OfferOverview/Offer/"+OfferId+"/DocumentConfigurations", true);
            }
            
        }

        private async Task DeleteConfiguration()
        {
            if (Configuration != null && Offer != null)
            {
                await _documentConfigurationManager.DeleteDocumentConfiguration(ConfigurationId);
                _navigationManager.NavigateTo("/OfferOverview/Offer/"+OfferId+"/DocumentConfigurations", true);
            }
        }

        private async Task GenerateDocument()
        {
            if (Configuration != null) await _documentGenerationService.GenerateDocument(Configuration);
        }
    }
}