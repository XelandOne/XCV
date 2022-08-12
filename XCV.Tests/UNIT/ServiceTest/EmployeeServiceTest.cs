using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Dapper;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using XCV.Data;
using XCV.Entities;
using XCV.Entities.Enums;
using XCV.Services;

namespace XCV.Tests.UNIT.ServiceTest
{
    public class EmployeeServiceBase
    {
        protected DatabaseUtils DatabaseUtils { get; set; }
        protected EmployeeService EmployeeService { get; set; }
        
        protected IProjectService ProjectService { get; set; }
        protected IProjectActivityService ProjectActivityService { get; set; }

        protected IShownEmployeePropertiesService _shownEmployeePropertiesService { get; set; }
        private static IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder().AddJsonFile(Path.Combine(".","appsettings.Development.json")).Build();
            return config;
            
            
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var config = InitConfiguration();
            this.DatabaseUtils = new DatabaseUtils(config);
            EmployeeService = new EmployeeService(DatabaseUtils);
            ProjectService = new ProjectService(DatabaseUtils);
            ProjectActivityService = new ProjectActivityService(DatabaseUtils);
            _shownEmployeePropertiesService = new ShownEmployeePropertiesService(DatabaseUtils, EmployeeService);
            DatabaseUtils.LoadTables();

        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            connection.Execute("DELETE FROM Employee WHERE Surname LIKE '%Employee%Test%' OR Firstname LIKE '%Employee%Test%'");
            connection.Execute("DELETE FROM Field WHERE FieldName LIKE 'testFieldEmployee%'");
            connection.Execute("DELETE FROM Role WHERE RoleName LIKE 'testRoleEmployee%'");
            connection.Execute("DELETE FROM SoftSkill WHERE SoftSkillName LIKE 'testSoftSkillEmployee%'");
            connection.Execute("DELETE FROM HardSkill WHERE HardSkillName LIKE 'testHardSkillEmployee%'");
            connection.Execute("DELETE FROM Language WHERE LanguageName LIKE 'testLanguageEmployee%'");
            connection.Execute("DELETE FROM Project WHERE Title LIKE '%Employee%Test%'");
        }

        protected async Task<UsedExperience> GetUsedExperience(string testname)
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString); 
            var usedExp = new UsedExperience();
            usedExp.Fields.Add(
                await connection.QueryFirstAsync<Field>(
                    "IF NOT EXISTS (SELECT * FROM Field WHERE FieldName = @name) BEGIN INSERT INTO Field VALUES (@id, @name, CURRENT_TIMESTAMP) END SELECT Id, FieldName AS name, lastChanged FROM Field WHERE FieldName = @name",
                    new {id = Guid.NewGuid(), name = "testFieldEmployee" + testname}));
            usedExp.Roles.Add(
                await connection.QueryFirstAsync<Role>(
                    "IF NOT EXISTS (SELECT * FROM Role WHERE RoleName = @name) BEGIN INSERT INTO Role VALUES (@id, @name, CURRENT_TIMESTAMP) END SELECT Id, RoleName AS name, lastChanged FROM Role WHERE RoleName = @name",
                    new {id = Guid.NewGuid(), name = "testRoleEmployee" + testname}));
            usedExp.SoftSkills.Add(
                await connection.QueryFirstAsync<SoftSkill>(
                    "IF NOT EXISTS (SELECT * FROM SoftSkill WHERE SoftSkillName = @name) BEGIN INSERT INTO SoftSkill VALUES (@id, @name, CURRENT_TIMESTAMP) END SELECT Id, SoftSkillName AS name, lastChanged FROM SoftSkill WHERE SoftSkillName = @name",
                    new {id = Guid.NewGuid(), name = "testSoftSkillEmployee" + testname}));
            usedExp.HardSkills.Add(Tuple.Create(await connection.QueryFirstAsync<HardSkill>(
                    "IF NOT EXISTS (SELECT * FROM HardSkill WHERE HardSkillName = @name) BEGIN INSERT INTO HardSkill VALUES (@id, @name, @category, CURRENT_TIMESTAMP) END SELECT Id, HardSkillName as name, HardSkillCategory, lastChanged FROM HardSkill WHERE HardSkillName = @name",
                    new {id = Guid.NewGuid(), name = "testHardSkillEmployee" + testname, category = "test"}),
                HardSkillLevel.Expert).ToValueTuple());
            usedExp.Languages.Add(Tuple.Create(await connection.QueryFirstAsync<Language>(
                    "IF NOT EXISTS (SELECT * FROM Language WHERE LanguageName = @name) BEGIN INSERT INTO Language VALUES (@id, @name, CURRENT_TIMESTAMP) END SELECT Id, LanguageName as name, lastChanged FROM Language WHERE LanguageName = @name",
                    new {id = Guid.NewGuid(), name = "testLanguageEmployee" + testname}),
                LanguageLevel.Advanced).ToValueTuple());
            return usedExp;
        }
        
        
    }

    [TestFixture]
    public class EmployeeServiceTest : EmployeeServiceBase
    {
        
        [Test]
        public async Task UpdateEmployeeTest_ShouldInsertEmployee_WhenNotExists()
        {
            var employee = new Employee(Authorizations.Sales, "Inserted", "UpdateEmployeeTest", "UpdateEmployeeTestUser", DateTime.Now,
                15, 1, 1, RateCardLevel.Level3, null);
            employee.Experience = await GetUsedExperience("UpdateInsert");
            var result = await EmployeeService.UpdateEmployee(employee);
            Assert.True(result.Item2 == DataBaseResult.Inserted);
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            var insertedEmployee =
                await connection.QueryFirstOrDefaultAsync("SELECT Id FROM Employee WHERE Id = @id",
                    new {id = employee.Id});
            Assert.NotNull(insertedEmployee);
        }

        [Test]
        public async Task UpdateEmployeeTest_ShouldUpdateEmployee_WhenExists()
        {
            //arrange
            var employee = new Employee(Authorizations.Sales, "ToBeUpdated","UpdateEmployeeTest","UpdateEmployeeTestUserTwo", DateTime.Now,
                15, 1, 1, RateCardLevel.Level3, null);
            employee.RateCardLevel = RateCardLevel.Level3;
            employee.Experience = await GetUsedExperience("UpdateUpdate");
            await EmployeeService.UpdateEmployee(employee);
            
            //act
            employee.RateCardLevel = RateCardLevel.Level1;
            employee.SurName = "Updated";
            
            //assert
            var result = await EmployeeService.UpdateEmployee(employee);
            Assert.True(result.Item2 == DataBaseResult.Updated);
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            var rcl = connection.QueryFirstOrDefault<RateCardLevel>("SELECT RateCardLevel FROM Employee WHERE Id = @id",
                new {id = employee.Id});
            Assert.True(RateCardLevel.Level1 == rcl);
        }

        [Test]
        public async Task GetEmployeeTest_ShouldReturnEmployee_WhenExits()
        {
            var employee = new Employee(Authorizations.Sales, "GetEmployeeTest", "Returned","ReturnedUser",DateTime.Now,
                15, 1, 1, RateCardLevel.Level3, null);
            employee.RateCardLevel = RateCardLevel.Level3;
            employee.Experience = await GetUsedExperience("Get");
            await EmployeeService.UpdateEmployee(employee);
            Thread.Sleep(10);
            var result = await EmployeeService.GetEmployee(employee.Id);
            Assert.NotNull(result);
            Assert.AreEqual(employee, result);
        }

        [Test]
        public async Task GetEmployeeTest_ShouldReturnNull_WhenNotExits()
        {
            var result = await EmployeeService.GetEmployee(Guid.NewGuid());
            Assert.Null(result);
        }
        
        [Test]
        public async Task GetEmployeeByUserNameTest_ShouldReturnEmployee_WhenExits()
        {
            var employee = new Employee(Authorizations.Sales, "GetEmployeeTest", "Returned","testUserName",DateTime.Now,
                15, 1, 1, RateCardLevel.Level3, null);
            employee.RateCardLevel = RateCardLevel.Level3;
            employee.Experience = await GetUsedExperience("Get");
            await EmployeeService.UpdateEmployee(employee);
            Thread.Sleep(10);
            var result = await EmployeeService.GetEmployee(employee.UserName);
            Assert.NotNull(result);
            Assert.AreEqual(employee, result);
        }

        [Test]
        public async Task GetEmployeeByUserNameTest_ShouldReturnNull_WhenNotExits()
        {
            var result = await EmployeeService.GetEmployee("test");
            Assert.Null(result);
        }
        
        [Test]
        public async Task GetAllEmployeesTest_ShouldReturnEmployees_WhenExits()
        {
            var employee = new Employee(Authorizations.Sales, "GetAllEmployeeTest", "first", "GetAllEmployeeTestFirstUser", DateTime.Now,
                15, 1, 1, RateCardLevel.Level3, null) {Experience = await GetUsedExperience("GetAllOne")};
            var employee2 = new Employee(Authorizations.Sales, "GetAllEmployeeTest", "second", "GetAllEmployeeTestSecondUser", DateTime.Now,
                15, 1, 1, RateCardLevel.Level1, null) {Experience = await GetUsedExperience("GetAllTwo")};
            await EmployeeService.UpdateEmployee(employee);
            await EmployeeService.UpdateEmployee(employee2);
            var list = await EmployeeService.GetAllEmployees();
            Assert.True(list.Any());
            Assert.Contains(employee, list);
            Assert.Contains(employee2, list);
        }


        [Test]
        public async Task GetExperienceForEmployee_ShouldReturnEmpty_WhenNotExits()
        {
            var experience = await EmployeeService.GetExperienceForEmployee(Guid.NewGuid());
            Assert.IsEmpty(experience.Fields);
            Assert.IsEmpty(experience.Languages);
            Assert.IsEmpty(experience.HardSkills);
            Assert.IsEmpty(experience.Roles);
            Assert.IsEmpty(experience.SoftSkills);
        }
        
        [Test]
        public async Task GetExperienceForEmployee_ShouldReturnExperiences_WhenExits()
        { 
            var employee = new Employee(Authorizations.Sales, "GetExperienceForEmployeeTest", "returned", "GetExperienceForEmployeeTest", DateTime.Now,
                15, 1, 1, RateCardLevel.Level3, null) {Experience = await GetUsedExperience("GetExperience")};
            await EmployeeService.UpdateEmployee(employee);
            var experience = await EmployeeService.GetExperienceForEmployee(employee.Id);
            Assert.AreEqual(employee.Experience, experience);
        }

        [Test]
        public async Task GetAllEmployeeIfNoneExist()
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            var givenEmployees = await EmployeeService.GetAllEmployees();
            List<ShownEmployeeProperties> list = new List<ShownEmployeeProperties>();
            foreach (var employee in givenEmployees)
            {
                var result = await connection.QueryAsync<Guid>("Select Id from ShownEmployeeProperty where Employee_Id = @id",
                    new {id = employee.Id});
                var enumerable = result.ToList();
                if (enumerable.Any())
                {
                    var shortEmployee =
                        await _shownEmployeePropertiesService.GetShownEmployeeProperties(enumerable.First());
                    if (shortEmployee != null)
                    {
                        list.Add(shortEmployee);
                    }
                }
            }
            
            foreach (var employee in givenEmployees)
            {
                await EmployeeService.DeleteEmployee(employee.Id);
            }

            var emptyEmployeeList = await EmployeeService.GetAllEmployees();

            if (emptyEmployeeList.Any())
            {
                Assert.Fail("EmployeeList is not empty");
            }

            foreach (var employee in givenEmployees)
            {
                await EmployeeService.UpdateEmployee(employee);
            }

            foreach (var shortEmp in list)
            {
                await _shownEmployeePropertiesService.UpdateShownEmployeeProperties(shortEmp);
            }
            
            Assert.Pass("EmployeeList was empty");
        }

        [Test]
        public async Task UpdateEmployeeWithProject()
        {
            var employee = new Employee(Authorizations.Sales, "UpdateEmployeeWithProjectTest", "Project","UpdateEmployeeWithProjectUser",DateTime.Now,
                15, 1, 1, RateCardLevel.Level3, null);
            var project =
                new Project("ForEmployeeTest", DateTime.Now, null, "For Testing");
            employee.ProjectIds.Add(project.Id);

            await ProjectService.UpdateProject(project);

            await EmployeeService.UpdateEmployee(employee);

            var employeeTest = await EmployeeService.GetEmployee(employee.Id);

            var projectTest = await ProjectService.GetProject(project.Id);
            
            
            Assert.AreEqual(employee, employeeTest);
            Assert.AreEqual(project, projectTest);
            

            await EmployeeService.DeleteEmployee(employee.Id);
            await ProjectService.DeleteProject(project.Id);
            
            Assert.Pass("Employee with Project and Project was properly inserted");
        }

        [Test]
        public async Task GetNameTest_ShouldReturnName_WhenExits()
        {
            var employee = new Employee(Authorizations.Sales, "GetNameTest", "Returned","GetNameTestUser",DateTime.Now,
                15, 1, 1, RateCardLevel.Level3, null);
            await EmployeeService.UpdateEmployee(employee);
            var expected = (employee.FirstName, employee.SurName);
            var result = await EmployeeService.GetName(employee.Id);
            Assert.AreEqual(expected, result);
        }
        
        [Test]
        public async Task GetNameTest_ShouldReturnNull_WhenNotExits()
        {
            var result = await EmployeeService.GetName(Guid.NewGuid());
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetAllNamesTest_ShouldReturnNames_WhenExits()
        {
            var employee = new Employee(Authorizations.Sales, "GetAllNamesTest", "one","GetAllNameTestUserOne",DateTime.Now,
                15, 1, 1, RateCardLevel.Level3, null);
            var employee2 = new Employee(Authorizations.Sales, "GetAllNamesTest", "two","GetAllNameTestUserTwo",DateTime.Now,
                15, 1, 1, RateCardLevel.Level3, null);
            await EmployeeService.UpdateEmployee(employee);
            await EmployeeService.UpdateEmployee(employee2);

            var result = await EmployeeService.GetAllNames();

            
            var expectedOne = (employee.FirstName, employee.SurName);
            var expectedTwo = (employee2.FirstName, employee2.SurName);
            Assert.True(result.TryGetValue(employee.Id, out var nameTupleOne));
            Assert.True(result.TryGetValue(employee2.Id, out var nameTupleTwo));
            Assert.AreEqual(expectedOne, nameTupleOne);
            Assert.AreEqual(expectedTwo, nameTupleTwo);
        }

        [Test]
        public async Task GetAllNamesTest_ShouldEmptyDict_WhenNotExits()
        {
            using IDbConnection connection = new SqlConnection(DatabaseUtils.ConnectionString);
            var givenEmployees = await EmployeeService.GetAllEmployees();
            List<ShownEmployeeProperties> list = new List<ShownEmployeeProperties>();
            foreach (var employee in givenEmployees)
            {
                var result = await connection.QueryAsync<Guid>("Select Id from ShownEmployeeProperty where Employee_Id = @id",
                    new {id = employee.Id});
                var enumerable = result.ToList();
                if (enumerable.Any())
                {
                    var shortEmployee =
                        await _shownEmployeePropertiesService.GetShownEmployeeProperties(enumerable.First());
                    if (shortEmployee != null)
                    {
                        list.Add(shortEmployee);
                    }
                }
            }
            

            foreach (var employee in givenEmployees)
            {
                await EmployeeService.DeleteEmployee(employee.Id);
            }

            var emptyNameDict = await EmployeeService.GetAllNames();
            Assert.True(emptyNameDict.IsNullOrEmpty());
            
            foreach (var employee in givenEmployees)
            {
                await EmployeeService.UpdateEmployee(employee);
            }

            foreach (var shortEmp in list)
            {
                await _shownEmployeePropertiesService.UpdateShownEmployeeProperties(shortEmp);
            }
        }

        [Test]
        public async Task GetEmployeesInProjectTest_ShouldReturnEmptyList_WhenNotExits()
        {
            Assert.IsEmpty(await EmployeeService.GetEmployeesInProject(Guid.NewGuid()));
        }
        
        [Test]
        public async Task GetEmployeesInProjectTest_ShouldReturnEmployees_WhenExits()
        {
            //create entities
            var employeeOne = new Employee(Authorizations.Sales, "GetEmployeesInProjectTest", "one","GetEmployeesInProjectTestOne",DateTime.Now,
                15, 1, 1, RateCardLevel.Level3, null);
            var employeeTwo = new Employee(Authorizations.Sales, "GetEmployeesInProjectTest", "two","GetEmployeesInProjectTestTwo",DateTime.Now,
                15, 1, 1, RateCardLevel.Level3, null);
            var employeeThree = new Employee(Authorizations.Sales, "GetEmployeesInProjectTest", "Three","GetEmployeesInProjectTestThree",DateTime.Now,
                15, 1, 1, RateCardLevel.Level3, null);
            var employeeFour = new Employee(Authorizations.Sales, "GetEmployeesInProjectTest", "Four","GetEmployeesInProjectTestFour",DateTime.Now,
                15, 1, 1, RateCardLevel.Level3, null);

            var projectActOne = new ProjectActivity("GetEmployeesInProjectTestOne");
            projectActOne.AddEmployee(employeeOne.Id);
            projectActOne.AddEmployee(employeeTwo.Id);
            var projectActTwo = new ProjectActivity("GetEmployeesInProjectTestTwo");
            projectActTwo.AddEmployee(employeeThree.Id);
            projectActTwo.AddEmployee(employeeFour.Id);
            
            var project = new Project("GetEmployeesInProjectTest", DateTime.Now, null, "Test");
            project.ProjectActivities.Add(projectActOne);
            project.ProjectActivities.Add(projectActTwo);
            
            //insert them
            await EmployeeService.UpdateEmployee(employeeOne);
            await EmployeeService.UpdateEmployee(employeeTwo);
            await EmployeeService.UpdateEmployee(employeeThree);
            await EmployeeService.UpdateEmployee(employeeFour);

            await ProjectService.UpdateProject(project);
            
            await ProjectActivityService.UpdateProjectActivity(projectActOne, project.Id);
            await ProjectActivityService.UpdateProjectActivity(projectActTwo, project.Id);
            
            
            //act
            var employees = await EmployeeService.GetEmployeesInProject(project.Id);
            
            //assert
            Assert.Contains(employeeOne, employees, "employeeOne was not returned");
            Assert.Contains(employeeTwo, employees, "employeeTwo was not returned");
            Assert.Contains(employeeThree, employees, "employeeThree was not returned");
            Assert.Contains(employeeFour, employees, "employeeFour was not returned");
        }
    }
}