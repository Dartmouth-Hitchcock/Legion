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

using System.Linq;

using System;
using Legion.Core.DataStructures;

namespace Legion.Core.Modules {

    /// <summary>
    /// An instance of a logged event
    /// </summary>
    public class LoggedEvent {
        private DateTime _timestamp;

        /// <summary>
        /// When was this event logged
        /// </summary>
        public DateTime Timestamp {
            get { return _timestamp; }
        }

        /// <summary>
        /// The level of this event
        /// </summary>
        public EventLevel Level;
        /// <summary>
        /// The type of this event
        /// </summary>
        public string Type;
        /// <summary>
        /// The details associated with this event
        /// </summary>
        public string Details;

        /// <summary>
        /// Is this event allowed to be logged asyncronously
        /// </summary>
        public bool Async = false;
        /// <summary>
        /// Is this event allowed to be buffered
        /// </summary>
        public bool Buffered = true;

        /// <summary>
        /// The type of application doing the logging
        /// </summary>
        public ApplicationType ApplicationType = ApplicationType.Legion;
        /// <summary>
        /// The logging group to associate this event with (defaults to application name)
        /// </summary>
        public string Group = "Legion";
        /// <summary>
        /// The ID of the application
        /// </summary>
        public int ApplicationId = 1;
        /// <summary>
        /// The name of the application
        /// </summary>
        public string ApplicationName = "Core";
        /// <summary>
        /// The type of the user who generated the event
        /// </summary>
        public string LoggingUserType = null;
        /// <summary>
        /// The ID of the user who generated the event
        /// </summary>
        public string LoggingUserId = null;
        /// <summary>
        /// The type of the user who was being acted upon (if applicable)
        /// </summary>
        public string AffectedUserType = null;
        /// <summary>
        /// The ID of the user who was being acted upon (if applicable)
        /// </summary>
        public string AffectedUserId = null;
        /// <summary>
        /// The IP of the client making the call
        /// </summary>
        public string ClientIp = null;
        /// <summary>
        /// The IP of the host procesisng the call
        /// </summary>
        public string HostIp = null;

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="level">The level of the event</param>
        /// <param name="type">The type of the event</param>
        /// <param name="details">The details associated with the event</param>
        public LoggedEvent(EventLevel level, string type, string details) : this(level, type, details, false) { }

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="level">The level of the event</param>
        /// <param name="type">The type of the event</param>
        /// <param name="details">The details associated with the event</param>
        /// <param name="isSystemEvent">Is this a system event rather than a service event</param>
        public LoggedEvent(EventLevel level, string type, string details, bool isSystemEvent) {
            _timestamp = DateTime.Now;

            Level = level;
            Type = type;
            Details = details;

            if (isSystemEvent) {
                string ip = ServerDetails.IPv4Addresses.First().ToString();
                ClientIp = ip;
                HostIp = ip;
            }
        }
    }
}
