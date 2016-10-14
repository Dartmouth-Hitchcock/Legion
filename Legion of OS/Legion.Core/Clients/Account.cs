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

using System.Collections.Generic;

using Legion.Core.Exceptions;
using Legion.Core.Modules;

namespace Legion.Core.Clients {

    /// <summary>
    /// Authenticated account details object
    /// </summary>
    public class Account {
        private const string FQID_FORMAT = "{0}:{1}";

        private string _type, _identifierType, _identifier, _givenname, _middlename, _surname, _emailaddress, _authToken;

        #region accessors

        /// <summary>
        /// The type of account or authoritative source for this account
        /// </summary>
        public string AccountType {
            get { return _type; }
        }

        /// <summary>
        /// The type of identifer which is used (email, id, etc)
        /// </summary>
        public string IdentifierType {
            get { return _identifierType; }
        }

        /// <summary>
        /// The account identifier
        /// </summary>
        public string Identifier {
            get { return _identifier; }
        }

        /// <summary>
        /// The fully qualified Legion identifier for this account ({Type}:{Identifier})
        /// </summary>
        public string FullyQualifiedIdentifier {
            get { return string.Format(FQID_FORMAT, _type, _identifier); }
        }

        /// <summary>
        /// The user's given name (typically first name)
        /// </summary>
        public string GivenName {
            get { return _givenname; }
        }

        /// <summary>
        /// The user's middle name, if any
        /// </summary>
        public string MiddleName {
            get { return _middlename; }
        }

        /// <summary>
        /// The user's surname (typically last name)
        /// </summary>
        public string Surname {
            get { return _surname; }
        }

        /// <summary>
        /// The email address associated with this account
        /// </summary>
        public string EmailAddress {
            get { return _emailaddress; }
        }

        /// <summary>
        /// The auth token that was used to retrieve this account
        /// </summary>
        public string AuthToken {
            get { return _authToken; }
        }

        #endregion
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">the type of account</param>
        /// <param name="identifierType">the account identifier type</param>
        /// <param name="identifier">the account identifier</param>
        /// <param name="emailaddress">ther user's email address</param>
        /// <param name="givenname">the user's given name</param>
        /// <param name="middlename">the user's middle name</param>
        /// <param name="surname">the user's surname</param>
        /// <param name="authToken">the auth token which was used to retrieve this account</param>
        public Account(string type, string identifierType, string identifier, string emailaddress, string givenname, string middlename, string surname, string authToken) {
            _type = type;
            _identifierType = identifierType;
            _identifier = identifier;
            _givenname = givenname;
            _middlename = middlename;
            _surname = surname;
            _emailaddress = emailaddress;
            _authToken = authToken;
        }

        /// <summary>
        /// Hearbeats this account's auth token
        /// </summary>
        public void Heartbeat() {
            try {
                Authentication.Module.HeartbeatAccount(AuthToken, ClientDetails.Module.IpAddress());
            }
            catch(ModuleNotFoundException){ /* suppress */ }
        }

        /// <summary>
        /// Check if this account has permission to the specified resource
        /// </summary>
        /// <param name="resource">the resource to check</param>
        /// <returns>true if the account has permissions, false otherwise</returns>
        public bool CheckPermissions(string resource) {
            return Permissions.Module.Check(
                _type,
                _identifierType,
                _identifier,
                resource
            );
        }

        /// <summary>
        /// Check if this account has permission to the specified resources
        /// </summary>
        /// <param name="resources">the list of resources to check</param>
        /// <returns>a Dictionary of string, bool pairs containing the account's permissions to the specified resources</returns>
        public Dictionary<string, bool> CheckPermissions(List<string> resources) {
            return Permissions.Module.Check(
                _type,
                _identifierType,
                _identifier,
                resources
            );
        }

        internal static Account Retrieve(string token, string clientipaddress) {
            Account account = Authentication.Module.RetrieveAccount(token, clientipaddress);
            if (account != default(Account))
                return account;
            else
                throw new HandlingError("Invalid or expired auth token.");
        }
    }
}
