--Employees:

IF object_id('Employee') IS NULL
CREATE TABLE Employee
(
    Id                  uniqueidentifier PRIMARY KEY,
    Surname             varchar(30) NOT NULL,
    Firstname           varchar(30) NOT NULL,
    Username            varchar(30) NOT NULL UNIQUE,
    EmployedSince       Date,
    WorkExperience      int,
    ScientificAssistant int,
    StudentAssistant    int,
    RateCardLevel       int         NOT NULL,
    ProfilePicture      varbinary(max),
    Authorizations      int         NOT NULL,
    LastChanged         DateTime,
    CHECK (NOT Surname like '^\D*$'),
    CHECK (NOT Firstname LIKE '^\D*$'),
    CHECK (WorkExperience >= 0),
    CHECK (ScientificAssistant >= 0),
    CHECK (StudentAssistant >= 0),
    CHECK (RateCardLevel between 1 and 8),
    CHECK (Authorizations between 1 and 4)
);

----------------------------------------------------------------
--Experiences:


IF object_id('HardSkill') IS NULL
CREATE TABLE HardSkill
(
    Id                uniqueidentifier PRIMARY KEY,
    HardSkillName     varchar(50)  NOT NULL UNIQUE,
    HardSkillCategory varchar(100) NOT NULL,
    LastChanged       DateTime
);

IF object_id('SoftSkill') IS NULL
CREATE TABLE SoftSkill
(
    Id            uniqueidentifier PRIMARY KEY,
    SoftSkillName varchar(50) NOT NULL UNIQUE,
    LastChanged   DateTime
);

IF object_id('Field') IS NULL
CREATE TABLE Field
(
    Id          uniqueidentifier PRIMARY KEY,
    FieldName   varchar(50) NOT NULL UNIQUE,
    LastChanged DateTime
);

IF object_id('Role') IS NULL
CREATE TABLE Role
(
    Id          uniqueidentifier PRIMARY KEY,
    RoleName    varchar(50) NOT NULL UNIQUE,
    LastChanged DateTime
);

IF object_id('Language') IS NULL
CREATE TABLE Language
(
    Id           uniqueidentifier PRIMARY KEY,
    LanguageName varchar(50) NOT NULL UNIQUE,
    LastChanged  DateTime
);

--------------------------------------------------------------
--Projects:

IF object_id('Project') IS NULL
CREATE TABLE Project
(
    Id                 uniqueidentifier PRIMARY KEY,
    Title              varchar(100) NOT NULL,
    Field_Id           uniqueidentifier,
    StartDate          Date        NOT NULL,
    EndDate            Date,
    ProjectDescription varchar(1000),
    LastChanged        DateTime,
    CHECK (StartDate <= EndDate)
);

IF object_id('Project_Purpose') IS NULL
CREATE TABLE Project_Purpose
(
    Project_Id uniqueidentifier NOT NULL,
    Purpose    varchar(200)     NOT NULL,
    FOREIGN KEY (Project_Id) references Project (Id) on delete cascade on update cascade,
    PRIMARY KEY (Project_Id, Purpose)
);

IF object_id('ProjectActivity') IS NULL
CREATE TABLE ProjectActivity
(
    Id          uniqueidentifier PRIMARY KEY,
    Description varchar(100)     NOT NULL,
    Project_Id  uniqueidentifier NOT NULL,
    FOREIGN KEY (Project_Id) REFERENCES Project (Id) on delete cascade on update cascade
);

IF object_id('ProjectActivities_Employee') IS NULL
CREATE TABLE ProjectActivities_Employee
(
    Project_Id         uniqueidentifier NOT NULL,
    ProjectActivity_Id uniqueidentifier NOT NULL,
    Employee_Id        uniqueidentifier NOT NULL,
    Foreign Key (Project_Id) REFERENCES Project (Id),
    Foreign Key (ProjectActivity_Id) REFERENCES ProjectActivity (Id) on delete cascade on update cascade,
    Foreign Key (Employee_Id) References Employee (Id) on delete cascade on update cascade,
    PRIMARY KEY (Project_Id, ProjectActivity_Id, Employee_Id)
);

--------------------------------------------------------------
--Offers:

IF object_id('Offer') IS NULL
CREATE TABLE Offer
(
    Id          uniqueidentifier PRIMARY KEY,
    Title       varchar(200) NOT NULL,
    StartDate   Date,
    EndDate     Date,
    LastChanged DateTime,
    CHECK (StartDate <= EndDate)
);
--------------------------------------------------------------
--ShownEmployeeProperties:

IF object_id('ShownEmployeeProperty') IS NULL
CREATE TABLE ShownEmployeeProperty
(
    Id                 uniqueidentifier PRIMARY KEY,
    Employee_Id        uniqueidentifier NOT NULL,
    RateCardLevel      int              NOT NULL,
    PlannedWeeklyHours int,
    Offer_Id           uniqueidentifier NOT NULL,
    Discount           float,
    LastChanged        DateTime,
    CHECK (RateCardLevel between 1 and 8),
    FOREIGN KEY (Offer_Id) references Offer (Id) on delete cascade on update cascade,
    FOREIGN KEY (Employee_Id) REFERENCES Employee (Id) on delete cascade on update cascade,
);

--------------------------------------------------------------
Employees:

IF object_id('Employee_Field') IS NULL
CREATE TABLE Employee_Field
(
    Employee_Id uniqueidentifier NOT NULL,
    Field_Id    uniqueidentifier NOT NULL,
    Foreign Key (Employee_Id) REFERENCES Employee (Id) on delete cascade on update cascade,
    Foreign Key (Field_Id) REFERENCES Field (Id) on delete cascade on update cascade,
    PRIMARY KEY (Employee_Id, Field_Id)
);

IF object_id('Employee_Project') IS NULL
CREATE TABLE Employee_Project
(
    Employee_Id uniqueidentifier NOT NULL,
    Project_Id  uniqueidentifier NOT NULL,
    Foreign Key (Employee_Id) REFERENCES Employee (Id) on delete cascade on update cascade,
    Foreign Key (Project_Id) REFERENCES Project (Id) on delete cascade on update cascade,
    PRIMARY KEY (Employee_Id, Project_Id)
);

IF object_id('Employee_SoftSkill') IS NULL
CREATE TABLE Employee_SoftSkill
(
    Employee_Id  uniqueidentifier NOT NULL,
    SoftSkill_Id uniqueidentifier NOT NULL,
    Foreign Key (Employee_Id) REFERENCES Employee (Id) on delete cascade on update cascade,
    Foreign Key (SoftSkill_Id) REFERENCES SoftSkill (Id) on delete cascade on update cascade,
    PRIMARY KEY (Employee_Id, SoftSkill_Id)
);

IF object_id('Employee_HardSkill') IS NULL
CREATE TABLE Employee_HardSkill
(
    Employee_Id     uniqueidentifier NOT NULL,
    HardSkill_Id    uniqueidentifier NOT NULL,
    HardSkill_Level int              NOT NULL,
    Foreign Key (Employee_Id) REFERENCES Employee (Id) on delete cascade on update cascade,
    Foreign Key (HardSkill_Id) REFERENCES HardSkill (Id) on delete cascade on update cascade,
    PRIMARY KEY (Employee_Id, HardSkill_Id),
    CHECK (HardSkill_Level between 1 and 4)
);

IF object_id('Employee_Language') IS NULL
CREATE TABLE Employee_Language
(
    Employee_Id    uniqueidentifier NOT NULL,
    Language_Id    uniqueidentifier NOT NULL,
    Language_Level int              NOT NULL,
    Foreign Key (Employee_Id) REFERENCES Employee (Id) on delete cascade on update cascade,
    Foreign Key (Language_Id) REFERENCES Language (Id) on delete cascade on update cascade,
    PRIMARY KEY (Employee_Id, Language_Id),
    CHECK (Language_Level between 1 and 5)
);

IF object_id('Employee_Role') IS NULL
CREATE TABLE Employee_Role
(
    Employee_Id uniqueidentifier NOT NULL,
    Role_Id     uniqueidentifier NOT NULL,
    Foreign Key (Employee_Id) REFERENCES Employee (Id) on delete cascade on update cascade,
    Foreign Key (Role_Id) REFERENCES Role (Id) on delete cascade on update cascade,
    PRIMARY KEY (Employee_Id, Role_Id)
);

-------------------------------------------------------------
--ShownEmployeeProperties:

IF object_id('ShownEmployeeProperty_Field') IS NULL
CREATE TABLE ShownEmployeeProperty_Field
(
    ShownEmployeeProperty_Id uniqueidentifier NOT NULL,
    Field_Id                 uniqueidentifier NOT NULL,
    Foreign Key (ShownEmployeeProperty_Id) REFERENCES ShownEmployeeProperty (Id) on delete cascade on update cascade,
    Foreign Key (Field_Id) REFERENCES Field (Id) on delete cascade on update cascade,
    PRIMARY KEY (ShownEmployeeProperty_Id, Field_Id)
);

IF object_id('ShownEmployeeProperty_Project') IS NULL
CREATE TABLE ShownEmployeeProperty_Project
(
    ShownEmployeeProperty_Id uniqueidentifier NOT NULL,
    Project_Id               uniqueidentifier NOT NULL,
    Foreign Key (ShownEmployeeProperty_Id) REFERENCES ShownEmployeeProperty (Id) on delete cascade on update cascade,
    Foreign Key (Project_Id) REFERENCES Project (Id) on delete cascade on update cascade,
    PRIMARY KEY (ShownEmployeeProperty_Id, Project_Id)
);

IF object_id('ShownEmployeeProperty_SoftSkill') IS NULL
CREATE TABLE ShownEmployeeProperty_SoftSkill
(
    ShownEmployeeProperty_Id uniqueidentifier NOT NULL,
    SoftSkill_Id             uniqueidentifier NOT NULL,
    Foreign Key (ShownEmployeeProperty_Id) REFERENCES ShownEmployeeProperty (Id) on delete cascade on update cascade,
    Foreign Key (Softskill_Id) REFERENCES SoftSkill (Id) on delete cascade on update cascade,
    PRIMARY KEY (ShownEmployeeProperty_Id, SoftSkill_Id)
);

IF object_id('ShownEmployeeProperty_HardSkill') IS NULL
CREATE TABLE ShownEmployeeProperty_HardSkill
(
    ShownEmployeeProperty_Id uniqueidentifier NOT NULL,
    HardSkill_Id             uniqueidentifier NOT NULL,
    HardSkill_Level          int              NOT NULL,
    Foreign Key (ShownEmployeeProperty_Id) REFERENCES ShownEmployeeProperty (Id) on delete cascade on update cascade,
    Foreign Key (Hardskill_Id) REFERENCES HardSkill (Id) on delete cascade on update cascade,
    PRIMARY KEY (ShownEmployeeProperty_Id, HardSkill_Id),
    CHECK (HardSkill_Level between 1 and 4)
);

IF object_id('ShownEmployeeProperty_Language') IS NULL
CREATE TABLE ShownEmployeeProperty_Language
(
    ShownEmployeeProperty_Id uniqueidentifier NOT NULL,
    Language_Id              uniqueidentifier NOT NULL,
    Language_Level           int              NOT NULL,
    Foreign Key (ShownEmployeeProperty_Id) REFERENCES ShownEmployeeProperty (Id) on delete cascade on update cascade,
    Foreign Key (Language_Id) REFERENCES Language (Id) on delete cascade on update cascade,
    PRIMARY KEY (ShownEmployeeProperty_Id, Language_Id),
    CHECK (Language_Level between 1 and 5)
);

IF object_id('ShownEmployeeProperty_Role') IS NULL
CREATE TABLE ShownEmployeeProperty_Role
(
    ShownEmployeeProperty_Id uniqueidentifier NOT NULL,
    Role_Id                  uniqueidentifier NOT NULL,
    Foreign Key (ShownEmployeeProperty_Id) REFERENCES ShownEmployeeProperty (Id) on delete cascade on update cascade,
    Foreign Key (Role_Id) REFERENCES Role (Id) on delete cascade on update cascade,
    PRIMARY KEY (ShownEmployeeProperty_Id, Role_Id)
);

IF object_id('ShownEmployeeProperty_ProjectActivity') IS NULL
CREATE TABLE ShownEmployeeProperty_ProjectActivity
(
    ShownEmployeeProperty_Id uniqueidentifier NOT NULL,
    ProjectActivity_Id       uniqueidentifier NOT NULL,
    FOREIGN KEY (ShownEmployeeProperty_Id) references ShownEmployeeProperty (Id) on delete cascade on update cascade,
    FOREIGN KEY (ProjectActivity_Id) references ProjectActivity (Id) on delete cascade on update cascade,
    PRIMARY KEY (ShownEmployeeProperty_Id, ProjectActivity_Id)
);


-------------------------------------------------------------
--Offers:

IF object_id('Offer_Field') IS NULL
CREATE TABLE Offer_Field
(
    Offer_Id uniqueidentifier NOT NULL,
    Field_Id uniqueidentifier NOT NULL,
    Foreign Key (Offer_Id) REFERENCES Offer (Id) on delete cascade on update cascade,
    Foreign Key (Field_Id) REFERENCES Field (Id) on delete cascade on update cascade,
    PRIMARY KEY (Offer_Id, Field_Id)
);

IF object_id('Offer_SoftSkill') IS NULL
CREATE TABLE Offer_SoftSkill
(
    Offer_Id     uniqueidentifier NOT NULL,
    SoftSkill_Id uniqueidentifier NOT NULL,
    Foreign Key (Offer_Id) REFERENCES Offer (Id) on delete cascade on update cascade,
    Foreign Key (SoftSkill_Id) REFERENCES SoftSkill (Id) on delete cascade on update cascade,
    PRIMARY KEY (Offer_Id, SoftSkill_Id)
);

IF object_id('Offer_HardSkill') IS NULL
CREATE TABLE Offer_HardSkill
(
    Offer_Id        uniqueidentifier NOT NULL,
    HardSkill_Id    uniqueidentifier NOT NULL,
    HardSkill_Level int              NOT NULL,
    Foreign Key (Offer_Id) REFERENCES Offer (Id) on delete cascade on update cascade,
    Foreign Key (HardSkill_Id) REFERENCES HardSkill (Id) on delete cascade on update cascade,
    PRIMARY KEY (Offer_Id, HardSkill_Id),
    CHECK (HardSkill_Level between 1 and 4)
);

IF object_id('Offer_Language') IS NULL
CREATE TABLE Offer_Language
(
    Offer_Id       uniqueidentifier NOT NULL,
    Language_Id    uniqueidentifier NOT NULL,
    Language_Level int              NOT NULL,
    Foreign Key (Offer_Id) REFERENCES Offer (Id) on delete cascade on update cascade,
    Foreign Key (Language_Id) REFERENCES Language (Id) on delete cascade on update cascade,
    PRIMARY KEY (Offer_Id, Language_Id),
    CHECK (Language_Level between 1 and 5)
);

IF object_id('Offer_Role') IS NULL
CREATE TABLE Offer_Role
(
    Offer_Id uniqueidentifier NOT NULL,
    Role_Id  uniqueidentifier NOT NULL,
    Foreign Key (Offer_Id) REFERENCES Offer (Id) on delete cascade on update cascade,
    Foreign Key (Role_Id) REFERENCES Role (Id) on delete cascade on update cascade,
    PRIMARY KEY (Offer_Id, Role_Id)
);


----------------------------------------------------------------------------------
--Documents:

If object_id('DocumentConfigurations') IS NULL
CREATE TABLE DocumentConfigurations
(
    Id                      uniqueidentifier NOT NULL,
    Title                   varchar(50)      NOT NULL,
    CreationTime            DATETIMEOFFSET   NOT NULL,
    ShowCoverSheet          BIT              NOT NULL,
    ShowRequiredExperience  BIT              NOT NULL,
    IncludePriceCalculation BIT              NOT NULL,
    Offer_Id                uniqueidentifier NOT NULL,
    FOREIGN KEY (Offer_Id) REFERENCES Offer (Id) on delete cascade on update cascade,
    PRIMARY KEY (Id)
);

IF object_id('DocumentConfigurations_ShownEmployeeProperties') IS NULL
CREATE TABLE DocumentConfigurations_ShownEmployeeProperties
(
    Documents_Id             uniqueidentifier NOT NULL,
    ShownEmployeeProperty_Id uniqueidentifier NOT NULL,
    FOREIGN KEY (Documents_Id) REFERENCES DocumentConfigurations (Id) on delete no action on update no action,
    FOREIGN KEY (ShownEmployeeProperty_Id) REFERENCES ShownEmployeeProperty (Id) on delete cascade on update cascade,
    PRIMARY KEY (Documents_Id, ShownEmployeeProperty_Id)
);

---------------------------------------------------------------------------------
--HourlyWages

IF object_id('HourlyWages') IS NULL
    BEGIN
        CREATE TABLE HourlyWages
        (
            RateCardLevel int   NOT NULL,
            Wage          float NOT NULL,
            PRIMARY KEY (RateCardLevel),
            CHECK (Wage > 0)
        )
        INSERT INTO HourlyWages
        values (1, 80.00)
        INSERT INTO HourlyWages
        values (2, 95.00)
        INSERT INTO HourlyWages
        values (3, 105.00)
        INSERT INTO HourlyWages
        values (4, 120.00)
        INSERT INTO HourlyWages
        values (5, 135.00)
        INSERT INTO HourlyWages
        values (6, 155.00)
        INSERT INTO HourlyWages
        values (7, 195.00)
        INSERT INTO HourlyWages
        values (8, 260.00)
    END
