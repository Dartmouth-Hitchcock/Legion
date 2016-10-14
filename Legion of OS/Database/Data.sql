		
SET NUMERIC_ROUNDABORT OFF
GO
SET ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT, QUOTED_IDENTIFIER, ANSI_NULLS, NOCOUNT ON
GO
SET DATEFORMAT YMD
GO
SET XACT_ABORT ON
GO
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
GO
BEGIN TRANSACTION
-- Pointer used for text / image updates. This might not be needed, but is declared here just in case
DECLARE @pv binary(16)

PRINT(N'Add 128 rows to [dbo].[tblSettings]')
SET IDENTITY_INSERT [dbo].[tblSettings] ON
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (1, 'AssemblyDirectory', 'c:\inetpub\Legion\assemblies', 'the service assemblies directory')
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (3, 'AesKey', 'xxx', 'the aes encryption key for service settings')
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (4, 'AesVector', 'xxx', 'the aes encryption IV for service settings')
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (5, 'RateLimitDefault', '1000', 'number of calls per rate limit to allow')
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (6, 'RateLimitIntervalDefault', '1', 'number of seconds to rate')
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (7, 'CacheRefreshCheckInterval', '60', 'number of seconds in between polls to the database to check cache refresh flags')
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (8, 'Environment', 'Development', 'the current environment')
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (10, 'ResultCachingConcurrentCallSleepInterval', '10', 'number of milliseconds to wait for a caching result to load befor checking again')
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (11, 'CacheLoadingSleepInterval', '500', 'number of milliseconds to wait for the cache to load befor checking again')
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (12, 'ResultCachingDurationPadding', '30', 'Number of seconds to pad the cache time to allow it to be recached in the backend')
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (13, 'ServiceStorageObjectExpiration', '3600', 'Number of seconds to cache service storage objects')
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (16, 'VolatileStorageObjectMaxExpiration', '86400', 'Maximum number of seconds to cache service storage objects')
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (18, 'VolatileStorageObjectDefaultExpiration', '1200', 'The sliding default expiration of volatile objects')
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (19, 'AuthTokenLifetime', '1200', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (20, 'AuthTokenType', 'LegionAuth', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (21, 'NodeNameReply', 'reply', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (22, 'NodeAttributeErrorDescriptionTypeFriendly', 'friendly', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (23, 'NodeAttributeErrorDescriptionTypeDetailed', 'detailed', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (24, 'NodeNameErrorDescription', 'description', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (25, 'NodeNameResult', 'result', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (26, 'NodeNameResponse', 'response', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (27, 'NodeNameError', 'error', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (28, 'ParameterKeyRequestorUserIp', '__legion_userip', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (29, 'ParameterKeyRequestorImpersonateIp', '__legion_impersonateip', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (31, 'ParameterKeyRequestIsProxied', '__legion_isproxied', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (34, 'ParameterKeyRequestId', '__request_id', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (35, 'ParameterKeySetRequestService', 's;__s;__service', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (36, 'ParameterKeySetRequestMethod', 'm;__m;__method', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (37, 'ParameterKeySetRequestApplication', 'k;__k;__key;__apikey;__application;__applicationkey', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (38, 'ParameterKeySetRequestFormat', 'f;__f;__format', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (39, 'ParameterSetLoggingHiddenParameters', 'password', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (40, 'ParameterKeyRequestorUserCall', '__legion_usercall', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (41, 'ParameterKeyRequestorAuthToken', '__legion_authtoken', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (43, 'FaultMessageInsufficientPermissions', 'Application ''{ApplicationName}'' does not have permission to access Method ''{MethodKey}'' of Service ''{ServiceKey}''', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (45, 'FaultMessageMethodNotFound', 'Method ''{MethodKey}'' not found in Service ''{ServiceKey}''', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (47, 'FaultMessageMethodNotSpecified', 'Method not specified for Service ''{ServiceKey}''', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (50, 'FaultMessageServiceSourceIpInvalid', 'IP ''{HostIpAddress}'' is not a valid source IP for the specified Service ''{ServiceKey}''', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (52, 'FaultMessageServiceNotFound', 'Service ''{ServiceKey}'' not found in Legion', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (55, 'FaultMessageServiceNotSpecified', 'Service not specified', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (57, 'FaultMessageApplicationSourceIpInvalid', 'IP ''{HostIpAddress}'' is not a valid source IP for the specified API Key ''{ApiKey}''', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (59, 'FaultMessageApiKeyInvalid', 'API Key ''{ApiKey}'' not found in Legion', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (61, 'FaultMessageApiKeyRevoked', 'API Key ''{ApiKey}'' has been revoked', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (63, 'FaultMessageApiKeyNotSpecified', 'API Key not specified', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (66, 'FaultMessageRateLimitExceeded', 'Rate limit exceeded ({RateLimit})', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (68, 'FaultMessageApplicationNotPublic', 'API Key ''{ApiKey}'' is not publicly accessible', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (70, 'FaultMessageMethodNotPublic', 'Method ''{MethodKey}'' is not publicly accessible', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (72, 'FaultMessageMethodAuthenticatedUserRequired', 'Method ''{MethodKey}'' requires a Heimdall User authentication token', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (74, 'FaultMessageUserIpRequired', 'IP address not passed for logged call ''{ServiceName}.{MethodName}()''', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (76, 'FaultMessageServiceAuthenticatedUserRequired', 'Service ''{ServiceKey}'' requires a Heimdall User authentication token', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (77, 'LogFormatFault', '<details><request><apikey>{ApiKey}</apikey><application id = "{ApplicationId}">{ApplicationName}</application><servicekey>{ServiceKey}</servicekey><methodkey>{MethodKey}</methodkey></request><fault>{Fault}</fault></details>', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (78, 'LogFormatExceptionDetailed', '<exception eventid = "{EventId}">Method ''{MethodName}'' threw an unhandled exception.</exception>', 'EventId,MethodKey,MethodName')
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (79, 'ExceptionMessageServiceSettingNotFound', 'The given setting ''{SettingKey}'' was not present in the Service''s setting set.', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (80, 'DateTimeFormat', 'yyyy-MM-dd HH:mm:ss ''GMT''zz', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (81, 'ExceptionMessageVolatileStorageInvalidExpiration', 'The expiration time of the stored value may not be later than {MaxExpiration}.', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (82, 'ToStringFormatRateLimit', 'Limited: {Limit} request{LimitPlural} per {Interval}{IntervalUnit}{LimitType}', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (83, 'ToStringFormatRequest', 'Service Key: {ServiceKey}
Method Key: {MethodKey}
Format Key: {FormatKey}

Application ID: {ApplicationId}
Application Name: {ApplicationName}
API Key: {ApiKey}

Is Service to Service: {IsServiceToService}
Client IP Address: {ClientIpAddress}
Host IP Address: {HostIpAddress}
Is Host Internal: {IsHostInternal}

Parameters: {Parameters}', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (84, 'LogFormatMethodCall', '<application id = "{ApplicationId}">{ApplicationName}</application><method id = "{MethodId}">{ServiceKey}.{MethodKey}</method>{Parameters}', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (86, 'ExceptionMessageApplicationInvalidIpRange', 'Invalid ConsumerIPRange ''{IpRange}'' specified for application ''{Application}'' with API Key ''{ApiKey}''.', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (87, 'SymbolUnknown', 'unknown', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (88, 'SymbolNotApplicable', 'n/a', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (89, 'SymbolNotSpecified', 'not specified', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (90, 'NodeNameService', 'service', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (91, 'NodeNameApplication', 'application', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (92, 'NodeNameMethod', 'method', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (93, 'NodeNameLegionHost', 'host', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (94, 'NodeNameProcessedOn', 'processedon', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (95, 'NodeNameElapsedTime', 'elapsedtime', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (96, 'ToStringFormatIntegerNames', 'Zero;One;Two;Three;Four;Five;Six;Seven;Eight;Nine', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (97, 'NodeNameCachedResult', 'cachedresult', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (98, 'ToStringFormatPVGZero', 'No parameters are required.', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (99, 'ToStringFormatPVGOne', 'The {ParameterType}parameter {ParameterName} is required{RequiredType}.', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (100, 'ToStringFormatPVGMany', '{MinimumParameterCount} of the following {ParameterType}parameter{TotalParametersPlural} {MinimumParametersPlural} required{RequiredType}: {Parameters}.', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (102, 'ToStringFormatPVGMultipleTypes', '{MinimumParameterCount} of the following parameter{TotalParametersPlural} {MinimumParametersPlural} required{RequiredType}: {Parameters}', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (103, 'ExceptionMessageCacheNotFound', 'Cache not found.', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (104, 'NodeAttributeServiceCompiledOn', 'compiledon', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (105, 'NodeAttributeServiceVersion', 'version', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (106, 'NodeNameApplicationApiKey', 'apikey', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (107, 'NodeNameApplicationName', 'name', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (108, 'NodeAttributeCachedResultUpdatedOn', 'updatedon', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (109, 'NodeAttributeCachedResultExpiresOn', 'expireson', '')
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (110, 'NodeNameFault', 'fault', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (111, 'LogFormatCacheLoadStart', 'Legion cache load started.', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (112, 'LogFormatCacheServiceLoadStart', 'Loading services to cache.', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (113, 'ExceptionMessageCacheClassNotFound', 'Class ''{ClassName}'' not found in Assembly ''{AssemblyName}''', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (115, 'ExceptionMessageCacheAssemblyNotFound', 'Assembly ''{AssemblyName}'' not found', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (116, 'LogFormatCacheServiceLoadFinish', 'Finished loading services to cache.', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (117, 'LogFormatCacheMethodLoadStart', 'Loading methods to cache.', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (118, 'ExceptionMessageCacheServiceKeyNotFound', 'ServiceKey ''{ServiceKey}'' for method ''{MethodKey}'' ({MethodId}) not found in cache', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (119, 'ExceptionMessageCacheMethodNotCacheable', 'Method ''{ServiceKey}.{MethodKey} ({MethodId})'' is flagged for caching but is marked as {AttributeName} and is not cacheable.', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (120, 'ExceptionMessageCacheMethodNotFound', 'Method ''{MethodName} ({MethodId})'' not found in class specified by ServiceKey ''{ServiceKey}''', 'MethodId, MethodKey, MethodName, ServiceKey')
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (121, 'LogFormatCacheMethodLoadFinish', 'Finished loading methods to cache.', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (122, 'LogFormatCacheRefresh', 'Legion cache was refreshed.', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (123, 'LogFormatCacheLoadWait', 'Cache not loaded, waiting {Interval}ms...', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (124, 'LogFormatCacheUnload', 'Legion cache was unloaded.', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (125, 'LogFormatCacheLoadFinish', 'Legion cache load finished.', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (126, 'LogFormatCacheKeyRevocationListLoadStart', 'Loading key revocation list to cache.', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (127, 'LogFormatCacheKeyRevocationListLoadFinish', 'Finished loading key revocation list to cache.', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (128, 'LogFormatCacheServiceStatusListLoadStart', 'Loading service status method list to cache.', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (129, 'LogFormatCacheServiceStatusListLoadFinish', 'Finished loading service status method list to cache.', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (130, 'ExceptionMessageCacheInvalidServiceIpRange', 'Invalid ConsumerIPRange ''{IpRange}'' specified for service ''{ServiceKey}''', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (131, 'ExceptionMessageCacheInvalidAssemblyDirectory', 'Assembly directory does not exist', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (132, 'ToStringCacheNotLoaded', '
	Assembly Cache not loaded', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (133, 'ToStringCache', 'Host: {Host}
Assembly Directory: {AssemblyDirectory}
Assembly Cache Last Updated: {CacheLastUpdatedOn}

Assembly Cache Contents:{CacheContents}', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (134, 'NodeNameException', 'exception', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (135, 'NodeAttributeExceptionId', 'eventid', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (136, 'NodeNameExceptionName', 'name', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (137, 'NodeNameExceptionMessage', 'message', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (138, 'NodeNameExceptionStacktrace', 'stacktrace', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (139, 'LogFormatExceptionFriendly', 'Method ''{MethodName}'' threw an unhandled exception.', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (141, 'ExceptionMessageCacheInvalidServiceKey', 'ServiceKey ''{ServiceKey}'' is invalid.', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (142, 'SystemMethods', '', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (143, 'SystemServiceKey', '__system', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (144, 'ModuleRootPath', 'c:\inetpub\Legion\modules', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (145, 'ModuleNameLogging', 'EventLogLoggingModule.dll', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (146, 'ModuleNameAuthentication', 'AuthenticationModule.dll', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (148, 'ExceptionMessageAsyncReferenceRequestCurrent', 'Asynchronous reference to Request.Current.', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (149, 'FaultMessageMethodAuthorizedUserRequired', '{MethodKey} requires that method permissions be granted to user in order to call.', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (150, 'FaultMessageServiceAuthorizedUserRequired', '{ServiceKey} requires that service permissions be granted to user in order to call.', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (151, 'CredentialsEncryptionKey', 'xxx', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (152, 'EncryptionKey', 'xxx', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (153, 'SettingsCredentialInsertionRegex', '(?<Credential>{credential:(?<Name>.*?)})', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (154, 'ModuleNameCredentials', 'CredentialsModule.dll', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (155, 'ModuleNameEncryption', 'AESEncryptionModule.dll', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (156, 'ModuleNamePermissions', 'PermissionsModule.dll', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (157, 'ModuleNameEmail', 'EmailModule.dll', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (158, 'BufferFlushInterval', '1000', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (159, 'CallLogBufferEnabled', 'false', NULL)
INSERT INTO [dbo].[tblSettings] ([id], [key], [value], [description]) VALUES (160, 'ModuleNameClientDetails', 'ClientDetailsModule.dll', NULL)
SET IDENTITY_INSERT [dbo].[tblSettings] OFF
COMMIT TRANSACTION
GO