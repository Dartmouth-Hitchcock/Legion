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
using System.Web;

using Legion.Core.Caching;
using Legion.Core.Services;

namespace Legion.Core.Modules {

    /// <summary>
    /// A logged exception
    /// </summary>
    public class LoggedException {

        /// <summary>
        /// The type of application doing the logging
        /// </summary>
        public ApplicationType ApplicationType = ApplicationType.Legion;
        /// <summary>
        /// The name of the application
        /// </summary>
        public string ApplicationName = "Core";
        /// <summary>
        /// The ID of the application
        /// </summary>
        public int ApplicationId = 1;
        /// <summary>
        /// The logging group to associate this event with (defaults to application name)
        /// </summary>
        public string Group = "Legion";
        /// <summary>
        /// The exception type
        /// </summary>
        public string Type;
        /// <summary>
        /// The exception message
        /// </summary>
        public string Message;
        /// <summary>
        /// The exception stack trace
        /// </summary>
        public string StackTrace;
        /// <summary>
        /// The details associated with this event
        /// </summary>
        public string Details;
        /// <summary>
        /// The IP of the client making the call
        /// </summary>
        public string ClientIp;
        /// <summary>
        /// The IP of the host procesisng the call
        /// </summary>
        public string HostIp;

        /// <summary>
        /// The exception to be logged (overwrites Type, Message, and StackTrace if already set)
        /// </summary>
        public Exception Exception {
            set {
                Type = value.GetType().ToString();
                Message = value.Message;
                StackTrace = value.StackTrace;
            }
        }

        /// <summary>
        /// The incoming request (overwrites ApplicationType, ApplicationId, ApplicatoinName, Details, CLientIp, and HostIp if already set)
        /// </summary>
        public Request Request {
            set {
                Service service = ((Dictionary<string, Service>)HttpRuntime.Cache[Cache.CACHE_KEYS.Services])[value.ServiceKey];
                ApplicationType = ApplicationType.Service;
                ApplicationId = service.Id;
                ApplicationName = service.Name;
                Details = value.ToXml();

                ClientIp = (value.Requestor.ClientIPAddress ?? value.Requestor.HostIPAddress);
                HostIp = value.Requestor.HostIPAddress;
            }
        }
    }
}
