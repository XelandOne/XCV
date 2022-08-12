using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using XCV.Data;
using XCV.Entities;
using XCV.Services;

namespace XCV.Tests.UNIT.ManagerTest
{
    public class ExperienceManagerTest
    {
        private readonly ExperienceManager _experienceManager;

        public ExperienceManagerTest()
        {
            // Init mocked services
            var mockExperienceService = new Mock<IExperienceService>();
            _experienceManager = new ExperienceManager(mockExperienceService.Object);
        }

        [Test]
        public async Task TestInsertExperience_ShouldInsertExperience()
        {
            var field = new Field(Guid.NewGuid(), "ShouldInsert");
            var role = new Role(Guid.NewGuid(), "ShouldInsert");
            var softSkill = new SoftSkill(Guid.NewGuid(), "ShouldInsert");
            var hardSkill = new HardSkill(Guid.NewGuid(), "ShouldInsert");
            var language = new Language(Guid.NewGuid(), "ShouldInsert");

            await _experienceManager.InsertExperience(field);
            await _experienceManager.InsertExperience(role);
            await _experienceManager.InsertExperience(softSkill);
            await _experienceManager.InsertExperience(hardSkill);
            await _experienceManager.InsertExperience(language);

            Assert.Contains(field, _experienceManager.Fields);
            Assert.Contains(role, _experienceManager.Roles);
            Assert.Contains(softSkill, _experienceManager.SoftSkills);
            Assert.Contains(hardSkill, _experienceManager.HardSkills);
            Assert.Contains(language, _experienceManager.Languages);
        }

        [Test]
        public async Task TestInsertExperience_ShouldNotUpdateWhenInsertExperience()
        {
            var field = new Field(Guid.NewGuid(), "ShouldNotUpdateWhenInsert1");
            var role = new Role(Guid.NewGuid(), "ShouldNotUpdateWhenInsert1");
            var softSkill = new SoftSkill(Guid.NewGuid(), "ShouldNotUpdateWhenInsert1");
            var hardSkill = new HardSkill(Guid.NewGuid(), "ShouldNotUpdateWhenInsert1");
            var language = new Language(Guid.NewGuid(), "ShouldNotUpdateWhenInsert1");

            await _experienceManager.InsertExperience(field);
            await _experienceManager.InsertExperience(role);
            await _experienceManager.InsertExperience(softSkill);
            await _experienceManager.InsertExperience(hardSkill);
            await _experienceManager.InsertExperience(language);

            var fieldUpdate = new Field(field.Id, "ShouldNotUpdateWhenInsert2");
            var roleUpdate = new Role(role.Id, "ShouldNotUpdateWhenInsert2");
            var softSkillUpdate = new SoftSkill(softSkill.Id, "ShouldNotUpdateWhenInsert2");
            var hardSkillUpdate = new HardSkill(hardSkill.Id, "ShouldNotUpdateWhenInsert2");
            var languageUpdate = new Language(language.Id, "ShouldNotUpdateWhenInsert2");

            await _experienceManager.InsertExperience(fieldUpdate);
            await _experienceManager.InsertExperience(roleUpdate);
            await _experienceManager.InsertExperience(softSkillUpdate);
            await _experienceManager.InsertExperience(hardSkillUpdate);
            await _experienceManager.InsertExperience(languageUpdate);

            // _Check skill are updated
            Assert.Contains(field, _experienceManager.Fields);
            Assert.Contains(role, _experienceManager.Roles);
            Assert.Contains(softSkill, _experienceManager.SoftSkills);
            Assert.Contains(hardSkill, _experienceManager.HardSkills);
            Assert.Contains(language, _experienceManager.Languages);

            //Check old version doesn´t exist
            Assert.False(_experienceManager.Fields.Contains(fieldUpdate));
            Assert.False(_experienceManager.Roles.Contains(roleUpdate));
            Assert.False(_experienceManager.SoftSkills.Contains(softSkillUpdate));
            Assert.False(_experienceManager.HardSkills.Contains(hardSkillUpdate));
            Assert.False(_experienceManager.Languages.Contains(languageUpdate));
        }

        [Test]
        public async Task TestUpdateExperience_ShouldUpdateExperience()
        {
            var field = new Field(Guid.NewGuid(), "ShouldUpdate1");
            var role = new Role(Guid.NewGuid(), "ShouldUpdate1");
            var softSkill = new SoftSkill(Guid.NewGuid(), "ShouldUpdate1");
            var hardSkill = new HardSkill(Guid.NewGuid(), "ShouldUpdate1");
            var language = new Language(Guid.NewGuid(), "ShouldUpdate1");

            await _experienceManager.InsertExperience(field);
            await _experienceManager.InsertExperience(role);
            await _experienceManager.InsertExperience(softSkill);
            await _experienceManager.InsertExperience(hardSkill);
            await _experienceManager.InsertExperience(language);

            var fieldUpdate = new Field(field.Id, "ShouldUpdate2");
            var roleUpdate = new Role(role.Id, "ShouldUpdate2");
            var softSkillUpdate = new SoftSkill(softSkill.Id, "ShouldUpdate2");
            var hardSkillUpdate = new HardSkill(hardSkill.Id, "ShouldUpdate2");
            var languageUpdate = new Language(language.Id, "ShouldUpdate2");

            await _experienceManager.UpdateExperience(fieldUpdate);
            await _experienceManager.UpdateExperience(roleUpdate);
            await _experienceManager.UpdateExperience(softSkillUpdate);
            await _experienceManager.UpdateExperience(hardSkillUpdate);
            await _experienceManager.UpdateExperience(languageUpdate);

            // _Check skill are updated
            Assert.Contains(fieldUpdate, _experienceManager.Fields);
            Assert.Contains(roleUpdate, _experienceManager.Roles);
            Assert.Contains(softSkillUpdate, _experienceManager.SoftSkills);
            Assert.Contains(hardSkillUpdate, _experienceManager.HardSkills);
            Assert.Contains(languageUpdate, _experienceManager.Languages);

            //Check old version doesn´t exist
            Assert.False(_experienceManager.Fields.Contains(field));
            Assert.False(_experienceManager.Roles.Contains(role));
            Assert.False(_experienceManager.SoftSkills.Contains(softSkill));
            Assert.False(_experienceManager.HardSkills.Contains(hardSkill));
            Assert.False(_experienceManager.Languages.Contains(language));
        }

        [Test]
        public async Task TestDeleteExperience_ShouldDeleteExperience()
        {
            // Init sample Experience
            var field = new Field(Guid.NewGuid(), "ShouldDelete");
            var role = new Role(Guid.NewGuid(), "ShouldDelete");
            var softSkill = new SoftSkill(Guid.NewGuid(), "ShouldDelete");
            var hardSkill = new HardSkill(Guid.NewGuid(), "ShouldDelete");
            var language = new Language(Guid.NewGuid(), "ShouldDelete");

            await _experienceManager.InsertExperience(field);
            await _experienceManager.InsertExperience(role);
            await _experienceManager.InsertExperience(softSkill);
            await _experienceManager.InsertExperience(hardSkill);
            await _experienceManager.InsertExperience(language);

            await _experienceManager.DeleteExperience(field);
            await _experienceManager.DeleteExperience(role);
            await _experienceManager.DeleteExperience(softSkill);
            await _experienceManager.DeleteExperience(hardSkill);
            await _experienceManager.DeleteExperience(language);

            Assert.False(_experienceManager.Fields.Exists(x => x.Name.Equals(field.Name)));
            Assert.False(_experienceManager.Roles.Exists(x => x.Name.Equals(role.Name)));
            Assert.False(_experienceManager.SoftSkills.Exists(x => x.Name.Equals(softSkill.Name)));
            Assert.False(_experienceManager.HardSkills.Exists(x => x.Name.Equals(hardSkill.Name)));
            Assert.False(_experienceManager.Languages.Exists(x => x.Name.Equals(language.Name)));
        }
    }
}