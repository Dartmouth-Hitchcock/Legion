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
using System.Data.Linq;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Caching;

using Legion.Core.Extensions;
using Legion.Core.Databases;
using Legion.Core.DataStructures;
using Legion.Core.Modules;
using Legion.Core.Services;

namespace Legion.Core.Caching {

    /// <summary>
    /// The Legion Services cache
    /// </summary>
    public class Cache {
        private const string COMPONENT_KEY_FORMAT = "__legion_component_{0}_{1}";
        private const string COMPONENT_MASTER_KEY = "//masterkeylist//";

        private static object _componentMasterKeyLock = new object();

        /// <summary>
        /// The keys under which the Legion stores things in the Cache
        /// </summary>
        public struct CACHE_KEYS{
            /// <summary>
            /// The cached services
            /// </summary>
            public const string Services = "__legion_Services";
            /// <summary>
            /// Statuses
            /// </summary>
            public const string Statuses = "__legion_Statuses";
            /// <summary>
            /// List of system methods
            /// </summary>
            public const string SystemMethods = "__legion_SystemMethods";
            /// <summary>
            /// When was the cache last updated?
            /// </summary>
            public const string LastUpdated = "__legion_ServicesLastUpdated";
            /// <summary>
            /// The directory the services were loaded from
            /// </summary>
            public const string AssemblyDirectory = "__legion_ServicesDirectory";
            /// <summary>
            /// List of components
            /// </summary>
            public const string ComponentList = "__legion_ComponentList";
            /// <summary>
            /// List of API keys which were explicitly revoked
            /// </summary>
            public const string KeyRevocationList = "__legion_KeyRevocationList";
            /// <summary>
            /// Is the cache currently loading from disk
            /// </summary>
            public const string CacheLoading = "__legion_CacheLoading";
        }

        #region accessors 

        /// <summary>
        /// Is the cache loaded?
        /// </summary>
        public static bool IsLoaded {
            get { return (HttpRuntime.Cache[CACHE_KEYS.Services] == null ? false : true); }
        }

        /// <summary>
        /// Is the cache currently being loaded from disk
        /// </summary>
        public static bool IsLoading {
            get { return (HttpRuntime.Cache[CACHE_KEYS.CacheLoading] == null ? false : true); }
        }

        #endregion

        /// <summary>
        /// Loads the Legion Services from the database into memory
        /// </summary>
        /// <returns>The number of exceptions that were thrown while loading the cache</returns>
        public static int Load() {
            Logging.Module.WriteEvent(new LoggedEvent(EventLevel.Info, "CacheLoadStart", Settings.GetString("LogFormatCacheLoadStart"), true));
            Insert(CACHE_KEYS.CacheLoading, "true");

            System.Web.Caching.Cache cache = HttpRuntime.Cache;
            Dictionary<string, Service> legion = new Dictionary<string, Service>();
            List<string> keyRevocationList = new List<string>();
            int exceptionCount = 0;

            string assemblyDirectory = null;

            bool loadMethod, isAutheticatedUserRequired, isAuthorizedUserRequired;
            Assembly assembly; Type t; Service service; MethodInfo mi;
            IpAddressRange consumerIPRange;
            Version version;
            Dictionary<string, Method> serviceMethods;
            Dictionary<string, Method> systemMethods = new Dictionary<string, Method>();
            Dictionary<string, Method> statusMethods = new Dictionary<string, Method>();

            Dictionary<string, string> systemMethodKeys = Settings.GetDictionary("SystemMethods");

            using (LegionLinqDataContext db = new LegionLinqDataContext(ConfigurationManager.ConnectionStrings["LegionConnectionString"].ToString())) {
                assemblyDirectory = Settings.GetString("AssemblyDirectory");
                if (Directory.Exists(assemblyDirectory)) {
                    //load services
                    Logging.Module.WriteEvent(new LoggedEvent(EventLevel.Info, "ServiceLoadStart", Settings.GetString("LogFormatCacheServiceLoadStart"), true));
                    ISingleResult<xspGetServicesResult> services = db.xspGetServices();
                    foreach (xspGetServicesResult result in services) {
                        isAutheticatedUserRequired = false;
                        isAuthorizedUserRequired = false;

                        if (!legion.ContainsKey(result.ServiceKey)) {
                            if (!result.ServiceKey.StartsWith("__")) {
                                try {
                                    assembly = Assembly.LoadFile(string.Format(@"{0}\{1}.dll", assemblyDirectory, result.AssemblyName));
                                    t = assembly.GetType(result.AssemblyName + "." + result.ClassName);

                                    if (t != null && t.IsClass) {
                                        Attribute[] attributes = Attribute.GetCustomAttributes(t);
                                        foreach (Attribute attribute in attributes) {
                                            LegionAttribute lAttribute = attribute as LegionAttribute;
                                            if (lAttribute != null) {
                                                if (!isAutheticatedUserRequired && lAttribute.IsAuthenticatedUserRequired) {
                                                    isAutheticatedUserRequired = true;
                                                }

                                                if (!isAuthorizedUserRequired && lAttribute.IsAuthorizedUserRequired) {
                                                    isAuthorizedUserRequired = true;
                                                }
                                            }
                                        }

                                        consumerIPRange = (result.ConsumerIPRange == null || result.ConsumerIPRange == string.Empty ? null : new IpAddressRange(result.ConsumerIPRange));
                                        version = assembly.GetName().Version;

                                        legion.Add(
                                            result.ServiceKey.ToLower(),
                                            new Service(
                                                result.ServiceId,
                                                result.ServiceKey,
                                                t,
                                                consumerIPRange,
                                                string.Format("{0}.{1}.{2}.{3}",
                                                    version.Major,
                                                    version.Minor,
                                                    version.Build,
                                                    version.Revision
                                                ),
                                                Settings.FormatDateTime(assembly.GetCompileDate()),
                                                result.IsPublic,
                                                result.IsLogged,
                                                isAutheticatedUserRequired,
                                                isAuthorizedUserRequired
                                            )
                                        );
                                    }
                                    else {
                                        Logging.Module.WriteException(new LoggedException() {
                                            Type = "CacheClassNotFound",
                                            Message = Settings.GetString("ExceptionMessageCacheClassNotFound", new Dictionary<string, string>(){
                                                {"AssemblyName", result.AssemblyName},
                                                {"ClassName", result.ClassName}
                                            })
                                        });
                                    }
                                }
                                catch (System.IO.FileNotFoundException e) {
                                    Logging.Module.WriteException(new LoggedException() {
                                        Type = "AssemblyNotFound",
                                        Message = Settings.GetString("ExceptionMessageCacheAssemblyNotFound", new Dictionary<string, string>() {
                                            {"AssemblyName", result.AssemblyName}
                                        }),
                                        StackTrace = e.StackTrace
                                    });

                                    exceptionCount++;
                                }
                                catch (Exception) {
                                    Logging.Module.WriteException(new LoggedException() {
                                        Type = "BadIPRange",
                                        Message = Settings.GetString("ExceptionMessageCacheInvalidServiceIpRange", new Dictionary<string, string>(){
                                            {"IpRange", result.ConsumerIPRange},
                                            {"ServiceKey", result.ServiceKey}
                                        })
                                    });

                                    exceptionCount++;
                                }
                            }
                            else {
                                Logging.Module.WriteException(new LoggedException() {
                                    Type = "CacheInvalidServiceKey",
                                    Message = Settings.GetString("ExceptionMessageCacheInvalidServiceKey", new Dictionary<string, string>(){
                                        {"ServiceKey", result.ServiceKey}
                                    })
                                });
                            }
                        }
                    }

                    //add system service
                    legion.Add(
                        Settings.GetString("SystemServiceKey"),
                        new Service(
                            -1,
                            Settings.GetString("SystemServiceKey"),
                            null,
                            null,
                            "0.0.0.0",
                            Settings.FormatDateTime(DateTime.Now),
                            false,
                            false,
                            false,
                            false
                        )
                    );

                    Logging.Module.WriteEvent(new LoggedEvent(EventLevel.Info, "ServiceLoadFinish", Settings.GetString("LogFormatCacheServiceLoadFinish"), true));

                    //load methods
                    Logging.Module.WriteEvent(new LoggedEvent(EventLevel.Info, "MethodLoadStart", Settings.GetString("LogFormatCacheMethodLoadStart"), true));
                    ISingleResult<xspGetMethodsResult> methods = db.xspGetMethods();
                    foreach (xspGetMethodsResult result in methods) {
                        if (legion.ContainsKey(result.ServiceKey.ToLower()))
                            service = legion[result.ServiceKey.ToLower()];
                        else {
                            Logging.Module.WriteException(new LoggedException() {
                                Type = "ServiceKeyNotFound",
                                Message = Settings.GetString("ExceptionMessageCacheServiceKeyNotFound", new Dictionary<string, string>(){
                                    {"ServiceKey", result.ServiceKey.ToLower()},
                                    {"MethodKey", result.MethodKey},
                                    {"MethodId", result.MethodId.ToString()}
                                })
                            });

                            continue;
                        }
                        
                        serviceMethods = (result.IsRestricted == true ? service.Restricted : service.Open);
                        string rc = null;
                        mi = service.ServiceType.GetMethod(result.MethodName, BindingFlags.Public | BindingFlags.Static);
                        if (mi != null) {
                            loadMethod = true;
                            isAutheticatedUserRequired = false;
                            isAuthorizedUserRequired = false;

                            Attribute[] attributes = Attribute.GetCustomAttributes(mi);
                            foreach (Attribute attribute in attributes) {
                                LegionAttribute lAttribute = attribute as LegionAttribute;
                                if (lAttribute != null) {
                                    if (result.IsResultCacheable && !lAttribute.IsCacheable) {
                                        Logging.Module.WriteException(new LoggedException() {
                                            Type = "ExceptionMessageCacheMethodNotCacheable",
                                            Message = Settings.GetString("ExceptionMessageCacheMethodNotCacheable", new Dictionary<string, string>(){
                                                {"ServiceKey", result.ServiceKey},
                                                {"MethodKey", result.MethodKey},
                                                {"MethodId", result.MethodId.ToString()},
                                                {"AttributeName", attribute.GetType().Name}
                                            })
                                        });

                                        exceptionCount++;
                                        loadMethod = false;
                                        break;
                                    }

                                    if (!isAutheticatedUserRequired && lAttribute.IsAuthenticatedUserRequired) {
                                        isAutheticatedUserRequired = true;
                                    }
                                    
                                    if(!isAuthorizedUserRequired && lAttribute.IsAuthorizedUserRequired){
                                        isAuthorizedUserRequired = true;
                                    }
                                }
                            }

                            if (loadMethod) {
                                serviceMethods.Add(
                                    result.MethodKey.ToLower(), 
                                    new Method(
                                        result.MethodId, 
                                        result.MethodKey.ToLower(), 
                                        result.MethodName, 
                                        result.ServiceId,
                                        result.ServiceKey,
                                        result.IsPublic, 
                                        result.IsLogged, 
                                        isAutheticatedUserRequired,
                                        isAuthorizedUserRequired,
                                        result.IsResultCacheable, 
                                        result.IsLogReplayDetailsOnException, 
                                        result.CachedResultLifetime, 
                                        mi
                                    )
                                );

                                //add system method handles
                                foreach (KeyValuePair<string, string> systemMethod in systemMethodKeys.Where(k => k.Value.ToLower() == string.Format("{0}.{1}()", result.ServiceKey, result.MethodKey).ToLower())) {
                                    legion[Settings.GetString("SystemServiceKey")].Open.Add(
                                        systemMethod.Key.ToLower(), 
                                        serviceMethods[result.MethodKey.ToLower()]
                                    );
                                }

                                if (result.IsMissing)
                                    db.xspUpdateMethodMissingFlag(result.MethodId, false, ref rc);
                            }
                        }
                        else {
                            if (!result.IsMissing) {
                                db.xspUpdateMethodMissingFlag(result.MethodId, true, ref rc);

                                Logging.Module.WriteException(new LoggedException() {
                                    Type = "MethodNotFound",
                                    Message = Settings.GetString("", new Dictionary<string, string>(){
                                        {"ServiceKey", result.ServiceKey},
                                        {"MethodKey", result.MethodKey},
                                        {"MethodName", result.MethodName},
                                        {"MethodId", result.MethodId.ToString()},
                                    })
                                });

                                exceptionCount++;
                            }
                        }
                    }
                    Logging.Module.WriteEvent(new LoggedEvent(EventLevel.Info, "MethodLoadFinish", Settings.GetString("LogFormatCacheMethodLoadFinish"), true));

                    //load revocation list
                    Logging.Module.WriteEvent(new LoggedEvent(EventLevel.Info, "KeyRevocationListLoadStart", Settings.GetString("LogFormatCacheKeyRevocationListLoadStart"), true));
                    ISingleResult<xspGetKeyRevocationListResult> revocations = db.xspGetKeyRevocationList();
                    foreach (xspGetKeyRevocationListResult result in revocations) {
                        keyRevocationList.Add(result.Key);
                    }
                    Logging.Module.WriteEvent(new LoggedEvent(EventLevel.Info, "KeyRevocationListLoadFinish", Settings.GetString("LogFormatCacheKeyRevocationListLoadFinish"), true));

                    //cache service status methods
                    Logging.Module.WriteEvent(new LoggedEvent(EventLevel.Info, "ServiceStatusListLoadStart", Settings.GetString("LogFormatCacheServiceStatusListLoadStart"), true));
                    foreach (KeyValuePair<string, Service> s in legion) {
                        foreach (KeyValuePair<string, string> specialMethod in Method.SPECIAL_METHODS) {
                            Type st = s.Value.ServiceType;
                            if (st != null) {
                                mi = st.GetMethod(specialMethod.Key, BindingFlags.Public | BindingFlags.Static);
                                if (mi != null) {
                                    statusMethods.Add(s.Key, new Method(-1, specialMethod.Key.ToLower(), specialMethod.Value, -1, "Special", false, false, false, false, false, false, null, mi));
                                    s.Value.Special.Add(specialMethod.Key.ToLower(), statusMethods[s.Key]);
                                }
                            }
                        }
                    }
                    Logging.Module.WriteEvent(new LoggedEvent(EventLevel.Info, "ServiceStatusListLoadFinish", Settings.GetString("LogFormatCacheServiceStatusListLoadFinish"), true));

                    Insert(CACHE_KEYS.Services, legion);
                    Insert(CACHE_KEYS.Statuses, statusMethods);
                    Insert(CACHE_KEYS.SystemMethods, systemMethods);
                    Insert(CACHE_KEYS.KeyRevocationList, keyRevocationList);
                    Insert(CACHE_KEYS.AssemblyDirectory, assemblyDirectory);
                    Insert(CACHE_KEYS.LastUpdated, DateTime.Now);

                    string ipaddress = ServerDetails.IPv4Addresses.First().ToString();
                    string hostname = System.Environment.MachineName;
                    bool? refresh = null;

                    db.xspGetCacheStatus(ipaddress, ref refresh);
                    db.xspGetAssemblyStatus(ipaddress, hostname, ref refresh);
                }
                else {
                    Logging.Module.WriteException(new LoggedException() {
                        Type = "BadAssemblyDirectory",
                        Message = Settings.GetString("ExceptionMessageCacheInvalidAssemblyDirectory")
                    });

                    exceptionCount++;
                }
            }

            HttpRuntime.Cache.Remove(CACHE_KEYS.CacheLoading);
            Logging.Module.WriteEvent(new LoggedEvent(EventLevel.Info, "CacheLoadFinish", Settings.GetString("LogFormatCacheLoadFinish"), true));

            return exceptionCount;
        }

        /// <summary>
        /// Unloads the Legion Services cache from memory
        /// </summary>
        public static void Unload() {
            Logging.Module.WriteEvent(new LoggedEvent(EventLevel.Info, "CacheUnload", Settings.GetString("LogFormatCacheUnload"), true));
            HttpRuntime.Cache.Remove(CACHE_KEYS.Services);
        }

        /// <summary>
        /// Dumps the contents of the cache to a string
        /// </summary>
        /// <returns>A string dump of the cache</returns>
        public static string Dump() {
            Refresh();
            return ToString();
        }

        /// <summary>
        /// To String
        /// </summary>
        /// <returns>a string representation of the cahce contents</returns>
        new public static string ToString(){
            System.Web.Caching.Cache cache = HttpRuntime.Cache;
            string contents;

            Dictionary<string, Service> services = (Dictionary<string, Service>)cache[CACHE_KEYS.Services];
            if (services != null) {
                contents = string.Empty;
                foreach (KeyValuePair<string, Service> service in services) {
                    contents += string.Format("\n\t{0}", service.Key);

                    contents += string.Format("\n\t\tOpen: ");
                    foreach (KeyValuePair<string, Method> method in service.Value.Open)
                        contents += string.Format("{0}, ", method.Key);

                    contents = contents.Substring(0, contents.Length - 2);

                    contents += string.Format("\n\t\tRestricted: ");
                    foreach (KeyValuePair<string, Method> method in service.Value.Restricted)
                        contents += string.Format("{0}, ", method.Key);

                    contents = contents.Substring(0, contents.Length - 2);
                }
            }
            else
                contents = Settings.GetString("LogFormatCacheDumpNotLoaded");

            return Settings.GetString("ToStringCache", new Dictionary<string, string>() {
                {"Host", System.Environment.MachineName.ToLower()},
                {"AssemblyDirectory", cache[CACHE_KEYS.AssemblyDirectory].ToString()},
                {"CacheLastUpdatedOn", Settings.FormatDateTime((DateTime)cache[CACHE_KEYS.LastUpdated])},
                {"CacheContents", contents}
            });

            /*** debug code
            sCache += string.Format("\n\nComponent Cache Contents:");
            object oComponentList = cache[CACHE_KEYS.ComponentList];
            if (oComponentList != null) {
                List<string> componentList = (List<string>)oComponentList;
                foreach (string component in componentList) {
                    sCache += string.Format("\nComponent: {0}", component);

                    string masterkey = BuildComponentKey(component, COMPONENT_MASTER_KEY);
                    if (cache[masterkey] != null) {
                        sCache += string.Format("\n\tKeys: ");
                        List<string> keys = (List<string>)cache[masterkey];
                        foreach (string key in keys)
                            sCache += string.Format("{0}, ", key);

                        sCache = sCache.Substring(0, sCache.Length - 2);
                    }
                }
            }
            /***/
        }

        /// <summary>
        /// Refreshes the cache if there have been updates to the database
        /// </summary>
        /// <returns>The number of exceptions that were thrown while loading the cache</returns>
        public static int Refresh() {
            System.Web.Caching.Cache cache = HttpRuntime.Cache;
            
            while (IsLoading) {
                Logging.Module.WriteEvent(new LoggedEvent(EventLevel.Info, "CacheLoadWait", Settings.GetString("LogFormatCacheLoadWait", new Dictionary<string, string>(){
                    {"Interval", Settings.GetInt("CacheLoadingSleepInterval").ToString()}
                }), true));

                System.Threading.Thread.Sleep(Settings.GetInt("CacheLoadingSleepInterval"));
            }

            if (!IsLoaded)
                return ForceRefresh();
            else {
                DateTime lastCacheRefreshCheck = (HttpContext.Current.Application["LastComponentCacheRefreshCheck"] == null ? DateTime.MinValue : (DateTime)HttpContext.Current.Application["LastComponentCacheRefreshCheck"]);

                if (lastCacheRefreshCheck <= DateTime.Now.Subtract(new TimeSpan(0, 0, Settings.GetInt("CacheRefreshCheckInterval")))) {
                    using (LegionLinqDataContext db = new LegionLinqDataContext(ConfigurationManager.ConnectionStrings["LegionConnectionString"].ToString())) {
                        string ipaddress = ServerDetails.IPv4Addresses.First().ToString();
                        bool? refreshCache = null;

                        db.xspGetCacheStatus(ipaddress, ref refreshCache);
                        HttpContext.Current.Application["LastComponentCacheRefreshCheck"] = DateTime.Now;

                        if (refreshCache == true)
                            return ForceRefresh();
                        else
                            return -1;
                    }
                }
                else
                    return -1;
            }
        }

        /// <summary>
        /// Refreshes the cache regardless if there have been updates to the database
        /// </summary>
        /// <returns>The number of exceptions that were thrown while loading the cache</returns>
        public static int ForceRefresh() {
            Logging.Module.WriteEvent(new LoggedEvent(EventLevel.Info, "CacheRefresh", Settings.GetString("LogFormatCacheRefresh"), true));

            System.Web.Caching.Cache cache = HttpRuntime.Cache;

            object oComponentList = cache[CACHE_KEYS.ComponentList];
            if (oComponentList != null) {
                List<string> componentList = (List<string>)oComponentList;
                foreach(string component in componentList)
                    ClearComponent(component);
            }

            return Load();
        }

        /// <summary>
        /// Inserts an object into the cache with the highest priority and time
        /// </summary>
        /// <param name="key">The key to insert with</param>
        /// <param name="value">The object to insert</param>
        private static void Insert(string key, object value) {
            Insert(key, value, null, null, null);
        }

        /// <summary>
        /// Inserts an object into the cache with the highest priority and time
        /// </summary>
        /// <param name="key">The key to insert with</param>
        /// <param name="value">The object to insert</param>
        /// <param name="absoluteExpiration">expire on</param>
        /// <param name="slidingExpiration">expire after</param>
        /// <param name="callback">Method to call when item is removed</param>
        private static void Insert(string key, object value, DateTime? absoluteExpiration, TimeSpan? slidingExpiration, CacheItemRemovedCallback callback) {
            HttpRuntime.Cache.Insert(
                key, 
                value, 
                null, 
                (absoluteExpiration == null ? DateTime.MaxValue : (DateTime)absoluteExpiration), 
                (slidingExpiration ?? System.Web.Caching.Cache.NoSlidingExpiration), 
                CacheItemPriority.NotRemovable, 
                (callback ?? RemoveComponentCacheKey)
            );
        }

        #region component methods

        /// <summary>
        /// Add a component's value to the specified cache
        /// </summary>
        /// <param name="component">the component adding the value</param>
        /// <param name="key">the key to add with</param>
        /// <param name="value">the value to add</param>
        internal static void Add(string component, string key, object value) {
            Add(component, key, value, null, null);
        }

        /// <summary>
        /// Add a component's value to the specified cache
        /// </summary>
        /// <param name="component">the component adding the value</param>
        /// <param name="key">the key to add with</param>
        /// <param name="value">the value to add</param>
        /// <param name="expiration">Expire the item after this timespan</param>
        /// <param name="callback">Method to call when item is removed</param>
        internal static void Add(string component, string key, object value, TimeSpan? expiration, CacheItemRemovedCallback callback) {
            string cachekey = BuildComponentKey(component, key);
            SaveComponentCacheKey(component, key);
            Insert(cachekey, value, null, expiration, callback);
        }

        /// <summary>
        /// Add a component's value to the specified cache
        /// </summary>
        /// <param name="component">the component adding the value</param>
        /// <param name="key">the key to add with</param>
        /// <param name="value">the value to add</param>
        /// <param name="expiration">Expire the item at this timespan</param>
        /// <param name="callback">Method to call when item is removed</param>
        internal static void Add(string component, string key, object value, DateTime expiration, CacheItemRemovedCallback callback) {
            string cachekey = BuildComponentKey(component, key);
            SaveComponentCacheKey(component, key);
            Insert(cachekey, value, expiration, null, callback);
        }

        /// <summary>
        /// Get a component's value from the cache
        /// </summary>
        /// <param name="component">the component adding the value</param>
        /// <param name="key">the key to get</param>
        /// <returns>the value, or null if not found</returns>
        internal static object Get(string component, string key) {
            System.Web.Caching.Cache cache = HttpRuntime.Cache;
            string cachekey = BuildComponentKey(component, key);

            if (cache[cachekey] != null)
                return cache[cachekey];
            else
                return null;
        }

        /// <summary>
        /// Removes a key from the cached key list
        /// </summary>
        /// <param name="cachekey">The cache key</param>
        /// <param name="value">The cache value</param>
        /// <param name="reason">The reason the cache item is being removed</param>
        internal static void RemoveComponentCacheKey(string cachekey, object value, CacheItemRemovedReason reason) {
            System.Web.Caching.Cache cache = HttpRuntime.Cache;
            string[] keyNodes = cachekey.Split('_');

            if (keyNodes.Length == COMPONENT_KEY_FORMAT.Split('_').Length) {
                string component = keyNodes[keyNodes.Length - 2];
                string key = keyNodes[keyNodes.Length - 1];

                string cachemasterkey = BuildComponentKey(component, COMPONENT_MASTER_KEY);

                lock (_componentMasterKeyLock) {
                    if (cache[cachemasterkey] != null) {
                        List<string> keys = (List<string>)cache[cachemasterkey];
                        keys.Remove(key);
                    }
                }
            }
        }

        /// <summary>
        /// Clears all keys associated with the component
        /// </summary>
        /// <param name="component">the component to clear</param>
        private static void ClearComponent(string component) {
            System.Web.Caching.Cache cache = HttpRuntime.Cache;
            string cachemasterkey = BuildComponentKey(component, COMPONENT_MASTER_KEY);

            lock (_componentMasterKeyLock) {
                if (cache[cachemasterkey] != null) {
                    string[] keys = ((List<string>)cache[cachemasterkey]).ToArray();
                    foreach (string key in keys)
                        cache.Remove(BuildComponentKey(component, key));

                    cache.Remove(cachemasterkey);
                }
            }
        }

        /// <summary>
        /// Builds the key the component's value is stored under
        /// </summary>
        /// <param name="component">the component</param>
        /// <param name="key">the key</param>
        /// <returns>the cache key for the component/value</returns>
        private static string BuildComponentKey(string component, string key) {
            return string.Format(COMPONENT_KEY_FORMAT, component, key.Replace("_", string.Empty)).ToLower();
        }

        /// <summary>
        /// Save the cache key
        /// </summary>
        /// <param name="component">the cache component</param>
        /// <param name="key">the key</param>
        private static void SaveComponentCacheKey(string component, string key) {
            System.Web.Caching.Cache cache = HttpRuntime.Cache;
            string componentmasterkey = BuildComponentKey(component, COMPONENT_MASTER_KEY);

            lock (_componentMasterKeyLock) {
                List<string> keys;
                if (cache[componentmasterkey] == null) {
                    keys = new List<string>();

                    //add to type list
                    List<string> components = (cache[CACHE_KEYS.ComponentList] == null ? new List<string>() : (List<string>)cache[CACHE_KEYS.ComponentList]);
                    components.Add(component);
                    cache[CACHE_KEYS.ComponentList] = components;
                }
                else
                    keys = (List<string>)cache[componentmasterkey];

                //add value key to key list
                keys.Add(key);
                cache[componentmasterkey] = keys;
            }
        }

        #endregion
    }
}
