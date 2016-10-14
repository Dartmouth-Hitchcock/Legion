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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

using Caesar.Legion;

namespace Caesar {
    public partial class Services : System.Web.UI.Page {
        internal static void GetAssemblies(XmlDocument dom) {
            DirectoryInfo dir;
            Assembly assembly;
            string assemblyDirectory;

            XmlElement assemblies = (XmlElement)dom.DocumentElement.AppendChild(dom.CreateElement("assemblies"));

            LegionXmlService service = new LegionXmlService("Caesar", ConfigurationManager.AppSettings["LegionKey"]);
            LegionReply<XmlElement> reply = service.Call("GetAssemblyDirectory");

            if(!reply.HasFault && ! reply.HasError){
                assemblyDirectory = reply.Result.SelectSingleNode("assemblydirectory").InnerText;
                dir = new DirectoryInfo(assemblyDirectory);

                foreach (FileInfo file in dir.GetFiles("*.dll")) {
                    assembly = Assembly.LoadFile(file.FullName);
                    assemblies.AppendChild(dom.CreateElement("assembly")).InnerText = assembly.GetName().Name;
                }
            }
        }

        internal static void GetAssemblyManifest(XmlDocument dom) {
            string assemblyName = HttpContext.Current.Request.Form["assembly"];
            string assemblyDirectory;

            XmlElement classes = (XmlElement)dom.DocumentElement.AppendChild(dom.CreateElement("classes"));

            LegionXmlService service = new LegionXmlService("Caesar", ConfigurationManager.AppSettings["LegionKey"]);
            LegionReply<XmlElement> reply = service.Call("GetAssemblyDirectory");

            if (!reply.HasFault && !reply.HasError) {
                assemblyDirectory = reply.Result.SelectSingleNode("assemblydirectory").InnerText;

                try {
                    Assembly assembly = Assembly.LoadFile(string.Format(@"{0}\{1}.dll", assemblyDirectory, assemblyName));
                    Type[] types = assembly.GetTypes();

                    foreach (Type type in types) {
                        classes.AppendChild(dom.CreateElement("class")).InnerText = type.Name;
                    }
                }
                catch (System.IO.FileNotFoundException e) {
                    //LegionException lex = new LegionException(string.Format("Assembly '{0}' not found", result.AssemblyName), e, "AssemblyNotFound");
                }
            }
        }

        internal static void GetClassManifest(XmlDocument dom) {
            string assemblyName = HttpContext.Current.Request.Form["assembly"];
            string className = HttpContext.Current.Request.Form["class"];
            string assemblyDirectory;

            XmlElement methods = (XmlElement)dom.DocumentElement.AppendChild(dom.CreateElement("methods"));


            LegionXmlService service = new LegionXmlService("Caesar", ConfigurationManager.AppSettings["LegionKey"]);
            LegionReply<XmlElement> reply = service.Call("GetAssemblyDirectory");

            if (!reply.HasFault && !reply.HasError) {
                assemblyDirectory = reply.Result.SelectSingleNode("assemblydirectory").InnerText;

                try {
                    Assembly assembly = Assembly.LoadFile(string.Format(@"{0}\{1}.dll", assemblyDirectory, assemblyName));
                    Type t = assembly.GetType(assemblyName + "." + className);

                    SortedDictionary<string, MethodInfo> classMethods = new SortedDictionary<string,MethodInfo>(t.GetMethods(BindingFlags.Public | BindingFlags.Static).ToDictionary(m => m.Name, m => m));
                    foreach (KeyValuePair<string, MethodInfo> method in classMethods) {
                        methods.AppendChild(dom.CreateElement("method")).InnerText = method.Value.Name;
                    }
                }
                catch (System.IO.FileNotFoundException e) {
                    //LegionException lex = new LegionException(string.Format("Assembly '{0}' not found", result.AssemblyName), e, "AssemblyNotFound");
                }
            }
        }

        internal static void ImportService(XmlDocument dom) {
            LegionXmlService caesar = new LegionXmlService("Caesar", ConfigurationManager.AppSettings["LegionKey"]);
            ServiceHandler.Initialize(ConfigurationManager.AppSettings["LegionKey"]);

            string[] elements = HttpContext.Current.Request["elements"].Split(';');
            string id = HttpContext.Current.Request["id"];

            XmlDocument import = new XmlDocument();
            import.LoadXml(HttpContext.Current.Request.Params["dom"]);

            string servicekey = null, assembly = null, interfaceclass = null, description = null, consumeriprange = null, islogged = null, isrestricted = null, ispublic = null;

            if (elements.Contains("servicekey"))
                servicekey = GetNodeText(import, "servicekey");

            if (elements.Contains("assembly"))
                assembly = GetNodeText(import, "assembly");

            if (elements.Contains("interfaceclass"))
                interfaceclass = GetNodeText(import, "interfaceclass");

            if (elements.Contains("description"))
                description = GetNodeText(import, "description");

            if (elements.Contains("consumeriprange"))
                consumeriprange = GetNodeText(import, "consumeriprange");

            if (elements.Contains("flags")) {
                islogged = GetNodeText(import, "logged");
                isrestricted = GetNodeText(import, "restricted");
                ispublic = GetNodeText(import, "public");
            }

            if (id == null) {
                dom.DocumentElement.AppendChild(dom.CreateElement("error")).InnerText = "bad id";
                return;
            }

            if (servicekey == null || assembly == null || interfaceclass == null) {
                if (id == "-1") {
                    dom.DocumentElement.AppendChild(dom.CreateElement("error")).InnerText = "unable to create service, missing required elements";
                    return;
                }
                else {
                    LegionReply<XmlElement> current = caesar.Call("getService", new Dictionary<string, string>(){
                        {"serviceid", id}
                    });

                    if (servicekey == null)
                        servicekey = current.Result.SelectSingleNode("./servicekey").InnerText;
                    if(assembly == null)
                        assembly = current.Result.SelectSingleNode("./assemblyname").InnerText;
                    if (interfaceclass == null)
                        interfaceclass = current.Result.SelectSingleNode("./classname").InnerText;
                }
            }

            LegionReply<XmlElement> service = caesar.Call("updateService", new Dictionary<string, string>() {
                {"id", id},
                {"servicekey", servicekey},
                {"description", description},
                {"consumeriprange", consumeriprange},
                {"assembly", assembly},
                {"class", interfaceclass},
                {"logged", islogged},
                {"restricted", isrestricted},
                {"public", ispublic}
            });

            if (id == "-1")
                id = service.Result.SelectSingleNode("//id").InnerText;

            if (elements.Contains("settings")) {
                foreach (XmlNode setting in import.SelectNodes("//setting")) {
                    caesar.Call("updateServiceSetting", new Dictionary<string, string>() {
                        {"serviceid", id},
                        {"id", "-1"},
                        {"name", setting.SelectSingleNode("./name").InnerText},
                        {"value", setting.SelectSingleNode("./value").InnerText},
                        {"encrypted", setting.SelectSingleNode("./encrypted").InnerText}
                    });
                }
            }
            
            if (elements.Contains("methods")) {
                foreach (XmlNode method in import.SelectNodes("//method")) {
                    caesar.Call("updateServiceMethod", new Dictionary<string, string>() {
                        {"serviceid", id},
                        {"id", "-1"},
                        {"methodkey", method.SelectSingleNode("./key").InnerText},
                        {"methodname", method.SelectSingleNode("./name").InnerText},
                        {"cachedresultlifetime", method.SelectSingleNode("./cachedresultlifetime").InnerText},
                        {"cacheresult", method.SelectSingleNode("./cacheresult").InnerText},
                        {"restricted", method.SelectSingleNode("./restricted").InnerText},
                        {"logged", method.SelectSingleNode("./logged").InnerText},
                        {"public", method.SelectSingleNode("./public").InnerText}
                    });
                }
            }

            dom.DocumentElement.AppendChild(dom.CreateElement("serviceid")).InnerText = id;
            dom.DocumentElement.AppendChild(dom.CreateElement("resultcode")).InnerText = "SUCCESS";
        }

        internal static string GetNodeText(XmlDocument dom, string name) {
            XmlElement node = (XmlElement)dom.SelectSingleNode("//" + name);
            if (node != null)
                return node.InnerText;
            else
                return null;
        }

        protected void Page_Load(object sender, EventArgs e) {
            if (Request["exportservice"] == "true" && Request["serviceid"] != null && Request["elements"] != null) {
                string serviceid = Request["serviceid"];
                string[] elements = Request["elements"].Split(';');

                LegionXmlService caesar = new LegionXmlService("Caesar", ConfigurationManager.AppSettings["LegionKey"]);

                XmlDocument dom = new XmlDocument();
                XmlElement root = (XmlElement)dom.AppendChild(dom.CreateElement("service"));

                LegionReply<XmlElement> service = caesar.Call("getService", new Dictionary<string, string>(){
                    {"serviceid", serviceid}
                });

                DateTime exporttime = DateTime.Now;
                string servicekey = service.Result.SelectSingleNode("./servicekey").InnerText.Trim();
                string filename = string.Format("{0}-{1:yyyyMMddHHmmss}.legion", servicekey, exporttime);

                root.SetAttribute("exporttime", string.Format("{0:yyyy-MM-dd HH:mm:ss}", exporttime));
                if (elements.Contains("servicekey"))
                    root.AppendChild(dom.CreateElement("servicekey")).InnerText = servicekey;

                if (elements.Contains("assembly"))
                    root.AppendChild(dom.CreateElement("assembly")).InnerText = service.Result.SelectSingleNode("./assemblyname").InnerText.Trim();

                if (elements.Contains("interfaceclass"))
                    root.AppendChild(dom.CreateElement("interfaceclass")).InnerText = service.Result.SelectSingleNode("./classname").InnerText.Trim();

                if (elements.Contains("description"))
                    root.AppendChild(dom.CreateElement("description")).AppendChild(dom.CreateCDataSection(service.Result.SelectSingleNode("./description").InnerText.Trim()));

                if (elements.Contains("consumeriprange"))
                    root.AppendChild(dom.CreateElement("consumeriprange")).InnerText = service.Result.SelectSingleNode("./consumeriprange").InnerText.Trim();

                if (elements.Contains("flags")) {
                    root.AppendChild(dom.CreateElement("logged")).InnerText = service.Result.SelectSingleNode("./logged").InnerText.Trim();
                    root.AppendChild(dom.CreateElement("restricted")).InnerText = service.Result.SelectSingleNode("./restricted").InnerText.Trim();
                    root.AppendChild(dom.CreateElement("public")).InnerText = service.Result.SelectSingleNode("./public").InnerText.Trim();
                }

                if (elements.Contains("settings")) {
                    LegionReply<XmlElement> serviceSettings = caesar.Call("getServiceSettings", new Dictionary<string, string>(){
                        {"id", serviceid}
                    });

                    int count = 0;
                    XmlElement setting, settings = (XmlElement)root.AppendChild(dom.CreateElement("settings"));
                    foreach(XmlElement x in serviceSettings.Result.SelectNodes("//settings/setting")){
                        setting = (XmlElement)settings.AppendChild(dom.CreateElement("setting"));
                        setting.AppendChild(dom.CreateElement("name")).InnerText = x.SelectSingleNode("./name").InnerText.Trim();
                        setting.AppendChild(dom.CreateElement("value")).AppendChild(dom.CreateCDataSection(x.SelectSingleNode("./value").InnerText.Trim()));
                        setting.AppendChild(dom.CreateElement("encrypted")).InnerText = x.SelectSingleNode("./encrypted").InnerText.Trim();

                        count++;
                    }

                    settings.SetAttribute("count", count.ToString());
                }

                if (elements.Contains("methods")) {
                    LegionReply<XmlElement> serviceMethods = caesar.Call("getServiceMethods", new Dictionary<string, string>(){
                        {"serviceId", serviceid}
                    });

                    int count = 0;
                    XmlElement method, methods = (XmlElement)root.AppendChild(dom.CreateElement("methods"));
                    foreach (XmlElement x in serviceMethods.Result.SelectNodes("//methods/method")) {
                        method = (XmlElement)methods.AppendChild(dom.CreateElement("method"));
                        method.AppendChild(dom.CreateElement("key")).InnerText = x.SelectSingleNode("./key").InnerText.Trim();
                        method.AppendChild(dom.CreateElement("name")).InnerText = x.SelectSingleNode("./name").InnerText.Trim();
                        method.AppendChild(dom.CreateElement("cachedresultlifetime")).InnerText = x.SelectSingleNode("./cachedresultlifetime").InnerText.Trim();
                        method.AppendChild(dom.CreateElement("cacheresult")).InnerText = x.SelectSingleNode("./cacheresult").InnerText.Trim();
                        method.AppendChild(dom.CreateElement("restricted")).InnerText = x.SelectSingleNode("./restricted").InnerText.Trim();
                        method.AppendChild(dom.CreateElement("logged")).InnerText = x.SelectSingleNode("./logged").InnerText.Trim();
                        method.AppendChild(dom.CreateElement("public")).InnerText = x.SelectSingleNode("./logged").InnerText.Trim();
                        count++;
                    }

                    methods.SetAttribute("count", count.ToString());
                }

                Response.Clear();
                Response.AddHeader("content-disposition", "attachment;filename=" + filename);
                Response.ContentType = "text/xml";
                Response.Write(dom.OuterXml);
                Response.End();
            }
        }
    }
}