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
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

using Caesar.Legion;

namespace Caesar {
    public partial class Tester : System.Web.UI.Page {
        internal static void CallMethod(XmlDocument dom) {
            XmlNode root = dom.DocumentElement.AppendChild(dom.CreateElement("reply"));
            try {
                if (HttpContext.Current.Request.Params["service"] != null && HttpContext.Current.Request.Params["method"] != null && HttpContext.Current.Request.Params["apikey"] != null) {
                    LegionXmlService service = new LegionXmlService(HttpContext.Current.Request.Params["service"], HttpContext.Current.Request.Params["apikey"]);

                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    string[] kvp, p = HttpContext.Current.Request.Params["params"].Split(';');
                    foreach (string pair in p) {
                        kvp = pair.Split('=');
                        if (kvp.Length == 2)
                            parameters.Add(HttpContext.Current.Server.UrlDecode(kvp[0]), HttpContext.Current.Server.UrlDecode(kvp[1]));
                    }

                    LegionReply<XmlElement> reply = service.Call(HttpContext.Current.Request.Params["method"], parameters, false);
                    
                    root.AppendChild(dom.CreateElement("result")).InnerText = reply.Result.InnerXml;
                    root.AppendChild(dom.CreateElement("response")).InnerText = reply.Response.InnerXml;

                    if (reply.HasError)
                        root.AppendChild(dom.CreateElement("error")).InnerText = reply.Error;

                    if (reply.HasFault)
                        root.AppendChild(dom.CreateElement("fault")).InnerText = reply.Fault;
                }
                else {
                    root.AppendChild(dom.CreateElement("exception")).InnerText = "service, method and apikey are required";
                }
            }
            catch (Exception e) {
                root.AppendChild(dom.CreateElement("exception")).InnerText = string.Format("{0}\n{1}\n\n{2}", e.GetType().ToString(), e.Message, e.StackTrace);
            }
        }

        public static string HttpServerUrility { get; set; }
    }
}