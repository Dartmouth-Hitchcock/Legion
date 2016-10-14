/**
 *	Copyright 2016 Dartmouth-Hitchcock
 *	
 *	Licensed under the Apache License, Version 2.0 (the "License");
 *	you may not use this file except in compliance with the License.
 *	You may obtain a copy of the License at
 *	
 *	    http://www.apache.org/licenses/LICENSE-2.0
 *	
 *	Unless required by applicable law or agreed to in writing, software
 *	distributed under the License is distributed on an "AS IS" BASIS,
 *	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *	See the License for the specific language governing permissions and
 *	limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Xml;

using Legion.Core.Caching;
using Legion.Core.DataStructures;
using Legion.Core.Exceptions;
using Legion.Core.Modules;
using Legion.Core.Services;

namespace Legion.Core {

    /// <summary>
    /// Manages requests to the Legion API
    /// </summary>
    public static class Manager {

        private static Dictionary<string, Method> _systemMethods = new Dictionary<string, Method>() {
            { "__status", null }
        };

        /// <summary>
        /// Start manager system tasks
        /// </summary>
        public static void SpinUp() {
            LoggingBuffer.Start();
        }

        /// <summary>
        /// Stop manager system tasks
        /// </summary>
        public static void SpinDown() {
            LoggingBuffer.Stop();
        }

        /// <summary>
        /// Processes the Legion request utilizing the Service cache
        /// </summary>
        /// <param name="page">The current Page object</param>
        public static void Process(Page page){
            if (page.Cache[Cache.CACHE_KEYS.Services] != null) {
                Dictionary<string, Service> services = (Dictionary<string, Service>)HttpRuntime.Cache[Cache.CACHE_KEYS.Services];
                Reply reply = GetReply(new Request(page), services);

                page.Response.Clear();
                page.Response.AppendHeader("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1.
                page.Response.AppendHeader("Pragma", "no-cache"); // HTTP 1.0.
                page.Response.AppendHeader("Expires", "0"); // Proxies.
                page.Response.ContentType = reply.ContentType;
                page.Response.Write(reply);
            }
            else {
                Logging.Module.WriteException(new LoggedException() {
                    Type = "CacheNotFound",
                    Message = Settings.GetString("ExceptionMessageCacheNotFound")
                });
            }
        }

        /// <summary>
        /// Processes a pre-generated Legion Request
        /// </summary>
        /// <param name="request">The LegionRequest object to process</param>
        /// <returns>The LegionReply from the service</returns>
        internal static Reply Process(Request request) {
            System.Web.Caching.Cache cache = HttpRuntime.Cache;
            if (cache[Cache.CACHE_KEYS.Services] != null) {
                Reply reply = GetReply(request, (Dictionary<string, Service>)cache[Cache.CACHE_KEYS.Services]);
                return reply;
            }
            else {
                Logging.Module.WriteException(new LoggedException() {
                    Type = "CacheNotFound",
                    Message = "Cache not found"
                });

                return null;
            }
        }

        /// <summary>
        /// Gets the result from the requested handler
        /// </summary>
        /// <param name="request">The LegionRequest object to process</param>
        /// <param name="services">Dictionary of the available Services</param>
        /// <returns>A string representation of the XML result</returns>
        private static Reply GetReply(Request request, Dictionary<string, Service> services) {
            Cache.Refresh();
            ResultCache.TryExpire();

            Reply reply = new Reply(request);

            Service service = null; Method method = null;
            DateTime dtprocessed = DateTime.Now;
            TimeSpan executionTime = TimeSpan.Zero;

            try {
                if (!request.RateLimit.IsExceeded) {
                    request.RateLimit.BitchSlap();

                    //identify service.method call
                    if (request.APIKey.Key != null) {
                        if (!request.APIKey.IsRevoked) {
                            if (request.APIKey.IsValid) {
                                if (request.IsServiceToService || request.Application.HasValidSourceIP(request.Requestor.HostIPAddress)) {
                                    if (request.ServiceKey != null) {
                                        if (services.ContainsKey(request.ServiceKey)) {
                                            service = services[request.ServiceKey];
                                            if (request.IsServiceToService || service.HasValidSourceIP(request.Requestor.HostIPAddress)) {
                                                if (request.MethodKey != null) {
                                                    if (service.Open != null && service.Open.ContainsKey(request.MethodKey))
                                                        method = service.Open[request.MethodKey];
                                                    else if (service.Restricted != null && service.Restricted.ContainsKey(request.MethodKey)) {
                                                        if (request.IsServiceToService || request.Application.HasPermissionTo(request.ServiceKey, request.MethodKey))
                                                            method = service.Restricted[request.MethodKey];
                                                        else //application does not have access to this method
                                                            LogFault(request, reply, "InsufficientPermissions", Settings.GetString("FaultMessageInsufficientPermissions", new Dictionary<string, string>(){
                                                                {"ApplicationName", request.Application.Name},
                                                                {"ServiceKey", request.ServiceKey},
                                                                {"MethodKey", request.MethodKey}
                                                            }));
                                                    }
                                                    else if (service.Special.ContainsKey(request.MethodKey)) { //special method
                                                        method = service.Special[request.MethodKey];
                                                    }
                                                    else //method not found
                                                        LogFault(request, reply, "MethodNotFound", Settings.GetString("FaultMessageMethodNotFound", new Dictionary<string, string>(){
                                                            {"ServiceKey", request.ServiceKey},
                                                            {"MethodKey", request.MethodKey}
                                                        }));
                                                }
                                                else //method not specified
                                                    LogFault(request, reply, "MethodNotSpecified", Settings.GetString("FaultMessageMethodNotSpecified", new Dictionary<string, string>(){
                                                        {"ServiceKey", request.ServiceKey}
                                                    }));
                                            }
                                            else //invalid service source ip
                                                LogFault(request, reply, "ServiceSourceIpInvalid", Settings.GetString("FaultMessageServiceSourceIpInvalid", new Dictionary<string, string>(){
                                                    {"HostIpAddress", request.Requestor.HostIPAddress},
                                                    {"ServiceKey", request.ServiceKey}
                                                }));
                                        }
                                        else //service not found
                                            LogFault(request, reply, "ServiceNotFound", Settings.GetString("FaultMessageServiceNotFound", new Dictionary<string, string>(){
                                                {"ServiceKey", request.ServiceKey}
                                            }));
                                    }
                                    else //no service specified
                                        LogFault(request, reply, "ServiceNotSpecified", Settings.GetString("FaultMessageServiceNotSpecified", new Dictionary<string, string>() {
                                        }));
                                }
                                else //invalid application source ip
                                    LogFault(request, reply, "ApplicationSourceIpInvalid", Settings.GetString("FaultMessageApplicationSourceIpInvalid", new Dictionary<string, string>(){
                                        {"HostIpAddress", request.Requestor.HostIPAddress},
                                        {"ApiKey", request.APIKey.Key}
                                    }));
                            }
                            else //api key not found
                                LogFault(request, reply, "ApiKeyInvalid", Settings.GetString("FaultMessageApiKeyInvalid", new Dictionary<string, string>(){
                                    {"ApiKey", request.APIKey.Key}
                                }));
                        }
                        else //api key revoked
                            LogFault(request, reply, "ApiKeyRevoked", Settings.GetString("FaultMessageApiKeyRevoked", new Dictionary<string, string>(){
                                {"ApiKey", request.APIKey.Key}
                            }));
                    }
                    else //no api key specified
                        LogFault(request, reply, "ApiKeyNotSpecified", Settings.GetString("FaultMessageApiKeyNotSpecified", new Dictionary<string, string>() {
                        }));
                }
                else //rate limit exceeded
                    LogFault(request, reply, "RateLimitExceeded", Settings.GetString("FaultMessageRateLimitExceeded", new Dictionary<string, string>(){
                        {"RateLimit", request.RateLimit.ToString()}
                    }));

                //restrict public proxied requests
                if (method != null && request.IsProxied) {
                    if (!request.Application.IsPublic) {
                        method = null;
                        LogFault(request, reply, "ApplicationNotPublic", Settings.GetString("FaultMessageApplicationNotPublic", new Dictionary<string, string>(){
                            {"ApiKey", request.APIKey.Key}
                        }));
                    }
                    else if (!(service.IsPublic || method.IsPublic)) {
                        method = null;
                        LogFault(request, reply, "MethodNotPublic", Settings.GetString("FaultMessageMethodNotPublic", new Dictionary<string, string>(){
                            {"MethodKey", request.MethodKey}
                        }));
                    }
                }

                if (method != null) {
                    if (request.Requestor.Account == null) { //check for auth token if required
                        if (method.IsAuthenticatedUserRequired) {
                            method = null;
                            LogFault(request, reply, "AuthenticatedUserRequiredForMethod", Settings.GetString("FaultMessageMethodAuthenticatedUserRequired", new Dictionary<string, string>(){
                                {"MethodKey", request.MethodKey}
                            }));
                        }
                        else if (service.IsAuthenticatedUserRequired) {
                            method = null;
                            LogFault(request, reply, "AuthenticatedUserRequiredForService", Settings.GetString("FaultMessageServiceAuthenticatedUserRequired", new Dictionary<string, string>(){
                                {"ServiceKey", request.ServiceKey}
                            }));
                        }
                    }
                    else { //check for authorization if required
                        if (method.IsAuthorizedUserRequired) {
                            bool hasPermission = Permissions.Module.Check(
                                request.Requestor.Account.AccountType,
                                request.Requestor.Account.IdentifierType,
                                request.Requestor.Account.Identifier,
                                string.Format("Method/{0}.{1}", request.ServiceKey, request.MethodKey)
                            );

                            if (!hasPermission) {
                                method = null;
                                LogFault(request, reply, "AuthorizedUserRequiredForMethod", Settings.GetString("FaultMessageMethodAuthorizedUserRequired", new Dictionary<string, string>(){
                                    {"MethodKey", request.MethodKey}
                                }));
                            }
                        }
                        else if (service.IsAuthorizedUserRequired) {
                            bool hasPermission = Permissions.Module.Check(
                                request.Requestor.Account.AccountType,
                                request.Requestor.Account.IdentifierType,
                                request.Requestor.Account.Identifier,
                                string.Format("Service/{0}", request.ServiceKey, request.MethodKey)
                            );

                            if (!hasPermission) {
                                method = null;
                                LogFault(request, reply, "AuthorizedUserRequiredForService", Settings.GetString("FaultMessageServiceAuthorizedUserRequired", new Dictionary<string, string>(){
                                    {"ServiceKey", request.ServiceKey}
                                }));
                            }
                        }
                    }
                }

                //service and method were found and are valid, process request
                if (method != null) {
                    //is permanent logging required
                    if (request.Application.IsLogged || service.IsLogged || method.IsLogged) {
                        if (request.Requestor.ClientIPAddress != null)
                            executionTime = InvokeMethod(method, request, reply, dtprocessed, true);
                        else
                            LogFault(request, reply, "UserIpRequired", Settings.GetString("FaultMessageUserIpRequired", new Dictionary<string, string>(){
                                {"ServiceName", service.Name},
                                {"MethodName", method.Name}
                            }));
                    }
                    else
                        executionTime = InvokeMethod(method, request, reply, dtprocessed, false);
                }

                //add application details
                if (request.Application != null && !request.IsProxied) {
                    XmlElement app = reply.Response.AddElement(Settings.GetString("NodeNameApplication"));
                    reply.Response.AddElement(app, Settings.GetString("NodeNameApplicationApiKey"), (request.APIKey.Key == null ? Settings.GetString("SymbolNotSpecified") : request.APIKey.Key));
                    reply.Response.AddElement(app, Settings.GetString("NodeNameApplicationName"), (request.Application == null ? string.Empty : request.Application.Name));
                }

                //add service details
                if (service != null) {
                    XmlElement xService;
                    xService = reply.Response.AddElement(Settings.GetString("NodeNameService"), request.ServiceKey);
                    xService.SetAttribute(Settings.GetString("NodeAttributeServiceCompiledOn"), service.CompiledOn);
                    xService.SetAttribute(Settings.GetString("NodeAttributeServiceVersion"), service.Version);

                    //add method details
                    if (request.MethodKey != null)
                        reply.Response.AddElement(Settings.GetString("NodeNameMethod"), request.MethodKey);
                }

                //add host details
                reply.Response.AddElement(Settings.GetString("NodeNameLegionHost"), string.Format("{0}:{1}", System.Environment.MachineName, System.Diagnostics.Process.GetCurrentProcess().Id));
                reply.Response.AddElement(Settings.GetString("NodeNameProcessedOn"), Settings.FormatDateTime(dtprocessed));

                //add time diagnostics
                reply.Response.AddElement(Settings.GetString("NodeNameElapsedTime"), string.Format("{0} ms", (executionTime.TotalMilliseconds == 0 ? "less than 1" : string.Empty + executionTime.TotalMilliseconds)));
            }
            catch (HandlingError e) {
                LogFault(request, reply, e.GetType().ToString(), e.Message);
            }
            finally {
                //mark the request as complete and clean up the request stack
                request.Complete();
            }

            return reply;
        }

        /// <summary>
        /// Invokes a Method  
        /// </summary>
        /// <param name="method">the method to invoke</param>
        /// <param name="request">the Request object to pass to the Method</param>
        /// <param name="reply">the Reply object to be used by the Method</param>
        /// <param name="dtProcessingStart">the time the Legion request was recieved</param>
        /// <param name="permLog">Log this call to the permenant log</param>
        /// <returns>the time taken to invoke the method</returns>
        private static TimeSpan InvokeMethod(Method method, Request request, Reply reply, DateTime dtProcessingStart, bool permLog) {
            DateTime dtExecutionStart = DateTime.MinValue, dtExecutionEnd = DateTime.MinValue;

            try {
                request.Method = method;

                dtExecutionStart = DateTime.Now;
                method.Invoke(request, reply); //call it
                dtExecutionEnd = DateTime.Now;
            }
            catch (Exception e) {
                dtExecutionEnd = DateTime.Now; //this line is duplicated here to ensure that the excecution end time is set if there is a fault

                if (e is TargetInvocationException && e.InnerException is ServiceError)
                    reply.Error.Friendly = e.InnerException.Message;
                else
                    LogFault(request, reply, method, "InvocationException", e);
            }
            finally {
                method.LogCall(dtProcessingStart, dtExecutionStart, dtExecutionEnd, request.Application, request.ServiceKey, request.ParameterSet, request.Requestor.HostIPAddress, request.Requestor.ClientIPAddress, permLog);
            }

            return (dtExecutionStart == DateTime.MinValue ? TimeSpan.Zero : dtExecutionEnd - dtExecutionStart);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="reply"></param>
        /// <param name="method"></param>
        /// <param name="type"></param>
        /// <param name="e"></param>
        internal static void LogFault(Request request, Reply reply, Method method, string type, Exception e) {
            if (e is TargetInvocationException)
                e = e.InnerException;

            int eventid = Logging.Module.WriteException(new LoggedException() {
                Request = request,
                Exception = e
            });

            reply.Result.Clear();

            XmlElement exception = reply.Result.AddElement(Settings.GetString("NodeNameException"));
            exception.SetAttribute(Settings.GetString("NodeAttributeExceptionId"), eventid.ToString());

            reply.Result.AddElement(exception, Settings.GetString("NodeNameExceptionName"), e.GetType().Name);
            reply.Result.AddElement(exception, Settings.GetString("NodeNameExceptionMessage"), e.Message);
            reply.Result.AddElement(exception, Settings.GetString("NodeNameExceptionStacktrace"), e.StackTrace, true);

            if (method != null && method.IsLogReplayDetailsOnException)
                ReplayLog.LogException(eventid, method.Id, request.ParameterSet.ToXml(), e.GetType().Name, e.Message, e.StackTrace);

            LogFault(
                request, 
                reply, 
                type, 
                Settings.GetString("LogFormatExceptionFriendly", new Dictionary<string, string>(){
                    {"EventId", eventid.ToString()},
                    {"MethodKey", request.MethodKey},
                    {"MethodName", method.Name}
                }),
                Settings.GetString("LogFormatExceptionDetailed", new Dictionary<string, string>(){
                    {"EventId", eventid.ToString()},
                    {"MethodKey", request.MethodKey},
                    {"MethodName", method.Name}
                })
            );
        }

        /// <summary>
        /// Logs an fault to Lumberjack and to the client
        /// </summary>
        /// <param name="request">The original Request object</param>
        /// <param name="reply">The Reply object to be sent to the client</param>
        /// <param name="type">The type of fault</param>
        /// <param name="fault">The message to send to the client</param>
        internal static void LogFault(Request request, Reply reply, string type, string fault) {
            LogFault(request, reply, type, fault, fault);
        }

        /// <summary>
        /// Logs an fault to Lumberjack and to the client
        /// </summary>
        /// <param name="request">The original Request object</param>
        /// <param name="reply">The Reply object to be sent to the client</param>
        /// <param name="type">The type of fault</param>
        /// <param name="faultClient">The fault to send to the client</param>
        /// <param name="faultLog">The detailed fault to log to file</param>
        private static void LogFault(Request request, Reply reply, string type, string faultClient, string faultLog) {
            XmlElement xFault = reply.Response.AddElement(Settings.GetString("NodeNameFault"), faultClient);
            xFault.SetAttribute("type", type);

            string apikey = (request.APIKey == null ? Settings.GetString("SymbolNotApplicable") : request.APIKey.Key);
            string applicationid = (request.Application == null ? Settings.GetString("SymbolNotApplicable") : request.Application.Id.ToString());
            string applicationname = (request.Application == null ? Settings.GetString("SymbolNotApplicable") : request.Application.Name);

            string details = Settings.GetString("LogFormatFault", new Dictionary<string, string>(){
                {"ApiKey", apikey},
                {"ApplicationId", applicationid},
                {"ApplicationName", applicationname},
                {"ServiceKey", request.ServiceKey},
                {"MethodKey", request.MethodKey},
                {"Fault", faultLog}
            });

            Logging.Module.WriteEvent(new LoggedEvent(EventLevel.Fatal, type, details) {
                ClientIp = request.Requestor.ClientIPAddress,
                HostIp = request.Requestor.HostIPAddress
            });
        }
    }
}
