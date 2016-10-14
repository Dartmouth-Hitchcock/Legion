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
    /// Valid application types for logging
    /// </summary>
    public enum LoggingApplicationType {
        /// <summary>
        /// A Legion Application
        /// </summary>
        Application,
        /// <summary>
        /// A Legion Service
        /// </summary>
        Service
    }

    /// <summary>
    /// Logging Levels
    /// </summary>
    public enum EventLevel {
        /// <summary>
        /// Auditing
        /// </summary>
        Audit,
        /// <summary>
        /// Critical but non-fatal event
        /// </summary>
        Critical,
        /// <summary>
        /// Debug information
        /// </summary>
        Debug,
        /// <summary>
        /// A generic event
        /// </summary>
        Event,
        /// <summary>
        /// A fatal event
        /// </summary>
        Fatal,
        /// <summary>
        /// General info
        /// </summary>
        Info,
        /// <summary>
        /// Non-fatal event
        /// </summary>
        Warning
    }

    /// <summary>
    /// Enum extension
    /// </summary>
    public static class EnumExtensions {

        /// <summary>
        /// Convert a LoggingApplicationType to a Legion.ApplicationType
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static ApplicationType ToApplicationType(this LoggingApplicationType t) {
            switch (t) {
                case LoggingApplicationType.Application:
                    return ApplicationType.Application;
                case LoggingApplicationType.Service:
                    return ApplicationType.Service;
                default:
                    throw new Exception(string.Format("Uncastabled LoggingApplicationType to ApplicationType value '{0}'", t));
            }
        }

        /// <summary>
        /// Convert a Logging.EventLevel to a Legion.EventLevel
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        public static Legion.Core.EventLevel ToEventLevel(this EventLevel l) {
            switch (l) {
                case EventLevel.Audit:
                    return Legion.Core.EventLevel.Audit;
                case EventLevel.Critical:
                    return Legion.Core.EventLevel.Critical;
                case EventLevel.Debug:
                    return Legion.Core.EventLevel.Debug;
                case EventLevel.Event:
                    return Legion.Core.EventLevel.Event;
                case EventLevel.Fatal:
                    return Legion.Core.EventLevel.Fatal;
                case EventLevel.Info:
                    return Legion.Core.EventLevel.Info;
                case EventLevel.Warning:
                    return Legion.Core.EventLevel.Warning;
                default:
                    throw new Exception(string.Format("Uncastabled EventLevel to Legion.EventLevel value '{0}'", l));
            }
        }
    }
}
