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

using Legion.Core.Services.ServiceToService;

namespace Caesar.Legion {

    /// <summary>
    /// An unexpected fault in the Legion call
    /// </summary>
    public class LegionFault {
        private string _type;
        private string _message;

        /// <summary>
        /// The type of fault
        /// </summary>
        public string Type {
            get { return _type; }
        }

        /// <summary>
        /// The fault message
        /// </summary>
        public string Message {
            get { return _message; }
        }

        internal LegionFault(string type, string message) {
            _type = type;
            _message = message;
        }

        public override string ToString() {
            return Message;
        }

        public static implicit operator string (LegionFault f) {
            return f.ToString();
        }

        public static implicit operator LegionFault(Fault f) {
            return new LegionFault(f.Type, f.Message);
        }
    }
}