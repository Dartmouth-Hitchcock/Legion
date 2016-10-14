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
using System.Threading;

using Legion.Core.Services;

namespace Legion.Core.DataStructures {
    internal class LoggingBuffer {
        private static System.Timers.Timer _timer;

        internal static void Start() {
            _timer = new System.Timers.Timer() {
                Interval = Settings.GetInt("BufferFlushInterval")
            };

            _timer.Elapsed += FlushToDatabase;

            if (Settings.GetBool("CallLogBufferEnabled"))
                _timer.Enabled = true;
        }

        internal static void Stop() {
            _timer.Enabled = false;
        }

        private static void FlushToDatabase(Object source, System.Timers.ElapsedEventArgs a) {
            Thread callBufferThread = new Thread(new ThreadStart(Method.FlushCallBuffer));
            callBufferThread.Start();

            if (Modules.Logging.Module.IsEventWritingBuffered) {
                Thread eventsBufferThread = new Thread(new ThreadStart(Modules.Logging.Module.FlushEventsBuffer));
                eventsBufferThread.Start();
            }
        }
    }
}
