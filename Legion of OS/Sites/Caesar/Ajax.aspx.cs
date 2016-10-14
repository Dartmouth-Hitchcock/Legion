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
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

using Caesar.Legion;

namespace Caesar {
    public partial class Ajax : System.Web.UI.Page {
        Dictionary<string, AjaxManager.Handler> _handlersAuthenticated = new Dictionary<string, AjaxManager.Handler>() {
                {"getClassManifest", Services.GetClassManifest},
                {"getAssemblies", Services.GetAssemblies},
                {"getAssemblyManifest", Services.GetAssemblyManifest},
                {"importService", Services.ImportService},
                {"callMethod", Tester.CallMethod},
                {"getServerCache", Servers.GetServerCache},
                {"refreshServerCache", Servers.RefreshServerCache},
                {"passthru", Ajax.PassThru}
            };

        private static string[] _authorizedPassThrus = new string[]{
            "deleteApplication",
            "deleteService",
            "deleteServiceMethod",
            "deleteServiceSetting",
            "getServerStatus",
            "getServerCache",
            "getServiceList",
            "getServiceMethods",
            "getNewApplicationKey",
            "getApplicationDetailList",
            "getMethodList",
            "getApplicationPermissions",
            "getRateLimitTypes",
            "getServiceSettings",
            "refreshServerCache",
            "revokeApplicationKey",
            "updateApplication",
            "updateApplicationPermissions",
            "updateService",
            "updateServiceMethod",
            "updateServiceSetting"
        };

        protected void Page_Load(object sender, EventArgs e) {
            //authentication code goes here

            AjaxManager manager = new AjaxManager(Page, _handlersAuthenticated);
            manager.SendReply();
        }


        internal static void PassThru(XmlDocument dom) {
            if (_authorizedPassThrus.Contains(HttpContext.Current.Request.Form["__passthrumethod"])) {
                string passthrumethod = HttpContext.Current.Request.Form["__passthrumethod"];
                Dictionary<string, string> p = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (string key in HttpContext.Current.Request.Form.AllKeys)
                    p.Add(key, HttpContext.Current.Request.Form[key]);

                p.Remove("handler");
                p.Remove("__passthrukey");

                LegionXmlService legion = new LegionXmlService("Caesar", ConfigurationManager.AppSettings["LegionKey"]);
                LegionReply<XmlElement> reply = legion.Call(passthrumethod, p);

                dom.LoadXml(reply.Result.OuterXml);
            }
        }
    }
}