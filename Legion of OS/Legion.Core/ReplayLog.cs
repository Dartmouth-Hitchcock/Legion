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

using System.Configuration;
using System.Xml;

using Legion.Core.Extensions;
using Legion.Core.Databases;

namespace Legion.Core {

    /// <summary>
    /// 
    /// </summary>
    internal static class ReplayLog {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventid"></param>
        /// <param name="methodid"></param>
        /// <param name="xParameters"></param>
        /// <param name="exceptionName"></param>
        /// <param name="exceptionMessage"></param>
        /// <param name="exceptionStackTrace"></param>
        public static void LogException(int eventid, int methodid, XmlDocument xParameters, string exceptionName, string exceptionMessage, string exceptionStackTrace) {
            using (LegionLinqDataContext db = new LegionLinqDataContext(ConfigurationManager.ConnectionStrings["LegionConnectionString"].ToString())) {
                db.xspLogReplyException(eventid, methodid, xParameters.ToXElement(), exceptionName, exceptionMessage, exceptionStackTrace);
            }
        }
    }
}
