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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

using Caesar.Legion.Extensions;
using Caesar.Legion.ItemContainers;
using System.Web;

namespace Caesar.Legion {

    /// <summary>
    /// Abstract class for calling methods in a Legion Service
    /// </summary>
    abstract public class LegionService {
        private const string LEGION_URL = "http://reference.legion.local";
        private const string USER_IP_KEY = "__legion_userip";
        private const string AUTH_TOKEN_KEY = "__legion_authtoken";

        protected string _service, _apikey, _apiurl;
        protected ReplyFormat _replyformat;

        /// <summary>
        /// The format of the requested Reply
        /// </summary>
        public enum ReplyFormat {
            XML
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service">The Legion Service</param>
        /// <param name="apikey">The API key of the calling application</param>
        /// <param name="replyformat">The format of the Reply</param>
        public LegionService(string service, string apikey, ReplyFormat replyformat)
            : this(service, apikey, LEGION_URL, replyformat) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service">The Legion Service</param>
        /// <param name="apikey">The API key of the calling application</param>
        /// <param name="legionurl">The URL of the Legion Services API</param>
        /// <param name="replyformat">The format of the Reply</param>
        public LegionService(string service, string apikey, string apiurl, ReplyFormat replyformat) {
            _service = service;
            _apikey = apikey;
            _replyformat = replyformat;
            _apiurl = apiurl;
        }

        /// <summary>
        /// Get the sReply from a method in a Legion Service
        /// </summary>
        /// <param name="method">The method to call</param>
        /// <param name="p">The parameters to pass to the method</param>
        /// <returns>the raw reponse from the Service</returns>
        public string GetReply(string method, Dictionary<string, string> p) {
            return LegionService.GetReply(_apiurl, _apikey, _replyformat.ToString(), _service, method, p);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="apikey"></param>
        /// <param name="type"></param>
        /// <param name="identifier"></param>
        /// <param name="email"></param>
        /// <param name="givenname"></param>
        /// <param name="surname"></param>
        /// <returns></returns>
        public static string CreateAuthToken(string apikey, LegionAccountType type, string identifier, string email, string givenname, string middlename, string surname) {
            return CreateAuthToken(apikey, LEGION_URL, type, identifier);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="apikey"></param>
        /// <param name="apiurl"></param>
        /// <param name="type"></param>
        /// <param name="identifier"></param>
        /// <param name="email"></param>
        /// <param name="givenname"></param>
        /// <param name="surname"></param>
        /// <returns></returns>
        public static string CreateAuthToken(string apikey, string apiurl, LegionAccountType type, string identifier) {
            Dictionary<string, string> packet = new Dictionary<string, string>() {
                {"AccountType", type.ToString()},
                {"AccountId", identifier},
                {"__apikey", apikey},
                {"__apiurl", apiurl}
            };

            Dictionary<string, string> parameters = packet.Where(p => !p.Key.StartsWith("__")).ToDictionary(p => p.Key, p => p.Value);

            XmlDocument xReply = LegionService.GetReply(apiurl, apikey, ReplyFormat.XML.ToString(), "__system", "CreateAuthToken", parameters).ToXml();

            XmlNode xToken = xReply.SelectSingleNode("//token");
            if (xToken != null) {
                string token = xToken.InnerText;
                SessionItemContainer.Store(SessionItemContainerKey.AuthPacket, packet);
                return token;
            }
            else {
                XmlNode xFriendly = xReply.SelectSingleNode("//error/description[@type='friendly']");
                if(xFriendly != null)
                    throw new LegionServiceException(string.Format("{0} ({1}:{2})", xFriendly.InnerText, type, identifier));
                else {
                    XmlNode xError = xReply.SelectSingleNode("//error");
                    if(xError != null)
                        throw new LegionServiceException(string.Format("{0} ({1}:{2})", xFriendly.InnerText, type, identifier));
                    else {
                        XmlNode xFault = xReply.SelectSingleNode("//fault");
                        if(xFault != null)
                            throw new LegionServiceException(string.Format("{0} ({1})", xFault.InnerText, xFault.Attributes["type"].Value));
                        else
                            throw new LegionServiceException("Unknown error. Unable to parse CreateAuthToken() response.");
                    }
                }
            }
        }

        public static bool ReissueAuthToken() {
            Dictionary<string, string> packet = SessionItemContainer.Retrieve(SessionItemContainerKey.AuthPacket) as Dictionary<string,string>;
            if (packet != null) {
                string apiurl = packet["__apiurl"];
                string apikey = packet["__apikey"];
                Dictionary<string, string> parameters = packet.Where(p => !p.Key.StartsWith("__")).ToDictionary(p => p.Key, p => p.Value);

                XmlNode xToken = LegionService.GetReply(apiurl, apikey, ReplyFormat.XML.ToString(), "__system", "CreateAuthToken", parameters).ToXml().SelectSingleNode("//token");
                if (xToken != null) {
                    string token = xToken.InnerText;
                    SessionItemContainer.Store(SessionItemContainerKey.AuthPacket, packet);
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public static bool HeartbeatAuthToken(string apikey, string token) {
            string tokenheartbeat = LegionService.GetReply(LEGION_URL, apikey, ReplyFormat.XML.ToString(), "__system", "HeartbeatAuthToken", new Dictionary<string, string>() {
                {"token", token}
            }).ToXml().SelectSingleNode("//tokenheartbeat").InnerText;

            return (tokenheartbeat.ToLower() == "Success");
        }

        public static void DestroyAuthToken(string apikey, string token) {
            LegionService.GetReply(LEGION_URL, apikey, ReplyFormat.XML.ToString(), "__system", "DestroyAuthToken", new Dictionary<string, string>() {
                {"token", token}
            });
        }

        private static string GetReply(string apiurl, string apikey, string replyformat, string service, string method, Dictionary<string, string> p) {
            if (apiurl == null)
                apiurl = LEGION_URL;

            Dictionary<string, string> pToPass;
            if (p == null)
                pToPass = new Dictionary<string, string>();
            else if (p.ContainsKey(USER_IP_KEY))
                throw new Exception(string.Format("Reserved parameter '{0}' found in parameter set", USER_IP_KEY));
            else
                pToPass = p.Clone();

            string authtoken = SessionItemContainer.Retrieve(SessionItemContainerKey.AuthToken) as string;
            if (authtoken != null)
                pToPass.Add(AUTH_TOKEN_KEY, authtoken);

            pToPass.Add(USER_IP_KEY, HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]);

            Uri uri = new Uri(string.Format("{0}/{1}/{2}/{3}/?k={4}", apiurl, service, method, replyformat, apikey));

            WebClient client = new WebClient();
            client.Headers["Content-type"] = "application/x-www-form-urlencoded";
            string reply = client.UploadString(uri, pToPass.Flatten());

            return reply;
        }
    }
}
