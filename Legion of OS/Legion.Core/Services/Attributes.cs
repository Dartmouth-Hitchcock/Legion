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

namespace Legion.Core.Services {

    /// <summary>
    /// This member requires authorization
    /// </summary>
    public class AuthorizedUserRequired : LegionAttribute {
        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        public AuthorizedUserRequired() {
            _isAuthenticatedUserRequired = true;
            _isAuthorizedUserRequired = true;
        }

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="description">A description of the reason why this attribute is used</param>
        public AuthorizedUserRequired(string description) : this() {
            _description = description;
        }
    }

    /// <summary>
    /// This member requires an authenticated Heimdall User auth token
    /// </summary>
    public class AuthenticatedUserRequired : LegionAttribute {
        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        public AuthenticatedUserRequired() {
            _isAuthenticatedUserRequired = true;
        }

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="description">A description of the reason why this attribute is used</param>
        public AuthenticatedUserRequired(string description) : this() {
            _description = description;
        }
    }

    /// <summary>
    /// This method handles sensitive information and is not cacheable
    /// </summary>
    public class SensitiveInformation : NotCacheable {
        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        public SensitiveInformation() : base() { }

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="description">A description of the reason why this attribute is used</param>
        public SensitiveInformation(string description) : base(description) { }
    }

    /// <summary>
    /// This method is not able to be cached
    /// </summary>
    public class NotCacheable : LegionAttribute {
        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        public NotCacheable() {
            _isCacheable = false;
        }

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="description">A description of the reason why this attribute is used</param>
        public NotCacheable(string description) : this() {
            _description = description;
        }
    }

    /// <summary>
    /// Base Attribute class
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public abstract class LegionAttribute : System.Attribute {
        /// <summary>
        /// Is the method result caechable
        /// </summary>
        protected bool _isCacheable = true;
        /// <summary>
        /// Is an authenticated user required to make this request
        /// </summary>
        protected bool _isAuthenticatedUserRequired = false;
        /// <summary>
        /// Is an authorized user required to make this request
        /// </summary>
        protected bool _isAuthorizedUserRequired = false;
        /// <summary>
        /// The description
        /// </summary>
        protected string _description = string.Empty;

        /// <summary>
        /// Is this method cacheable
        /// </summary>
        public bool IsCacheable {
            get { return _isCacheable; }
        }

        /// <summary>
        /// Does this method require authentication?
        /// </summary>
        public bool IsAuthenticatedUserRequired {
            get { return _isAuthenticatedUserRequired; }
        }

        /// <summary>
        /// Does this method require authorization?
        /// </summary>
        public bool IsAuthorizedUserRequired {
            get { return _isAuthorizedUserRequired; }
        }

        /// <summary>
        /// A description of the method
        /// </summary>
        public string Description {
            get { return _description; }
        }
    }
}
