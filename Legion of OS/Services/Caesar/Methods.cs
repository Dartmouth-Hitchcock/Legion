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
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Xml;

using Legion.Core.Services;
using Mjolnir.Encryption;
using Mjolnir.Extensions;

namespace Caesar {
    public class Methods {
        internal struct VerificationGroups {
            public static ParameterVerificationGroup ExpireResultCache = new ParameterVerificationGroup(new string[] { "Service", "Method" });
        }

        #region Settings

        public static void GetAssemblyDirectory(Request request, ReplyNode result, ErrorNode error) {
            string assemblyDirectory = null;

            using (LegionLinqDataContext db = new LegionLinqDataContext(request.Service.Settings["LegionConnectionString"])) {
                db.xspGetSettingByKey("AssemblyDirectory", ref assemblyDirectory);
                result.AddElement("assemblydirectory", assemblyDirectory);
            }
        }

        #endregion

        #region Applications

        public static void GetRateLimitTypes(Request request, ReplyNode result, ErrorNode error) {
            using (LegionLinqDataContext db = new LegionLinqDataContext(request.Service.Settings["LegionConnectionString"])) {
                XmlElement xRateType, xRateTypes = result.AddElement("ratelimittypes");
                ISingleResult<xspGetRateLimitTypesResult> rates = db.xspGetRateLimitTypes();
                foreach (xspGetRateLimitTypesResult rate in rates) {
                    xRateType = result.AddElement(xRateTypes, "ratelimittype");
                    xRateType.SetAttribute("id", rate.Id.ToString());
                    xRateType.InnerText = rate.Name;
                }
            }
        }

        public static void RevokeApplicationKey(Request request, ReplyNode result, ErrorNode error){
            int? applicationid = null;
            string key = null;
            string resultcode = null;

            if (request.ParameterSet.Verify(new ParameterVerificationGroup("id", ParameterType.Int)))
                applicationid = request.ParameterSet.GetInt("id");
            else if (request.ParameterSet.ContainsKey("key"))
                key = request.ParameterSet["key"];

            using (LegionLinqDataContext db = new LegionLinqDataContext(request.Service.Settings["LegionConnectionString"])) {
                db.xspRevokeKey(applicationid, key, resultcode);

                if (resultcode == "SUCCESS")
                    result.AddElement("result", resultcode);
                else
                    error.Text = resultcode;
            }
        }

        public static void GetNewApplicationKey(Request request, ReplyNode result, ErrorNode error) {
            string appKey = null;
            bool? collision = true;

            using (LegionLinqDataContext db = new LegionLinqDataContext(request.Service.Settings["LegionConnectionString"])) {
                while (collision != false) {
                    appKey = Guid.NewGuid().ToString().ToUpper();
                    db.xspCheckApplicationKeyForCollision(appKey, ref collision);
                }
            }

            result.AddElement("applicationkey", appKey);
        }

        public static void GetApplicationList(Request request, ReplyNode result, ErrorNode error) {
            XmlElement applications = result.AddElement("applications");
            using (LegionLinqDataContext db = new LegionLinqDataContext(request.Service.Settings["LegionConnectionString"])) {
                ISingleResult<xspGetApplicationListResult> results = db.xspGetApplicationList();

                XmlElement application;
                foreach (xspGetApplicationListResult r in results) {
                    application = result.AddElement(applications, "application");
                    result.AddElement(application, "id", r.ApplicationId.ToString());
                    result.AddElement(application, "name", r.ApplicationName);
                }
            }
        }

        public static void GetApplicationDetailList(Request request, ReplyNode result, ErrorNode error) {
            using (LegionLinqDataContext db = new LegionLinqDataContext(request.Service.Settings["LegionConnectionString"])) {
                ISingleResult<xspGetApplicationListResult> applications = db.xspGetApplicationList();

                XmlElement xApplication, xApplications = result.AddElement("applications");
                foreach (xspGetApplicationListResult applciation in applications) {
                    xApplication = result.AddElement(xApplications, "application");
                    result.AddElement(xApplication, "id", applciation.ApplicationId.ToString());
                    result.AddElement(xApplication, "key", applciation.ApplicationKey);
                    result.AddElement(xApplication, "name", applciation.ApplicationName);
                    result.AddElement(xApplication, "consumeriprange", applciation.ConsumerIPRange);
                    result.AddElement(xApplication, "description", applciation.Description);
                    result.AddElement(xApplication, "ratelimittypeid", applciation.RateLimitTypeId.ToString());
                    result.AddElement(xApplication, "ratelimittype", applciation.RateLimitType);
                    result.AddElement(xApplication, "ratelimit", applciation.RateLimit.ToString());
                    result.AddElement(xApplication, "ratelimitinterval", applciation.RateLimitInterval.ToString());
                    result.AddElement(xApplication, "public", (applciation.IsPublic ? "true" : "false"));
                    result.AddElement(xApplication, "logged", (applciation.IsLogged ? "true" : "false"));
                }
            }
        }

        public static void UpdateApplication(Request request, ReplyNode result, ErrorNode error) {
            string sId = request.ParameterSet["id"],
                    name = request.ParameterSet["name"],
                    key = request.ParameterSet["appkey"],
                    range = (request.ParameterSet["range"].Length == 0 ? null : request.ParameterSet["range"]),
                    description = request.ParameterSet["description"],
                    resultcode = null;
            bool? ispublic = (request.ParameterSet["public"] == "true" ? true : false);
            bool? islogged = (request.ParameterSet["logged"] == "true" ? true : false);

            int? ratelimitid = (request.ParameterSet["ratelimittypeid"].Length > 0 ? (int?)int.Parse(request.ParameterSet["ratelimittypeid"]) : null);
            int? ratelimit = (request.ParameterSet["ratelimittypeid"].Length > 0 ? (int?)int.Parse(request.ParameterSet["ratelimit"]) : null);
            int? ratelimitinterval = (request.ParameterSet["ratelimittypeid"].Length > 0 ? (int?)int.Parse(request.ParameterSet["ratelimitinterval"]) : null);

            int id;
            int? nId;

            if (Int32.TryParse(sId, out id)) {
                nId = id;
                using (LegionLinqDataContext db = new LegionLinqDataContext(request.Service.Settings["LegionConnectionString"])) {
                    db.xspUpdateApplication(ref nId, name, key, range, description, ratelimitid, ratelimit, ratelimitinterval, ispublic, islogged, ref resultcode);
                }

                XmlElement r = result.AddElement("result");
                result.AddElement(r, "resultcode", resultcode);

                if (sId == "-1")
                    result.AddElement(r, "id", nId.ToString());
            }
            else
                error.Text = "not a valid id";
        }

        public static void DeleteApplication(Request request, ReplyNode result, ErrorNode error) {
            string sId = request.ParameterSet["id"],
                    resultcode = null;
            int id;
            int? nId;

            if (Int32.TryParse(sId, out id)) {
                nId = id;
                using (LegionLinqDataContext db = new LegionLinqDataContext(request.Service.Settings["LegionConnectionString"])) {
                    db.xspDeleteApplication(nId, ref resultcode);
                }

                XmlElement r = result.AddElement("result");
                result.AddElement(r, "resultcode", resultcode);
            }
            else
                error.Text = "not a valid id";
        }

        public static void GetMethodList(Request request, ReplyNode result, ErrorNode error) {
            XmlElement methods = result.AddElement("methods");
            using (LegionLinqDataContext db = new LegionLinqDataContext(request.Service.Settings["LegionConnectionString"])) {
                ISingleResult<xspGetMethodsResult> results = db.xspGetMethods();

                XmlElement method;
                foreach (xspGetMethodsResult r in results) {
                    method = result.AddElement(methods, "method");
                    result.AddElement(method, "id", r.MethodId.ToString());
                    result.AddElement(method, "key", r.MethodKey);
                    result.AddElement(method, "name", r.MethodName);
                    result.AddElement(method, "public", (r.IsPublic ? "true" : "false"));
                    result.AddElement(method, "restricted", (r.IsRestricted == true ? "true" : "false"));
                }
            }
        }

        public static void UpdateApplicationPermissions(Request request, ReplyNode result, ErrorNode error) {
            string[] permissions = (request.ParameterSet["permissions"] == null ? null : request.ParameterSet["permissions"].Split(';'));
            int? applicationId, methodId;
            int temp;
            string resultcode = "SUCCESS";

            if (Int32.TryParse(request.ParameterSet["applicationId"], out temp)){
                if (permissions == null)
                    result.AddElement("resultcode", "SUCCESS");
                else {
                    applicationId = temp;
                    using (LegionLinqDataContext db = new LegionLinqDataContext(request.Service.Settings["LegionConnectionString"])) {
                        db.xspDeleteAllApplciationPermissions(applicationId);

                        foreach (string p in permissions) {
                            if (p.Length > 1) {
                                if (Int32.TryParse(p, out temp)) {
                                    methodId = temp;
                                    db.xspAddApplicationPermission(applicationId, methodId, ref resultcode);
                                    if (resultcode != "SUCCESS")
                                        break;
                                }
                                else {
                                    error.Text = string.Format("'{0}' not a valid id", p);
                                    break;
                                }
                            }
                        }

                        result.AddElement("resultcode", resultcode);
                    }
                }
            }
        }

        public static void GetApplicationPermissions(Request request, ReplyNode result, ErrorNode error) {
            int? applicationId;
            int temp;

            if (Int32.TryParse(request.ParameterSet["applicationId"], out temp)) {
                applicationId = temp;

                XmlElement permissions = result.AddElement("permissions");
                using (LegionLinqDataContext db = new LegionLinqDataContext(request.Service.Settings["LegionConnectionString"])) {
                    ISingleResult<xspGetApplicationPermissionsResult> results = db.xspGetApplicationPermissions(applicationId);

                    XmlElement permission;
                    foreach (xspGetApplicationPermissionsResult r in results) {
                        permission = result.AddElement(permissions, "permission");
                        result.AddElement(permission, "id", r.MethodId.ToString());
                    }
                }
            }
        }

        #endregion

        #region Services

        public static void GetService(Request request, ReplyNode result, ErrorNode error) {
            if (request.ParameterSet.ContainsKey("serviceid")) {
                int serviceid = request.ParameterSet.GetInt("serviceid");
                string resultcode = null;

                using (LegionLinqDataContext db = new LegionLinqDataContext(request.Service.Settings["LegionConnectionString"])) {
                    ISingleResult<xspGetServiceResult> results = db.xspGetService(serviceid, ref resultcode);

                    foreach (xspGetServiceResult service in results) {
                        result.AddElement("assemblyname", service.AssemblyName);
                        result.AddElement("classname", service.ClassName);
                        result.AddElement("consumeriprange", service.ConsumerIPRange);
                        result.AddElement("description", service.Description);
                        result.AddElement("servicekey", service.ServiceKey);
                        result.AddElement("logged", (service.IsLogged ? "true" : "false"));
                        result.AddElement("public", (service.IsPublic ? "true" : "false"));
                        result.AddElement("restricted", (service.IsRestricted ? "true" : "false"));
                    }

                    result.AddElement("resultcode", resultcode);
                }
            }
            else
                error.Text = "No service specified.";
        }

        public static void GetServiceList(Request request, ReplyNode result, ErrorNode error) {
            XmlElement services = result.AddElement("services");
            using (LegionLinqDataContext db = new LegionLinqDataContext(request.Service.Settings["LegionConnectionString"])) {
                ISingleResult<xspGetServicesResult> results = db.xspGetServices();

                XmlElement service;
                foreach (xspGetServicesResult r in results) {
                    service = result.AddElement(services, "service");
                    result.AddElement(service, "id", r.ServiceId.ToString());
                    result.AddElement(service, "key", r.ServiceKey);
                    result.AddElement(service, "assembly", r.AssemblyName);
                    result.AddElement(service, "class", r.ClassName);
                    result.AddElement(service, "restricted", (r.IsRestricted ? "true" : "false"));
                    result.AddElement(service, "public", (r.IsPublic ? "true" : "false"));
                    result.AddElement(service, "logged", (r.IsLogged ? "true" : "false"));
                    result.AddElement(service, "description", r.Description);
                    result.AddElement(service, "consumeriprange", r.ConsumerIPRange);
                }
            }
        }

        public static void GetServiceMethods(Request request, ReplyNode result, ErrorNode error) {
            int serviceId;
            if (Int32.TryParse(request.ParameterSet["serviceId"], out serviceId)) {
                using (LegionLinqDataContext db = new LegionLinqDataContext(request.Service.Settings["LegionConnectionString"])) {
                    ISingleResult<xspGetServiceMethodsResult> results = db.xspGetServiceMethods(serviceId);
                    XmlElement methods = result.AddElement("methods");

                    XmlElement method;
                    foreach (xspGetServiceMethodsResult r in results) {
                        method = result.AddElement(methods, "method");
                        result.AddElement(method, "id", r.MethodId.ToString());
                        result.AddElement(method, "serviceid", r.ServiceId.ToString());
                        result.AddElement(method, "key", r.MethodKey);
                        result.AddElement(method, "name", r.MethodName);
                        result.AddElement(method, "cachedresultlifetime", r.CachedResultLifetime.ToString());
                        result.AddElement(method, "cacheresult", (r.IsResultCacheable ? "true" : "false"));
                        result.AddElement(method, "restricted", (r.IsRestricted ? "true" : "false"));
                        result.AddElement(method, "public", (r.IsPublic ? "true" : "false"));
                        result.AddElement(method, "logged", (r.IsLogged ? "true" : "false"));
                    }
                }
            }
        }

        public static void UpdateService(Request request, ReplyNode result, ErrorNode error) {
            string sId = request.ParameterSet["id"],
                    key = request.ParameterSet["servicekey"],
                    description = request.ParameterSet["description"],
                    consumeriprange = request.ParameterSet["consumeriprange"],
                    assembly = request.ParameterSet["assembly"],
                    serviceclass = request.ParameterSet["class"],
                    resultcode = null;
            bool? isrestricted = (request.ParameterSet["restricted"] == "true" ? true : false);
            bool? ispublic = (request.ParameterSet["public"] == "true" ? true : false);
            bool? islogged = (request.ParameterSet["logged"] == "true" ? true : false);
            int id;
            int? nId;

            if (Int32.TryParse(sId, out id)) {
                nId = id;
                using (LegionLinqDataContext db = new LegionLinqDataContext(request.Service.Settings["LegionConnectionString"])) {
                    db.xspUpdateService(ref nId, key, description, consumeriprange, assembly, serviceclass, isrestricted, ispublic, islogged, ref resultcode);
                }

                XmlElement r = result.AddElement("result");
                result.AddElement(r, "resultcode",  resultcode);

                if (sId == "-1")
                    result.AddElement(r, "id",  nId.ToString());
            }
            else
                error.Text = "not a valid id";
        }

        public static void UpdateServiceMethod(Request request, ReplyNode result, ErrorNode error) {
            string sId = request.ParameterSet["id"],
                    sServiceId = request.ParameterSet["serviceid"],
                    methodKey = request.ParameterSet["methodkey"],
                    methodName = request.ParameterSet["methodname"],
                    resultcode = null;
            bool? isresultcached = (request.ParameterSet["cacheresult"] == "true" ? true : false);
            bool? isrestricted = (request.ParameterSet["restricted"] == "true" ? true : false);
            bool? ispublic = (request.ParameterSet["public"] == "true" ? true : false);
            bool? islogged = (request.ParameterSet["logged"] == "true" ? true : false);
            //bool? islogreplaydetailsonexception = (request.ParameterSet["logreplaydetailsonexception"] == "true" ? true : false);
            int id, serviceId, cachedResultLifetime;
            int? nId, nServiceId, nCachedResultLifetime = null;

            if (Int32.TryParse(sId, out id) && Int32.TryParse(sServiceId, out serviceId) && Int32.TryParse(sServiceId, out serviceId)) {
                nId = id; nServiceId = serviceId;

                if (Int32.TryParse(request.ParameterSet["cachedresultlifetime"], out cachedResultLifetime))
                    nCachedResultLifetime = cachedResultLifetime;

                using (LegionLinqDataContext db = new LegionLinqDataContext(request.Service.Settings["LegionConnectionString"])) {
                    db.xspUpdateServiceMethod(ref nId, nServiceId, methodKey, methodName, cachedResultLifetime, isresultcached, isrestricted, ispublic, islogged, ref resultcode);
                }

                XmlElement r = result.AddElement("result");
                result.AddElement(r, "resultcode",  resultcode);

                if (sId == "-1")
                    result.AddElement(r, "id", nId.ToString());
            }
            else
                error.Text = "not a valid id";
        }

        public static void DeleteService(Request request, ReplyNode result, ErrorNode error) {
            string sId = request.ParameterSet["id"],
                    resultcode = null;
            int id;
            int? nId;

            if (Int32.TryParse(sId, out id)) {
                nId = id;
                using (LegionLinqDataContext db = new LegionLinqDataContext(request.Service.Settings["LegionConnectionString"])) {
                    db.xspDeleteService(nId, ref resultcode);
                }

                XmlElement r = result.AddElement("result");
                result.AddElement(r, "resultcode", resultcode);
            }
            else
                error.Text = "not a valid id";
        }

        public static void DeleteServiceMethod(Request request, ReplyNode result, ErrorNode error) {
            string sId = request.ParameterSet["id"],
                    resultcode = null;
            int id;
            int? nId;

            if (Int32.TryParse(sId, out id)) {
                nId = id;
                using (LegionLinqDataContext db = new LegionLinqDataContext(request.Service.Settings["LegionConnectionString"])) {
                    db.xspDeleteServiceMethod(nId, ref resultcode);
                }

                XmlElement r = result.AddElement("result");
                result.AddElement(r, "resultcode",  resultcode);
            }
            else
                error.Text = "not a valid id";
        }

        public static void GetServiceSettings(Request request, ReplyNode result, ErrorNode error) {
            int serviceid;
            string resultcode = null;
            XmlElement element, parent;

            SimpleAES aes;
            string aesKey = null, aesVector = null;

            if (request.ParameterSet["id"] != null && Int32.TryParse(request.ParameterSet["id"], out serviceid)) {
                using (LegionLinqDataContext db = new LegionLinqDataContext(request.Service.Settings["LegionConnectionString"])) {
                    db.xspGetSettingByKey("AesKey", ref aesKey);
                    db.xspGetSettingByKey("AesVector", ref aesVector);

                    ISingleResult<xspGetServiceSettingsResult> settings = db.xspGetServiceSettings(null, serviceid, ref resultcode);

                    parent = result.AddElement("settings");
                    foreach (xspGetServiceSettingsResult s in settings) {
                        element = result.AddElement(parent, "setting");
                        result.AddElement(element, "id", s.Id.ToString());
                        result.AddElement(element, "name", s.Name);

                        if (s.IsEncrypted) {
                            aes = new SimpleAES(aesKey, string.Format("{0}//{1}//{2}", s.Id, aesVector, s.Name));
                            result.AddElement(element, "value", aes.DecryptString(s.Value));
                            result.AddElement(element, "encrypted", "true");
                        }
                        else {
                            result.AddElement(element, "value", s.Value);
                            result.AddElement(element, "encrypted",  "false");
                        }
                    }
                    
                    result.AddElement(parent, "resultcode", resultcode);
                }
            }
        }

        public static void UpdateServiceSetting(Request request, ReplyNode result, ErrorNode error) {
            int? id = Int32.Parse(request.ParameterSet["id"]);
            int? serviceid = (request.ParameterSet["serviceid"] == null ? null : (int?)Int32.Parse(request.ParameterSet["serviceid"]));
            string name = request.ParameterSet["name"];
            string value = request.ParameterSet["value"];
            bool? encrypted = (request.ParameterSet["encrypted"] == "true" ? true : false);

            string resultcode = null;

            using (LegionLinqDataContext db = new LegionLinqDataContext(request.Service.Settings["LegionConnectionString"])) {
                if (id == -1) {
                    db.xspUpdateServiceSetting(ref id, serviceid, name, value, encrypted, ref resultcode);

                    if (encrypted == true) {
                        string aesKey = null, aesVector = null;
                        db.xspGetSettingByKey("AesKey", ref aesKey);
                        db.xspGetSettingByKey("AesVector", ref aesVector);

                        SimpleAES aes = new SimpleAES(aesKey, string.Format("{0}//{1}//{2}", id, aesVector, name));

                        value = aes.EncryptToString(value);
                        db.xspUpdateServiceSetting(ref id, serviceid, name, value, encrypted, ref resultcode);
                    }

                    result.AddElement("id",  id.ToString());
                }
                else {
                    if (encrypted == true) {
                        string aesKey = null, aesVector = null;
                        db.xspGetSettingByKey("AesKey", ref aesKey);
                        db.xspGetSettingByKey("AesVector", ref aesVector);

                        SimpleAES aes = new SimpleAES(aesKey, string.Format("{0}//{1}//{2}", id, aesVector, name));

                        value = aes.EncryptToString(value);
                    }

                    db.xspUpdateServiceSetting(ref id, serviceid, name, value, encrypted, ref resultcode);
                }

                result.AddElement("resultcode",  resultcode);
            }
        }

        public static void DeleteServiceSetting(Request request, ReplyNode result, ErrorNode error) {
            int? id = Int32.Parse(request.ParameterSet["id"]);
            int? serviceid = Int32.Parse(request.ParameterSet["serviceid"]);
            string resultcode = null;

            using (LegionLinqDataContext db = new LegionLinqDataContext(request.Service.Settings["LegionConnectionString"])) {
                db.xspDeleteServiceSetting(id, serviceid, ref resultcode);
                result.AddElement("resultcode",  resultcode);
            }
        }

        #endregion

        #region result caching

        public static void ExpireResultCache(Request request, ReplyNode result, ErrorNode error) {
            if (request.ParameterSet.Verify(VerificationGroups.ExpireResultCache)) {
                if (request.Application.HasPermissionTo(request.ParameterSet.GetString("Service"), request.ParameterSet.GetString("Method"))) {
                    int? itemcount = null;
                    using (LegionLinqDataContext db = new LegionLinqDataContext(request.Service.Settings["LegionConnectionString"])) {
                        db.xspExpireResultsCache(
                            request.ParameterSet.GetString("Service"),
                            request.ParameterSet.GetString("Method"),
                            ref itemcount
                        );

                        result.AddElement("count", (itemcount == null ? "0" : itemcount.ToString()));
                    }
                }
                else {
                    throw new Exception(request.Service.Settings["ApplicationPermissionErrorMessage"].Build(new Dictionary<string, string>() {
                        {"applicationname", request.Application.Name},
                        {"service", request.ParameterSet.GetString("Service")},
                        {"method", request.ParameterSet.GetString("Method")}
                    }));
                }
            }
            else
                error.Text = VerificationGroups.ExpireResultCache.GetDefinition();
        }

        #endregion

        #region Servers

        public static void GetServerStatus(Request request, ReplyNode result, ErrorNode error) {
            using (LegionLinqDataContext db = new LegionLinqDataContext(request.Service.Settings["LegionConnectionString"])) {
                ISingleResult<xspGetServerStatusResult> results = db.xspGetServerStatus();

                XmlElement host, hosts = result.AddElement("servers");
                foreach (xspGetServerStatusResult r in results) {
                    host = result.AddElement(hosts, "server");
                    host.SetAttribute("id", r.Id.ToString());

                    result.AddElement(host, "ipaddress", r.IPAddress);
                    result.AddElement(host, "hostname", r.HostName);
                    result.AddElement(host, "cacherefreshrequired", (r.IsCacheRefreshRequired ? "true" : "false"));
                    result.AddElement(host, "assemblyrefreshrequired", (r.IsAssemblyRefreshRequired ? "true" : "false"));
                }
            }
        }

        #endregion
    }
}
