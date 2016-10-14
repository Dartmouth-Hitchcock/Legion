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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;

using Legion.Core.Caching;
using Legion.Core.Clients;
using Legion.Core.Databases;
using Legion.Core.DataStructures;
using Legion.Core.Extensions;
using Legion.Core.Modules;

namespace Legion.Core.Services {

    /// <summary>
    /// A Legion Method
    /// </summary>
    public class Method {
        private const string CACHING_LOCK_TYPE = "MethodResultCache";
        private const string CACHING_KEY_FORMAT = "{0}:{1}";

        private int _id, _serviceid;
        private string _name, _key, _servicekey;
        private bool _isLogged, _isPublic, _isResultCacheable, _isAuthenticatedUserRequired, _isAuthorizedUserRequired, _isLogReplayDetailsOnException;
        private int? _cachedResultLifetime;
        private MethodInfo _mi;
        
        private static ConcurrentQueue<LogMethodParams> _bufferedCallLogQueue = new ConcurrentQueue<LogMethodParams>();

        internal static Dictionary<string, string> SPECIAL_METHODS = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
            {"__status", "status"}
        };

        #region accessors

        /// <summary>
        /// The method's Legion ID
        /// </summary>
        internal int Id {
            get { return _id; }
        }

        /// <summary>
        /// The method's key
        /// </summary>
        internal string Key {
            get { return _key; }
        }

        /// <summary>
        /// The method's name
        /// </summary>
        internal string Name {
            get { return _name; }
        }

        /// <summary>
        /// The id of the service this method belongs to
        /// </summary>
        internal int ServiceId {
            get { return _serviceid; }
        }

        /// <summary>
        /// The key of the service this method belongs to
        /// </summary>
        internal string ServiceKey {
            get { return _servicekey; }
        }

        /// <summary>
        /// Is this method public
        /// </summary>
        internal bool IsPublic {
            get { return _isPublic; }
        }

        /// <summary>
        /// Are calls to this method logged
        /// </summary>
        internal bool IsLogged {
            get { return _isLogged; }
        }

        /// <summary>
        /// Is an authenticated user required to call this method
        /// </summary>
        internal bool IsAuthenticatedUserRequired {
            get { return _isAuthenticatedUserRequired; }
        }

        /// <summary>
        /// Is authorization required to call this method
        /// </summary>
        internal bool IsAuthorizedUserRequired {
            get { return _isAuthorizedUserRequired; }
        }

        /// <summary>
        /// Is the method result cacheable
        /// </summary>
        internal bool IsResultCacheable {
            get { return _isResultCacheable; }
        }

        /// <summary>
        /// Log the details of this method call for replay when an exception occurs
        /// </summary>
        internal bool IsLogReplayDetailsOnException {
            get { return true; }
        }

        /// <summary>
        /// The lifetime of a cached result
        /// </summary>
        internal int? CachedResultLifetime {
            get { return _cachedResultLifetime; }
        }

        /// <summary>
        /// The reflection info associated with the method
        /// </summary>
        internal MethodInfo Info {
            get { return _mi; }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The method's Legion ID</param>
        /// <param name="key">The method's key</param>
        /// <param name="name">The method's name</param>
        /// <param name="serviceid">The id of the Service this Mehtod belongs to</param>
        /// <param name="servicekey">The key of the Service this Mehtod belongs to</param>
        /// <param name="isLogged">Is this a logged method</param>
        /// <param name="isPublic">Is this method publicly available</param>
        /// <param name="isAuthenticatedUserRequired">is an authenticated user required to call this method</param>
        /// <param name="isAuthorizedUserRequired">is an authorized user required to call this method</param>
        /// <param name="isResultCacheable">is this result cachable</param>
        /// <param name="isLogReplayDetailsOnException">is the method replayable in the event of an exception</param>
        /// <param name="cachedResultLifetime">the lifetime this method's results are valid for</param>
        /// <param name="mi">The method's info</param>
        internal Method(int id, string key, string name, int serviceid, string servicekey, bool isPublic, bool isLogged, bool isAuthenticatedUserRequired, bool isAuthorizedUserRequired, bool isResultCacheable, bool isLogReplayDetailsOnException, int? cachedResultLifetime, MethodInfo mi) {
            _id = id;
            _key = key;
            _name = name;
            _serviceid = serviceid;
            _servicekey = servicekey;
            _isPublic = isPublic;
            _isLogged = isLogged;
            _isAuthenticatedUserRequired = isAuthenticatedUserRequired;
            _isAuthorizedUserRequired = isAuthorizedUserRequired;
            _isResultCacheable = isResultCacheable;
            _isLogReplayDetailsOnException = isLogReplayDetailsOnException;
            _cachedResultLifetime = cachedResultLifetime;
            _mi = mi;
        }

        /// <summary>
        /// Invokes the method
        /// </summary>
        /// <param name="request">The Request object to pass to the Method</param>
        /// <param name="reply">The Reply to the client</param>
        internal void Invoke(Request request, Reply reply) {
            bool invoke = true;
            Binary resultKey = null;
            string cachingKey = null;

            if (IsResultCacheable) {
                resultKey = request.ParameterSet.GetHash();
                cachingKey = string.Format(CACHING_KEY_FORMAT, _id, resultKey);

                CachedResult cachedResult = ResultCache.GetCachedResult(_id, resultKey);
                if (cachedResult.Found) {
                    if (cachedResult.IsExpired && !Monitor.IsEntered(DynamicLock.Get(CACHING_LOCK_TYPE, cachingKey))) {
                        request.MakeThreadSafe();

                        ParameterizedThreadStart work = new ParameterizedThreadStart(CacheMethodCall);
                        Thread thread = new Thread(work);

                        thread.Start(new CacheMethodCallParams() {
                            Method = this,
                            ResultKey = resultKey,
                            Request = request,
                            Reply = new Reply(request.FormatKey)
                        });
                    }

                    reply.Result.Raw.InnerXml = cachedResult.Result.GetInnerXml();

                    XmlElement xCached = reply.Response.AddElement(Settings.GetString("NodeNameCachedResult"));
                    xCached.SetAttribute(Settings.GetString("NodeAttributeCachedResultUpdatedOn"), Settings.FormatDateTime(cachedResult.UpdatedOn));
                    xCached.SetAttribute(Settings.GetString("NodeAttributeCachedResultExpiresOn"), Settings.FormatDateTime(cachedResult.ExpiresOn));

                    invoke = false;
                }
                else {
                    //if the current result is caching, sleep
                    while (Monitor.IsEntered(DynamicLock.Get(CACHING_LOCK_TYPE, cachingKey)))
                        Thread.Sleep(Settings.GetInt("ResultCachingConcurrentCallSleepInterval"));

                    cachedResult = ResultCache.GetCachedResult(_id, resultKey);
                    if (cachedResult.Found) {
                        reply.Result.Raw.InnerXml = cachedResult.Result.GetInnerXml();

                        XmlElement xCached = reply.Response.AddElement(Settings.GetString("NodeNameCachedResult"));
                        xCached.SetAttribute(Settings.GetString("NodeAttributeCachedResultUpdatedOn"), Settings.FormatDateTime(cachedResult.UpdatedOn));
                        xCached.SetAttribute(Settings.GetString("NodeAttributeCachedResultExpiresOn"), Settings.FormatDateTime(cachedResult.ExpiresOn));

                        invoke = false;
                    }
                }
            }

            if (invoke) {
                if (SPECIAL_METHODS.ContainsKey(_key))
                    InvokeSpecialMethod(reply.Result);
                else {
                    if (IsResultCacheable) {
                        request.MakeThreadSafe();

                        CacheMethodCall(new CacheMethodCallParams() {
                            Method = this,
                            ResultKey = resultKey,
                            CachingKey = cachingKey,
                            Request = request,
                            Reply = reply
                        });
                    }
                    else
                        _mi.Invoke(null, (new object[] { request, reply.Result, reply.Error }));
                }
            }
        }

        /// <summary>
        /// Logs the method call to database
        /// </summary>
        /// <param name="calledAt">The DateTime the Requeswas made</param>
        /// <param name="executeStart">The DateTime the Request was passed to the Method</param>
        /// <param name="executeEnd">The DateTime the Method finished processing and control was passed back to the framework</param>
        /// <param name="application">The application calling the method</param>
        /// <param name="serviceKey">The key of the service which was called</param>
        /// <param name="parameters">The parameters which were passed into the method</param>
        /// <param name="hostIPAddress">The IP address of the host calling the method</param>
        /// <param name="userIPAddress">The IP address of the user calling the method</param>
        /// <param name="permanentLog">Should a record of this call be made in the premenant log</param>
        internal void LogCall(DateTime calledAt, DateTime executeStart, DateTime executeEnd, Application application, string serviceKey, ParameterSet parameters, string hostIPAddress, string userIPAddress, bool permanentLog) {
            LogMethodParams call = new LogMethodParams() {
                MethodId = (_id == -1 ? null : (int?)_id),
                ExecutionDuration = (executeEnd - executeStart).TotalMilliseconds,
                CalledAt = calledAt,
                ApplicationId = application.Id,
                APIKey = application.APIKey,
                HandledByIpAddress = ServerDetails.IPv4Addresses.First().ToString(),
                HostIpAddress = hostIPAddress,
                UserIpAddress = userIPAddress,
                PermanentLog = permanentLog,
                ParameterSet = parameters
            };

            if (Settings.GetBool("CallLogBufferEnabled")) {
                _bufferedCallLogQueue.Enqueue(call);
            }
            else {
                ParameterizedThreadStart work = new ParameterizedThreadStart(LogMethodCallThread);
                Thread thread = new Thread(work);

                thread.Start(call);
            }

            if (permanentLog) {
                string entry = Settings.GetString("LogFormatMethodCall", new Dictionary<string, string>(){
                    {"ApplicationId", application.Id.ToString()},
                    {"ApplicationName", application.Name},
                    {"MethodId", _id.ToString()},
                    {"ServiceKey", serviceKey},
                    {"MethodKey", _key},
                    {"Parameters", parameters.ToXml().OuterXml}
                });

                Logging.Module.WriteEvent(new LoggedEvent(EventLevel.Event, string.Format("{0}.{1}", serviceKey, _key), entry) {
                    Async = true,
                    Group = "ServiceCallAudit",
                    ApplicationType = ApplicationType.Application,
                    ApplicationName = application.Name,
                    ClientIp = userIPAddress,
                    HostIp = hostIPAddress
                });
            }
        }

        internal static void FlushCallBuffer() {
            DataTable tblCallLogBuffer = GetCallLogBufferTable();

            LogMethodParams callLog;
            while (_bufferedCallLogQueue.TryDequeue(out callLog)) {
                DataRow row = tblCallLogBuffer.NewRow();

                row["MethodId"] = callLog.MethodId;
                row["ExecutionDuration"] = callLog.ExecutionDuration;
                row["CalledAt"] = callLog.CalledAt;
                row["ApplicationId"] = callLog.ApplicationId;
                row["HandledByIpAddress"] = callLog.HandledByIpAddress;
                row["HostIpAddress"] = callLog.HostIpAddress;
                row["UserIpAddress"] = callLog.UserIpAddress;

                tblCallLogBuffer.Rows.Add(row);
            }

            using (LegionLinqDataContext db = new LegionLinqDataContext(ConfigurationManager.ConnectionStrings["LegionConnectionString"].ToString()))
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(ConfigurationManager.ConnectionStrings["LegionConnectionString"].ToString())) {
                bulkCopy.DestinationTableName = "dbo.tblCallLogBuffer";

                try {
                    bulkCopy.WriteToServer(tblCallLogBuffer);
                }
                catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void InvokeSpecialMethod(object oDetailsNode) {
            ReplyNode detailsNode = (ReplyNode)oDetailsNode;
            bool r = (bool)_mi.Invoke(null, new object[1] { oDetailsNode });
            detailsNode.AddElement(_name, (r ? "true" : "false"));
        }

        private static void LogMethodCallThread(object oParameters){
            LogMethodParams parameters = (LogMethodParams)oParameters;

            try {
                using (LegionLinqDataContext legion = new LegionLinqDataContext(ConfigurationManager.ConnectionStrings["LegionConnectionString"].ToString())) {
                    legion.xspLogMethodCall(
                        parameters.MethodId,
                        parameters.ExecutionDuration,
                        parameters.CalledAt,
                        parameters.ApplicationId,
                        parameters.HandledByIpAddress,
                        parameters.HostIpAddress, 
                        parameters.UserIpAddress,
                        parameters.PermanentLog
                    );
                }
            }
            catch (SqlException) {
                //TODO: Log to flat file
            }
        }

        private static void CacheMethodCall(object oParameters) {
            CacheMethodCallParams parameters = (CacheMethodCallParams)oParameters;

            try {
                Monitor.Enter(DynamicLock.Get(CACHING_LOCK_TYPE, parameters.CachingKey));
                parameters.Method._mi.Invoke(null, (new object[] { parameters.Request, parameters.Reply.Result, parameters.Reply.Error }));

                if (!parameters.Reply.Error.Exists) {
                    ResultCache.CacheResult(
                        parameters.Method.Id,
                        parameters.ResultKey,
                        parameters.Request.ParameterSet.ToXml().ToXElement(),
                        parameters.Reply.Result.Raw.ToXElement()
                    );
                }
            }
            catch (Exception e) {
                Manager.LogFault(parameters.Request, parameters.Reply, parameters.Method, "AsynchronousInvocationException", e);
            }
            finally {
                lock (DynamicLock.Lock) {
                    Monitor.Exit(DynamicLock.Get(CACHING_LOCK_TYPE, parameters.CachingKey));
                    DynamicLock.Remove(CACHING_LOCK_TYPE, parameters.CachingKey);
                }
            }
        }

        private static DataTable GetCallLogBufferTable() {
            DataTable tblCallLogBuffer = new DataTable("tblCallLogBuffer");

            Dictionary<string, string> columns = new Dictionary<string, string>() {
                {"MethodId", "System.Int32" },
                {"ExecutionDuration", "System.Float" },
                {"CalledAt", "System.DateTime" },
                {"ApplicationId", "System.Int32" },
                {"HandledByIpAddress", "System.String" },
                {"HostIpAddress", "System.String" },
                {"ClientIpAddress", "System.String" }
            };

            foreach (KeyValuePair<string, string> column in columns) {
                tblCallLogBuffer.AddColumn(column.Key, column.Value);
            }

            return tblCallLogBuffer;
        }

        private struct LogMethodParams {
            public int? MethodId;
            public double ExecutionDuration;
            public DateTime CalledAt;
            public int ApplicationId;
            public string APIKey;
            public string HandledByIpAddress;
            public string HostIpAddress;
            public string UserIpAddress;
            public bool PermanentLog;
            public ParameterSet ParameterSet;
        }

        private struct CacheMethodCallParams {
            public Binary ResultKey;
            public string CachingKey;
            public Method Method;
            public Request Request;
            public Reply Reply;
        }
    }
}
