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

using System.Collections.Generic;
using System.Web;

using Legion.Core.Clients;
using Legion.Core.Modules;

namespace Legion.Core.Services.ServiceToService {

    /// <summary>
    /// Local Legion Request
    /// </summary>
    internal class Request : Legion.Core.Services.Request {
        private const string HOSTNAME = "ServiceToService";

        /// <summary>
        /// Constructor
        /// This constructor re-parses system values from the HttpContext.Current object.
        /// Use of this constructor should be avoided if possible, use the overload which takes the originally parsed request. 
        /// </summary>
        /// <param name="serviceKey">the key of the service to call</param>
        /// <param name="methodKey">the key of the method in the service to call</param>
        /// <param name="parameterSet">the parameters to pass to the method</param>
        internal Request(string serviceKey, string methodKey, Dictionary<string, string> parameterSet)
            : this(
                HttpContext.Current,
                serviceKey,
                methodKey,
                parameterSet
            ) { }

        /// <summary>
        /// Constructor
        /// This constructor re-parses system values from the HttpContext.Current object.
        /// Use of this constructor should be avoided if possible, use the overload which takes the originally parsed request. 
        /// </summary>
        /// <param name="context">The HttpContext to run against</param>
        /// <param name="serviceKey">the key of the service to call</param>
        /// <param name="methodKey">the key of the method in the service to call</param>
        /// <param name="parameterSet">the parameters to pass to the method</param>
        internal Request(HttpContext context, string serviceKey, string methodKey, Dictionary<string, string> parameterSet)
            : base(
                Legion.Core.Services.Request.GetRequestId(context),
                Legion.Core.Services.Request.GetApplicationKey(new RawRequest(context.Request)),
                serviceKey,
                methodKey,
                parameterSet,
                Requestor.GetUserIPAddress(new RawRequest(context.Request)),
                ClientDetails.Module.IpAddress(context.Request),
                HOSTNAME,
                true
            ) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="originalRequest">the original request passed to the server</param>
        /// <param name="serviceKey">the key of the service to call</param>
        /// <param name="methodKey">the key of the method in the service to call</param>
        /// <param name="parameterSet">the parameters to pass to the method</param>
        internal Request(Services.Request originalRequest, string serviceKey, string methodKey, Dictionary<string, string> parameterSet)
            : base(
                originalRequest.Id,
                originalRequest.APIKey.Key, 
                serviceKey, 
                methodKey, 
                parameterSet, 
                originalRequest.Requestor.ClientIPAddress, 
                originalRequest.Requestor.HostIPAddress, 
                HOSTNAME, 
                true
            ) { }
    }
}
