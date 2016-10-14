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
using System.Text.RegularExpressions;

using Legion.Core.Caching;
using Legion.Core.Databases;
using Legion.Core.Exceptions;
using Legion.Core.Modules;

namespace Legion.Core.Services {

    /// <summary>
    /// Gets service settings from Legion
    /// </summary>
    public class ServiceSettings {
        private int _serviceId;
        private string _serviceKey;
        private const string CACHE_TYPE = "servicesettings";
        private Dictionary<string, string> _settings = null;

        /// <summary>
        /// Gets a setting
        /// </summary>
        /// <param name="key">the key of the setting</param>
        /// <returns>the setting</returns>
        public string this[string key] {
            get {
                if (Settings.ContainsKey(key))
                    return Settings[key];
                else
                    throw new SettingNotFoundException(key);
            }
        }

        internal Dictionary<string, string> Settings {
            get {
                GetSettings();
                return _settings;
            }
        }

        internal ServiceSettings(int serviceId, string serviceKey) {
            _serviceId = serviceId;
            _serviceKey = serviceKey;
        }

        /// <summary>
        /// Returns a subset of the ServiceSettings
        /// </summary>
        /// <param name="pattern">The pattern to finter against</param>
        /// <returns></returns>
        public Dictionary<string, string> Filter(string pattern) {
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

            return Settings
                    .Where(i => regex.Match(i.Key).Success)
                    .ToDictionary(i => i.Key, i => i.Value, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns a Dictionary representation.
        /// </summary>
        /// <returns>A Dictionary</returns>
        public Dictionary<string, string> ToDictionary() {
            return Settings;
        }

        /// <summary>
        /// Determins whether the specified key is present
        /// </summary>
        /// <param name="key">the key to look for</param>
        /// <returns>true if found, false otherwise</returns>
        public bool ContainsKey(string key) {
            return Settings.ContainsKey(key);
        }

        private void GetSettings() {
            if (_settings == null) {
                Regex credentialsRegex = new Regex(Legion.Core.Settings.GetString("SettingsCredentialInsertionRegex"));

                string setting;

                object oSettings = Cache.Get(CACHE_TYPE, _serviceKey);
                if (oSettings != null)
                    _settings = (Dictionary<string, string>)oSettings;
                else {
                    _settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    string resultcode = null;
                    using (LegionLinqDataContext db = new LegionLinqDataContext(ConfigurationManager.ConnectionStrings["LegionConnectionString"].ToString())) {
                        ISingleResult<xspGetServiceSettingsResult> settings = db.xspGetServiceSettings(_serviceKey, null, ref resultcode);
                        foreach (xspGetServiceSettingsResult s in settings) {
                            if (s.IsEncrypted) {
                                Encryption.Packet packet = Modules.Encryption.Module.DecryptString(new Encryption.Packet() {
                                    IV = s.IV,
                                    CipherText = s.Value,
                                });

                                setting = packet.ClearText;
                            }
                            else
                                setting = s.Value;

                            MatchCollection credentials = credentialsRegex.Matches(setting);
                            foreach (Match credential in credentials) {
                                setting = setting.Replace(
                                    credential.Groups["Credential"].Value,
                                    Credentials.Module.GetServiceCredential(_serviceId, credential.Groups["Name"].Value) ?? credential.Groups["Credential"].Value
                                );
                            }

                            _settings.Add(s.Name, setting);
                        }
                    }

                    Cache.Add(CACHE_TYPE, _serviceKey, _settings);
                }
            }
        }
    }
}