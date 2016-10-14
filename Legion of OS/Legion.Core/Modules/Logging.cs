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
using System.Collections.Concurrent;
using System.Threading;

namespace Legion.Core.Modules {

    /// <summary>
    /// Events and exceptions logging module
    /// </summary>
    public abstract class Logging : ExternalFuntionalityModule {

        /// <summary>
        /// The reference to the module
        /// </summary>
        public static Logging Module {
            get { return ExternalFuntionalityModule.GetModule("Logging") as Logging; }
        }

        /// <summary>
        /// Is event buffering currently enabled
        /// </summary>
        public abstract bool IsEventWritingBuffered { get; }

        /// <summary>
        /// Write an event to the logs
        /// </summary>
        /// <param name="e">The event to log</param>
        /// <returns>an event id</returns>
        public abstract int WriteEvent(LoggedEvent e);

        /// <summary>
        /// Write an exception to the logs
        /// </summary>
        /// <param name="e">The exception to log</param>
        /// <returns>An event id</returns>
        public abstract int WriteException(LoggedException e);

        /// <summary>
        /// Flush the events buffer to permenant storage
        /// </summary>
        public abstract void FlushEventsBuffer();
    }
}
