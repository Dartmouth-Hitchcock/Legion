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

using Legion.Core.Caching;
using Legion.Core.Databases;
using Legion.Core.Extensions;

namespace Legion.Core {

    /// <summary>
    /// Global Legion Settings
    /// </summary>
    internal class Settings {
        private const string CACHE_TYPE = "coresettings";
        private const string CACHE_KEY = "settings";

        private static object _lock = new object();

        /// <summary>
        /// Gets a string setting from the database
        /// </summary>
        /// <param name="key">the key of the setting</param>
        /// <returns>the setting value</returns>
        internal static string GetString(string key) {
            string value = GetSettingFromCache<string>(key);
            if (value == null) {
                value = GetSettingFromDatabase(key);
                PutSettingInCache(key, value);
            }

            return value;
        }

        /// <summary>
        /// Gets a string setting from the database
        /// </summary>
        /// <param name="key">the key of the setting</param>
        /// <param name="parameters">parameters to build into the setting string</param>
        /// <returns>the setting value</returns>
        internal static string GetString(string key, Dictionary<string, string> parameters) {
            return GetString(key).Build(parameters);
        }

        /// <summary>
        /// Gets a bool setting from the database
        /// </summary>
        /// <param name="key">the key of the setting</param>
        /// <returns>the setting value</returns>
        internal static bool GetBool(string key) {
            bool? value = GetSettingFromCache<bool?>(key);
            if (value == null) {
                value = bool.Parse(GetSettingFromDatabase(key));
                PutSettingInCache(key, value);
            }

            return (bool)value;
        }

        /// <summary>
        /// Gets an int setting from the database
        /// </summary>
        /// <param name="key">the key of the setting</param>
        /// <returns>the setting value</returns>
        internal static int GetInt(string key) {
            int? value = GetSettingFromCache<int?>(key);
            if (value == null) {
                value = int.Parse(GetSettingFromDatabase(key));
                PutSettingInCache(key, value);
            }

            return (int)value;
        }

        /// <summary>
        /// Gets a double setting from the database
        /// </summary>
        /// <param name="key">the key of the setting</param>
        /// <returns>the setting value</returns>
        internal static double GetDouble(string key) {
            double? value = GetSettingFromCache<double?>(key);
            if (value == null) {
                value = double.Parse(GetSettingFromDatabase(key));
                PutSettingInCache(key, value);
            }

            return (double)value;
        }

        /// <summary>
        /// Gets an string[] setting from the database
        /// </summary>
        /// <param name="key">the key of the setting</param>
        /// <returns>the setting value</returns>
        internal static string[] GetArray(string key) {
            string[] value = GetSettingFromCache<string[]>(key);
            if (value == null) {
                value = GetSettingFromDatabase(key).Split(';');
                PutSettingInCache(key, value);
            }
            
            return value;
        }

        /// <summary>
        /// Gets a Dictionary setting from the database
        /// </summary>
        /// <param name="key">the key of the setting</param>
        /// <returns>the setting value</returns>
        internal static Dictionary<string, string> GetDictionary(string key) {
            Dictionary<string, string> value = GetSettingFromCache<Dictionary<string, string>>(key);
            if (value == null) {
                value = new Dictionary<string,string>();

                string[] pieces, sDictionary = GetSettingFromDatabase(key).Split(';');
                foreach(string s in sDictionary){
                    pieces = s.Split(':');
                    if(pieces.Length == 2)
                        value.Add(pieces[0], pieces[1]);
                }

                PutSettingInCache(key, value);
            }

            return value;
        }

        /// <summary>
        /// Formats a DateTime object in the format specified in settings
        /// </summary>
        /// <param name="dt">the DateTime to format</param>
        /// <returns>a datetime string</returns>
        internal static string FormatDateTime(DateTime dt) {
            return string.Format("{0:" + GetString("DateTimeFormat") + "}", dt);
        }

        private static T GetSettingFromCache<T>(string key) {
            T value = default(T);

            object oSettings = Cache.Get(CACHE_TYPE, CACHE_KEY);
            Dictionary<string, object> settings = (oSettings == null ? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) : (Dictionary<string, object>)oSettings);

            if (settings.ContainsKey(key))
                value = (T)settings[key];

            return value;
        }

        private static void PutSettingInCache(string key, object value) {
            lock (_lock) {
                object oSettings = Cache.Get(CACHE_TYPE, CACHE_KEY);
                Dictionary<string, object> settings = (oSettings == null ? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) : (Dictionary<string, object>)oSettings);

                settings[key] = value;

                Cache.Add(CACHE_TYPE, CACHE_KEY, settings);
            }
        }

        private static string GetSettingFromDatabase(string key) {
            string value = null;

            using (LegionLinqDataContext db = new LegionLinqDataContext(ConfigurationManager.ConnectionStrings["LegionConnectionString"].ToString())) {
                db.xspGetSettingByKey(key, ref value);
            }

            return value;
        }
    }
}
