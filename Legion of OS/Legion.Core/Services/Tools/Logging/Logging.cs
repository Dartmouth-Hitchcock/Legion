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

namespace Legion.Core.Services.Tools.Logging {

    /// <summary>
    /// Logging 
    /// </summary>
    public static class Logging {
        private static ApplicationProxyLoggingModule _application = null;
        private static ServiceLoggingModule _service = null;

        /// <summary>
        /// Log on behalf of the calling Application
        /// </summary>
        public static ApplicationProxyLoggingModule ApplicationProxy {
            get {
                if (_application == null)
                    _application = new ApplicationProxyLoggingModule();

                return _application;
            }
        }

        /// <summary>
        /// Log on behalf of the current Service
        /// </summary>
        public static ServiceLoggingModule Service {
            get {
                if (_service == null)
                    _service = new ServiceLoggingModule();

                return _service;
            }
        }
    }
}
