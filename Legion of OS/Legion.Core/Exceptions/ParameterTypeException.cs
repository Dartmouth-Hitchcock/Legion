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

using Legion.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Legion.Core.Exceptions {

    /// <summary>
    /// Requested parameter is not a valid instance of the specified type
    /// </summary>
    [Serializable]
    public class ParameterTypeException : Exception {
        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="type">The type of the parameter</param>
        /// <param name="key">The key that does not match the parameter type</param>
        public ParameterTypeException(ParameterType type, string key)
            : base(string.Format("Value contained in '{0}' is not a valid {1}.", key, type.ToString())) { }
    }
}
