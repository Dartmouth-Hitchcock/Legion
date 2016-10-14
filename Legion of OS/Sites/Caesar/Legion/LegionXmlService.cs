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

namespace Caesar.Legion {

    /// <summary>
    /// Calls methods in a Legion Service and returns an XML formatted LegionReply
    /// </summary>
    public class LegionXmlService : LegionService {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service">The legion Service</param>
        /// <param name="apikey">The API key of the calling application</param>
        public LegionXmlService(string service, string apikey, string apiurl) : base(service, apikey, apiurl, ReplyFormat.XML) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service">The legion Service</param>
        /// <param name="apikey">The API key of the calling application</param>
        public LegionXmlService(string service, string apikey) : base(service, apikey, ReplyFormat.XML) { }

        /// <summary>
        /// Calls a method in the Legion Service
        /// </summary>
        /// <param name="method">The method to call</param>
        /// <returns>An XML based LegionReply</returns>
        public LegionReply<XmlElement> Call(string method) {
            return Call(method, null, true);
        }

        /// <summary>
        /// Calls a method in the Legion Service
        /// </summary>
        /// <param name="method">The method to call</param>
        /// <param name="exceptionOnFault">Throw and excpetion if a fault occurs in the service call</param>
        /// <returns>An XML based LegionReply</returns>
        public LegionReply<XmlElement> Call(string method, bool exceptionOnError) {
            return Call(method, null, exceptionOnError);
        }

        /// <summary>
        /// Calls a method in the Legion Service
        /// </summary>
        /// <param name="method">The method to call</param>
        /// <param name="methodParams">A Dictionary of the parameters to pass to the service</param>
        /// <returns>An XML based LegionReply</returns>
        public LegionReply<XmlElement> Call(string method, Dictionary<string, string> methodParams) {
            return Call(method, methodParams, true);
        }

        /// <summary>
        /// Calls a method in the Legion Service
        /// </summary>
        /// <param name="method">The method to call</param>
        /// <param name="methodParams">A Dictionary of the parameters to pass to the service</param>
        /// <param name="exceptionOnFault">Throw and excpetion if a fault occurs in the service call</param>
        /// <returns>An XML based LegionReply</returns>
        public LegionReply<XmlElement> Call(string method, Dictionary<string, string> methodParams, bool exceptionOnError) {
            LegionReply<XmlElement> reply = null;

            try {
                string sReply = base.GetReply(method, methodParams);
                reply = LegionReply<XmlElement>.BuildLegionXmlReply(sReply);
            }
            catch (Exception e) {
                //optional logging
                throw;
            }

            if (reply.HasFault) {
                if (reply.Fault.Type.StartsWith("ApiKey") || _service.ToLower() == "lumberjack") {
                    //opttional logging
                    throw new Exception(reply.Fault);
                }
                else if (reply.Fault.Type.StartsWith("AuthenticatedUserRequired")) {
                    LegionService.ReissueAuthToken();
                    return Call(method, methodParams, exceptionOnError);
                }
                else
                    throw new LegionServiceException(reply.Fault);
            }
            else if (exceptionOnError && reply.HasError) {
                throw new LegionServiceErrorException(reply.Error);
            }

            return reply;
        }
    }
}
