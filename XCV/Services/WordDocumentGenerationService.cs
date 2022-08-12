using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using BlazorDownloadFile;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using OpenXmlPowerTools;
using XCV.Data;
using XCV.Entities;
using XCV.Entities.Enums;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Source = OpenXmlPowerTools.Source;

namespace XCV.Services
{
    /// <summary>
    /// This class is responsible for converting a word document from a document configuration.
    /// </summary>
    public class WordDocumentGenerationService : IDocumentGenerationService
    {
        private readonly IBlazorDownloadFileService _blazorDownloadFileService;

        private readonly IEmployeeService _employeeService;
        private readonly IOfferService _offerService;
        private readonly IProjectService _projectService;
        private readonly IShownEmployeePropertiesService _shownEmployeePropertiesService;
        private readonly IHourlyWagesService _hourlyWagesService;


        private static readonly string WordTemplatesPath = Path.Combine(".", "Files", "WordTemplates");

        private static readonly string CoverSheetTemplatePath =
            Path.Combine(WordTemplatesPath, "OfferCoverSheet.docx");

        private static readonly string OfferExperienceTemplatePath =
            Path.Combine(WordTemplatesPath, "OfferExperienceTemplate.docx");

        private static readonly string EmployeePageTemplatePath =
            Path.Combine(WordTemplatesPath, "EmployeePageTemplate.docx");

        private static readonly string PricesPageTemplatePath =
            Path.Combine(WordTemplatesPath, "PricesPageTemplate.docx");

        private static readonly string ProfilePictureName = "image1";

        private static readonly CultureInfo CultureInfo = new CultureInfo("de-DE");


        public WordDocumentGenerationService(IBlazorDownloadFileService blazorDownloadFileService,
            IEmployeeService employeeService, IOfferService offerService,
            IShownEmployeePropertiesService shownEmployeePropertiesService, IProjectService projectService,
            IHourlyWagesService hourlyWagesService)
        {
            _blazorDownloadFileService = blazorDownloadFileService;
            _employeeService = employeeService;
            _offerService = offerService;
            _shownEmployeePropertiesService = shownEmployeePropertiesService;
            _projectService = projectService;
            _hourlyWagesService = hourlyWagesService;
        }

        /// <summary>
        /// Generates a Word document out of a document configuration and downloads it.
        /// </summary>
        /// <param name="documentConfiguration">The document configuration</param>
        /// <returns>True if the generation was successful. False if an error occured.</returns>
        public async Task<bool> GenerateDocument(DocumentConfiguration documentConfiguration)
        {
            List<WmlDocument> wmlPages = new();
            var offer = await _offerService.GetOffer(documentConfiguration.OfferId);
            if (offer == null) return false;
            var offerXml = GetOfferXml(offer).GetXmlDocument();

            if (documentConfiguration.ShowCoverSheet)
            {
                var coverSheet = GenerateWmlDocumentFromXml(offerXml, CoverSheetTemplatePath);
                if (coverSheet != null)
                    wmlPages.Add(coverSheet);
            }

            if (documentConfiguration.ShowRequiredExperience)
            {
                var offerExperiencePage = GenerateWmlDocumentFromXml(offerXml, OfferExperienceTemplatePath);
                if (offerExperiencePage != null)
                    wmlPages.Add(offerExperiencePage);
            }

            var configuredEmployees = await GetConfiguredEmployees(documentConfiguration);
            if (configuredEmployees == null) return false;

            var wmlEmployeePages = GenerateEmployeePages(configuredEmployees);
            if (wmlEmployeePages == null) return false;
            wmlPages.AddRange(wmlEmployeePages);

            if (documentConfiguration.IncludePriceCalculation)
            {
                TimeSpan? projectDuration = offer.StartDate.HasValue && offer.EndDate.HasValue
                    ? offer.EndDate.Value.Subtract(offer.StartDate.Value)
                    : null;
                var pricesPage = GenerateWmlDocumentFromXml(GetPriceXml(projectDuration,
                    configuredEmployees).GetXmlDocument(), PricesPageTemplatePath);
                if (pricesPage != null)
                    wmlPages.Add(pricesPage);
            }

            DownloadDocument(CombinePages(wmlPages), documentConfiguration.Title);
            return true;
        }

        private List<WmlDocument>? GenerateEmployeePages(IEnumerable<ConfiguredEmployee> configuredEmployees)
        {
            List<WmlDocument> wmlEmployeePages = new();
            foreach (var configuredEmployee in configuredEmployees)
            {
                var employeeXml = EmployeeToXml(configuredEmployee);

                var employeeWml = GenerateWmlDocumentFromXml(employeeXml.GetXmlDocument(), EmployeePageTemplatePath);
                if (employeeWml == null) continue;

                if (configuredEmployee.ProfilePicture != null)
                    employeeWml = ReplaceDefaultProfilePicture(employeeWml, configuredEmployee.ProfilePicture);

                wmlEmployeePages.Add(employeeWml);
            }

            return wmlEmployeePages;
        }

        private async Task<List<ConfiguredEmployee>?> GetConfiguredEmployees(
            DocumentConfiguration documentConfiguration)
        {
            List<ConfiguredEmployee> configuredEmployees = new();
            foreach (var shownEmployeeProperties in documentConfiguration.ShownEmployeePropertyIds)
            {
                var employeeProperties =
                    await _shownEmployeePropertiesService.GetShownEmployeeProperties(shownEmployeeProperties);
                if (employeeProperties == null) return null;
                var employee = await _employeeService.GetEmployee(employeeProperties.EmployeeId);
                if (employee == null) return null;

                List<Project> projects = new();
                foreach (var projectId in employeeProperties.ProjectIds)
                {
                    var project = await _projectService.GetProject(projectId);
                    if (project == null) return null;
                    projects.Add(project);
                }

                var hourlyWage = await _hourlyWagesService.GetHourlyWage(employee.RateCardLevel);
                ConfiguredEmployee configuredEmployee =
                    new(employee, employeeProperties, projects, hourlyWage);
                configuredEmployees.Add(configuredEmployee);
            }

            return configuredEmployees;
        }

        private XDocument GetOfferXml(Offer offer)
        {
            XDocument xmlTree = new XDocument(
                new XElement("Root",
                    new XElement("Title", offer.Title),
                    (offer.StartDate.HasValue
                        ? new XElement("StartDate",
                            "Projektbeginn: " + offer.StartDate.Value.ToString("d", CultureInfo))
                        : null)!,
                    (offer.EndDate.HasValue
                        ? new XElement("EndDate", "Projektende: " + offer.EndDate.Value.ToString("d", CultureInfo))
                        : null)!,
                    new XElement("Experience",
                        new XElement("Fields",
                            offer.Experience.Fields.Select(f => new XElement("Name", f.Name))),
                        new XElement("Roles", offer.Experience.Roles.Select(r => new XElement("Name", r.Name))),
                        new XElement("SoftSkills",
                            offer.Experience.SoftSkills.Select(s => new XElement("Name", s.Name))),
                        new XElement("HardSkills",
                            offer.Experience.HardSkills.Select(h => new XElement("HardSkill",
                                new XElement("Name", h.Item1.Name),
                                new XElement("Level", HardSkillLevelHelper.ToFriendlyString(h.Item2))))
                        ),
                        new XElement("Languages",
                            offer.Experience.Languages.Select(l => new XElement("Language",
                                new XElement("Name", l.Item1.Name),
                                new XElement("Level", LanguageLevelHelper.ToFriendlyString(l.Item2))))
                        )
                    )
                )
            );

            return xmlTree;
        }


        private XDocument EmployeeToXml(ConfiguredEmployee employee)
        {
            var xmlTree = new XDocument(
                new XElement("Root",
                    new XElement("FirstName", employee.FirstName),
                    new XElement("SurName", employee.SurName),
                    new XElement("EmployedSince", employee.EmployedSince.ToString("d", CultureInfo)),
                    new XElement("RelevantWorkExperience", employee.RelevantWorkExperience),
                    new XElement("RateCardLevel", RateCardLevelHelper.ToFriendlyString(employee.RateCardLevel)),
                    new XElement("Experience",
                        new XElement("Fields",
                            employee.Experience.Fields.Select(f => new XElement("Name", f.Name))),
                        new XElement("Roles", employee.Experience.Roles.Select(r => new XElement("Name", r.Name))),
                        new XElement("SoftSkills",
                            employee.Experience.SoftSkills.Select(s => new XElement("Name", s.Name))),
                        new XElement("HardSkills",
                            employee.Experience.HardSkills.Select(h => new XElement("HardSkill",
                                new XElement("Name", h.Item1.Name),
                                new XElement("Level",
                                    HardSkillLevelHelper
                                        .ToFriendlyString(h.Item2))))
                        ),
                        new XElement("Languages",
                            employee.Experience.Languages.Select(l => new XElement("Language",
                                new XElement("Name", l.Item1.Name),
                                new XElement("Level", LanguageLevelHelper.ToFriendlyString(l.Item2))))
                        )
                    ),
                    new XElement("Projects",
                        employee.Projects.Select(p =>
                            new XElement("Project",
                                new XElement("Title", p.Title),
                                new XElement("ProjectDescription", p.ProjectDescription),
                                new XElement("ProjectActivities",
                                    string.Join("\n", p.ProjectActivities
                                        .Where(pa => pa.GetEmployeeIds().Contains(employee.EmployeeId))
                                        .Select(pa =>
                                    {
                                        return pa.Description;
                                    }).ToList())
                                ),
                                (p.Field != null ? new XElement("Field", p.Field?.Name) : null)!,
                                new XElement("StartDate", p.StartDate.ToString("d", CultureInfo)),
                                (p.EndDate != null
                                    ? new XElement("EndDate",
                                        "\nEnddatum: " + p.EndDate.Value.ToString("d", CultureInfo))
                                    : null)!
                            )
                        )
                    )
                )
            );

            return xmlTree;
        }

        private XDocument GetPriceXml(TimeSpan? projectDuration,
            IEnumerable<ConfiguredEmployee> configuredEmployees)
        {
            double totalPriceDiscounted = 0;
            double totalPrice = 0;
            double totalFte = 0;
            double totalManDays = 0;
            double? projectWeeks = new();
            if (projectDuration.HasValue) projectWeeks = projectDuration.Value.Days / 7.0;

            XDocument xmlTree = new XDocument(
                new XElement("Root",
                    (projectWeeks.HasValue
                        ? new XElement("ProjectDuration", projectWeeks.Value.ToString("F2", CultureInfo) + " Wochen")
                        : null)!,
                    new XElement("Employees", configuredEmployees.Select(ce =>
                        {
                            double? employeePriceDiscounted = new();
                            double? fte = new();
                            double? manDays = new();


                            if (ce.PlannedWeeklyHours.HasValue &&
                                projectWeeks.HasValue)
                            {
                                if (ce.HourlyWageDiscounted != null)
                                {
                                    employeePriceDiscounted =
                                        ce.HourlyWageDiscounted.Value * ce.PlannedWeeklyHours.Value *
                                        projectWeeks.Value;
                                    if (employeePriceDiscounted != null)
                                        totalPriceDiscounted += employeePriceDiscounted.Value;
                                }

                                if (ce.HourlyWage != null)
                                {
                                    var employeePrice =
                                        ce.HourlyWage.Value * ce.PlannedWeeklyHours.Value *
                                        projectWeeks.Value;
                                    totalPrice += employeePrice;
                                }

                                manDays = projectWeeks.Value * ce.PlannedWeeklyHours.Value / 8;
                                totalManDays += manDays.Value;
                            }

                            if (ce.PlannedWeeklyHours.HasValue)
                            {
                                fte = ce.PlannedWeeklyHours.Value / 40.0;
                                totalFte += fte.Value;
                            }

                            return new XElement("Employee",
                                (ce.Experience.Roles.Count > 0
                                    ? new XElement("Roles", string.Join(", ", ce.Experience.Roles.Select(r => r.Name)))
                                    : null)!,
                                new XElement("RateCardLevel", RateCardLevelHelper.ToFriendlyString(ce.RateCardLevel)),
                                (ce.DailyRate.HasValue && ce.Discount != 0
                                    ? new XElement("DailyRate", ce.DailyRate.Value.ToString("C", CultureInfo) + "/PT")
                                    : null)!,
                                (ce.Discount != 0
                                    ? new XElement("Discount", ce.Discount.ToString("P2", CultureInfo))
                                    : null)!,
                                (ce.DailyRateDiscounted.HasValue
                                    ? new XElement("DailyRateDiscounted",
                                        ce.DailyRateDiscounted.Value.ToString("C", CultureInfo) + "/PT")
                                    : null)!,
                                (ce.HourlyWageDiscounted.HasValue
                                    ? new XElement("HourlyWageDiscounted",
                                        ce.HourlyWageDiscounted.Value.ToString("C", CultureInfo) + "/h")
                                    : null)!,
                                (ce.PlannedWeeklyHours.HasValue
                                    ? new XElement("PlannedWeeklyHours", ce.PlannedWeeklyHours.Value + " h/Woche")
                                    : null)!,
                                (fte.HasValue
                                    ? new XElement("FTE",
                                        fte.Value.ToString("F2", CultureInfo) + " FTE")
                                    : null)!,
                                (manDays.HasValue
                                    ? new XElement("ManDays", manDays.Value.ToString("F2", CultureInfo) + " PT")
                                    : null)!,
                                (employeePriceDiscounted.HasValue && employeePriceDiscounted.Value != 0
                                    ? new XElement("EmployeePrice",
                                        employeePriceDiscounted.Value.ToString("C", CultureInfo))
                                    : null)!
                            );
                        })
                    ),
                    new XElement("TotalPrice", totalPrice.ToString("C", CultureInfo)),
                    new XElement("TotalPriceDiscounted", totalPriceDiscounted.ToString("C", CultureInfo)),
                    new XElement("DiscountSum", "- " + (totalPrice - totalPriceDiscounted).ToString("C", CultureInfo)),
                    (totalPrice != 0 ? new XElement("AverageDiscount",
                        ((totalPrice - totalPriceDiscounted) / totalPrice).ToString("P2", CultureInfo)) : null)!,
                    (projectWeeks.HasValue && totalFte != 0 ? new XElement("AveragePricePerManDay",
                        (((totalPriceDiscounted / projectWeeks.Value) / (totalFte * 40)) * 8)
                        .ToString("C", CultureInfo) + "/PT") : null)!,
                    (projectWeeks.HasValue && totalFte != 0 ? new XElement("AveragePricePerHour",
                        ((totalPriceDiscounted / projectWeeks.Value) / (totalFte * 40)).ToString("C", CultureInfo) +
                        "/h") : null)!,
                    new XElement("TotalFte", totalFte.ToString("F2", CultureInfo) + " FTE"),
                    new XElement("TotalManDays", totalManDays.ToString("F2", CultureInfo) + " PT")
                )
            );

            return xmlTree;
        }

        private WmlDocument? GenerateWmlDocumentFromXml(XmlDocument xmlDocument, string templatePath)
        {
            var templateDocument = new FileInfo(templatePath);
            var wmlDocument = new WmlDocument(templateDocument.FullName);

            var wmlCreatedDocument =
                DocumentAssembler.AssembleDocument(wmlDocument, xmlDocument, out var templateError);
            return wmlCreatedDocument;
        }

        private WmlDocument CombinePages(List<WmlDocument> pages)
        {
            List<Source> sources = new();
            pages.ForEach(p => sources.Add(new Source(p, true)));
            return DocumentBuilder.BuildDocument(sources);
        }

        /// <summary>
        /// This method downloads the given wml document in the clients browser.
        /// </summary>
        /// <param name="wmlDocument">The wml document to download</param>
        /// <param name="documentName">The name of the download file</param>
        private void DownloadDocument(OpenXmlPowerToolsDocument wmlDocument, string documentName)
        {
            _blazorDownloadFileService.DownloadFile(documentName + ".docx",
                wmlDocument.DocumentByteArray,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document"); 
        }

        /// <summary>
        /// For testing purposes only!
        /// This method downloads the given wml document to the server.
        /// </summary>
        /// <param name="wmlDocument">The wml document to download</param>
        /// <param name="documentName">The name of the download file</param>
        private static void DownloadDocumentToServer(OpenXmlPowerToolsDocument wmlDocument, string documentName)
        {
            var path = Path.Combine(".", documentName + ".docx");
            File.WriteAllBytes(path, wmlDocument.DocumentByteArray);
        }

        private WmlDocument ReplaceDefaultProfilePicture(WmlDocument wmlDocument, byte[] profilePicture)
        {
            var documentByteArray = wmlDocument.DocumentByteArray;
            using var memoryStream = new MemoryStream();
            memoryStream.Write(documentByteArray, 0, documentByteArray.Length);

            using (var wordprocessingDocument = WordprocessingDocument.Open(memoryStream, true))
            {
                var imagePart = GetImagePart(wordprocessingDocument, ProfilePictureName);
                if (imagePart != null)
                {
                    using var writer = new BinaryWriter(imagePart.GetStream());
                    writer.Write(profilePicture);
                }
            }


            return new WmlDocument(wmlDocument.FileName, memoryStream.ToArray());
        }

        private ImagePart? GetImagePart(WordprocessingDocument document, string imageName)
        {
            return document.MainDocumentPart?.ImageParts // or EndsWith
                .First(p => p.Uri.ToString().Contains(imageName));
        }
    }

    /// <summary>
    /// Represents an configured employee from an offer with loaded projects.
    /// </summary>
    public class ConfiguredEmployee
    {
        public Guid EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string SurName { get; set; }

        public DateTime EmployedSince { get; set; }
        public int RelevantWorkExperience { get; set; }

        public RateCardLevel RateCardLevel { get; set; }
        public byte[]? ProfilePicture { get; set; }
        public UsedExperience Experience { get; }
        public List<Project> Projects { get; }
        public List<Guid> ProjectActivityIds { get; }
        public int? PlannedWeeklyHours { get; set; }
        public double Discount { get; set; }
        public double? HourlyWage { get; set; }
        public double? DailyRate => HourlyWage * WorkingHoursPerDay;
        public double? DailyRateDiscounted => DailyRate * (1 - Discount);
        public double? HourlyWageDiscounted => HourlyWage * (1 - Discount);

        private const double WorkingHoursPerDay = 8;

        public ConfiguredEmployee(Employee employee, ShownEmployeeProperties shownEmployeeProperties,
            List<Project> projects, double? hourlyWage)
        {
            EmployeeId = employee.Id;
            //Properties from employee
            FirstName = employee.FirstName;
            SurName = employee.SurName;
            EmployedSince = employee.EmployedSince;
            RelevantWorkExperience = employee.CalcRelevantWorkExperience();

            //Properties from shownEmployeeProperties
            RateCardLevel = shownEmployeeProperties.RateCardLevel;
            ProfilePicture = employee.ProfilePicture;
            Experience = shownEmployeeProperties.Experience;
            PlannedWeeklyHours = shownEmployeeProperties.PlannedWeeklyHours;
            Discount = shownEmployeeProperties.Discount;
            ProjectActivityIds = shownEmployeeProperties.ProjectActivityIds;

            Projects = projects;

            HourlyWage = hourlyWage;
        }
    }
}