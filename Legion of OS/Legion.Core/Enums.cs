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

namespace Legion.Core {

    /// <summary>
    /// Working environment
    /// </summary>
    public enum Environment {
        /// <summary>
        /// The development / working environment
        /// </summary>
        Development,
        /// <summary>
        /// The pre-live environment
        /// </summary>
        Staging,
        /// <summary>
        /// The live environment
        /// </summary>
        Production
    }

    /// <summary>
    /// Type of rate limiting metric
    /// </summary>
    public enum RateType {
        /// <summary>
        /// Rate limiting based on the host that is calling Legion
        /// </summary>
        Host,
        /// <summary>
        /// Rate limiting based on the user (by ip) that is calling Legion
        /// </summary>
        User,
        /// <summary>
        /// Default rate limiting
        /// </summary>
        Default
    }

    /// <summary>
    /// Logging event levels
    /// </summary>
    public enum EventLevel {
        /// <summary>
        /// A generic event
        /// </summary>
        Event,
        /// <summary>
        /// An access audit event
        /// </summary>
        Audit,
        /// <summary>
        /// An information event not tied to a specific action
        /// </summary>
        Info,
        /// <summary>
        /// A minior adverse event that was handled
        /// </summary>
        Warning,
        /// <summary>
        /// A major adverse event that was handled
        /// </summary>
        Critical,
        /// <summary>
        /// An adverse event that was unhandled
        /// </summary>
        Fatal,
        /// <summary>
        /// Debug information
        /// </summary>
        Debug
    }

    /// <summary>
    /// Type of application
    /// </summary>
    public enum ApplicationType {
        /// <summary>
        /// A calling application
        /// </summary>
        Application,
        /// <summary>
        /// Legion Core
        /// </summary>
        Legion,
        /// <summary>
        /// A Service
        /// </summary>
        Service
    }

}
