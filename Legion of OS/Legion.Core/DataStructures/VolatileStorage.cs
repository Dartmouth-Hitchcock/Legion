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

using Legion.Core.Caching;
using Legion.Core.Delegates;

namespace Legion.Core.DataStructures {

    /// <summary>
    /// Volitile in-memory storage. Contents may not exist.
    /// </summary>
    public class VolatileStorage {
        private const string CACHE_TYPE = "volatilestorage";
        private string _prefix = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="prefix">the store prefix</param>
        internal VolatileStorage(string prefix) {
            _prefix = prefix;
        }

        /// <summary>
        /// Retrieves a value from storage
        /// </summary>
        /// <typeparam name="T">the type of value</typeparam>
        /// <param name="key">the key to retrieve</param>
        /// <returns>the value</returns>
        public T Retrieve<T>(string key) {
            return Retrieve<T>(key, null);
        }

        /// <summary>
        /// Retrieves a value from storage
        /// </summary>
        /// <typeparam name="T">the type of value</typeparam>
        /// <param name="key">the key to retrieve</param>
        /// <param name="valueLoader">delegate to populate the value if it does not exist in storage</param>
        /// <returns>the value</returns>
        public T Retrieve<T>(string key, StoredValueLoader valueLoader) {
            return Retrieve<T>(key, null, valueLoader);
        }

        /// <summary>
        /// Retrieves a value from storage
        /// </summary>
        /// <typeparam name="T">the type of value</typeparam>
        /// <param name="key">the key to retrieve</param>
        /// <param name="valueLoader">delegate to populate the value if it does not exist in storage</param>
        /// <param name="expires">when this object expires</param>
        /// <returns>the value</returns>
        public T Retrieve<T>(string key, DateTime? expires, StoredValueLoader valueLoader) {
            object oValue = Cache.Get(CACHE_TYPE, GetCacheKey(key));

            if (oValue == null && valueLoader != null) {
                oValue = valueLoader();

                if (oValue != null)
                    Save(key, oValue, expires);
            }

            return (oValue == null ? default(T) : (T)oValue);
        }

        /// <summary>
        /// Retrieves a value from storage
        /// </summary>
        /// <typeparam name="T">the type of value</typeparam>
        /// <param name="key">the key to retrieve</param>
        /// <param name="valueLoader">delegate to populate the value if it does not exist in storage</param>
        /// <param name="expires">the sliding expiration for this object in seconds</param>
        /// <returns>the value</returns>
        internal T Retrieve<T>(string key, int expires, StoredValueLoader valueLoader) {
            object oValue = Cache.Get(CACHE_TYPE, GetCacheKey(key));

            if (oValue == null && valueLoader != null) {
                oValue = valueLoader();

                if (oValue != null)
                    Save(key, oValue, DateTime.Now.AddSeconds(expires));
            }

            return (oValue == null ? default(T) : (T)oValue);
        }

        /// <summary>
        /// Saves a value to storage
        /// </summary>
        /// <param name="key">the key to save as</param>
        /// <param name="oValue">the value to save to storage</param>
        public void Save(string key, object oValue) {
            Save(key, oValue, null);
        }

        /// <summary>
        /// Saves a value to storage
        /// </summary>
        /// <param name="key">the key to save as</param>
        /// <param name="oValue">the value to save to storage</param>
        /// <param name="expires">the time to expire the value</param>
        public void Save(string key, object oValue, DateTime? expires) {
            if (expires == null)
                expires = DateTime.Now.AddSeconds(Settings.GetInt("VolatileStorageObjectDefaultExpiration"));
            else {
                DateTime maxExpiration = DateTime.Now.AddSeconds(Settings.GetInt("VolatileStorageObjectMaxExpiration"));
                if (expires > maxExpiration)
                    throw new ArgumentException(Settings.GetString("ExceptionMessageVolatileStorageInvalidExpiration", new Dictionary<string, string>(){
                        {"MaxExpiration", string.Format(string.Format("{{0:{0}}}", Settings.GetString("DateTimeFormat")), maxExpiration)}
                    }));
            }

            Cache.Add(CACHE_TYPE, GetCacheKey(key), oValue, (DateTime)expires, null);
        }

        private string GetCacheKey(string key) {
            return string.Format("{0}//{1}", _prefix, key);
        }

        /// <summary>
        /// Gets the store set for a specified prefix
        /// </summary>
        /// <param name="prefix">the key prefix</param>
        /// <returns>The VolatileStorage object for the specified prefix</returns>
        public static VolatileStorage GetStore(string prefix) {
            return new VolatileStorage(prefix);
        }
    }
}
