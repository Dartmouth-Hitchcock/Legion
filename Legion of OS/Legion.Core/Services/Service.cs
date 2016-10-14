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
using System.Linq;
using System.Text;
using System.Net;
using System.Reflection;

using Legion.Core.DataStructures;

namespace Legion.Core.Services {

    /// <summary>
    /// A Legion API Service
    /// </summary>
    public class Service {
        private int _id;
        private string _name, _version;
        private bool _isLogged, _isPublic, _isAuthenticatedUserRequired, _isAuthorizedUserRequired;
        private string _dtCompiled;
        private Dictionary<string, Method> _open;
        private Dictionary<string, Method> _restricted;
        private Dictionary<string, Method> _special;
        private IpAddressRange _sourceIpRange = null;

        private Type _serviceType;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The internal id of the Service</param>
        /// <param name="name">The name of the Service</param>
        /// <param name="type">The Type of the service</param>
        /// <param name="sourceIpRange">The valid source ip range for the Service</param>
        /// <param name="version">The version of the Service</param>
        /// <param name="dtCompiled">The date the Service was compiled</param>
        /// <param name="isLogged">Is this a logged Service</param>
        /// <param name="isPublic">Is this service publicly available</param>
        /// <param name="isAuthenticatedUserRequired">is an authenticated user required to call this Service</param>
        /// <param name="isAuthorizedUserRequired">is an authorized user required to call this Service</param>
        public Service(int id, string name, Type type, IpAddressRange sourceIpRange, string version, string dtCompiled, bool isPublic, bool isLogged, bool isAuthenticatedUserRequired, bool isAuthorizedUserRequired) {
            _id = id;
            _name = name;
            _open = new Dictionary<string, Method>();
            _restricted = new Dictionary<string, Method>();
            _special = new Dictionary<string, Method>(); 
            _serviceType = type;
            _sourceIpRange = sourceIpRange;
            _version = version;
            _dtCompiled = dtCompiled;
            _isPublic = isPublic;
            _isLogged = isLogged;
            _isAuthenticatedUserRequired = isAuthenticatedUserRequired;
            _isAuthorizedUserRequired = isAuthorizedUserRequired;
        }

        /// <summary>
        /// The id of the Service
        /// </summary>
        public int Id {
            get { return _id; }
        }

        /// <summary>
        /// The name of the Service
        /// </summary>
        public string Name {
            get { return _name; }
        }

        /// <summary>
        /// The version of the assmbly governing this service
        /// </summary>
        public string Version {
            get { return _version; }
        }

        /// <summary>
        /// The date this service was last modified
        /// </summary>
        public string CompiledOn {
            get { return _dtCompiled; }
        }

        /// <summary>
        /// The type of the service
        /// </summary>
        public Type ServiceType {
            get { return _serviceType; }
        }

        /// <summary>
        /// Is this service public
        /// </summary>
        public bool IsPublic {
            get { return _isPublic; }
        }

        /// <summary>
        /// Are calls to this service logged
        /// </summary>
        public bool IsLogged {
            get { return _isLogged; }
        }

        /// <summary>
        /// Is an authenticated user required to call this service
        /// </summary>
        public bool IsAuthenticatedUserRequired {
            get { return _isAuthenticatedUserRequired; }
        }

        /// <summary>
        /// Is an authorized user required to call this service
        /// </summary>
        public bool IsAuthorizedUserRequired {
            get { return _isAuthorizedUserRequired; }
        }

        /// <summary>
        /// The Methods of the Service which are available anonymously
        /// </summary>
        public Dictionary<string, Method> Open {
            get { return _open; }
        }

        /// <summary>
        /// The Methods of the Service which require authentication
        /// </summary>
        public Dictionary<string, Method> Restricted {
            get { return _restricted; }
        }

        /// <summary>
        /// The special Methods of the Service
        /// </summary>
        public Dictionary<string, Method> Special {
            get { return _special; }
        }

        /// <summary>
        /// Checks if the specified IP Address is within the application's approved source IP range
        /// </summary>
        /// <param name="ip">The source IP address string to check</param>
        /// <returns>true if the specified ip address is the the service's source IP range, false otherwise</returns>
        public bool HasValidSourceIP(string ip) {
            IPAddress ipa;
            if (IPAddress.TryParse(ip, out ipa))
                return HasValidSourceIP(ipa);
            else
                return false;
        }

        /// <summary>
        /// Checks if the specified IP Address is within the application's approved source IP range
        /// </summary>
        /// <param name="ip">The source IP address to check</param>
        /// <returns>true if the specified ip address is the the service's source IP range, false otherwise</returns>
        public bool HasValidSourceIP(IPAddress ip) {
            if (_sourceIpRange == null)
                return true;
            else {
                return _sourceIpRange.IsInRange(ip);
            }
        }
    }
}
