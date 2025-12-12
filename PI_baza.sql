------------------------------------------------------------
-- 1. KREIRANJE BAZE
------------------------------------------------------------
IF DB_ID('MentorEvalDb') IS NULL
BEGIN
    CREATE DATABASE MentorEvalDb;
END;
GO

USE MentorEvalDb;
GO
------------------------------------------------------------
-- 2. TABLICA Users (TPH: User / Professor / Student)
------------------------------------------------------------
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

CREATE TABLE dbo.Users
(
    Id            INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Username      NVARCHAR(50)  NOT NULL UNIQUE,
    PasswordHash  NVARCHAR(200) NOT NULL,
    FullName      NVARCHAR(100) NOT NULL,
    Role          NVARCHAR(20)  NOT NULL,  -- "Professor" ili "Student"
    Discriminator NVARCHAR(50)  NOT NULL   -- "Professor" / "Student"
);
GO

------------------------------------------------------------
-- 3. TABLICA Courses
------------------------------------------------------------
IF OBJECT_ID('dbo.Courses', 'U') IS NOT NULL DROP TABLE dbo.Courses;
GO

CREATE TABLE dbo.Courses
(
    Id          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name        NVARCHAR(100) NOT NULL,
    Semester    INT           NOT NULL,
    [Year]      INT           NOT NULL,
    ProfessorId INT           NOT NULL
        CONSTRAINT FK_Course_Professor
            REFERENCES dbo.Users(Id)
            ON DELETE NO ACTION
);
GO

------------------------------------------------------------
-- 4. TABLICA Evaluations
------------------------------------------------------------
IF OBJECT_ID('dbo.Evaluations', 'U') IS NOT NULL DROP TABLE dbo.Evaluations;
GO

CREATE TABLE dbo.Evaluations
(
    Id        INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Title     NVARCHAR(200) NOT NULL,
    StartAt   DATETIME2     NOT NULL,
    EndAt     DATETIME2     NOT NULL,
    Status    NVARCHAR(20)  NOT NULL DEFAULT('Draft'), -- Draft/Active/Closed
    CourseId  INT           NOT NULL
        CONSTRAINT FK_Evaluation_Course
            REFERENCES dbo.Courses(Id)
            ON DELETE CASCADE
);
GO

------------------------------------------------------------
-- 5. TABLICA Questions
------------------------------------------------------------
IF OBJECT_ID('dbo.Questions', 'U') IS NOT NULL DROP TABLE dbo.Questions;
GO

CREATE TABLE dbo.Questions
(
    Id           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Text]       NVARCHAR(500) NOT NULL,
    [Type]       NVARCHAR(20)  NOT NULL,   -- Likert / Text / Choice
    [Required]   BIT           NOT NULL DEFAULT(1),
    EvaluationId INT           NOT NULL
        CONSTRAINT FK_Question_Evaluation
            REFERENCES dbo.Evaluations(Id)
            ON DELETE CASCADE
);
GO

------------------------------------------------------------
-- 6. TABLICA Answers
------------------------------------------------------------
IF OBJECT_ID('dbo.Answers', 'U') IS NOT NULL DROP TABLE dbo.Answers;
GO

CREATE TABLE dbo.Answers
(
    Id          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Grade       INT           NULL,        -- za Likert skalu
    [Text]      NVARCHAR(MAX) NULL,        -- za tekstualna pitanja
    SubmittedAt DATETIME2     NOT NULL,
    QuestionId  INT           NOT NULL
        CONSTRAINT FK_Answer_Question
            REFERENCES dbo.Questions(Id)
            ON DELETE CASCADE,
    EvaluationId INT          NOT NULL
        CONSTRAINT FK_Answer_Evaluation
            REFERENCES dbo.Evaluations(Id)  -- bez ON DELETE CASCADE
            ON DELETE NO ACTION,
    StudentId   INT           NOT NULL
        CONSTRAINT FK_Answer_Student
            REFERENCES dbo.Users(Id)
            ON DELETE CASCADE
);

-- indeks za brže agregacije po evaluaciji/pitanju
CREATE INDEX IX_Answers_Eval_Question
    ON dbo.Answers(EvaluationId, QuestionId);
GO

------------------------------------------------------------
-- 7. TABLICA Reports (1:1 s Evaluations)
------------------------------------------------------------
IF OBJECT_ID('dbo.Reports', 'U') IS NOT NULL DROP TABLE dbo.Reports;
GO

CREATE TABLE dbo.Reports
(
    Id           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    GeneratedAt  DATETIME2     NOT NULL,
    [Type]       NVARCHAR(20)  NOT NULL,        -- npr. "Instructor"
    JsonSummary  NVARCHAR(MAX) NULL,
    EvaluationId INT           NOT NULL
        CONSTRAINT FK_Report_Evaluation
            REFERENCES dbo.Evaluations(Id)
            ON DELETE CASCADE,
    CONSTRAINT UQ_Report_Evaluation UNIQUE (EvaluationId) -- 1:1
);
GO

------------------------------------------------------------
-- 8. JOIN TABLICA EvaluationStudent (M:N)
------------------------------------------------------------
IF OBJECT_ID('dbo.EvaluationStudent', 'U') IS NOT NULL DROP TABLE dbo.EvaluationStudent;
GO

CREATE TABLE dbo.EvaluationStudent
(
    EvaluationId INT NOT NULL
        CONSTRAINT FK_EvalStudent_Evaluation
            REFERENCES dbo.Evaluations(Id)
            ON DELETE CASCADE,
    StudentId    INT NOT NULL
        CONSTRAINT FK_EvalStudent_Student
            REFERENCES dbo.Users(Id)
            ON DELETE CASCADE,
    CONSTRAINT PK_EvaluationStudent PRIMARY KEY (EvaluationId, StudentId)
);
GO

------------------------------------------------------------
-- 9. DEMO PODACI (po želji – možeš zakomentirati ako ne treba)
------------------------------------------------------------
-- Profesor i student (lozinke su ovdje čisti tekst radi testa)
INSERT INTO dbo.Users (Username, PasswordHash, FullName, Role, Discriminator)
VALUES ('prof1', 'prof1', 'Profesor Marko', 'Professor', 'Professor'),
       ('stud1', 'stud1', 'Student Ana',   'Student',   'Student');
GO

-- Jedan kolegij
INSERT INTO dbo.Courses (Name, Semester, [Year], ProfessorId)
VALUES ('Programiranje 1', 1, 2025, 1);
GO

-- Jedna aktivna evaluacija
INSERT INTO dbo.Evaluations (Title, StartAt, EndAt, Status, CourseId)
VALUES ('Evaluacija P1 - zimski', SYSDATETIME(), DATEADD(DAY, 14, SYSDATETIME()), 'Active', 1);
GO

-- Pitanja
INSERT INTO dbo.Questions (Text, Type, [Required], EvaluationId)
VALUES ('Nastavnik jasno objašnjava gradivo.', 'Likert', 1, 1),
       ('Komentar / prijedlog poboljšanja.',   'Text',   0, 1);
GO

-- Veza evaluacije i studenta (student pozvan na ovu evaluaciju)
INSERT INTO dbo.EvaluationStudent (EvaluationId, StudentId)
VALUES (1, 2);
GO

select * from Evaluations

select * from Questions