using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using XCV.Data;
using XCV.Entities;
using XCV.Entities.Enums;
using XCV.Services;

namespace XCV.Tests.UNIT.ServiceTest
{
    public class ExperienceServiceTest
    {
        private DatabaseUtils _databaseUtils;

        private IExperienceService _experienceService;


        [OneTimeSetUp]
        public void SetUp()
        {
            var config = InitConfiguration();
            this._databaseUtils = new DatabaseUtils(config);
            this._experienceService = new ExperienceService(_databaseUtils);
            
            _databaseUtils.LoadTables();
        }
        public static IEnumerable<TestCaseData> ExperienceTestCases
        {
            get
            {
                yield return new TestCaseData(new HardSkill("hardSkill", "Category"));
                yield return new TestCaseData(new SoftSkill("SoftSkill"));
                yield return new TestCaseData(new Language("Language"));
                yield return new TestCaseData(new Role("Role"));
                yield return new TestCaseData(new Field("field"));
            }
        }

        private static IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder().AddJsonFile(Path.Combine(".", "appsettings.Development.json"))
                .Build();
            return config;
        }

        [Test]
        public async Task HardSkillTest()
        {
            var hardSkill = new HardSkill("Fahrradreperatur", "FahrradKategorie");

            var hardSkills = await _experienceService.LoadHardSkills();

            foreach (var skill in hardSkills)
            {
                if (hardSkill.Name == skill.Name)
                {
                    await _experienceService.DeleteExperience(skill);
                }
            }

            await _experienceService.UpdateExperience(hardSkill);

            var hardSkillTest = (HardSkill) await _experienceService.GetExperience(hardSkill.Id);

            if (!hardSkill.Equals(hardSkillTest))
            {
                Assert.Fail("HardSkill was not properly inserted");
            }

            await _experienceService.DeleteExperience(hardSkill);

            var hardSkillIsDeleted = await _experienceService.LoadHardSkills();

            foreach (var skill in hardSkillIsDeleted)
            {
                if (hardSkill.Id.Equals(skill.Id))
                {
                    Assert.Fail("HardSkill was not properly deleted");
                }
            }
            
            Assert.Pass("HardSkill was properly inserted");
        }
        
        [Test]
        public async Task SoftSkillTest()
        {
            var softSkill = new SoftSkill("Kinderbetreuung");

            var softSkills = await _experienceService.LoadSoftSkills();

            foreach (var skill in softSkills)
            {
                if (softSkill.Name == skill.Name)
                {
                    await _experienceService.DeleteExperience(skill);
                }
            }

            await _experienceService.UpdateExperience(softSkill);

            var softSkillTest = (SoftSkill) await _experienceService.GetExperience(softSkill.Id);

            if (!softSkill.Equals(softSkillTest))
            {
                Assert.Fail("SoftSkill was not properly inserted");
            }

            await _experienceService.DeleteExperience(softSkill);
            
            var softSkillIsDeleted = await _experienceService.LoadSoftSkills();

            foreach (var skill in softSkillIsDeleted)
            {
                if (softSkill.Id.Equals(skill.Id))
                {
                    Assert.Fail("SoftSkill was not properly deleted");
                }
            }
            
            Assert.Pass("SoftSkill was properly inserted");
        }
        
        [Test]
        public async Task FieldTest()
        {
            var field = new Field("Freibad");
            
            var fields = await _experienceService.LoadFields();

            foreach (var skill in fields)
            {
                if (field.Name == skill.Name)
                {
                    await _experienceService.DeleteExperience(skill);
                }
            }

            await _experienceService.UpdateExperience(field);

            var fieldTest = (Field) await _experienceService.GetExperience(field.Id);

            if (!field.Equals(fieldTest))
            {
                Assert.Fail("Field was not properly inserted");
            }

            await _experienceService.DeleteExperience(field);
            
            var fieldIsDeleted = await _experienceService.LoadFields();

            foreach (var skill in fieldIsDeleted)
            {
                if (field.Id.Equals(skill.Id))
                {
                    Assert.Fail("Field was not properly deleted");
                }
            }
            
            Assert.Pass("Field was properly inserted");
        }
        
        [Test]
        public async Task RoleTest()
        {
            var role = new Role("Bademeister");

            var roles = await _experienceService.LoadRoles();

            foreach (var skill in roles)
            {
                if (role.Name == skill.Name)
                {
                    await _experienceService.DeleteExperience(skill);
                }
            }
            
            await _experienceService.UpdateExperience(role);

            var roleTest = (Role) await _experienceService.GetExperience(role.Id);

            if (!role.Equals(roleTest))
            {
                Assert.Fail("Role was not properly inserted");
            }

            await _experienceService.DeleteExperience(role);
            
            var roleIsDeleted = await _experienceService.LoadRoles();

            foreach (var skill in roleIsDeleted)
            {
                if (role.Id.Equals(skill.Id))
                {
                    Assert.Fail("Role was not properly deleted");
                }
            }
            
            Assert.Pass("Role was properly inserted");
        }
        
        [Test]
        public async Task LanguageTest()
        {
            var language = new Language("kolumbisch");
            
            var languages = await _experienceService.LoadLanguages();

            foreach (var skill in languages)
            {
                if (language.Name == skill.Name)
                {
                    await _experienceService.DeleteExperience(skill);
                }
            }

            await _experienceService.UpdateExperience(language);

            var languageTest = (Language) await _experienceService.GetExperience(language.Id);

            if (!language.Equals(languageTest))
            {
                Assert.Fail("Language was not properly inserted");
            }

            await _experienceService.DeleteExperience(language);
            
            var languageIsDeleted = await _experienceService.LoadLanguages();

            foreach (var skill in languageIsDeleted)
            {
                if (language.Id.Equals(skill.Id))
                {
                    Assert.Fail("Language was not properly deleted");
                }
            }
            
            Assert.Pass("Language was properly inserted");
        }

        [TestCaseSource(nameof(ExperienceTestCases))]
        public async Task UpdateExperienceTest_ShouldUpdate_WhenExits(Experience exp)
        {
            await _experienceService.UpdateExperience(exp);

            exp.Name = "updated";
            Assert.AreEqual((await _experienceService.UpdateExperience(exp)).Item2, DataBaseResult.Updated);
            Assert.AreEqual(exp, await _experienceService.GetExperience(exp.Id));
            await _experienceService.DeleteExperience(exp);
        }

        [Test]
        public async Task GetExperienceTest_ShouldReturnNull_WhenNotExits()
        {
            Assert.Null(await _experienceService.GetExperience(Guid.NewGuid()));
        }

        [Test]
        public async Task GetEmptyExperienceList()
        {
            var languages = await _experienceService.LoadLanguages();
            foreach (var language in languages)
            {
                await _experienceService.DeleteExperience(language);
            }

            var emptyLanguages = await _experienceService.LoadLanguages();
            
            Assert.IsEmpty(emptyLanguages);

            foreach (var language in languages)
            {
                await _experienceService.UpdateExperience(language);
            }
        }

        [Test]
        public async Task GetExperienceWhichDoesntExist()
        {
            var language = new Language("kolumbisch");
            var result = await _experienceService.GetExperience(language.Id);
            Assert.IsNull(result);
        }
    }
}