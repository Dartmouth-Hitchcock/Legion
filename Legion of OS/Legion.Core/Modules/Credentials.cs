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

namespace Legion.Core.Modules {

    /// <summary>
    /// Centralized credential management for services
    /// </summary>
    public abstract class Credentials : ExternalFuntionalityModule {

        /// <summary>
        /// The reference to the module
        /// </summary>
        public static Credentials Module {
            get { return ExternalFuntionalityModule.GetModule("Credentials") as Credentials; }
        }

        /// <summary>
        /// Gets a credential
        /// </summary>
        /// <param name="sKey">The encryption key</param>
        /// <param name="serviceid">the Service id</param>
        /// <param name="credentialKey">the name of the credential</param>
        /// <returns>The unencrypted credential</returns>
        protected abstract string GetServiceCredential(string sKey, int serviceid, string credentialKey);

        /// <summary>
        /// Gets a credential
        /// </summary>
        /// <param name="serviceid">the Service id</param>
        /// <param name="credentialKey">the name of the credential</param>
        /// <returns>The unencrypted credential</returns>
        public string GetServiceCredential(int serviceid, string credentialKey) {
            return GetServiceCredential(Settings.GetString("CredentialsEncryptionKey"), serviceid, credentialKey);
        }
    }
}
