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
using System.Xml;

using Legion.Core.Services;

namespace Legion.Core.Delegates {

    /// <summary>
    /// Loads the value to store
    /// </summary>
    /// <returns>the object to store</returns>
    public delegate object StoredValueLoader();

    /// <summary>
    /// A service method
    /// </summary>
    /// <param name="request">the incoming Request</param>
    /// <param name="result">the outgoing Result</param>
    /// <param name="error">handle to the Error node</param>
    internal delegate void Method(Request request, ReplyNode result, ErrorNode error);

    /// <summary>
    /// A special service method
    /// </summary>
    /// <param name="details">the details node of the Reply</param>
    internal delegate void SpecialMethod(ReplyNode details);
}
