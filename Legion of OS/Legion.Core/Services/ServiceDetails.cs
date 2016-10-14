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
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Web;

namespace Legion.Core.Services {

    /// <summary>
    /// Details associated with a Service
    /// </summary>
    public class ServiceDetails {
        private const string CACHE_TYPE = "servicedetails";

        private int _serviceid;
        private string _serviceKey;

        private ServiceSettings _settings = null;
        private ServiceCredentials _credentials = null;
        private ServiceVolatileStorage _volatileStorage = null;

        /// <summary>
        /// The internal ID of this Service
        /// </summary>
        internal int Id {
            get { return _serviceid; }
        }

        /// <summary>
        /// The key of this Service
        /// </summary>
        internal string Key {
            get { return _serviceKey; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceid">The ID of Service to get details for</param>
        /// <param name="serviceKey">The key of the Service to get details for</param>
        internal ServiceDetails(int serviceid, string serviceKey) {
            _serviceid = serviceid;
            _serviceKey = serviceKey;
        }

        /// <summary>
        /// The Service's settings
        /// </summary>
        [Obsolete("Use Settings instead")]
        public Dictionary<string, string> ConnectionStrings {
            get { return this.Settings.ToDictionary(); }
        }

        /// <summary>
        /// The Service's credentials
        /// </summary>
        public ServiceCredentials Credentials {
            get {
                if (_credentials == null)
                    _credentials = new ServiceCredentials(_serviceid);

                return _credentials;
            }
        }

        /// <summary>
        /// The Service's settings
        /// </summary>
        public ServiceSettings Settings {
            get {
                if (_settings == null)
                    _settings = new ServiceSettings(_serviceid, _serviceKey);

                return _settings;
            }
        }

        /// <summary>
        /// Volitile storage for this service
        /// </summary>
        public ServiceVolatileStorage VolatileStorage {
            get {
                if (_volatileStorage == null)
                    _volatileStorage = new ServiceVolatileStorage(_serviceKey);

                return _volatileStorage;
            }
        }

        /// <summary>
        /// The current working environment
        /// </summary>
        public Environment Environment {
            get { return (Environment)Enum.Parse(typeof(Environment), Legion.Core.Settings.GetString("Environment"), true); }
        }
    }
}
