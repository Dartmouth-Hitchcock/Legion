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

using Legion.Core.DataStructures;

namespace Legion.Core.Services {

    /// <summary>
    /// Service volitile in-memory storage. Contents may not exist.
    /// </summary>
    public class ServiceVolatileStorage : VolatileStorage {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceKey">the service's key</param>
        public ServiceVolatileStorage(string serviceKey) : base(string.Format("service//{0}", serviceKey)) { }

        /// <summary>
        /// Saves a value to storage
        /// </summary>
        /// <param name="key">the key to save as</param>
        /// <param name="oValue">the value to save to storage</param>
        new public void Save(string key, object oValue) {
            Save(key, oValue, DateTime.Now.AddSeconds(Settings.GetInt("ServiceStorageObjectExpiration")));
        }
    }
}
