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
    /// Permissions authorization 
    /// </summary>
    public abstract class Permissions : ExternalFuntionalityModule {

        /// <summary>
        /// The reference to the module
        /// </summary>
        public static Permissions Module {
            get { return ExternalFuntionalityModule.GetModule("Permissions") as Permissions; }
        }

        /// <summary>
        /// Check a list of resources for permissions against the specified user
        /// </summary>
        /// <param name="userType">The type of user</param>
        /// <param name="userIdentifierType">The type of user identifier</param>
        /// <param name="userIdentifier">The user identifier</param>
        /// <param name="resources">The list of resources to check</param>
        /// <returns>A Dictionary of true/false permissions</returns>
        public abstract Dictionary<string, bool> Check(string userType, string userIdentifierType, string userIdentifier, List<string> resources);

        /// <summary>
        /// Check a resource for permissions against the specified user
        /// </summary>
        /// <param name="userType">The type of user</param>
        /// <param name="userIdentifierType">The type of user identifier</param>
        /// <param name="userIdentifier">The user identifier</param>
        /// <param name="resource">The resource to check</param>
        /// <returns>True if the user has permissions, false otherwise</returns>
        public bool Check(string userType, string userIdentifierType, string userIdentifier, string resource) {
            Dictionary<string, bool> permissions = Check(userType, userIdentifierType, userIdentifier, new List<string>() { resource });
            return permissions[resource];
        }
    }
}
