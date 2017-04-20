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
using System.Data.Linq;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

namespace Caesar {
    public partial class Servers : System.Web.UI.Page {

        internal static void GetServerCache(XmlDocument dom) {
            Uri uri = new Uri(string.Format("http://reference.legion.local/Cache/Dump.aspx", HttpContext.Current.Request.Params["server"]));

            WebClient client = new WebClient();
            string reply = client.DownloadString(uri);

            XmlElement status = (XmlElement)dom.DocumentElement.AppendChild(dom.CreateElement("status"));
            status.AppendChild(dom.CreateCDataSection(reply));
        }

        internal static void RefreshServerCache(XmlDocument dom) {
            Uri uri = new Uri(string.Format("http://reference.legion.local/Cache/Refresh.aspx", HttpContext.Current.Request.Params["server"]));

            WebClient client = new WebClient();
            string reply = client.DownloadString(uri);

            XmlElement status = (XmlElement)dom.DocumentElement.AppendChild(dom.CreateElement("status"));
            status.AppendChild(dom.CreateCDataSection(reply));
        }
    }
}