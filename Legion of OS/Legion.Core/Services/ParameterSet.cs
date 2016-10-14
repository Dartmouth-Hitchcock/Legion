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
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

using Legion.Core.Exceptions;

namespace Legion.Core.Services {

    /// <summary>
    /// Parameter types known to ParameterSet
    /// </summary>
    public enum ParameterType {
        /// <summary>
        /// Boolean
        /// </summary>
        Bool,
        /// <summary>
        /// Double
        /// </summary>
        Double,
        /// <summary>
        /// DateTime
        /// </summary>
        DateTime,
        /// <summary>
        /// Integer
        /// </summary>
        Int,
        /// <summary>
        /// No type
        /// </summary>
        None,
        /// <summary>
        /// String (Default)
        /// </summary>
        String,
        /// <summary>
        /// Xml
        /// </summary>
        Xml
    }

    /// <summary>
    /// Manages paramters passed into a service
    /// </summary>
    public class ParameterSet {
        private Dictionary<string, string> _params;
        private Dictionary<string, Parameter> _parsedParams = new Dictionary<string, Parameter>(StringComparer.OrdinalIgnoreCase);

        #region operator overloads

        /// <summary>
        /// Gets a raw parameter value
        /// </summary>
        /// <param name="key">the key to get</param>
        /// <returns>a raw string parameter value</returns>
        public string this[string key] {
            get { return _params[key]; }
        }

        #endregion

        #region accessors

        /// <summary>
        /// The raw dictionary of unparsed parameters
        /// </summary>
        public Dictionary<string, string> Parameters {
            get { return _params; }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p">The Dictionary of parameters from the service request</param>
        public ParameterSet(Dictionary<string, string> p) {
            if (p.Comparer != StringComparer.OrdinalIgnoreCase) {
                try {
                    _params = new Dictionary<string, string>(p, StringComparer.OrdinalIgnoreCase);
                }
                catch (ArgumentException) {
                    _params = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach(KeyValuePair<string, string> kvp in p)
                        _params[kvp.Key] = kvp.Value;
                }
            }
            else
                _params = p;
        }

        #endregion

        #region gets

        /// <summary>
        /// Gets a bool parameter value
        /// </summary>
        /// <param name="key">the key to get</param>
        /// <returns>a parsed bool from the ParameterSet</returns>
        public bool GetBool(string key) {
            if (!TryParse(ParameterType.Bool, key))
                throw new ParameterTypeException(ParameterType.Bool, key);

            return (bool)_parsedParams[key].Value;
        }

        /// <summary>
        /// Gets a DateTime parameter value
        /// </summary>
        /// <param name="key">the key to get</param>
        /// <returns>a parsed DateTime object from the ParameterSet</returns>
        public DateTime GetDateTime(string key) {
            if (!TryParse(ParameterType.DateTime, key))
                throw new ParameterTypeException(ParameterType.DateTime, key);

            return (DateTime)_parsedParams[key].Value;
        }

        /// <summary>
        /// Gets a double parameter value
        /// </summary>
        /// <param name="key">the key to get</param>
        /// <returns>a parsed double from the ParameterSet</returns>
        public double GetDouble(string key) {
            if (!TryParse(ParameterType.Double, key))
                throw new ParameterTypeException(ParameterType.Double, key);

            return (double)_parsedParams[key].Value;
        }

        /// <summary>
        /// Gets a int parameter value
        /// </summary>
        /// <param name="key">the key to get</param>
        /// <returns>a parsed int from the ParameterSet</returns>
        public int GetInt(string key) {
            if (!TryParse(ParameterType.Int, key))
                throw new ParameterTypeException(ParameterType.Int, key);

            return (int)_parsedParams[key].Value;
        }

        /// <summary>
        /// Gets a string parameter value
        /// </summary>
        /// <param name="key">the key to get</param>
        /// <returns>a parsed string object from the ParameterSet</returns>
        public string GetString(string key) {
            if (!TryParse(ParameterType.String, key))
                throw new ParameterTypeException(ParameterType.String, key);

            return (string)_parsedParams[key].Value;
        }

        /// <summary>
        /// Gets an XML parameter value
        /// </summary>
        /// <param name="key">the key to get</param>
        /// <returns>a parsed XML object from the ParameterSet</returns>
        public XmlDocument GetXml(string key) {
            if (!TryParse(ParameterType.Xml, key))
                throw new ParameterTypeException(ParameterType.Xml, key);

            return (XmlDocument)_parsedParams[key].Value;
        }

        #endregion

        #region verification

        /// <summary>
        /// Verifies that one of the keys in the specified list is present in the ParameterSet
        /// </summary>
        /// <param name="keys">The list of keys to verify</param>
        /// <returns>true if one of the keys is found, false otherwise</returns>
        public bool Verify(List<string> keys) {
            return this.Verify(keys.ToArray());
        }

        /// <summary>
        /// Verifies that one of the keys in the specified array is present in the ParameterSet
        /// </summary>
        /// <param name="keys">The array of keys to verify</param>
        /// <returns>true if one of the keys is found, false otherwise</returns>
        public bool Verify(string[] keys) {
            return this.Verify(new ParameterVerificationGroup(keys, true));
        }

        /// <summary>
        /// Verifies an array of ParameterVerificationGroups against the ParameterSet
        /// </summary>
        /// <param name="pvgs">The ParameterVerificationGroups to verify</param>
        /// <returns>true if all of the ParameterVerificationGroups verified, false otherwise</returns>
        public bool Verify(ParameterVerificationGroup[] pvgs) {
            foreach (ParameterVerificationGroup pvg in pvgs)
                if (!this.Verify(pvg))
                    return false;

            return true;
        }

        /// <summary>
        /// Verifies a ParameterVerificationGroup against the ParameterSet
        /// </summary>
        /// <param name="pvg">The ParameterVerificationGroup to verify</param>
        /// <returns>true if the ParameterVerificationGroup verifies, false otherwise</returns>
        public bool Verify(ParameterVerificationGroup pvg) {
            int count = 0;

            foreach (string key in pvg.Keys) {
                if (pvg.IsParameterRequired) {
                    if (!_params.ContainsKey(key))
                        continue;
                    if (pvg.IsValueRequired && _params[key].Length == 0)
                        continue;
                    if (_params[key].Length > 0 && !TryParse(pvg.GetParameterType(key), key))
                        continue;
                }
                else {
                    if (_params.ContainsKey(key)) {
                        if (pvg.IsValueRequired && _params[key].Length == 0)
                            continue;
                        if (_params[key].Length > 0 && !TryParse(pvg.GetParameterType(key), key))
                            continue;
                    }
                }

                if (++count >= pvg.MinimumValidParameters)
                    return true;
            }

            return (count < pvg.MinimumValidParameters ? false : true);
        }

        #endregion

        #region parsing

        private bool TryParse(ParameterType type, string key) {
            bool valid = false;

            if (_parsedParams.ContainsKey(key) && _parsedParams[key].Type == type)
                valid = true;
            else {
                if (_params.ContainsKey(key)) {
                    switch (type) {
                        case ParameterType.None:
                        case ParameterType.String:
                            _parsedParams[key] = new Parameter(ParameterType.String, _params[key]);
                            valid = true;
                            break;
                        case ParameterType.Bool:
                            bool tpBool;
                            if (Boolean.TryParse(_params[key], out tpBool)) {
                                _parsedParams[key] = new Parameter(ParameterType.Bool, tpBool);
                                valid = true;
                            }
                            break;
                        case ParameterType.DateTime:
                            DateTime tpDateTime;
                            if (DateTime.TryParse(_params[key], out tpDateTime)) {
                                _parsedParams[key] = new Parameter(ParameterType.DateTime, tpDateTime);
                                valid = true;
                            }
                            break;
                        case ParameterType.Double:
                            double tpDouble;
                            if (Double.TryParse(_params[key], out tpDouble)) {
                                _parsedParams[key] = new Parameter(ParameterType.Double, tpDouble);
                                valid = true;
                            }
                            break;
                        case ParameterType.Int:
                            int tpInt;
                            if (Int32.TryParse(_params[key], out tpInt)) {
                                _parsedParams[key] = new Parameter(ParameterType.Int, tpInt);
                                valid = true;
                            }
                            break;
                        case ParameterType.Xml:
                            XmlDocument dom = new XmlDocument();
                            try {
                                dom.LoadXml(_params[key]);
                                _parsedParams[key] = new Parameter(ParameterType.Xml, dom);
                                valid = true;
                            }
                            catch (Exception) { }
                            break;
                        default:
                            return false;
                    }
                }
            }

            return valid;
        }

        #endregion

        /// <summary>
        /// Checks if the parameter set contains the specified key
        /// </summary>
        /// <param name="key">the key to check for</param>
        /// <returns>true if the key is found, false otherwise</returns>
        public bool ContainsKey(string key) {
            return _params.ContainsKey(key);
        }

        /// <summary>
        /// Checks if a parameter is of the specified type
        /// </summary>
        /// <param name="type">the type to check for</param>
        /// <param name="key">the key of the parameter to look for</param>
        /// <returns>true if the parameter is of the specified type, false otherwise</returns>
        public bool CheckParameterType(ParameterType type, string key) {
            return TryParse(type, key);
        }

        /// <summary>
        /// Get a string representation of this ParameterSet
        /// </summary>
        /// <returns>Get a string representation of this ParameterSet</returns>
        public override string ToString() {
            string s = string.Empty;

            foreach (KeyValuePair<string, string> kvp in _params)
                s += string.Format("{0}: {1}\n\n", kvp.Key, (Settings.GetArray("ParameterSetLoggingHiddenParameters").Any(p => kvp.Key.ToLower().Contains(p)) ? "********" : kvp.Value));

            return s;
        }

        /// <summary>
        /// Get an XML representation of this ParameterSet
        /// </summary>
        /// <returns>an XML representation of this ParameterSet</returns>
        public XmlDocument ToXml(){
            string key, val;

            XmlDocument dom = new XmlDocument();
            XmlElement parameter, root = (XmlElement)dom.AppendChild(dom.CreateElement("parameters"));

            SortedDictionary<string, string> sortedParams = new SortedDictionary<string,string>(_params, StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, string> kvp in sortedParams) {
                if (kvp.Key.Length <= 2 || kvp.Key.Substring(0, 2) != "__") {
                    key = kvp.Key.ToLower();
                    val = (Settings.GetArray("ParameterSetLoggingHiddenParameters").Any(p => kvp.Key.ToLower().Contains(p)) ? "********" : kvp.Value);

                    try {
                        parameter = (XmlElement)root.AppendChild(dom.CreateElement(key));
                        parameter.AppendChild(dom.CreateCDataSection(val));
                    }
                    catch (XmlException) {
                        parameter = (XmlElement)root.AppendChild(dom.CreateElement("parameter"));
                        parameter.SetAttribute("badname", "true");
                        parameter.AppendChild(dom.CreateElement("key")).AppendChild(dom.CreateCDataSection(key));
                        parameter.AppendChild(dom.CreateElement("value")).AppendChild(dom.CreateCDataSection(val));
                    }
                }
            }

            return dom;
        }

        /// <summary>
        /// Computes the SHA256 hash of the ParameterSet
        /// </summary>
        /// <returns></returns>
        public byte[] GetHash() {
            SHA256Managed crypt = new SHA256Managed();
            return crypt.ComputeHash(Encoding.UTF8.GetBytes(ToXml().OuterXml));
        }

        private class Parameter {
            public object Value;
            public ParameterType Type;

            public Parameter(ParameterType type, object value) {
                Value = value;
                Type = type;
            }
        }
    }
}
