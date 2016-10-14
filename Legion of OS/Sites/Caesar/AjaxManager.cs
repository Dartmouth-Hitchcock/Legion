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
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

namespace Caesar {

    /// <summary>
    /// Manages Ajax requests between the client JS and the server .NET
    /// Note: This class assumes the existence of a variable named 'handler' in the page request
    /// </summary>
    public class AjaxManager {
        /// <summary>
        /// Default handler delagate for construction new Ajax handlers
        /// </summary>
        /// <param name="xResponse">A handle to the XML DOM to be be returned</param>
        public delegate void Handler(XmlDocument xResponse);

        private Dictionary<string, Handler> _handlers;
        private Page _page;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The caller's Page object</param>
        public AjaxManager(Page page) : this(page, new Dictionary<string, Handler>()) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The caller's Page object</param>
        /// <param name="handlers">The request handlers</param>
        public AjaxManager(Page page, Dictionary<string, Handler> handlers) {
            _page = page;
            _handlers = handlers;

            SetContentType(_page.Response);
        }

        /// <summary>
        /// Sends the handler's response to the client
        /// </summary>
        public void SendReply(){
            _page.Response.Clear();
            _page.Response.Write(GetReply());
        }

        /// <summary>
        /// Gets the reply from the requested handler
        /// </summary>
        /// <returns>A string representation of the XML reply</returns>
        public string GetReply() {
            XmlDocument xReply = new XmlDocument();
            XmlElement root;
            string handlerId = _page.Request["handler"];

            xReply.AppendChild(xReply.CreateNode(XmlNodeType.XmlDeclaration, "", ""));
            xReply.AppendChild((root = xReply.CreateElement("", "response", "")));

            if (!string.IsNullOrEmpty(handlerId) && _handlers.ContainsKey(handlerId)) {
                Handler handler = _handlers[handlerId];

                handler(xReply);
            }
            else {
                LogError(xReply, "Handler '" + handlerId + "' not found.", "Request for handler '" + handlerId + "': not found.", _page.Request);
            }

            return xReply.OuterXml;
        }

        /// <summary>
        /// Sets the reponse's content type to handle XML
        /// </summary>
        /// <param name="response">The page response</param>
        private void SetContentType(HttpResponse response) {
            response.ContentType = "text/xml";
        }

        /// <summary>
        /// Logs an error to file and to the client
        /// </summary>
        /// <param name="document">The document to place the reponse in</param>
        /// <param name="errorClient">The error to send to the client</param>
        /// <param name="errorLog">The detailed error to log to file</param>
        /// <param name="request">The current http request</param>
        private void LogError(XmlDocument document, string errorClient, string errorLog, HttpRequest request) {
            XmlElement error = document.CreateElement("error");
            error.InnerText = errorClient;
            document.DocumentElement.AppendChild(error);

            string referrer = (request.UrlReferrer == null ? "unknown" : request.UrlReferrer.AbsoluteUri);
            
            //log error here
        }
    }
}
