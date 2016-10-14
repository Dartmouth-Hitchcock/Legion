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
using System.Xml;

using Caesar.Legion.ItemContainers;

namespace Caesar.Legion {
    public static class ServiceHandler {

        /// <summary>
        /// Is the wrapper initialized
        /// </summary>
        public static bool IsInitialized {
            get {
                object apikey = ApplicationItemContainer.Retrieve(ApplicationItemContainerKey.ApiKey);
                return (apikey == null ? false : true);  
            }
        }

        /// <summary>
        /// Is an exception thrown on a service error
        /// </summary>
        public static bool ExceptionOnError {
            get {
                object exceptionOnError = ApplicationItemContainer.Retrieve(ApplicationItemContainerKey.ExceptionOnError);
                return (exceptionOnError == null ? true : (bool)exceptionOnError);
            }
            set { ApplicationItemContainer.Store(ApplicationItemContainerKey.ExceptionOnError, value); }
        }

        /// <summary>
        /// The API key used by this wrapper
        /// </summary>
        public static string APIKey {
            get {
                object apikey = ApplicationItemContainer.Retrieve(ApplicationItemContainerKey.ApiKey);
                if (apikey != null)
                    return apikey.ToString();
                else
                    throw new Exception("Legion Service Wrapper API key not set");
            }
            set {
                ApplicationItemContainer.Store(ApplicationItemContainerKey.ApiKey, value);
            }
        }

        /// <summary>
        /// The URL of the API
        /// </summary>
        public static string APIURL {
            get {
                object apiurl = ApplicationItemContainer.Retrieve(ApplicationItemContainerKey.ApiUrl);
                return (apiurl == null ? null : apiurl.ToString());
            }
            set { ApplicationItemContainer.Store(ApplicationItemContainerKey.ApiUrl, value); }
        }

        /// <summary>
        /// The Legion Auth Token for this session if one has been created.
        /// </summary>
        public static string AuthToken {
            get {
                object authtoken = SessionItemContainer.Retrieve(SessionItemContainerKey.AuthToken);
                return (authtoken == null ? null : authtoken.ToString());
            }
            set { SessionItemContainer.Store(SessionItemContainerKey.AuthToken, value); }
        }

        /// <summary>
        /// Initialize the base wrapper 
        /// </summary>
        /// <param name="apikey">The API key of the application</param>
        public static void Initialize(string apikey) {
            Initialize(apikey, null);
        }

        /// <summary>
        /// Initialize the base wrapper 
        /// </summary>
        /// <param name="apikey">The API key of the application</param>
        /// <param name="apiurl">The URL of the API</param>
        public static void Initialize(string apikey, string apiurl) {
            string appDomainName = System.AppDomain.CurrentDomain.FriendlyName.ToLower();
            if (appDomainName.Contains("w3scv") && appDomainName.Contains("legion"))
                throw new Exception("Use of ServiceHandler not allwed in Legion context.");

            APIKey = apikey;
            APIURL = apiurl;
            ExceptionOnError = true;
        }

        /// <summary>
        /// Cals a service using the wrapper's API key
        /// </summary>
        /// <param name="service">The name of the service to call</param>
        /// <param name="method">The name of the method to call</param>
        /// <returns>An XML based LegionReply</returns>
        public static LegionReply<XmlElement> CallService(string service, string method) {
            return CallService(service, method, null);
        }

        /// <summary>
        /// Cals a service using the wrapper's API key
        /// </summary>
        /// <param name="service">The name of the service to call</param>
        /// <param name="method">The name of the method to call</param>
        /// <param name="exceptionOnError">throw an exception on service error</param>
        /// <returns>An XML based LegionReply</returns>
        public static LegionReply<XmlElement> CallService(string service, string method, bool exceptionOnError) {
            return CallService(service, method, null, exceptionOnError);
        }

        /// <summary>
        /// Calls a service using the wrapper's API key
        /// </summary>
        /// <param name="service">The name of the service to call</param>
        /// <param name="method">The name of the method to call</param>
        /// <param name="parameters">The parameters to pass to the method</param>
        /// <returns>An XML based LegionReply</returns>
        public static LegionReply<XmlElement> CallService(string service, string method, Dictionary<string, string> parameters) {
            return CallService(service, method, parameters, ExceptionOnError);
        }

        /// <summary>
        /// Calls a service using the wrapper's API key
        /// </summary>
        /// <param name="service">The name of the service to call</param>
        /// <param name="method">The name of the method to call</param>
        /// <param name="parameters">The parameters to pass to the method</param>
        /// <param name="exceptionOnError">throw an exception on service error</param>
        /// <returns>An XML based LegionReply</returns>
        public static LegionReply<XmlElement> CallService(string service, string method, Dictionary<string, string> parameters, bool exceptionOnError) {
            if (!IsInitialized)
                throw new Exception("Legion Service Wrapper not initialized");

            LegionXmlService legionService;
            if (APIURL == null)
                legionService = new LegionXmlService(service, APIKey);
            else
                legionService = new LegionXmlService(service, APIKey, APIURL);

            return legionService.Call(method, parameters, exceptionOnError);
        }

        public static string CreateAuthToken(LegionAccountType type, string identifier) {
            AuthToken = LegionService.CreateAuthToken(APIKey, APIURL, type, identifier);
            return AuthToken;
        }

        public static bool HeartbeatAuthToken() {
            return LegionService.HeartbeatAuthToken(APIKey, AuthToken);
        }

        public static void DestoryAuthToken() {
            LegionService.DestroyAuthToken(APIKey, AuthToken);
            AuthToken = null;
        }

        private static void CacheThings(string key, string value) {
            HttpContext.Current.Application[key] = value;
        }
    }
}
