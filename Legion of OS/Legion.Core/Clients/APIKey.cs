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

using Legion.Core.Caching;
using System.Collections.Generic;
using System.Web;

namespace Legion.Core.Clients {

    /// <summary>
    /// Legion API Key
    /// </summary>
    public class APIKey {
        private string _key = null;
        private bool? _isRevoked = null;
        private Application _application = null;

        /// <summary>
        /// Is the API key valid (is associated with an active application
        /// </summary>
        public bool IsValid {
            get { return (Application.Id != -1 && !IsRevoked); }
        }

        /// <summary>
        /// Has the API key been revoked?
        /// </summary>
        public bool IsRevoked {
            get {
                if (_isRevoked == null) {
                    List<string> revokedKeys = (List<string>)HttpRuntime.Cache[Cache.CACHE_KEYS.KeyRevocationList];
                    _isRevoked = revokedKeys.Contains(_key);
                }

                return (_isRevoked == true ? true : false);
            }
        }

        /// <summary>
        /// The Application associated with this API key
        /// </summary>
        public Application Application {
            get {
                if (_application == null)
                    _application = new Application(_key);

                return _application;
            }
        }

        /// <summary>
        /// The API key
        /// </summary>
        public string Key {
            get { return (_key == string.Empty ? null : _key); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key">the string API key</param>
        public APIKey(string key) {
            _key = key;
        }

        /// <summary>
        /// Casts this APIKey to a string
        /// </summary>
        /// <returns>the string form of this APIKEY</returns>
        public override string ToString() {
            return Key;
        }
    }
}
