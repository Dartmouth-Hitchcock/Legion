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
using System.Xml;

using Legion.Core.Modules;

namespace Legion.Core.Services.Tools {

    /// <summary>
    /// Check user permissions
    /// </summary>
    public class Permissions {
        /// <summary>
        /// Type of user
        /// </summary>
        public enum UserType {
            /// <summary>
            /// A Dartmouth-Hitchcock user
            /// </summary>
            DHUser
        }

        /// <summary>
        /// User identifier type
        /// </summary>
        public enum IndentifierType {
            /// <summary>
            /// Identified by email
            /// </summary>
            Email,
            /// <summary>
            /// Identified by LDAP UID
            /// </summary>
            UID,
            /// <summary>
            /// Identified by Heimdall ID
            /// </summary>
            HID
        }

        /// <summary>
        /// Check a resource for permissions against the specified user
        /// </summary>
        /// <param name="userType">The type of user</param>
        /// <param name="userIdentifierType">The type of user identifier</param>
        /// <param name="userIdentifier">The user identifier</param>
        /// <param name="resource">The resource to check</param>
        /// <returns>A Dictionary of true/false permissions</returns>
        public static bool Check(UserType userType, IndentifierType userIdentifierType, string userIdentifier, string resource) {
            return Legion.Core.Modules.Permissions.Module.Check(
                userType.ToString(),
                userIdentifierType.ToString(), 
                userIdentifier, 
                resource
            );
        }

        /// <summary>
        /// Check a list of resources for permissions against the specified user
        /// </summary>
        /// <param name="userType">The type of user</param>
        /// <param name="userIdentifierType">The type of user identifier</param>
        /// <param name="userIdentifier">The user identifier</param>
        /// <param name="resources">The list of resources to check</param>
        /// <returns>A Dictionary of true/false permissions</returns>
        public static Dictionary<string, bool> Check(UserType userType, IndentifierType userIdentifierType, string userIdentifier, List<string> resources) {
            return Legion.Core.Modules.Permissions.Module.Check(
                userType.ToString(),
                userIdentifierType.ToString(), 
                userIdentifier, 
                resources
            );
        }

        /// <summary>
        /// Check a resource for permissions against the specified user
        /// </summary>
        /// <param name="account">The Account to check</param>
        /// <param name="resource">The resource to check</param>
        /// <returns>A Dictionary of true/false permissions</returns>
        public static bool Check(Clients.Account account, string resource) {
            return Legion.Core.Modules.Permissions.Module.Check(
                account.AccountType, 
                account.IdentifierType, 
                account.Identifier,
                resource
            );
        }

        /// <summary>
        /// Check a list of resources for permissions against the specified user
        /// </summary>
        /// <param name="account">The Account to check</param>
        /// <param name="resources">The list of resources to check</param>
        /// <returns>A Dictionary of true/false permissions</returns>
        public static Dictionary<string, bool> Check(Clients.Account account, List<string> resources) {
            return Legion.Core.Modules.Permissions.Module.Check(
                account.AccountType,
                account.IdentifierType,
                account.Identifier,
                resources
            );
        }
    }
}
