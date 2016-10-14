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
using System.Web;
using System.Web.SessionState;

namespace Caesar.Legion.ItemContainers {

    internal class SessionItemContainer {
        private const string KEY_PREFIX = "SessionItemContainter";
        private static Dictionary<string, object> _fakeSessionContainer = new Dictionary<string, object>();

        public static void Store(SessionItemContainerKey key, object value) {
            if (HttpContext.Current != null && HttpContext.Current.Session != null) {
                HttpSessionState session = HttpContext.Current.Session;
                session[BuildKey(key)] = value;
            }
            else {
                _fakeSessionContainer[BuildKey(key)] = value;
            }
        }

        public static object Retrieve(SessionItemContainerKey key) {
            if (HttpContext.Current != null && HttpContext.Current.Session != null) {
                HttpSessionState session = HttpContext.Current.Session;
                if (session[BuildKey(key)] != null)
                    return session[BuildKey(key)];
                else
                    return null;
            }
            else {
                if (_fakeSessionContainer.ContainsKey(BuildKey(key)))
                    return _fakeSessionContainer[BuildKey(key)];
                else
                    return null;
            }
        }

        private static string BuildKey(SessionItemContainerKey key) {
            return string.Format("{0}_{1}", KEY_PREFIX, key.ToString());
        }
    }
}
