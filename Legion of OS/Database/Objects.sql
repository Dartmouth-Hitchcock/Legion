
SET NUMERIC_ROUNDABORT OFF
GO
SET ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT, QUOTED_IDENTIFIER, ANSI_NULLS ON
GO
SET XACT_ABORT ON
GO
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
GO
IF NOT EXISTS (SELECT * FROM master.dbo.syslogins WHERE loginname = N'legion_admin')
CREATE LOGIN [legion_admin] WITH PASSWORD = 'p@ssw0rd'
GO
CREATE USER [legion_admin] FOR LOGIN [legion_admin]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
IF NOT EXISTS (SELECT * FROM master.dbo.syslogins WHERE loginname = N'legion_frontend')
CREATE LOGIN [legion_frontend] WITH PASSWORD = 'p@ssw0rd'
GO
CREATE USER [legion_frontend] FOR LOGIN [legion_frontend]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
IF NOT EXISTS (SELECT * FROM master.dbo.syslogins WHERE loginname = N'legion_watcher')
CREATE LOGIN [legion_watcher] WITH PASSWORD = 'p@ssw0rd'
GO
CREATE USER [legion_watcher] FOR LOGIN [legion_watcher]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
BEGIN TRANSACTION
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[tblRateLimitTypes]'
GO
CREATE TABLE [dbo].[tblRateLimitTypes]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[Name] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_tblRateTypes] on [dbo].[tblRateLimitTypes]'
GO
ALTER TABLE [dbo].[tblRateLimitTypes] ADD CONSTRAINT [PK_tblRateTypes] PRIMARY KEY CLUSTERED  ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspGetRateLimitTypes]'
GO

CREATE PROCEDURE [dbo].[xspGetRateLimitTypes]
AS 
    BEGIN
        SET NOCOUNT ON;

        SELECT  id AS Id ,
                name AS Name
        FROM    tblRateLimitTypes
    END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[tblCachedResults]'
GO
CREATE TABLE [dbo].[tblCachedResults]
(
[id_Method] [int] NOT NULL,
[CacheKey] [binary] (32) NOT NULL,
[ParameterSet] [xml] NOT NULL,
[Result] [xml] NOT NULL,
[InstanceHits] [int] NOT NULL CONSTRAINT [DF_tblCachedResults_Hits] DEFAULT ((0)),
[LifetimeHits] [int] NOT NULL CONSTRAINT [DF_tblCachedResults_LifetimeHits] DEFAULT ((0)),
[dtCreated] [datetime] NOT NULL,
[dtUpdated] [datetime] NOT NULL,
[dtExpires] [datetime] NOT NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_tblCachedResults] on [dbo].[tblCachedResults]'
GO
ALTER TABLE [dbo].[tblCachedResults] ADD CONSTRAINT [PK_tblCachedResults] PRIMARY KEY CLUSTERED  ([id_Method], [CacheKey])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [IX_tblCachedResults_dtExpires] on [dbo].[tblCachedResults]'
GO
CREATE NONCLUSTERED INDEX [IX_tblCachedResults_dtExpires] ON [dbo].[tblCachedResults] ([dtExpires])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspGetExpiredCachedResults]'
GO

CREATE PROCEDURE [dbo].[xspGetExpiredCachedResults] @timespan INT
AS 
    BEGIN
        SET NOCOUNT ON;

        SELECT  id_Method AS MethodId ,
                CacheKey
        FROM    dbo.tblCachedResults
        WHERE   dtExpires >= DATEADD(SECOND, -1 * @timespan, CURRENT_TIMESTAMP)
    END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[tblHosts]'
GO
CREATE TABLE [dbo].[tblHosts]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[IPAddress] [varchar] (15) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[HostName] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[tfCacheRefreshRequired] [bit] NOT NULL CONSTRAINT [DF_tblHosts_tfRefreshRequired] DEFAULT ((0)),
[tfAssemblyRefreshRequired] [bit] NOT NULL CONSTRAINT [DF_tblHosts_tfAssemblyRefreshRequired] DEFAULT ((0)),
[tfActive] [bit] NOT NULL CONSTRAINT [DF_tblHosts_tfActive] DEFAULT ((1))
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_tblHosts] on [dbo].[tblHosts]'
GO
ALTER TABLE [dbo].[tblHosts] ADD CONSTRAINT [PK_tblHosts] PRIMARY KEY CLUSTERED  ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [IX_ipaddress] on [dbo].[tblHosts]'
GO
CREATE NONCLUSTERED INDEX [IX_ipaddress] ON [dbo].[tblHosts] ([IPAddress])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspSetCacheRefreshFlags]'
GO

CREATE PROCEDURE [dbo].[xspSetCacheRefreshFlags]
AS
BEGIN
	SET NOCOUNT ON;

    UPDATE tblHosts
		SET tfCacheRefreshRequired = 1
END

GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[tblPermissions]'
GO
CREATE TABLE [dbo].[tblPermissions]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[id_Application] [int] NOT NULL,
[id_Method] [int] NOT NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_Permissions] on [dbo].[tblPermissions]'
GO
ALTER TABLE [dbo].[tblPermissions] ADD CONSTRAINT [PK_Permissions] PRIMARY KEY CLUSTERED  ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[tblMethods]'
GO
CREATE TABLE [dbo].[tblMethods]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[id_Service] [int] NOT NULL,
[MethodKey] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[MethodName] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[tfPublic] [bit] NOT NULL CONSTRAINT [DF_tblMethods_tfPublic] DEFAULT ((0)),
[tfRestricted] [bit] NOT NULL CONSTRAINT [DF_tblMethods_tfAuthenticated] DEFAULT ((0)),
[tfLogged] [bit] NOT NULL CONSTRAINT [DF_tblMethods_tfLogged] DEFAULT ((0)),
[tfMissing] [bit] NOT NULL CONSTRAINT [DF_tblMethods_tfMissing] DEFAULT ((0)),
[tfActive] [bit] NOT NULL CONSTRAINT [DF_tblMethods_tfActive] DEFAULT ((1)),
[tfCacheResult] [bit] NOT NULL CONSTRAINT [DF_tblMethods_tfCacheResults] DEFAULT ((0)),
[tfLogReplayDetailsOnException] [bit] NOT NULL CONSTRAINT [DF_tblMethods_tfLogReplayDetailsOnException] DEFAULT ((0)),
[CachedResultLifetime] [int] NULL,
[CAExecutionDuration] [float] NOT NULL CONSTRAINT [DF_tblMethods_CAExecutionDuration] DEFAULT ((0)),
[CACount] [bigint] NOT NULL CONSTRAINT [DF_tblMethods_CACount] DEFAULT ((0))
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_Methods] on [dbo].[tblMethods]'
GO
ALTER TABLE [dbo].[tblMethods] ADD CONSTRAINT [PK_Methods] PRIMARY KEY CLUSTERED  ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [UIX_tblMethods_ServiceMethod] on [dbo].[tblMethods]'
GO
CREATE UNIQUE NONCLUSTERED INDEX [UIX_tblMethods_ServiceMethod] ON [dbo].[tblMethods] ([id_Service], [MethodKey])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[tblApplications]'
GO
CREATE TABLE [dbo].[tblApplications]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[ApplicationName] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[APIKey] [char] (36) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ConsumerIPRange] [varchar] (31) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Description] [varchar] (1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[id_RateLimitType] [int] NULL,
[RateLimit] [int] NULL,
[RateLimitInterval] [int] NULL,
[tfPublic] [bit] NOT NULL CONSTRAINT [DF_tblApplications_tfPublic] DEFAULT ((0)),
[tfLogged] [bit] NOT NULL CONSTRAINT [DF_tblApplications_tfLogged] DEFAULT ((0)),
[tfActive] [bit] NOT NULL CONSTRAINT [DF_tblApplications_tfActive] DEFAULT ((1))
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_Applications] on [dbo].[tblApplications]'
GO
ALTER TABLE [dbo].[tblApplications] ADD CONSTRAINT [PK_Applications] PRIMARY KEY CLUSTERED  ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [UIX_tblApplications_APIKey] on [dbo].[tblApplications]'
GO
CREATE UNIQUE NONCLUSTERED INDEX [UIX_tblApplications_APIKey] ON [dbo].[tblApplications] ([APIKey])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspAddApplicationPermission]'
GO

CREATE PROCEDURE [dbo].[xspAddApplicationPermission]
    @applicationId INT ,
    @methodId INT ,
    @resultcode VARCHAR(10) OUTPUT
AS 
    BEGIN
        SET NOCOUNT ON;

        SELECT  COUNT(*)
        FROM    dbo.tblApplications
        WHERE   id = @applicationid
                AND tfActive = 1
        IF @@ROWCOUNT = 1 
            BEGIN
                SELECT  COUNT(*)
                FROM    dbo.tblMethods
                WHERE   id = @methodid
                        AND tfActive = 1
                IF @@ROWCOUNT = 1 
                    BEGIN
                        INSERT  INTO tblPermissions
                                ( id_Application, id_Method )
                        VALUES  ( @applicationId, @methodId )

                        EXEC xspSetCacheRefreshFlags
						SET @resultcode = 'SUCCESS'
                    END
                ELSE 
                    BEGIN
                        SET @resultcode = 'badappid'
                    END
            END
        ELSE 
            BEGIN
                SET @resultcode = 'badappid'
            END          
    END

GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspCheckApplicationKeyForCollision]'
GO

CREATE PROCEDURE [dbo].[xspCheckApplicationKeyForCollision] 
	@appKey char(36), 
	@tfCollision bit OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	set @tfCollision = 1
	
    select id
		from tblApplications
		where APIKey = @appKey
    
    if @@ROWCOUNT = 0
	begin
		set @tfCollision = 0
	end
END

GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[tblServices]'
GO
CREATE TABLE [dbo].[tblServices]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[ServiceKey] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[AssemblyName] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[ClassName] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[Description] [varchar] (1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ConsumerIPRange] [varchar] (31) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[tfPublic] [bit] NOT NULL CONSTRAINT [DF_tblServices_tfPublic] DEFAULT ((0)),
[tfRestricted] [bit] NOT NULL CONSTRAINT [DF_tblServices_tfAnonymous] DEFAULT ((0)),
[tfLogged] [bit] NOT NULL CONSTRAINT [DF_tblServices_tfLogged] DEFAULT ((0)),
[tfActive] [bit] NOT NULL CONSTRAINT [DF_tblServices_tfActive] DEFAULT ((1))
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_Services] on [dbo].[tblServices]'
GO
ALTER TABLE [dbo].[tblServices] ADD CONSTRAINT [PK_Services] PRIMARY KEY CLUSTERED  ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [UIX_tblServices_ServiceKey] on [dbo].[tblServices]'
GO
CREATE UNIQUE NONCLUSTERED INDEX [UIX_tblServices_ServiceKey] ON [dbo].[tblServices] ([ServiceKey])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[vwPermissions]'
GO
CREATE VIEW [dbo].[vwPermissions]
AS
SELECT        id_Application, id_Service, id_Method, APIKey, ServiceKey, MethodKey
FROM            (SELECT        a.id AS id_Application, s.id AS id_Service, m.id AS id_Method, a.APIKey, m.MethodKey, s.ServiceKey, a.tfActive AS ApplicationActive, 
                                                    s.tfActive AS ServiceActive, m.tfActive AS MethodActive, m.tfMissing AS MethodMissing
                          FROM            dbo.tblMethods AS m LEFT OUTER JOIN
                                                    dbo.tblServices AS s ON m.id_Service = s.id CROSS JOIN
                                                    dbo.tblApplications AS a
                          WHERE        (s.tfRestricted = 0) AND (m.tfRestricted = 0)
                          UNION
                          SELECT        a.id AS id_Application, s.id AS id_Service, m.id AS id_Method, a.APIKey, m.MethodKey, s.ServiceKey, a.tfActive AS ApplicationActive, 
                                                   s.tfActive AS ServiceActive, m.tfActive AS MethodActive, m.tfMissing AS MethodMissing
                          FROM            dbo.tblPermissions AS p LEFT OUTER JOIN
                                                   dbo.tblApplications AS a ON p.id_Application = a.id LEFT OUTER JOIN
                                                   dbo.tblMethods AS m ON p.id_Method = m.id LEFT OUTER JOIN
                                                   dbo.tblServices AS s ON m.id_Service = s.id) AS permissions
WHERE        (ApplicationActive = 1) AND (ServiceActive = 1) AND (MethodActive = 1) AND (MethodMissing = 0) AND (APIKey IS NOT NULL)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspCheckPermission]'
GO

CREATE PROCEDURE [dbo].[xspCheckPermission]
	@applicationid int,
	@servicekey varchar(50),
	@methodkey varchar(50),
	@tfHasPermission bit OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

    set @tfHasPermission = 0
    
    select id_application
		from vwPermissions
		where id_Application = @applicationid
			and ServiceKey = @servicekey
			and Methodkey = @methodkey
		
	if @@ROWCOUNT > 0
	begin
		set @tfHasPermission = 1
	end
END

GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspDeleteAllApplciationPermissions]'
GO
CREATE PROCEDURE [dbo].[xspDeleteAllApplciationPermissions] @applicationId INT
AS 
    BEGIN
        SET NOCOUNT ON;

        DELETE  FROM tblPermissions
        WHERE   id_Application = @applicationId

        EXEC xspSetCacheRefreshFlags
    END

GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspDeleteApplication]'
GO
CREATE PROCEDURE [dbo].[xspDeleteApplication]
    @id INT ,
    @resultcode VARCHAR(10) OUTPUT
AS 
    BEGIN
        SET NOCOUNT ON;

        SET @resultcode = 'FAIL'
	
        SELECT  @id = id
        FROM    tblApplications
        WHERE   id = @id
    
        IF @id IS NOT NULL 
            BEGIN
                DELETE  FROM tblPermissions
                WHERE   id_Application = @id

                UPDATE  tblApplications
                SET     tfActive = 0
                WHERE   id = @id

                EXEC xspSetCacheRefreshFlags
                SET @resultcode = 'SUCCESS'
            END
    END

GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[tblServiceSettings]'
GO
CREATE TABLE [dbo].[tblServiceSettings]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[id_Service] [int] NOT NULL,
[Name] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[IV] [varchar] (64) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Value] [varchar] (8000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[tfEncrypted] [bit] NOT NULL CONSTRAINT [DF_tblConnectionStrings_tfEncrypted] DEFAULT ((0))
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_tblConnectionStrings] on [dbo].[tblServiceSettings]'
GO
ALTER TABLE [dbo].[tblServiceSettings] ADD CONSTRAINT [PK_tblConnectionStrings] PRIMARY KEY CLUSTERED  ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [IX_tblConnectionStrings] on [dbo].[tblServiceSettings]'
GO
CREATE NONCLUSTERED INDEX [IX_tblConnectionStrings] ON [dbo].[tblServiceSettings] ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding constraints to [dbo].[tblServiceSettings]'
GO
ALTER TABLE [dbo].[tblServiceSettings] ADD CONSTRAINT [IX_tblServiceSettings] UNIQUE NONCLUSTERED  ([id_Service], [Name])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspDeleteService]'
GO
CREATE PROCEDURE [dbo].[xspDeleteService]
    @id INT ,
    @resultcode VARCHAR(10) OUTPUT
AS 
    BEGIN
        SET NOCOUNT ON;

        SET @resultcode = 'FAIL'
	
        SELECT  @id = id
        FROM    tblServices
        WHERE   id = @id
    
        IF @id IS NOT NULL 
            BEGIN
                DELETE  FROM tblPermissions
                WHERE   id_Method IN ( SELECT   id
                                       FROM     tblMethods
                                       WHERE    id_Service = @id )

                DELETE  FROM tblServiceSettings
                WHERE   id_Service = @id

                UPDATE  tblMethods
                SET     tfActive = 0
                WHERE   id_Service = @id

                UPDATE  tblServices
                SET     tfActive = 0
                WHERE   id = @id
        
                SET @resultcode = 'SUCCESS'
		
                EXEC xspSetCacheRefreshFlags
            END
    END

GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspDeleteServiceMethod]'
GO
CREATE PROCEDURE [dbo].[xspDeleteServiceMethod]
    @id INT ,
    @resultcode VARCHAR(10) OUTPUT
AS 
    BEGIN
        SET NOCOUNT ON;

        SET @resultcode = 'FAIL'
	
        SELECT  @id = id
        FROM    tblMethods
        WHERE   id = @id
    
        IF @id IS NOT NULL 
            BEGIN
                DELETE  FROM tblPermissions
                WHERE   id_Method = @id

                UPDATE  tblMethods
                SET     tfActive = 0
                WHERE   id = @id

                SET @resultcode = 'SUCCESS'
		
                EXEC xspSetCacheRefreshFlags
            END
    END

GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspDeleteServiceSetting]'
GO

CREATE PROCEDURE [dbo].[xspDeleteServiceSetting]
	@id int,
	@serviceid int,
	@resultcode varchar(10) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	set @resultcode = 'FAIL'

    delete from tblServiceSettings
		where id = @id
			and id_Service = @serviceid

	exec dbo.xspSetCacheRefreshFlags

	if @@rowcount = 1
	begin
		set @resultcode = 'SUCCESS'
	end
END

GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[tblSettings]'
GO
CREATE TABLE [dbo].[tblSettings]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[key] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[value] [varchar] (1000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[description] [varchar] (1000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_tblSettings] on [dbo].[tblSettings]'
GO
ALTER TABLE [dbo].[tblSettings] ADD CONSTRAINT [PK_tblSettings] PRIMARY KEY CLUSTERED  ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspGetAllSettings]'
GO

CREATE PROCEDURE [dbo].[xspGetAllSettings]
AS
BEGIN
	SET NOCOUNT ON;

    select [id], [key], [value]
		from tblSettings
END

GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspGetApplication]'
GO
CREATE PROCEDURE [dbo].[xspGetApplication]
    @apikey CHAR(36) ,
    @id INT OUTPUT ,
    @name VARCHAR(50) OUTPUT ,
    @consumeriprange VARCHAR(31) OUTPUT ,
    @description VARCHAR(1000) OUTPUT ,
    @ratelimittypeid INT OUTPUT ,
    @ratelimittype VARCHAR(50) OUTPUT ,
    @ratelimit INT OUTPUT ,
    @ratelimitinterval INT OUTPUT ,
    @ispublic BIT OUTPUT ,
    @islogged BIT OUTPUT
AS 
    BEGIN
        SET NOCOUNT ON;
	
        SELECT  @id = a.id ,
                @name = ApplicationName ,
                @description = Description ,
                @consumeriprange = ConsumerIPRange ,
                @ratelimittypeid = r.id ,
                @ratelimittype = r.Name ,
                @ratelimit = RateLimit ,
                @ratelimitinterval = RateLimitInterval ,
                @ispublic = tfPublic ,
                @islogged = tfLogged
        FROM    tblApplications AS a
                LEFT JOIN tblRateLimitTypes AS r ON a.id_RateLimitType = r.id
        WHERE   APIKey = @apikey
                AND tfActive = 1
    END

GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspGetApplicationList]'
GO

CREATE PROCEDURE [dbo].[xspGetApplicationList]
AS 
    BEGIN
        SET NOCOUNT ON;

        SELECT  a.id AS ApplicationId ,
                ApplicationName ,
                APIKey AS ApplicationKey ,
                ConsumerIPRange ,
                Description ,
                r.id AS RateLimitTypeId ,
                r.Name AS RateLimitType ,
                RateLimit ,
                RateLimitInterval ,
                tfPublic AS IsPublic ,
                tfLogged AS IsLogged
        FROM    tblApplications AS a
                LEFT JOIN tblRateLimitTypes AS r ON a.id_rateLimitType = r.id
        WHERE   tfActive = 1
        ORDER BY ApplicationName ASC
    END

GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspGetApplicationPermissions]'
GO
CREATE PROCEDURE [dbo].[xspGetApplicationPermissions] @applicationId INT
AS 
    BEGIN
        SET NOCOUNT ON;

        SELECT  id_Method AS MethodId
        FROM    tblPermissions
        WHERE   id_Application = @applicationId
    END

GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspGetAssemblyStatus]'
GO


CREATE PROCEDURE [dbo].[xspGetAssemblyStatus] 
	@ipaddress VARCHAR(15),
	@hostname VARCHAR(50),
	@isRefreshRequired BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @currenthostname VARCHAR(50)
    
    SELECT @isRefreshRequired = tfAssemblyRefreshRequired, @currenthostname = HostName
		FROM tblHosts
		WHERE IPAddress = @ipaddress

	IF @isRefreshRequired IS NOT NULL
	BEGIN
		UPDATE tblHosts
			SET tfAssemblyRefreshRequired = 0
			WHERE IPAddress = @ipaddress
		
		IF @currenthostname IS NULL
		BEGIN
			UPDATE tblHosts
				SET HostName = @hostname
				WHERE IPAddress = @ipaddress      
		END
	END
	ELSE  
	BEGIN
		INSERT INTO tblHosts
			(IPAddress, HostName)
			VALUES
			(@ipaddress, @hostname)
			
		SET @isRefreshRequired = 0         
	END
END

GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspGetAverageExecutionDurations]'
GO

CREATE PROCEDURE [dbo].[xspGetAverageExecutionDurations] @top INT
AS 
    BEGIN
        SET NOCOUNT ON;

        IF @top IS NULL 
            SET @top = 10      

        SELECT TOP 10
                ( servicekey + '.' + methodname ) AS Method ,
                AVG(ExecutionDuration) AS AverageExecutionDuration
        FROM    tblRollingLog AS l
                LEFT JOIN tblMethods AS m ON l.id_method = m.id
                LEFT JOIN tblServices AS s ON s.id = m.id_service
        GROUP BY ( servicekey + '.' + methodname )
        ORDER BY AverageExecutionDuration DESC
    END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspGetCacheStatus]'
GO

CREATE PROCEDURE [dbo].[xspGetCacheStatus] 
	@ipaddress VARCHAR(15),
	@isRefreshRequired BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT @isRefreshRequired = tfCacheRefreshRequired
		FROM tblHosts
		WHERE IPAddress = @ipaddress

	IF @isRefreshRequired IS NOT NULL
	BEGIN
		UPDATE tblHosts
			SET tfCacheRefreshRequired = 0
			WHERE IPAddress = @ipaddress
	END
	ELSE  
	BEGIN
		INSERT INTO tblHosts
			(IPAddress)
			VALUES
			(@ipaddress)
			
		SET @isRefreshRequired = 1         
	END
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[tblCallLog]'
GO
CREATE TABLE [dbo].[tblCallLog]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[id_method] [int] NULL,
[id_application] [int] NULL,
[ExecutionDuration] [float] NOT NULL,
[CallingHostIpAddress] [varchar] (15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[HandledByHost] [varchar] (15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[dtCall] [datetime] NOT NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_tblRecentCallLog] on [dbo].[tblCallLog]'
GO
ALTER TABLE [dbo].[tblCallLog] ADD CONSTRAINT [PK_tblRecentCallLog] PRIMARY KEY CLUSTERED  ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[vwRecentMethodCalls]'
GO
CREATE VIEW [dbo].[vwRecentMethodCalls]
AS
SELECT        a.ApplicationName, s.ServiceKey, m.MethodKey, l.ExecutionDuration, l.dtCall AS CalledOn, l.HandledByHost, l.CallingHostIpAddress
FROM            dbo.tblCallLog AS l LEFT OUTER JOIN
                         dbo.tblMethods AS m ON l.id_method = m.id LEFT OUTER JOIN
                         dbo.tblServices AS s ON m.id_Service = s.id LEFT OUTER JOIN
                         dbo.tblApplications AS a ON l.id_application = a.id
WHERE        (l.id_method IS NOT NULL)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspGetCallCount]'
GO

CREATE PROCEDURE [dbo].[xspGetCallCount]
    @span INT ,
    @count INT OUTPUT
AS 
    BEGIN
        SET NOCOUNT ON;

        SELECT  @count = COUNT(*)
        FROM    tblCallLog
        WHERE   dtCall > DATEADD(MILLISECOND, -1 * @span, CURRENT_TIMESTAMP)
		
		SELECT TOP 10 ServiceKey, MethodKey, ExecutionDuration, CalledOn
		FROM vwRecentMethodCalls
		ORDER BY CalledOn desc
    END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspGetHostLoad]'
GO
CREATE PROCEDURE [dbo].[xspGetHostLoad]
AS 
    BEGIN
        SET NOCOUNT ON;

        SELECT  HandledByHost ,
                COUNT(*) AS [Load]
        FROM    ( SELECT    HandledByHost
                  FROM      dbo.tblCallLog
                  WHERE     dtCall >= DATEADD(mi, -5, CURRENT_TIMESTAMP)
                ) AS a
        GROUP BY HandledByHost
    END

GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[tblKeyRevocations]'
GO
CREATE TABLE [dbo].[tblKeyRevocations]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[id_application] [int] NULL,
[APIKey] [char] (36) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[dtRevoked] [datetime] NOT NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_tblRevokedKeys] on [dbo].[tblKeyRevocations]'
GO
ALTER TABLE [dbo].[tblKeyRevocations] ADD CONSTRAINT [PK_tblRevokedKeys] PRIMARY KEY CLUSTERED  ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating index [IX_tblRevokedKeys] on [dbo].[tblKeyRevocations]'
GO
CREATE NONCLUSTERED INDEX [IX_tblRevokedKeys] ON [dbo].[tblKeyRevocations] ([APIKey])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspGetKeyRevocationList]'
GO

CREATE PROCEDURE [dbo].[xspGetKeyRevocationList]
AS 
    BEGIN
        SET NOCOUNT ON;

        SELECT  APIKey AS [Key]
        FROM    tblKeyRevocations
    END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspGetMethods]'
GO

CREATE PROCEDURE [dbo].[xspGetMethods]
AS 
    BEGIN
        SET NOCOUNT ON;

        SELECT  m.id AS MethodId ,
                m.id_Service AS ServiceId ,
                m.MethodKey ,
                s.ServiceKey ,
                m.MethodName ,
				m.CachedResultLifetime,
				m.tfCacheResult AS IsResultCacheable,
				m.tfLogReplayDetailsOnException AS IsLogReplayDetailsOnException,
                m.tfMissing AS IsMissing ,
                CASE WHEN m.tfRestricted = 1
                          OR s.tfRestricted = 1 THEN CAST(1 AS BIT)
                     ELSE CAST(0 AS BIT)
                END AS IsRestricted ,
				m.tfPublic AS IsPublic,
                m.tfLogged AS IsLogged
        FROM    tblMethods AS m
                LEFT JOIN tblServices AS s ON m.id_Service = s.id
        WHERE   m.tfActive = 1
                AND s.tfActive = 1
        ORDER BY m.MethodKey ASC
    END

GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspGetServerStatus]'
GO

CREATE PROCEDURE [dbo].[xspGetServerStatus]
AS 
    BEGIN
        SET NOCOUNT ON;

        SELECT  id AS Id ,
                IPAddress ,
                HostName ,
                tfAssemblyRefreshRequired AS IsAssemblyRefreshRequired ,
                tfCacheRefreshRequired AS IsCacheRefreshRequired
        FROM    dbo.tblHosts
        WHERE   tfActive = 1
    END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspGetServiceMethods]'
GO

CREATE PROCEDURE [dbo].[xspGetServiceMethods] @serviceId INT
AS 
    BEGIN
        SET NOCOUNT ON;

        SELECT  m.id AS MethodId ,
                m.id_Service AS ServiceId ,
                m.MethodKey ,
                s.ServiceKey ,
                m.MethodName ,
				m.CachedResultLifetime,
				m.tfCacheResult AS IsResultCacheable,
				m.tfPublic AS IsPublic,
                m.tfRestricted AS IsRestricted ,
                m.tfLogged AS IsLogged
        FROM    tblMethods AS m
                LEFT JOIN tblServices AS s ON m.id_Service = s.id
        WHERE   s.id = @serviceId
                AND s.tfActive = 1
                AND m.tfActive = 1
				AND m.tfMissing = 0
        ORDER BY MethodKey ASC
    END

GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspGetServices]'
GO

CREATE PROCEDURE [dbo].[xspGetServices]
AS 
    BEGIN
        SET NOCOUNT ON;

        SELECT  id AS ServiceId ,
                ServiceKey ,
                AssemblyName ,
                ClassName ,
                ConsumerIPRange ,
                tfRestricted AS IsRestricted ,
				tfPublic AS IsPublic,
                tfLogged AS IsLogged ,
                Description
        FROM    tblServices
        WHERE   tfActive = 1
        ORDER BY ServiceKey ASC
    END

GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[tblReplayLog]'
GO
CREATE TABLE [dbo].[tblReplayLog]
(
[id_Event] [int] NOT NULL,
[id_Method] [int] NOT NULL,
[ParameterSet] [xml] NOT NULL,
[ExceptionName] [varchar] (1000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[ExceptionMessage] [varchar] (1000) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[ExceptionStackTrace] [text] COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[dtEntry] [datetime] NOT NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_tblReplayLog] on [dbo].[tblReplayLog]'
GO
ALTER TABLE [dbo].[tblReplayLog] ADD CONSTRAINT [PK_tblReplayLog] PRIMARY KEY CLUSTERED  ([id_Event])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspLogReplyException]'
GO

CREATE PROCEDURE [dbo].[xspLogReplyException]
    @eventid INT ,
    @methodid INT ,
    @parameters XML ,
    @exceptionname VARCHAR(1000) ,
    @exceptionmessage VARCHAR(1000) ,
    @exceptionstacktrace TEXT
AS 
    BEGIN
        SET NOCOUNT ON;

        INSERT  INTO tblReplayLog
                ( id_Event ,
                  id_Method ,
                  ParameterSet ,
                  ExceptionName ,
                  ExceptionMessage ,
                  ExceptionStackTrace ,
                  dtEntry
                )
        VALUES  ( @eventid ,
                  @methodid ,
                  @parameters ,
                  @exceptionname ,
                  @exceptionmessage ,
                  @exceptionstacktrace ,
                  CURRENT_TIMESTAMP
                )
    END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspGetServiceSettings]'
GO


CREATE PROCEDURE [dbo].[xspGetServiceSettings]
    @servicekey CHAR(36) ,
    @serviceid INT ,
    @resultcode VARCHAR(10) OUTPUT
AS 
    BEGIN
        SET NOCOUNT ON;

        SET @resultcode = 'FAIL'

        IF @servicekey IS NOT NULL 
            BEGIN
                SELECT  c.id AS Id ,
                        Name ,
						IV ,
                        Value ,
                        tfEncrypted AS IsEncrypted
                FROM    tblServiceSettings AS c
                        LEFT JOIN tblServices AS s ON c.id_Service = s.id
                WHERE   s.ServiceKey = @servicekey
                        AND s.tfActive = 1
                ORDER BY name ASC
                
                SET @resultcode = 'SUCCESS'
            END
        ELSE 
            IF @serviceid IS NOT NULL 
                BEGIN
                    SELECT  c.id AS Id ,
                            Name ,
							IV ,
                            Value ,
                            tfEncrypted AS IsEncrypted
                    FROM    tblServiceSettings AS c
                            LEFT JOIN tblServices AS s ON c.id_Service = s.id
                    WHERE   id_Service = @serviceid
                            AND s.tfActive = 1
                    ORDER BY name ASC
                    
                    SET @resultcode = 'SUCCESS'
                END
    END


GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspGetSettingById]'
GO

CREATE PROCEDURE [dbo].[xspGetSettingById]
	@id int,
	@value varchar(1000) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	select @value = [value]
		from tblSettings
		where id = @id
END


GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspGetSettingByKey]'
GO
CREATE PROCEDURE [dbo].[xspGetSettingByKey]
	@key varchar(50),
	@value varchar(1000) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	select @value = [value]
		from tblSettings
		where [key]  = @key

END

GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspLogMethodCall]'
GO

CREATE PROCEDURE [dbo].[xspLogMethodCall]
    @methodid INT ,
    @executionDuration FLOAT ,
    @dtCall DATETIME ,
    @applicationid INT ,
    @handledbyipaddress VARCHAR(15) ,
    @hostipaddress VARCHAR(15) ,
    @useripaddress VARCHAR(15) ,
    @permanentLog BIT
AS 
    BEGIN
        SET NOCOUNT ON;

        DECLARE @logsize INT = 10000

        DECLARE @caed FLOAT
        DECLARE @cacnt BIGINT
    
		--update continuous average
        SELECT  @caed = CAExecutionDuration ,
                @cacnt = CACount
        FROM    dbo.tblMethods
        WHERE   id = @methodid  

        UPDATE  dbo.tblMethods
        SET     CAExecutionDuration = ( ( @executionDuration + ( @caed
                                                              * @cacnt ) )
                                        / ( @cacnt + 1 ) ) ,
                CACount = ( @cacnt + 1 )
        WHERE   id = @methodid

		--insert rolling log entry
        INSERT  INTO tblCallLog
                ( id_method ,
                  id_application ,
                  dtCall ,
                  ExecutionDuration ,
                  CallingHostIpAddress ,
                  HandledByHost
                )
        VALUES  ( @methodid ,
                  @applicationid ,
                  @dtCall ,
                  @executionDuration ,
                  @hostipaddress ,
                  @handledbyipaddress
                )

        DELETE  FROM tblCallLog
        WHERE   id < ( SCOPE_IDENTITY() - @logsize )
    END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspRevokeKey]'
GO

CREATE PROCEDURE [dbo].[xspRevokeKey]
    @applicationid INT ,
    @key CHAR(36) ,
    @resultcode CHAR(10)
AS 
    BEGIN
        SET NOCOUNT ON;

        SET @resultcode = 'FAIL'

        DECLARE @revoke BIT = 0

        IF @applicationid IS NOT NULL 
            BEGIN
                SET @revoke = 1
            END
        ELSE 
            IF @applicationid IS NULL
                AND @key IS NOT NULL 
                BEGIN
                    SELECT  @applicationid = id
                    FROM    dbo.tblApplications
                    WHERE   APIKey = @key

                    IF @applicationid IS NOT NULL 
                        SET @revoke = 1
                END      
		
        IF @revoke = 1 
            BEGIN
                INSERT  INTO dbo.tblKeyRevocations
                        ( id_application ,
                          APIKey ,
                          dtRevoked 
                        )
                        SELECT  id ,
                                APIKey ,
                                CURRENT_TIMESTAMP
                        FROM    dbo.tblApplications
                        WHERE   id = @applicationid
		
                UPDATE  dbo.tblApplications
                SET     APIKey = NULL
                WHERE   id = @applicationid

				EXEC xspSetCacheRefreshFlags

                SET @resultcode = 'SUCCESS'
            END
    END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspSetAssemblyRefreshFlags]'
GO


CREATE PROCEDURE [dbo].[xspSetAssemblyRefreshFlags]
AS 
    BEGIN
        SET NOCOUNT ON;

        UPDATE  tblHosts
        SET     tfAssemblyRefreshRequired = 1
        WHERE   tfActive = 1
    END


GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspSetSettingById]'
GO

CREATE PROCEDURE [dbo].[xspSetSettingById]
	@id int,
	@key varchar(50),
	@value varchar(1000)
AS
BEGIN
	SET NOCOUNT ON;

    select [id] from tblSettings where [id] = @id
    if @@ROWCOUNT = 1
    begin
		if @key is not null
		begin
			update tblSettings
				set [key] = @key
				where id = @id
		end
		
		if @value is not null
		begin
			update tblSettings
				set value = @value
				where id = @id
		end
    end
    else
    begin
		insert into tblSettings
			([key], [value]) values
			(@key, @value)
    end
END

GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspSetSettingByKey]'
GO

CREATE PROCEDURE [dbo].[xspSetSettingByKey]
	@key varchar(50),
	@value varchar(1000)
AS
BEGIN
	SET NOCOUNT ON;

    select [id] from tblSettings where [key] = @key
    if @@ROWCOUNT = 1
    begin
		update tblSettings
			set value = @value
			where [key] = @key
    end
    else
    begin
		insert into tblSettings
			([key], [value]) values
			(@key, @value)
    end
END

GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspUpdateApplication]'
GO
CREATE PROCEDURE [dbo].[xspUpdateApplication]
    @id INT OUTPUT ,
    @applicationName VARCHAR(50) ,
    @applicationKey CHAR(36) ,
    @consumeriprange VARCHAR(31) ,
    @description VARCHAR(1000) ,
	@ratelimitid INT,
	@ratelimit INT,
	@ratelimitinterval INT,
	@public BIT,
    @logged BIT ,
    @resultcode VARCHAR(10) OUTPUT
AS 
    BEGIN
        SET NOCOUNT ON;

        SET @resultcode = 'FAIL'

        DECLARE @update INT = 0
	
        IF @id = -1 
            BEGIN
                SELECT  @id = id
                FROM    dbo.tblApplications
                WHERE   ApplicationName = ApplicationName
                        AND tfActive = 0

                IF @@rowcount = 0
                    BEGIN
                        INSERT  INTO tblApplications
                                ( ApplicationName ,
                                  APIKey ,
                                  ConsumerIPRange ,
                                  Description ,
								  id_RateLimitType,
								  RateLimit,
								  RateLimitInterval,
								  tfPublic,
                                  tfLogged
                                )
                        VALUES  ( @applicationName ,
                                  @applicationKey ,
                                  @consumeriprange ,
                                  @description ,
								  @ratelimitid,
								  @ratelimit,
								  @ratelimitinterval,
								  @public,
                                  @logged
                                )
		
                        EXEC xspSetCacheRefreshFlags
                        SET @id = SCOPE_IDENTITY()
                        SET @resultcode = 'SUCCESS'
                    END
                ELSE 
                    BEGIN
                        SET @update = 1
                    END      
            END
        ELSE 
            BEGIN
                SET @update = 1
            END
    
        IF @update = 1 
            BEGIN
                UPDATE  tblApplications
                SET     ApplicationName = @applicationName ,
                        APIKey = @applicationKey ,
                        ConsumerIpRange = @consumeriprange ,
                        Description = @description ,
						id_RateLimitType = @ratelimitid,
						RateLimit = @ratelimit,
						RateLimitInterval = @ratelimitinterval,
						tfPublic = @public,
                        tfLogged = @logged,
						tfActive = 1
                WHERE   id = @id
		
                EXEC xspSetCacheRefreshFlags
                SET @resultcode = 'SUCCESS'
            END
    END

GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspUpdateMethodMissingFlag]'
GO

CREATE PROCEDURE [dbo].[xspUpdateMethodMissingFlag]
    @methodid INT ,
    @isMissing BIT ,
    @resultcode VARCHAR(10) OUTPUT
AS 
    BEGIN
        SET NOCOUNT ON;

		SET @resultcode = 'FAIL'

        UPDATE  dbo.tblMethods
        SET     tfMissing = @isMissing
        WHERE   id = @methodid

		IF @@rowcount > 0
			SET @resultcode = 'SUCCESS'
    END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspUpdateService]'
GO
CREATE PROCEDURE [dbo].[xspUpdateService]
    @id INT OUTPUT ,
    @serviceKey VARCHAR(50) ,
    @serviceDescription CHAR(36) ,
    @serviceIPRange VARCHAR(31) ,
    @serviceAssembly VARCHAR(31) ,
    @serviceClass VARCHAR(1000) ,
    @restricted BIT ,
	@public BIT,
    @logged BIT ,
    @resultcode VARCHAR(10) OUTPUT
AS 
    BEGIN
        SET NOCOUNT ON;

        SET @resultcode = 'FAIL'
	
        DECLARE @update INT = 0


        IF @id = -1 
            BEGIN
                SELECT  @id = id
                FROM    dbo.tblServices
                WHERE   ServiceKey = @serviceKey
                        AND tfActive = 0

                IF @@rowcount = 1 
                    BEGIN
                        SET @update = 1
                    END
                ELSE 
                    BEGIN  
                        INSERT  INTO tblServices
                                ( ServiceKey ,
                                  AssemblyName ,
                                  ClassName ,
                                  Description ,
                                  ConsumerIPRange ,
                                  tfRestricted ,
								  tfPublic,
                                  tfLogged
                                )
                        VALUES  ( @serviceKey ,
                                  @serviceAssembly ,
                                  @serviceClass ,
                                  @serviceDescription ,
                                  @serviceIPRange ,
                                  @restricted ,
								  @public,
                                  @logged
                                )
		
                        SET @id = SCOPE_IDENTITY()
                        SET @resultcode = 'SUCCESS'
		
                        EXEC xspSetCacheRefreshFlags
                    END      
            END
        ELSE 
            BEGIN
                SET @update = 1
            END          
    
        IF @update = 1 
            BEGIN
                UPDATE  tblServices
                SET     ServiceKey = @serviceKey ,
                        AssemblyName = @serviceAssembly ,
                        ClassName = @serviceClass ,
                        Description = @serviceDescription ,
                        ConsumerIPRange = @serviceIpRange ,
                        tfRestricted = @restricted ,
						tfPublic = @public,
                        tfLogged = @logged,
						tfActive = 1
                WHERE   id = @id
		
                SET @resultcode = 'SUCCESS'
		
                EXEC xspSetCacheRefreshFlags
            END
    END

GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspUpdateServiceMethod]'
GO
CREATE PROCEDURE [dbo].[xspUpdateServiceMethod]
    @id INT OUTPUT ,
    @serviceid INT ,
    @methodkey VARCHAR(50) ,
    @methodname VARCHAR(50) ,
    @cachedresultlifetime INT ,
    @cacheresult BIT ,
    @restricted BIT ,
    @public BIT ,
    @logged BIT ,
    @resultcode VARCHAR(10) OUTPUT
AS 
    BEGIN
        SET NOCOUNT ON;

        SET @resultcode = 'FAIL'
	
        DECLARE @update INT = 0

        IF @id = -1 
            BEGIN
                SELECT  @id = id
                FROM    tblMethods
                WHERE   id_Service = @serviceid
                        AND methodkey = @methodkey
		
                IF @@rowcount = 1 
                    BEGIN
                        SET @update = 1 
                    END
                ELSE 
                    BEGIN              
                        INSERT  INTO tblMethods
                                ( id_Service ,
                                  MethodKey ,
                                  MethodName ,
                                  CachedResultLifetime ,
                                  tfCacheResult ,
                                  tfRestricted ,
                                  tfPublic ,
                                  tfLogged
                                )
                        VALUES  ( @serviceid ,
                                  @methodkey ,
                                  @methodname ,
                                  @cachedresultlifetime ,
                                  @cacheresult ,
                                  @restricted ,
                                  @public ,
                                  @logged
                                )
		
                        SET @id = SCOPE_IDENTITY()
                        SET @resultcode = 'SUCCESS'

                        EXEC xspSetCacheRefreshFlags
                    END
            END
        ELSE 
            BEGIN
                SET @update = 1
            END   

        IF @UPDATE = 1 
            BEGIN
                UPDATE  tblMethods
                SET     id_Service = @serviceid ,
                        MethodKey = @methodkey ,
                        MethodName = @methodname ,
                        CachedResultLifetime = @cachedresultlifetime ,
                        tfCacheResult = @cacheresult ,
                        tfRestricted = @restricted ,
                        tfPublic = @public ,
                        tfLogged = @logged ,
                        tfActive = 1
                WHERE   id = @id
		
                SET @resultcode = 'SUCCESS'
		
                EXEC xspSetCacheRefreshFlags
            END
    END

GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspUpdateServiceSetting]'
GO

CREATE PROCEDURE [dbo].[xspUpdateServiceSetting]
    @id INT OUTPUT ,
    @serviceid INT ,
    @name VARCHAR(50) ,
	--@iv VARCHAR(64) ,
    @value VARCHAR(8000) ,
    @tfEncrypted BIT ,
    @resultcode VARCHAR(10) OUTPUT
AS 
    BEGIN
        SET NOCOUNT ON;

        SET @resultcode = 'FAIL'

        DECLARE @update INT = 0

		DECLARE @iv AS VARCHAR(64) = NULL
		IF @iv IS NULL
		BEGIN
			SET @iv = CONCAT(@id, ':', @name)
		END

        IF @id = -1 
            BEGIN
                SELECT  @id = id
                FROM    dbo.tblServiceSettings
                WHERE   id_Service = @serviceid
                        AND Name = @name
	
                IF @@rowcount = 1 
                    BEGIN
                        SET @update = 1 
                    END
                ELSE 
                    BEGIN 
                        INSERT  INTO tblServiceSettings
                                ( id_Service ,
                                  Name ,
								  IV ,
                                  Value ,
                                  tfEncrypted
								)
                        VALUES  ( @serviceid ,
                                  @name ,
								  @iv ,
                                  @value ,
                                  @tfEncrypted
								)

                        SET @id = SCOPE_IDENTITY()
                        SET @resultcode = 'SUCCESS'

                        EXEC xspSetCacheRefreshFlags
                    END
            END                  
        ELSE 
            BEGIN
                SET @update = 1
            END                   

        IF @update = 1 
            BEGIN
                UPDATE  tblServiceSettings
                SET     Name = @name ,
						IV = @iv ,
                        Value = @value ,
                        tfEncrypted = @tfEncrypted
                WHERE   id = @id

                SET @resultcode = 'SUCCESS'

                EXEC xspSetCacheRefreshFlags
            END
    END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspGetCachedResult]'
GO

CREATE PROCEDURE [dbo].[xspGetCachedResult]
    @methodid INT ,
    @cachekey BINARY(32)
AS 
    BEGIN
        SET NOCOUNT ON;

        SELECT  Result ,
                dtUpdated AS UpdatedOn ,
                dtExpires AS ExpiresOn
        FROM    dbo.tblCachedResults
        WHERE   id_Method = @methodid
                AND CacheKey = @cachekey
                AND dtExpires >= CURRENT_TIMESTAMP
  
        IF @@ROWCOUNT = 1 
            BEGIN
                UPDATE  dbo.tblCachedResults
                SET     InstanceHits = InstanceHits + 1 ,
                        LifetimeHits = LifetimeHits + 1
                WHERE   id_Method = @methodid
                        AND CacheKey = @cachekey 
            END              
    END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspInsertCachedResult]'
GO
CREATE PROCEDURE [dbo].[xspInsertCachedResult]
    @methodid INT ,
    @parameterset XML ,
    @result XML ,
    @cachekey BINARY(32) ,
    @expireson DATETIME OUTPUT
AS 
    BEGIN
        SET NOCOUNT ON;

        SET @expireson = NULL
        
        DECLARE @isCacheable BIT
        DECLARE @cacheLifetime INT
  
        SELECT  @isCacheable = tfCacheResult ,
                @cacheLifetime = CachedResultLifetime
        FROM    dbo.tblMethods
        WHERE   id = @methodid

        IF @isCacheable = 1
            AND @cacheLifetime IS NOT NULL 
            BEGIN
                DECLARE @now DATETIME = CURRENT_TIMESTAMP

                SET @expireson = DATEADD(s, @cacheLifetime, @now)

                UPDATE  dbo.tblCachedResults
                SET     Result = @result ,
                        InstanceHits = 0 ,
                        dtUpdated = @now ,
                        dtExpires = @expireson
                WHERE   id_method = @methodid
                        AND CacheKey = @cachekey

                IF @@ROWCOUNT = 0 
                    BEGIN
                        INSERT  INTO dbo.tblCachedResults
                                ( id_Method ,
                                  CacheKey ,
                                  ParameterSet ,
                                  Result ,
                                  dtCreated ,
                                  dtUpdated ,
                                  dtExpires
                                )
                        VALUES  ( @methodid ,
                                  @cachekey ,
                                  @parameterset ,
                                  @result ,
                                  @now ,
                                  @now ,
                                  @expireson
							        
                                )
                    END
            END      
    END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[vwResultCache]'
GO
CREATE VIEW [dbo].[vwResultCache]
AS
SELECT        dbo.tblCachedResults.CacheKey, dbo.tblServices.ServiceKey AS Service, dbo.tblMethods.MethodKey AS Method, dbo.tblCachedResults.ParameterSet, 
                         dbo.tblCachedResults.Result, dbo.tblCachedResults.InstanceHits, dbo.tblCachedResults.LifetimeHits, dbo.tblCachedResults.dtCreated AS CreatedOn, 
                         dbo.tblCachedResults.dtUpdated AS UpdatedOn, dbo.tblCachedResults.dtExpires AS ExpiresOn
FROM            dbo.tblCachedResults INNER JOIN
                         dbo.tblMethods ON dbo.tblCachedResults.id_Method = dbo.tblMethods.id INNER JOIN
                         dbo.tblServices ON dbo.tblMethods.id_Service = dbo.tblServices.id
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspPurgeResultsCache]'
GO

CREATE PROCEDURE [dbo].[xspPurgeResultsCache]
AS 
    BEGIN
        SET NOCOUNT ON;

        DELETE  FROM dbo.tblCachedResults
        WHERE   dtExpires < DATEADD(d, -1, CURRENT_TIMESTAMP)
    END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspGetService]'
GO


CREATE PROCEDURE [dbo].[xspGetService]
	@serviceid INT,
	@resultcode VARCHAR(10) OUTPUT
AS 
    BEGIN
        SET NOCOUNT ON;

		SET @resultcode = 'FAIL'

        SELECT  ServiceKey ,
                AssemblyName ,
                ClassName ,
                Description ,
                ConsumerIPRange ,
                tfRestricted AS IsRestricted ,
				tfPublic AS IsPublic,
                tfLogged  AS IsLogged
        FROM    tblServices
        WHERE   id = @serviceid

		IF @@ROWCOUNT = 1
			SET @resultcode = 'SUCCESS'
    END


GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspExpireResultsCache]'
GO

CREATE PROCEDURE [dbo].[xspExpireResultsCache]
    @servicekey VARCHAR(50) ,
    @methodkey VARCHAR(50),
	@itemcount INT OUTPUT
AS 
    BEGIN
        SET NOCOUNT ON;
        
        update dbo.tblCachedResults
		SET dtExpires = CURRENT_TIMESTAMP      
        WHERE   id_Method = ( SELECT    m.id
                              FROM      tblMethods AS m
                                        LEFT JOIN dbo.tblServices AS s ON m.id_Service = s.id
                              WHERE     s.ServiceKey = @servicekey
                                        AND m.MethodKey = @methodkey
                            )

		SET @itemcount = @@ROWCOUNT
    END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[tblCallLogBuffer]'
GO
CREATE TABLE [dbo].[tblCallLogBuffer]
(
[MethodId] [int] NOT NULL,
[ExecutionDuration] [float] NOT NULL,
[CalledAt] [datetime] NOT NULL,
[ApplicationId] [int] NOT NULL,
[HandledByIpAddress] [varchar] (15) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[HostIpAddress] [varchar] (15) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[ClientIpAddress] [varchar] (15) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[xspFlushCallLogBuffer]'
GO
CREATE PROCEDURE [dbo].[xspFlushCallLogBuffer]
AS
BEGIN
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [dbo].[tblPermissions]'
GO
ALTER TABLE [dbo].[tblPermissions] ADD CONSTRAINT [FK_Permissions_Applications] FOREIGN KEY ([id_Application]) REFERENCES [dbo].[tblApplications] ([id])
GO
ALTER TABLE [dbo].[tblPermissions] ADD CONSTRAINT [FK_Permissions_Methods] FOREIGN KEY ([id_Method]) REFERENCES [dbo].[tblMethods] ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [dbo].[tblKeyRevocations]'
GO
ALTER TABLE [dbo].[tblKeyRevocations] ADD CONSTRAINT [FK_tblKeyRevocations_tblApplications] FOREIGN KEY ([id_application]) REFERENCES [dbo].[tblApplications] ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [dbo].[tblCallLog]'
GO
ALTER TABLE [dbo].[tblCallLog] ADD CONSTRAINT [FK_tblRollingLog_tblApplications] FOREIGN KEY ([id_application]) REFERENCES [dbo].[tblApplications] ([id])
GO
ALTER TABLE [dbo].[tblCallLog] ADD CONSTRAINT [FK_tblRecentCallLog_tblMethods] FOREIGN KEY ([id_method]) REFERENCES [dbo].[tblMethods] ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [dbo].[tblApplications]'
GO
ALTER TABLE [dbo].[tblApplications] ADD CONSTRAINT [FK_tblApplications_tblRateLimitTypes] FOREIGN KEY ([id_RateLimitType]) REFERENCES [dbo].[tblRateLimitTypes] ([id])
GO
ALTER TABLE [dbo].[tblApplications] ADD CONSTRAINT [FK_tblApplications_tblRateTypes] FOREIGN KEY ([id_RateLimitType]) REFERENCES [dbo].[tblRateLimitTypes] ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [dbo].[tblCachedResults]'
GO
ALTER TABLE [dbo].[tblCachedResults] ADD CONSTRAINT [FK_tblCachedResults_tblMethods] FOREIGN KEY ([id_Method]) REFERENCES [dbo].[tblMethods] ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [dbo].[tblMethods]'
GO
ALTER TABLE [dbo].[tblMethods] ADD CONSTRAINT [FK_Methods_Services] FOREIGN KEY ([id_Service]) REFERENCES [dbo].[tblServices] ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [dbo].[tblServiceSettings]'
GO
ALTER TABLE [dbo].[tblServiceSettings] ADD CONSTRAINT [FK_tblConnectionStrings_tblApplications] FOREIGN KEY ([id_Service]) REFERENCES [dbo].[tblServices] ([id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating extended properties'
GO
EXEC sp_addextendedproperty N'MS_DiagramPane1', N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[22] 4[9] 2[64] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "permissions"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 135
               Right = 221
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
', 'SCHEMA', N'dbo', 'VIEW', N'vwPermissions', NULL, NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
DECLARE @xp int
SELECT @xp=1
EXEC sp_addextendedproperty N'MS_DiagramPaneCount', @xp, 'SCHEMA', N'dbo', 'VIEW', N'vwPermissions', NULL, NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
EXEC sp_addextendedproperty N'MS_DiagramPane1', N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "m"
            Begin Extent = 
               Top = 6
               Left = 262
               Bottom = 135
               Right = 464
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "s"
            Begin Extent = 
               Top = 6
               Left = 502
               Bottom = 135
               Right = 689
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "a"
            Begin Extent = 
               Top = 6
               Left = 727
               Bottom = 136
               Right = 914
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "l"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 241
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
', 'SCHEMA', N'dbo', 'VIEW', N'vwRecentMethodCalls', NULL, NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
DECLARE @xp int
SELECT @xp=1
EXEC sp_addextendedproperty N'MS_DiagramPaneCount', @xp, 'SCHEMA', N'dbo', 'VIEW', N'vwRecentMethodCalls', NULL, NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
EXEC sp_addextendedproperty N'MS_DiagramPane1', N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "tblCachedResults"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 219
               Right = 292
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "tblMethods"
            Begin Extent = 
               Top = 9
               Left = 295
               Bottom = 139
               Right = 499
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "tblServices"
            Begin Extent = 
               Top = 6
               Left = 488
               Bottom = 187
               Right = 717
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
', 'SCHEMA', N'dbo', 'VIEW', N'vwResultCache', NULL, NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
DECLARE @xp int
SELECT @xp=1
EXEC sp_addextendedproperty N'MS_DiagramPaneCount', @xp, 'SCHEMA', N'dbo', 'VIEW', N'vwResultCache', NULL, NULL
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering permissions on [dbo].[tblApplications]'
GO
GRANT SELECT ON  [dbo].[tblApplications] TO [heimdall_user]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering permissions on [dbo].[tblCallLogBuffer]'
GO
GRANT INSERT ON  [dbo].[tblCallLogBuffer] TO [legion_frontend]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering permissions on [dbo].[xspCheckPermission]'
GO
GRANT EXECUTE ON  [dbo].[xspCheckPermission] TO [legion_frontend]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering permissions on [dbo].[xspGetApplication]'
GO
GRANT EXECUTE ON  [dbo].[xspGetApplication] TO [legion_frontend]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering permissions on [dbo].[xspGetAssemblyStatus]'
GO
GRANT EXECUTE ON  [dbo].[xspGetAssemblyStatus] TO [legion_frontend]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering permissions on [dbo].[xspGetCacheStatus]'
GO
GRANT EXECUTE ON  [dbo].[xspGetCacheStatus] TO [legion_frontend]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering permissions on [dbo].[xspGetCachedResult]'
GO
GRANT EXECUTE ON  [dbo].[xspGetCachedResult] TO [legion_frontend]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering permissions on [dbo].[xspGetCallCount]'
GO
GRANT EXECUTE ON  [dbo].[xspGetCallCount] TO [legion_stats]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering permissions on [dbo].[xspGetExpiredCachedResults]'
GO
GRANT EXECUTE ON  [dbo].[xspGetExpiredCachedResults] TO [legion_frontend]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering permissions on [dbo].[xspGetHostLoad]'
GO
GRANT EXECUTE ON  [dbo].[xspGetHostLoad] TO [legion_stats]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering permissions on [dbo].[xspGetKeyRevocationList]'
GO
GRANT EXECUTE ON  [dbo].[xspGetKeyRevocationList] TO [legion_frontend]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering permissions on [dbo].[xspGetMethods]'
GO
GRANT EXECUTE ON  [dbo].[xspGetMethods] TO [legion_frontend]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering permissions on [dbo].[xspGetRateLimitTypes]'
GO
GRANT EXECUTE ON  [dbo].[xspGetRateLimitTypes] TO [legion_admin]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering permissions on [dbo].[xspGetServerStatus]'
GO
GRANT EXECUTE ON  [dbo].[xspGetServerStatus] TO [legion_admin]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering permissions on [dbo].[xspGetServiceSettings]'
GO
GRANT EXECUTE ON  [dbo].[xspGetServiceSettings] TO [legion_frontend]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering permissions on [dbo].[xspGetService]'
GO
GRANT EXECUTE ON  [dbo].[xspGetService] TO [legion_admin]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering permissions on [dbo].[xspGetServices]'
GO
GRANT EXECUTE ON  [dbo].[xspGetServices] TO [legion_frontend]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering permissions on [dbo].[xspGetSettingById]'
GO
GRANT EXECUTE ON  [dbo].[xspGetSettingById] TO [legion_frontend]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering permissions on [dbo].[xspGetSettingByKey]'
GO
GRANT EXECUTE ON  [dbo].[xspGetSettingByKey] TO [legion_frontend]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering permissions on [dbo].[xspInsertCachedResult]'
GO
GRANT EXECUTE ON  [dbo].[xspInsertCachedResult] TO [legion_frontend]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering permissions on [dbo].[xspLogMethodCall]'
GO
GRANT EXECUTE ON  [dbo].[xspLogMethodCall] TO [legion_frontend]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering permissions on [dbo].[xspLogReplyException]'
GO
GRANT EXECUTE ON  [dbo].[xspLogReplyException] TO [legion_frontend]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering permissions on [dbo].[xspRevokeKey]'
GO
GRANT EXECUTE ON  [dbo].[xspRevokeKey] TO [legion_admin]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering permissions on [dbo].[xspSetAssemblyRefreshFlags]'
GO
GRANT EXECUTE ON  [dbo].[xspSetAssemblyRefreshFlags] TO [legion_watcher]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering permissions on [dbo].[xspSetSettingById]'
GO
GRANT EXECUTE ON  [dbo].[xspSetSettingById] TO [legion_frontend]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering permissions on [dbo].[xspSetSettingByKey]'
GO
GRANT EXECUTE ON  [dbo].[xspSetSettingByKey] TO [legion_frontend]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Altering permissions on [dbo].[xspUpdateMethodMissingFlag]'
GO
GRANT EXECUTE ON  [dbo].[xspUpdateMethodMissingFlag] TO [legion_frontend]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
COMMIT TRANSACTION
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
DECLARE @Success AS BIT
SET @Success = 1
SET NOEXEC OFF
IF (@Success = 1) PRINT 'The database update succeeded'
ELSE BEGIN
	IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION
	PRINT 'The database update failed'
END
GO