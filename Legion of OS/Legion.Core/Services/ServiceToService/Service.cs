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
using System.Xml;

using System.Web;
using Legion.Core.DataStructures;

namespace Legion.Core.Services.ServiceToService {

    /// <summary>
    /// Calls a Legion service hosted on the local server
    /// </summary>
    public class Service {
        private Services.Request _originalRequest;
        private HttpContext _originalContext;
        private string _service;

        /// <summary>
        /// Constructor
        /// This constructor re-parses system values from the HttpContext.Current object.
        /// Use of this constructor should be avoided if possible, use the overload which takes the originally parsed request. 
        /// </summary>
        /// <param name="service">The name of the service</param>
        public Service(string service) {
            _originalRequest = null;
            _service = service;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="originalRequest">The origianl service request</param>
        /// <param name="service">The name of the service</param>
        public Service(Services.Request originalRequest, string service) {
            _originalRequest = originalRequest;
            _service = service;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="originalContext">The origianl HttpContext</param>
        /// <param name="service">The name of the service</param>
        public Service(HttpContext originalContext, string service) {
            _originalContext = originalContext;
            _service = service;
        }

        /// <summary>
        /// Calls a method in the Legion Service
        /// </summary>
        /// <param name="method">The method to call</param>
        /// <returns>An XML based LegionReply</returns>
        public Reply Call(string method) {
            return this.Call(method, new Dictionary<string, string>() { });
        }

        /// <summary>
        /// Calls a method in the Legion Service
        /// </summary>
        /// <param name="method">The method to call</param>
        /// <param name="methodParams">A Dictionary of the parameters to pass to the service</param>
        /// <returns>An XML based LegionReply</returns>
        public Reply Call(string method, Dictionary<string, string> methodParams) {
            Request request;
            if(_originalContext != null)
                request = new Request(_originalContext, _service, method, methodParams);
            if (_originalRequest != null)
                request = new Request(_originalRequest, _service, method, methodParams);
            else
                request = new Request(_service, method, methodParams);

            Services.Reply reply = Manager.Process(request);
            return ServiceToService.Reply.Parse(reply.ToString());
        }
    }
}
