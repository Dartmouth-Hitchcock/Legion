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
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;

using Legion.Services;
using Mjolnir;
using System.Threading;

namespace Legion {
    internal static class XLumberjack {

        private struct EventParams {
            public string Level;
            public string Group;
            public ApplicationType ApplicationType;
            public string ApplicationName;
            public string LoggingUserType;
            public string LogginUserId;
            public string AffectedUserType;
            public string AffectedUserId;
            public string Type;
            public string Details;
            public string Clientip;
            public string Hostip;
        }

        #region WriteEvent

        public static int WriteEvent(EventLevel level, string type, string details) {
            string ip = ServerTools.IPv4Addresses.First().ToString();
            return WriteEvent(level, type, details, ip, ip);
        }

        public static int WriteEvent(EventLevel level, string type, string details, string clientip, string hostip) {
            return WriteEvent(level, "Legion", ApplicationType.Legion, "Core", type, details, clientip, hostip);
        }

        public static int WriteEvent(EventLevel level, string group, ApplicationType applicationType, string applicationName, string type, string details, string clientip, string hostip) {
            return WriteEvent(level, group, applicationType, applicationName, null, null, null, null, type, details, clientip, hostip);
        }

        public static int WriteEvent(EventLevel level, string group, ApplicationType applicationType, string applicationName, string loggingUserType, string logginUserId, string affectedUserType, string affectedUserId, string type, string details, string clientip, string hostip) {
            return WriteEvent(level.ToString(), group, applicationType, applicationName, loggingUserType, logginUserId, affectedUserType, affectedUserId, type, details, clientip, hostip);
        }

        public static int WriteEvent(string level, string group, ApplicationType applicationType, string applicationName, string loggingUserType, string logginUserId, string affectedUserType, string affectedUserId, string type, string details, string clientip, string hostip){
            int? eventid = null;
            string resultcode = null;

            if (clientip == null)
                clientip = ClientTools.IPAddress();
            if (hostip == null)
                hostip = ServerTools.IPv4Addresses.First().ToString();

            XElement xDetails = XElement.Parse(string.Format("<details>{0}</details>", details));
            using (LumberjackLinqDataContext db = new LumberjackLinqDataContext(ConfigurationManager.ConnectionStrings["LumberjackConnectionString"].ToString())) {
                db.xspWriteDirectEvent(
                    applicationType.ToString(), 
                    applicationName, 
                    1, 
                    loggingUserType, 
                    logginUserId, 
                    affectedUserType, 
                    affectedUserId, 
                    group, 
                    level, 
                    type, 
                    xDetails, 
                    clientip, 
                    hostip, 
                    ref eventid, 
                    ref resultcode
                );
            }

            return (int)eventid;
        }

        public static void WriteAsyncEvent(string level, string group, ApplicationType applicationType, string applicationName, string loggingUserType, string logginUserId, string affectedUserType, string affectedUserId, string type, string details, string clientip, string hostip) {
            ParameterizedThreadStart work = new ParameterizedThreadStart(WriteAsyncEvent);
            Thread thread = new Thread(work);

            thread.Start(new EventParams() {
                Level = level,
                Group = group,
                ApplicationType = applicationType,
                ApplicationName = applicationName,
                LoggingUserType = loggingUserType,
                LogginUserId = logginUserId,
                AffectedUserType = affectedUserType,
                AffectedUserId = affectedUserId,
                Type = type,
                Details = details,
                Clientip = clientip,
                Hostip = hostip
            });
        }

        private static void WriteAsyncEvent(object oParameters) {
            EventParams parameters = (EventParams)oParameters;

            int? eventid = null;
            string resultcode = null;

            if (parameters.Clientip == null)
                parameters.Clientip = ClientTools.IPAddress();
            if (parameters.Hostip == null)
                parameters.Hostip = ServerTools.IPv4Addresses.First().ToString();

            XElement xDetails = XElement.Parse(string.Format("<details>{0}</details>", parameters.Details));
            using (LumberjackLinqDataContext db = new LumberjackLinqDataContext(ConfigurationManager.ConnectionStrings["LumberjackConnectionString"].ToString())) {
                db.xspWriteDirectEvent(
                    parameters.ApplicationType.ToString(), 
                    parameters.ApplicationName, 
                    1, 
                    parameters.LoggingUserType, 
                    parameters.LogginUserId, 
                    parameters.AffectedUserType,
                    parameters.AffectedUserId,
                    parameters.Group,
                    parameters.Level,
                    parameters.Type, 
                    xDetails,
                    parameters.Clientip,
                    parameters.Hostip, 
                    ref eventid, 
                    ref resultcode
                );
            }
        }

        #endregion

        #region WriteException

        public static int WriteException(Request request, Exception e) {
            return WriteException(request, e.GetType().ToString(), e.Message, e.StackTrace);
        }

        public static int WriteException(Request request, string type, string message, string stacktrace) {
            int? eventid = null;
            string resultcode = null;

            Service service = ((Dictionary<string, Service>)HttpRuntime.Cache[Cache.CACHE_KEYS.Services])[request.ServiceKey];
            string details = request.ToXml();
            
            using (LumberjackLinqDataContext db = new LumberjackLinqDataContext(ConfigurationManager.ConnectionStrings["LumberjackConnectionString"].ToString())) {
                db.xspWriteDirectException("Service", service.Name, service.Id, null, null, "Legion", request.Requestor.ClientIPAddress ?? request.Requestor.HostIPAddress, request.Requestor.HostIPAddress, type, message, details, stacktrace, ref eventid, ref resultcode);
            }

            return (int)eventid;
        }

        public static int WriteException(Exception e, string details) {
            return WriteException(e.GetType().ToString(), e.Message, e.StackTrace, details);
        }

        public static int WriteException(string type, string message, string stacktrace, string details) {
            string ip = ServerTools.IPv4Addresses.First().ToString();
            return WriteException(type, message, stacktrace, details, ip, ip);
        }

        public static int WriteException(string type, string message, string stacktrace, string details, string clientip, string hostip) {
            int? eventid = null;
            string resultcode = null;

            if (clientip == null)
                clientip = ClientTools.IPAddress();
            if (hostip == null)
                hostip = ServerTools.IPv4Addresses.First().ToString();

            using (LumberjackLinqDataContext db = new LumberjackLinqDataContext(ConfigurationManager.ConnectionStrings["LumberjackConnectionString"].ToString())) {
                db.xspWriteDirectException("Legion", "Core", 1, null, null, "Legion", clientip, hostip, type, message, details, stacktrace, ref eventid, ref resultcode);
            }

            return (int)eventid;
        }

        #endregion
    }
}
