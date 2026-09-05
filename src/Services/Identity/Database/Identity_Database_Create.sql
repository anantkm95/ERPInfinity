-- Identity_Database_Create.sql
-- Creates tables and stored procedures for the Identity database (SQL Server)
-- Run this script in the target database (e.g., USE [Identity]; GO)

SET NOCOUNT ON;

-- =====================
-- Tables
-- =====================

IF OBJECT_ID('dbo.Users','U') IS NULL
BEGIN
	CREATE TABLE dbo.Users (
		Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
		Username NVARCHAR(50) NOT NULL,
		Email NVARCHAR(100) NOT NULL,
		PasswordHash NVARCHAR(MAX) NOT NULL,
		FirstName NVARCHAR(50) NULL,
		LastName NVARCHAR(50) NULL,
		PhoneNumber NVARCHAR(20) NULL,
		IsActive BIT NOT NULL DEFAULT(1),
		LastLoginAt DATETIME2 NULL,
		CreatedAt DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
		UpdatedAt DATETIME2 NULL
	);
	CREATE UNIQUE INDEX IX_Users_Username ON dbo.Users(Username);
	CREATE UNIQUE INDEX IX_Users_Email ON dbo.Users(Email);
END

IF OBJECT_ID('dbo.Roles','U') IS NULL
BEGIN
	CREATE TABLE dbo.Roles (
		Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
		Name NVARCHAR(50) NOT NULL,
		Description NVARCHAR(200) NULL,
		IsSystemRole BIT NOT NULL DEFAULT(0)
	);
	CREATE UNIQUE INDEX IX_Roles_Name ON dbo.Roles(Name);
END

IF OBJECT_ID('dbo.Permissions','U') IS NULL
BEGIN
	CREATE TABLE dbo.Permissions (
		Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
		PermissionCode NVARCHAR(100) NOT NULL,
		Module NVARCHAR(50) NOT NULL,
		Description NVARCHAR(MAX) NULL
	);
	CREATE UNIQUE INDEX IX_Permissions_PermissionCode ON dbo.Permissions(PermissionCode);
END

IF OBJECT_ID('dbo.UserRoles','U') IS NULL
BEGIN
	CREATE TABLE dbo.UserRoles (
		UserId UNIQUEIDENTIFIER NOT NULL,
		RoleId INT NOT NULL,
		AssignedAt DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
		CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
		CONSTRAINT FK_UserRoles_User FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
		CONSTRAINT FK_UserRoles_Role FOREIGN KEY (RoleId) REFERENCES dbo.Roles(Id) ON DELETE CASCADE
	);
	CREATE INDEX IX_UserRoles_RoleId ON dbo.UserRoles(RoleId);
END

IF OBJECT_ID('dbo.RolePermissions','U') IS NULL
BEGIN
	CREATE TABLE dbo.RolePermissions (
		RoleId INT NOT NULL,
		PermissionId INT NOT NULL,
		CONSTRAINT PK_RolePermissions PRIMARY KEY (RoleId, PermissionId),
		CONSTRAINT FK_RolePermissions_Role FOREIGN KEY (RoleId) REFERENCES dbo.Roles(Id) ON DELETE CASCADE,
		CONSTRAINT FK_RolePermissions_Permission FOREIGN KEY (PermissionId) REFERENCES dbo.Permissions(Id) ON DELETE CASCADE
	);
	CREATE INDEX IX_RolePermissions_PermissionId ON dbo.RolePermissions(PermissionId);
END

IF OBJECT_ID('dbo.RefreshTokens','U') IS NULL
BEGIN
	CREATE TABLE dbo.RefreshTokens (
		Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
		UserId UNIQUEIDENTIFIER NOT NULL,
		Token NVARCHAR(200) NOT NULL,
		ExpiresAt DATETIME2 NOT NULL,
		IsRevoked BIT NOT NULL DEFAULT(0),
		CreatedAt DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
		CreatedByIp NVARCHAR(45) NULL,
		RevokedAt DATETIME2 NULL,
		ReplacedByToken NVARCHAR(200) NULL,
		CONSTRAINT FK_RefreshTokens_User FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
	);
	CREATE UNIQUE INDEX IX_RefreshTokens_Token ON dbo.RefreshTokens(Token);
	CREATE INDEX IX_RefreshTokens_UserId ON dbo.RefreshTokens(UserId);
END

IF OBJECT_ID('dbo.AuditLogs','U') IS NULL
BEGIN
	CREATE TABLE dbo.AuditLogs (
		Id BIGINT NOT NULL IDENTITY(1,1) PRIMARY KEY,
		UserId UNIQUEIDENTIFIER NULL,
		Username NVARCHAR(200) NULL,
		Action NVARCHAR(200) NOT NULL,
		IpAddress NVARCHAR(45) NULL,
		Details NVARCHAR(MAX) NULL,
		Timestamp DATETIME2 NOT NULL DEFAULT(GETUTCDATE())
	);
END

-- =====================
-- Stored Procedures
-- =====================

-- User CRUD
IF OBJECT_ID('dbo.CreateUser','P') IS NOT NULL DROP PROCEDURE dbo.CreateUser;
GO
CREATE PROCEDURE dbo.CreateUser
	@Id UNIQUEIDENTIFIER,
	@Username NVARCHAR(50),
	@Email NVARCHAR(100),
	@PasswordHash NVARCHAR(MAX),
	@FirstName NVARCHAR(50) = NULL,
	@LastName NVARCHAR(50) = NULL,
	@PhoneNumber NVARCHAR(20) = NULL,
	@IsActive BIT = 1
AS
BEGIN
	SET NOCOUNT ON;
	INSERT INTO dbo.Users (Id, Username, Email, PasswordHash, FirstName, LastName, PhoneNumber, IsActive, CreatedAt)
	VALUES (@Id, @Username, @Email, @PasswordHash, @FirstName, @LastName, @PhoneNumber, @IsActive, SYSUTCDATETIME());
END
GO

IF OBJECT_ID('dbo.UpdateUser','P') IS NOT NULL DROP PROCEDURE dbo.UpdateUser;
GO
CREATE PROCEDURE dbo.UpdateUser
	@Id UNIQUEIDENTIFIER,
	@Username NVARCHAR(50) = NULL,
	@Email NVARCHAR(100) = NULL,
	@PasswordHash NVARCHAR(MAX) = NULL,
	@FirstName NVARCHAR(50) = NULL,
	@LastName NVARCHAR(50) = NULL,
	@PhoneNumber NVARCHAR(20) = NULL,
	@IsActive BIT = NULL
AS
BEGIN
	SET NOCOUNT ON;
	UPDATE dbo.Users
	SET
		Username = COALESCE(@Username, Username),
		Email = COALESCE(@Email, Email),
		PasswordHash = COALESCE(@PasswordHash, PasswordHash),
		FirstName = COALESCE(@FirstName, FirstName),
		LastName = COALESCE(@LastName, LastName),
		PhoneNumber = COALESCE(@PhoneNumber, PhoneNumber),
		IsActive = COALESCE(@IsActive, IsActive),
		UpdatedAt = SYSUTCDATETIME()
	WHERE Id = @Id;
END
GO

IF OBJECT_ID('dbo.DeleteUser','P') IS NOT NULL DROP PROCEDURE dbo.DeleteUser;
GO
CREATE PROCEDURE dbo.DeleteUser
	@Id UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM dbo.Users WHERE Id = @Id;
END
GO

IF OBJECT_ID('dbo.GetUserById','P') IS NOT NULL DROP PROCEDURE dbo.GetUserById;
GO
CREATE PROCEDURE dbo.GetUserById
	@Id UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;
	SELECT * FROM dbo.Users WHERE Id = @Id;
END
GO

IF OBJECT_ID('dbo.GetUserByUsername','P') IS NOT NULL DROP PROCEDURE dbo.GetUserByUsername;
GO
CREATE PROCEDURE dbo.GetUserByUsername
	@Username NVARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;
	SELECT * FROM dbo.Users WHERE Username = @Username;
END
GO

-- Role Management
IF OBJECT_ID('dbo.CreateRole','P') IS NOT NULL DROP PROCEDURE dbo.CreateRole;
GO
CREATE PROCEDURE dbo.CreateRole
	@Name NVARCHAR(50),
	@Description NVARCHAR(200) = NULL,
	@IsSystemRole BIT = 0
AS
BEGIN
	SET NOCOUNT ON;
	INSERT INTO dbo.Roles (Name, Description, IsSystemRole)
	VALUES (@Name, @Description, @IsSystemRole);
	SELECT SCOPE_IDENTITY() AS NewRoleId;
END
GO

IF OBJECT_ID('dbo.AssignRoleToUser','P') IS NOT NULL DROP PROCEDURE dbo.AssignRoleToUser;
GO
CREATE PROCEDURE dbo.AssignRoleToUser
	@UserId UNIQUEIDENTIFIER,
	@RoleId INT
AS
BEGIN
	SET NOCOUNT ON;
	IF NOT EXISTS(SELECT 1 FROM dbo.UserRoles WHERE UserId = @UserId AND RoleId = @RoleId)
	BEGIN
		INSERT INTO dbo.UserRoles (UserId, RoleId, AssignedAt) VALUES (@UserId, @RoleId, SYSUTCDATETIME());
	END
END
GO

IF OBJECT_ID('dbo.RevokeRoleFromUser','P') IS NOT NULL DROP PROCEDURE dbo.RevokeRoleFromUser;
GO
CREATE PROCEDURE dbo.RevokeRoleFromUser
	@UserId UNIQUEIDENTIFIER,
	@RoleId INT
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM dbo.UserRoles WHERE UserId = @UserId AND RoleId = @RoleId;
END
GO

-- Permission Management
IF OBJECT_ID('dbo.AddPermissionToRole','P') IS NOT NULL DROP PROCEDURE dbo.AddPermissionToRole;
GO
CREATE PROCEDURE dbo.AddPermissionToRole
	@RoleId INT,
	@PermissionId INT
AS
BEGIN
	SET NOCOUNT ON;
	IF NOT EXISTS(SELECT 1 FROM dbo.RolePermissions WHERE RoleId = @RoleId AND PermissionId = @PermissionId)
	BEGIN
		INSERT INTO dbo.RolePermissions (RoleId, PermissionId) VALUES (@RoleId, @PermissionId);
	END
END
GO

IF OBJECT_ID('dbo.RevokePermissionFromRole','P') IS NOT NULL DROP PROCEDURE dbo.RevokePermissionFromRole;
GO
CREATE PROCEDURE dbo.RevokePermissionFromRole
	@RoleId INT,
	@PermissionId INT
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM dbo.RolePermissions WHERE RoleId = @RoleId AND PermissionId = @PermissionId;
END
GO

-- Refresh Token Management
IF OBJECT_ID('dbo.CreateRefreshToken','P') IS NOT NULL DROP PROCEDURE dbo.CreateRefreshToken;
GO
CREATE PROCEDURE dbo.CreateRefreshToken
	@Id UNIQUEIDENTIFIER,
	@UserId UNIQUEIDENTIFIER,
	@Token NVARCHAR(200),
	@ExpiresAt DATETIME2,
	@CreatedByIp NVARCHAR(45) = NULL
AS
BEGIN
	SET NOCOUNT ON;
	INSERT INTO dbo.RefreshTokens (Id, UserId, Token, ExpiresAt, IsRevoked, CreatedAt, CreatedByIp)
	VALUES (@Id, @UserId, @Token, @ExpiresAt, 0, SYSUTCDATETIME(), @CreatedByIp);
END
GO

IF OBJECT_ID('dbo.RevokeRefreshToken','P') IS NOT NULL DROP PROCEDURE dbo.RevokeRefreshToken;
GO
CREATE PROCEDURE dbo.RevokeRefreshToken
	@Token NVARCHAR(200),
	@RevokedByIp NVARCHAR(45) = NULL
AS
BEGIN
	SET NOCOUNT ON;
	UPDATE dbo.RefreshTokens
	SET IsRevoked = 1, RevokedAt = SYSUTCDATETIME(), ReplacedByToken = NULL
	WHERE Token = @Token;
END
GO

-- Audit Logging
IF OBJECT_ID('dbo.LogAudit','P') IS NOT NULL DROP PROCEDURE dbo.LogAudit;
GO
CREATE PROCEDURE dbo.LogAudit
	@UserId UNIQUEIDENTIFIER = NULL,
	@Username NVARCHAR(200) = NULL,
	@Action NVARCHAR(200),
	@IpAddress NVARCHAR(45) = NULL,
	@Details NVARCHAR(MAX) = NULL
AS
BEGIN
	SET NOCOUNT ON;
	INSERT INTO dbo.AuditLogs (UserId, Username, Action, IpAddress, Details, Timestamp)
	VALUES (@UserId, @Username, @Action, @IpAddress, @Details, SYSUTCDATETIME());
END
GO

SET NOCOUNT OFF;

-- End of script
