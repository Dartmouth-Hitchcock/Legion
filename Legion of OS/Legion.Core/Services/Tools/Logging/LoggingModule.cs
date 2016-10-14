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

using Legion.Core.DataStructures;
using System.Linq;

namespace Legion.Core.Services.Tools.Logging {

    /// <summary>
    /// Logging module base class
    /// </summary>
    public abstract class LoggingModule {

        #region abstracts

        /// <summary>
        /// ABSTRACT
        /// The type of the application
        /// </summary>
        protected abstract LoggingApplicationType ApplicationType { get; }
        /// <summary>
        /// ABSTRACT
        /// The name of the application
        /// </summary>
        protected abstract string ApplicationName { get; }
        /// <summary>
        /// ABSTRACT
        /// The application ID
        /// </summary>
        protected abstract int ApplicationId { get; }

        #endregion

        #region user details

        private string LoggingUserType {
            get {
                if (Request.Current.Requestor.Account != null)
                    return Request.Current.Requestor.Account.IdentifierType;
                else
                    return null;
            }
        }

        private string LoggingUserId {
            get {
                if (Request.Current.Requestor.Account != null)
                    return Request.Current.Requestor.Account.Identifier;
                else
                    return null;
            }
        }

        #endregion

        /// <summary>
        /// Writes an event to the Legion event log
        /// </summary>
        /// <param name="eventType">the type of event</param>
        /// <param name="details">the details data</param>
        /// <returns>The event id of the new entry</returns>
        public int WriteEvent(string eventType, string details) {
            return WriteEvent(null, null, null, EventLevel.Info, eventType, details);
        }

        /// <summary>
        /// Writes an event to the Legion event log
        /// </summary>
        /// <param name="eventLevel">the priority level of the event</param>
        /// <param name="eventType">the type of event</param>
        /// <param name="details">the entry data</param>
        /// <returns>The event id of the new entry</returns>
        public int WriteEvent(EventLevel eventLevel, string eventType, string details) {
            return WriteEvent(null, null, null, eventLevel, eventType, details);
        }

        /// <summary>
        /// Writes an event to the Legion event log
        /// </summary>
        /// <param name="group">the log group</param>
        /// <param name="eventType">the type of event</param>
        /// <param name="details">the event details</param>
        /// <returns>The event id of the new entry</returns>
        public int WriteEvent(string group, string eventType, string details) {
            return WriteEvent(null, null, group, EventLevel.Info, eventType, details);
        }

        /// <summary>
        /// Writes an event to the Legion event log
        /// </summary>
        /// <param name="group">the log group</param>
        /// <param name="eventLevel">the priority level of the event</param>
        /// <param name="eventType">the type of event</param>
        /// <param name="details">the event details</param>
        /// <returns>The event id of the new entry</returns>
        public int WriteEvent(string group, EventLevel eventLevel, string eventType, string details) {
            return WriteEvent(null, null, group, eventLevel, eventType, details);
        }

        /// <summary>
        /// Writes an event to the Legion event log
        /// </summary>
        /// <param name="affectedUserType">the type of user which is being acted upon</param>
        /// <param name="affectedUserId">the id of the user which is being acted upon</param>
        /// <param name="group">the log group</param>
        /// <param name="eventType">the type of event</param>
        /// <param name="details">the event details</param>
        /// <returns>The event id of the new entry</returns>
        public int WriteEvent(string affectedUserType, string affectedUserId, string group, string eventType, string details) {
            return WriteEvent(affectedUserType, affectedUserId, group, EventLevel.Info, eventType, details);
        }

        /// <summary>
        /// Writes an event to the Legion event log
        /// </summary>
        /// <param name="affectedUserType">the type of user which is being acted upon</param>
        /// <param name="affectedUserId">the id of the user which is being acted upon</param>
        /// <param name="group">the log group</param>
        /// <param name="eventLevel">the priority level of the event</param>
        /// <param name="eventType">the type of event</param>
        /// <param name="details">the event details</param>
        /// <returns>The event id of the new entry</returns>
        public int WriteEvent(string affectedUserType, string affectedUserId, string group, EventLevel eventLevel, string eventType, string details) {
            return Modules.Logging.Module.WriteEvent(new Modules.LoggedEvent(eventLevel.ToEventLevel(), eventType, details) {
                Group = group,
                ApplicationType = ApplicationType.ToApplicationType(),
                ApplicationName = ApplicationName,
                ApplicationId = ApplicationId,
                LoggingUserType = LoggingUserType,
                LoggingUserId = LoggingUserId,
                AffectedUserType = affectedUserType,
                AffectedUserId = affectedUserId,
                ClientIp = Request.Current.Requestor.ClientIPAddress,
                HostIp = ServerDetails.IPv4Addresses.First().ToString()
            });
        }
    }
}
