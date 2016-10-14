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

using Legion.Core.Clients;
using Legion.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Legion.Core.Modules {

    /// <summary>
    /// Client authentication module
    /// </summary>
    public abstract class Authentication : ExternalFuntionalityModule {

        /// <summary>
        /// The reference to the module
        /// </summary>
        public static Authentication Module {
            get { return ExternalFuntionalityModule.GetModule("Authentication") as Authentication; }
        }

        /// <summary>
        /// Retrieves an account via auth token
        /// </summary>
        /// <param name="token">the token to retrieve</param>
        /// <param name="clientipaddress">the ip address of the client claiming this token</param>
        /// <returns></returns>
        public abstract Account RetrieveAccount(string token, string clientipaddress);

        /// <summary>
        /// Heartbeats an account via auth token
        /// </summary>
        /// <param name="token">the token to heartbeat</param>
        /// <param name="clientipaddress">the ip address of the client claiming this token</param>
        public abstract void HeartbeatAccount(string token, string clientipaddress);
    }
}
