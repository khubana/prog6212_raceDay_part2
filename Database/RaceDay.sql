-- Starter schema. Compare with your Part 1 schema and use on a new/test database first.
CREATE TABLE dbo.Roles(RoleID INT IDENTITY PRIMARY KEY, RoleName NVARCHAR(50) NOT NULL UNIQUE);
CREATE TABLE dbo.Users(
 UserID INT IDENTITY PRIMARY KEY, RoleID INT NOT NULL, FirstName NVARCHAR(100) NOT NULL,
 LastName NVARCHAR(100) NOT NULL, Email NVARCHAR(255) NOT NULL UNIQUE,
 PasswordHash NVARCHAR(MAX) NOT NULL, Phone NVARCHAR(30) NULL,
 CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
 CONSTRAINT FK_Users_Roles FOREIGN KEY(RoleID) REFERENCES dbo.Roles(RoleID));
CREATE TABLE dbo.Events(
 EventID INT IDENTITY PRIMARY KEY, OrganiserID INT NOT NULL, EventName NVARCHAR(150) NOT NULL,
 EventDescription NVARCHAR(MAX) NULL, EventDate DATETIME2 NOT NULL, EventLocation NVARCHAR(200) NOT NULL,
 EventStatus NVARCHAR(20) NOT NULL DEFAULT 'open', CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
 DistanceKm DECIMAL(8,2) NULL, EventType NVARCHAR(80) NULL,
 CONSTRAINT CK_Events_Status CHECK(EventStatus IN ('open','closed','cancelled','completed')),
 CONSTRAINT FK_Events_Organiser FOREIGN KEY(OrganiserID) REFERENCES dbo.Users(UserID));
CREATE TABLE dbo.Categories(
 CategoryID INT IDENTITY PRIMARY KEY, EventID INT NOT NULL, CategoryName NVARCHAR(100) NOT NULL,
 MaxParticipants INT NOT NULL CHECK(MaxParticipants>=10), EntryFee DECIMAL(10,2) NOT NULL CHECK(EntryFee>=50),
 CONSTRAINT FK_Categories_Events FOREIGN KEY(EventID) REFERENCES dbo.Events(EventID));
CREATE TABLE dbo.EventEnrolment(
 EnrolmentID INT IDENTITY PRIMARY KEY, EventID INT NOT NULL, ParticipantsID INT NOT NULL, CategoryID INT NOT NULL,
 EnrolmentDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), Status NVARCHAR(20) NOT NULL DEFAULT 'Active',
 CONSTRAINT CK_Enrolment_Status CHECK(Status IN ('Active','Cancelled','Completed')),
 CONSTRAINT UQ_Event_Participant UNIQUE(EventID,ParticipantsID),
 CONSTRAINT FK_Enrolment_Event FOREIGN KEY(EventID) REFERENCES dbo.Events(EventID),
 CONSTRAINT FK_Enrolment_User FOREIGN KEY(ParticipantsID) REFERENCES dbo.Users(UserID),
 CONSTRAINT FK_Enrolment_Category FOREIGN KEY(CategoryID) REFERENCES dbo.Categories(CategoryID));
CREATE TABLE dbo.Results(
 ResultID INT IDENTITY PRIMARY KEY, EnrolmentID INT NOT NULL UNIQUE, Position INT NOT NULL CHECK(Position>0),
 FinishTime TIME NULL, Score DECIMAL(10,2) NULL CHECK(Score IS NULL OR Score>=0),
 RecordedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
 CONSTRAINT FK_Results_Enrolment FOREIGN KEY(EnrolmentID) REFERENCES dbo.EventEnrolment(EnrolmentID));
INSERT INTO dbo.Roles(RoleName) VALUES ('Participant'),('Organiser');
