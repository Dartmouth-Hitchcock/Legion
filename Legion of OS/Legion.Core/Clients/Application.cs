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
using System.Net;

using Legion.Core.Modules;
using Legion.Core.DataStructures;
using Legion.Core.Databases;
using Legion.Core.Caching;

namespace Legion.Core.Clients {

    /// <summary>
    /// A registered application utilizing the Legion API
    /// </summary>
    public class Application {
        private const string CACHE_TYPE = "application";
        private const string PERMISSIONS_KEY_FORMAT = "{0}.{1}";

        private string _apikey = null, _name = null, _description = null;
        private int? _id = null;
        private int _rateLimit, _rateInterval;
        private RateType _rateType;
        private bool _isLogged, _isPublic;
        private IpAddressRange _sourceIpRange = null;
        private Dictionary<string, bool> _permissions = new Dictionary<string, bool>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="apiKey">The API key of the application</param>
        public Application(string apiKey) {
            if (apiKey != null && apiKey.Length > 0) {
                object oApplication = Cache.Get(CACHE_TYPE, apiKey);

                if (oApplication != null) {
                    Application application = (Application)oApplication;
                    _apikey = application._apikey;
                    _name = application._name;
                    _description = application._description;
                    _id = application._id;
                    _isPublic = application._isPublic;
                    _isLogged = application._isLogged;
                    _sourceIpRange = application._sourceIpRange;
                    _permissions = application._permissions;
                    _rateLimit = application._rateLimit;
                    _rateInterval = application._rateInterval;
                    _rateType = application._rateType;
                }
                else {
                    string sourceIpRange = null, rateType = null;
                    bool? isLogged = null, isPublic = null;
                    int? rateLimitId = null, rateLimit = null, rateInterval = null;

                    using (LegionLinqDataContext legion = new LegionLinqDataContext(ConfigurationManager.ConnectionStrings["LegionConnectionString"].ToString())) {
                        legion.xspGetApplication(apiKey, ref _id, ref _name, ref sourceIpRange, ref _description, ref rateLimitId, ref rateType, ref rateLimit, ref rateInterval, ref isPublic, ref isLogged);
                    }

                    _apikey = apiKey;

                    _rateLimit = rateLimit ?? Settings.GetInt("RateLimitDefault");
                    _rateInterval = rateInterval ?? Settings.GetInt("RateLimitIntervalDefault");
                    _rateType = (rateType == null ? RateType.Default : (RateType)Enum.Parse(typeof(RateType), rateType, true));

                    _isPublic = (isPublic == true ? true : false);
                    _isLogged = (isLogged == true ? true : false);

                    if (sourceIpRange != null && sourceIpRange != string.Empty) {
                        try {
                            _sourceIpRange = new IpAddressRange(sourceIpRange);
                        }
                        catch {
                            string message = Settings.GetString("ExceptionMessageApplicationInvalidIpRange", new Dictionary<string, string>(){
                                {"IpRange", sourceIpRange},
                                {"ApplicationName", _name},
                                {"ApiKey", apiKey}
                            });

                            Logging.Module.WriteException(new LoggedException() {
                                Type = "BadIPRange",
                                Message = message
                            });
                            
                            throw new Exception(message);
                        }
                    }

                    Cache.Add(CACHE_TYPE, apiKey, this);
                }
            }
            else {
                _apikey = null;
                _name = Settings.GetString("SymbolUnknown");
                _description = string.Empty;
                _id = 0;
                _isPublic = false;
                _isLogged = false;
                _sourceIpRange = null;
                _permissions = null;
                _rateLimit = Settings.GetInt("RateLimitDefault");
                _rateInterval = Settings.GetInt("RateLimitIntervalDefault");
                _rateType = RateType.Default;
            }
        }

        /// <summary>
        /// The Application's ID
        /// </summary>
        public int Id {
            get { return (_id == null ? -1 : (int)_id); }
        }

        /// <summary>
        /// The Application's API Key
        /// </summary>
        public string APIKey {
            get { return (_apikey == null ? string.Empty : _apikey); }
        }

        /// <summary>
        /// The Application's name
        /// </summary>
        public string Name {
            get { return (_name == null ? Settings.GetString("SymbolUnknown") : _name); }
        }
        
        /// <summary>
        /// The Application's description
        /// </summary>
        public string Description {
            get { return (_description == null ? string.Empty : _description); }
        }

        /// <summary>
        /// Is this application public
        /// </summary>
        public bool IsPublic {
            get { return _isPublic; }
        }

        /// <summary>
        /// Are calls by this application logged
        /// </summary>
        public bool IsLogged {
            get { return _isLogged; }
        }

        /// <summary>
        /// Determins whether or not the application has access to the specificed Server,Method
        /// </summary>
        /// <param name="serviceKey">The key of the service to which access is being requested</param>
        /// <param name="methodKey">The key of the method to which access is being requested</param>
        /// <returns>true if the application has access to the method, false otherwise</returns>
        public bool HasPermissionTo(string serviceKey, string methodKey) {
            bool? authorized = false;
            string permissionsKey = string.Format(PERMISSIONS_KEY_FORMAT, serviceKey, methodKey);

            if (_permissions.ContainsKey(permissionsKey))
                return _permissions[permissionsKey];
            else {
                using (LegionLinqDataContext legion = new LegionLinqDataContext(ConfigurationManager.ConnectionStrings["LegionConnectionString"].ToString())) {
                    legion.xspCheckPermission(_id, serviceKey, methodKey, ref authorized);
                }

                //TODO: Should there be locking here?
                _permissions.Add(permissionsKey, (bool)authorized);
                Cache.Add(CACHE_TYPE, this.APIKey, this);

                return (bool)authorized;
            }
        }

        /// <summary>
        /// Checks if the specified IP Address is within the application's approved source IP range
        /// </summary>
        /// <param name="ip">The source IP address string to check</param>
        /// <returns>true if the specified ip address is the the application's source IP range, false otherwise</returns>
        public bool HasValidSourceIP(string ip) {
            IPAddress ipa;
            if (IPAddress.TryParse(ip, out ipa))
                return HasValidSourceIP(ipa);
            else
                return false;
        }

        /// <summary>
        /// Checks if the specified IP Address is within the application's approved source IP range
        /// </summary>
        /// <param name="ip">The source IP address to check</param>
        /// <returns>true if the specified ip address is the the application's source IP range, false otherwise</returns>
        public bool HasValidSourceIP(IPAddress ip) {
            if (_sourceIpRange == null)
                return true;
            else {
                return _sourceIpRange.IsInRange(ip);
            }
        }

        /// <summary>
        /// The max number of requests per interval
        /// </summary>
        internal int RateLmit {
            get { return _rateLimit; }
        }

        /// <summary>
        /// The interval to rate
        /// </summary>
        internal int RateInterval {
            get { return _rateInterval; }
        }

        /// <summary>
        /// Determine rate by this metric
        /// </summary>
        internal RateType RateBy {
            get { return _rateType; }
        }
    }
}
