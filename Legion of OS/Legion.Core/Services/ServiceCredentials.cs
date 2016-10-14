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
using System.Threading.Tasks;

namespace Legion.Core.Services {

    /// <summary>
    /// Credentials provider
    /// </summary>
    public class ServiceCredentials {
        private int _serviceid;

        /// <summary>
        /// Gets the specified credential
        /// </summary>
        /// <param name="key">The credential's key</param>
        /// <returns>The credential</returns>
        public string this[string key] {
            get { return GetCredential(key); }
        }

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="serviceid">The ID of the Service</param>
        internal ServiceCredentials(int serviceid) {
            _serviceid = serviceid;
        }

        /// <summary>
        /// Gets the specified credential
        /// </summary>
        /// <param name="key">The credential's key</param>
        /// <returns>The credential</returns>
        public string GetCredential(string key) {
            return Legion.Core.Modules.Credentials.Module.GetServiceCredential(_serviceid, key);
        }
    }
}
