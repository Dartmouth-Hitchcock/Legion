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
using System.Web;

using Thanatos;
using Mjolnir;

namespace Legion {
    class LegionException : ThanatosException {
        public static string APPLICATION_NAME = "Legion";
        
        public LegionException(string message, Exception e) : base(message, e, APPLICATION_NAME) {
            this.ExceptionName = e.GetType().ToString();
            this.LogException();
        }
       

        public LegionException(string message) : base(message, APPLICATION_NAME) {
            this.LogException();
        }

        public LegionException(string message, Exception e, string ip)
            : base(message, e, APPLICATION_NAME, ip) {
            this.ExceptionName = e.GetType().ToString();
            this.LogException();
        }

        public LegionException(string message, string exceptionName, string ip)
            : base(message, APPLICATION_NAME, ip) {
            this.ExceptionName = exceptionName;
            this.LogException();
        }

        public LegionException(string message, Exception e, string exceptionName, string ip)
            : base(message, e, APPLICATION_NAME, ip) {
            this.ExceptionName = exceptionName;
            this.LogException();
        }

        public LegionException(string message, string ip)
            : base(message, APPLICATION_NAME, ip) {
            this.LogException();
        }
    }
}
