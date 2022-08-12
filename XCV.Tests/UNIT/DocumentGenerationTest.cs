using System;
using System.Collections.Generic;
using BlazorDownloadFile;
using Moq;
using NUnit.Framework;
using XCV.Data;
using XCV.Entities;
using XCV.Entities.Enums;
using XCV.Services;

namespace XCV.Tests.UNIT
{
    [TestFixture]
    public class DocumentGenerationTest
    {
        [Test]
        public void GenerateDocumentTest()
        {
            var field = new Field("Brillen");
            var field2 = new Field("Erneuerbare Energien");
            var hardSkill = (new HardSkill("C#", ""), HardSkillLevel.Expert);
            var hardSkill2 = (new HardSkill("Java", ""), HardSkillLevel.ProductiveUse);
            var softSkill = new SoftSkill("Teamkompetenz");

            var project1 = new Project("Brillenvermessung", field, DateTime.Now,
                DateTime.Now.Add(TimeSpan.FromDays(3)),
                "Dies ist ein Projekt zur Vermessung von Brillen.");
            var projectActivity1 = new ProjectActivity("Entwicklung des Backends.");
            var projectActivity2 = new ProjectActivity("Kommunikation mit dem Kunden.");
            project1.ProjectActivities.Add(projectActivity1);
            project1.ProjectActivities.Add(projectActivity2);
            var project2 = new Project("Spracherkennung", field, DateTime.Now,
                null,
                "Dieses Projekt beschäftigt sich mit der Erkennung von Spracheingaben.");
            Employee employee1 = new Employee(Authorizations.Admin, "Maier", "Christian", "christianm", DateTime.Now,
                1, 2, 3, RateCardLevel.Level3, null);
            Employee employee2 = new Employee(Authorizations.Sales, "Müller", "Manuel", "manuelm", DateTime.Now.AddMonths(-12),
                3, 1, 0, RateCardLevel.Level8, null);
            Employee employee3 = new Employee(Authorizations.Sales, "Bauer", "Sarah", "sarahb", DateTime.Now.AddMonths(-24),
                3, 1, 0, RateCardLevel.Level8, null);
            employee1.Experience.HardSkills.Add(hardSkill);
            employee1.Experience.HardSkills.Add(hardSkill2);
            employee1.Experience.Fields.Add(field);
            employee1.Experience.Fields.Add(field2);
            employee1.Experience.SoftSkills.Add(softSkill);
            employee1.Experience.Roles.Add(new Role("PO"));
            employee1.Experience.Languages.Add(new(new Language("Spanisch"), LanguageLevel.Advanced));
            employee1.ProjectIds.Add(project1.Id);
            employee1.ProjectIds.Add(project2.Id);
            var offer = new Offer("Test Title", DateTime.Now , DateTime.Now.AddDays(8 * 7),new List<Employee>() {employee1, employee2, employee3});
            
            offer.Experience.Fields.Add(field);
            offer.Experience.Fields.Add(field2);
            offer.Experience.HardSkills.Add(hardSkill);
            offer.Experience.HardSkills.Add(hardSkill2);

            offer.ShortEmployees[0].Discount = 0.1;
            offer.ShortEmployees[0].PlannedWeeklyHours = 32;
            offer.ShortEmployees[0].ProjectActivityIds.Add(projectActivity1.Id);
            offer.ShortEmployees[0].ProjectActivityIds.Add(projectActivity2.Id);
            
            offer.ShortEmployees[1].Discount = 0.2;
            offer.ShortEmployees[1].PlannedWeeklyHours = 38;

            var documentConfiguration =
                new DocumentConfiguration("Config Title", true, true, true, offer, offer.ShortEmployees.ConvertAll(se => se.Id));
           
            
            

            var mockBlazorDownloadFileService = new Mock<IBlazorDownloadFileService>();
            mockBlazorDownloadFileService.Setup(m => m.DownloadFile(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>())).ReturnsAsync(new DowloadFileResult() {Succeeded = true});

            var mockEmployeeService = new Mock<IEmployeeService>();
            mockEmployeeService.Setup(m => m.GetEmployee(employee1.Id)).ReturnsAsync(employee1);
            mockEmployeeService.Setup(m => m.GetEmployee(employee2.Id)).ReturnsAsync(employee2);
            mockEmployeeService.Setup(m => m.GetEmployee(employee3.Id)).ReturnsAsync(employee3);

            var mockOfferService = new Mock<IOfferService>();
            mockOfferService.Setup(m => m.GetOffer(offer.Id)).ReturnsAsync(offer);

            var mockShownEmployeePropertiesService = new Mock<IShownEmployeePropertiesService>();
            mockShownEmployeePropertiesService.Setup(m => m.GetShownEmployeeProperties(offer.ShortEmployees[0].Id))
                .ReturnsAsync(offer.ShortEmployees[0]);
            mockShownEmployeePropertiesService.Setup(m => m.GetShownEmployeeProperties(offer.ShortEmployees[1].Id))
                .ReturnsAsync(offer.ShortEmployees[1]);
            mockShownEmployeePropertiesService.Setup(m => m.GetShownEmployeeProperties(offer.ShortEmployees[2].Id))
                .ReturnsAsync(offer.ShortEmployees[2]);

            var mockProjectService = new Mock<IProjectService>();
            mockProjectService.Setup(m => m.GetProject(project1.Id))
                .ReturnsAsync(project1);
            mockProjectService.Setup(m => m.GetProject(project2.Id))
                .ReturnsAsync(project2);


            var wordDocumentGenerationService = new WordDocumentGenerationService(mockBlazorDownloadFileService.Object,
                mockEmployeeService.Object, mockOfferService.Object, mockShownEmployeePropertiesService.Object,
                mockProjectService.Object, new DummyHourlyWagesService());

            
            var documentSuccess = wordDocumentGenerationService.GenerateDocument(documentConfiguration).Result;
            Assert.IsTrue(documentSuccess, "Error occured when exporting the document configuration.");
        }
    }
}