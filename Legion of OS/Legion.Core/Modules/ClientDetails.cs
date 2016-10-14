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
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Legion.Core.Modules {

    /// <summary>
    /// 
    /// </summary>
    public abstract class ClientDetails : ExternalFuntionalityModule {

        /// <summary>
        /// The reference to the module
        /// </summary>
        public static ClientDetails Module {
            get { return ExternalFuntionalityModule.GetModule("ClientDetails") as ClientDetails; }
        }

        public abstract string IpAddress(NameValueCollection serverVariables);

        public abstract bool IsSecure(HttpRequest request);

        public abstract bool IsInternal(string ipaddress);

        public abstract bool IsDatacenter(string ipaddress);

        public string IpAddress() {
            return IpAddress(HttpContext.Current.Request);
        }

        public string IpAddress(HttpRequest request) {
            return IpAddress(request.ServerVariables);
        }

        public bool IsInternal(HttpRequest request) {
            return IsInternal(IpAddress(request));
        }

        public bool IsDatacenter(HttpRequest request) {
            return IsDatacenter(IpAddress(request));
        }

    }
}