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
using System.Threading.Tasks;

namespace Legion.Core.DataStructures {

    /// <summary>
    /// Dynamic locks
    /// </summary>
    internal class DynamicLock {
        private static object _masterLock = new object();
        private static Dictionary<string, object> _locks = new Dictionary<string, object>();

        /// <summary>
        /// The global lock object for the dynamic lock collection
        /// </summary>
        internal static object Lock {
            get { return _masterLock; }
        }

        /// <summary>
        /// Gets a lock object
        /// </summary>
        /// <param name="type">the lock type (group)</param>
        /// <param name="key">the lock object key to get</param>
        /// <returns>the lock object</returns>
        internal static object Get(string type, string key) {
            string lockKey = GetLockKey(type, key);
            lock (_masterLock) {
                if (!_locks.ContainsKey(lockKey))
                    _locks.Add(lockKey, new object());

                return _locks[lockKey];
            }
        }

        /// <summary>
        /// Removes a lock object
        /// </summary>
        /// <param name="type">the lock type (group)</param>
        /// <param name="key">the key to remove</param>
        internal static void Remove(string type, string key) {
            string lockKey = GetLockKey(type, key);
            lock (_masterLock) {
                if (!_locks.ContainsKey(lockKey))
                    _locks.Remove(lockKey);
            }
        }

        private static string GetLockKey(string type, string key) {
            return string.Format("{0}//{1}", type, key);
        }
    }
}
