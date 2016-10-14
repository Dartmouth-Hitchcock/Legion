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
using System.Text;

namespace Legion.Core.Services {

    /// <summary>
    /// A group of parameters to verify
    /// </summary>
    public class ParameterVerificationGroup {
        private string[] _keys;
        private int _minimumValidParameters;
        private bool _isValueRequired;
        private bool _isParameterRequired;
        private ParameterType _type;
        private Dictionary<string, ParameterType> _parameters;

        /// <summary>
        /// The group of keys to verify
        /// </summary>
        public string[] Keys{
            get { return _keys; }
        }

        /// <summary>
        /// The minimum number of required keys which must be present
        /// </summary>
        public int MinimumValidParameters {
            get { return _minimumValidParameters; }
        }

        /// <summary>
        /// Are the parameters required to have a value to be valid
        /// </summary>
        public bool IsValueRequired {
            get { return _isValueRequired; }
        }

        /// <summary>
        /// Is this parameter required to be specified
        /// </summary>
        public bool IsParameterRequired {
            get { return _isParameterRequired; }
        }

        /// <summary>
        /// Has per-key parameter types
        /// </summary>
        internal bool HasMultipleParameterTypes {
            get { return (_parameters != null); }
        }

        /// <summary>
        /// The type the paramater must be
        /// </summary>
        public ParameterType Type {
            get { return _type; }
        }

        /// <summary>
        /// Gets the requirements definition for the ParemeterVerificationGroup
        /// </summary>
        public string Definition {
            get { return GetDefinition(); }
        }

        /// <summary>
        /// Constructor
        /// All keys are required, and are required to have a value
        /// </summary>
        /// <param name="key">the required key to validate</param>
        public ParameterVerificationGroup(string key) 
            : this(new string[] { key }, 1, true, true, ParameterType.None) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key">the required key to validate</param>
        /// <param name="isValueRequired">true if the parameter must have a value and not just exist</param>
        public ParameterVerificationGroup(string key, bool isValueRequired) 
            : this(new string[] { key }, 1, isValueRequired, true, ParameterType.None) { }

        /// <summary>
        /// Constructor
        /// All keys are required, and are required to have a value
        /// </summary>
        /// <param name="key">the required key to validate</param>
        /// <param name="type">the type of the parameter must validate as</param>
        public ParameterVerificationGroup(string key, ParameterType type) 
            : this(new string[] { key }, 1, true, true, type) { }

        /// <summary>
        /// Constructor
        /// All keys are required, and are required to have a value
        /// </summary>
        /// <param name="key">the required key to validate</param>
        /// <param name="isValueRequired">true if the parameter must have a value and not just exist</param>
        /// <param name="type">the type of the parameter must validate as</param>
        public ParameterVerificationGroup(string key, bool isValueRequired, ParameterType type) 
            : this(new string[] { key }, 1, isValueRequired, true, type) { }

        /// <summary>
        /// Constructor
        /// All keys are required, and are required to have a value
        /// </summary>
        /// <param name="key">the required key to validate</param>
        /// <param name="isValueRequired">true if the parameter must have a value and not just exist</param>
        /// <param name="isParameterRequired">true if the parameter is required to be specified</param>
        /// <param name="type">the type of the parameter must validate as</param>
        public ParameterVerificationGroup(string key, bool isValueRequired, bool isParameterRequired, ParameterType type)
            : this(new string[] { key }, 1, isValueRequired, isParameterRequired, type) { }

        /// <summary>
        /// Constructor
        /// All keys are required, and are required to have a value
        /// </summary>
        /// <param name="keys">the group of required keys</param>
        /// <param name="type">the type of the parameter must validate as</param>
        public ParameterVerificationGroup(string[] keys, ParameterType type) 
            : this(keys, keys.Length, true, true, type) { }

        /// <summary>
        /// Constructor
        /// All keys are required, and are required to have a value
        /// </summary>
        /// <param name="keys">the group of required keys</param>
        public ParameterVerificationGroup(string[] keys) 
            : this(keys, keys.Length, true, true, ParameterType.None) { }

        /// <summary>
        /// Constructor
        /// Keys are required to have a value to be counted
        /// </summary>
        /// <param name="keys">the group of keys to verify</param>
        /// <param name="minimum">the minimum number of required keys which must be present</param>
        public ParameterVerificationGroup(string[] keys, int minimum) 
            : this(keys, minimum, true, true, ParameterType.None) { }

        /// <summary>
        /// Constructor
        /// All keys are required to be present
        /// </summary>
        /// <param name="keys">the group of keys to verify</param>
        /// <param name="isValueRequired">true if the parameter must have a value and not just exist</param>
        public ParameterVerificationGroup(string[] keys, bool isValueRequired) 
            : this(keys, keys.Length, isValueRequired, true, ParameterType.None) { }

        /// <summary>
        /// Constructor
        /// All keys are required to be present
        /// </summary>
        /// <param name="keys">the group of keys to verify</param>
        /// <param name="minimumValidParameters">the minimum number of required keys which must be present</param>
        /// <param name="isValueRequired">true if the parameter must have a value and not just exist</param>
        public ParameterVerificationGroup(string[] keys, int minimumValidParameters, bool isValueRequired) 
            : this(keys, minimumValidParameters, isValueRequired, true, ParameterType.None) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keys">the group of keys to verify</param>
        /// <param name="minimumValidParameters">the minimum number of required keys which must be present</param>
        /// <param name="isValueRequired">true if the parameter must have a value and not just exist</param>
        /// <param name="type">the type of the parameter must validate as</param>
        public ParameterVerificationGroup(string[] keys, int minimumValidParameters, bool isValueRequired, ParameterType type)
            : this(keys, minimumValidParameters, isValueRequired, true, type) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keys">the group of keys to verify</param>
        /// <param name="minimumValidParameters">the minimum number of required keys which must be present</param>
        /// <param name="isValueRequired">true if the parameter must have a value and not just exist</param>
        /// <param name="isParameterRequired">true if the parameters is required to be specified</param>
        /// <param name="type">the type of the parameter must validate as</param>
        public ParameterVerificationGroup(string[] keys, int minimumValidParameters, bool isValueRequired, bool isParameterRequired, ParameterType type)
        {
            _keys = keys;
            _minimumValidParameters = minimumValidParameters;
            _isValueRequired = isValueRequired;
            _isParameterRequired = isParameterRequired;
            _type = type;
            _parameters = null;
        }

        /// <summary>
        /// Constrctor
        /// </summary>
        /// <param name="parameters">The parameters and their types to verify</param>
        public ParameterVerificationGroup(Dictionary<string, ParameterType> parameters)
            : this(parameters, 1, true) { }

        /// <summary>
        /// Constrctor
        /// </summary>
        /// <param name="parameters">The parameters and their types to verify</param>
        /// <param name="isValueRequired">true if the parameter must have a value and not just exist</param>
        public ParameterVerificationGroup(Dictionary<string, ParameterType> parameters, bool isValueRequired)
            : this(parameters, 1, isValueRequired) { }

        /// <summary>
        /// Constrctor
        /// </summary>
        /// <param name="parameters">The parameters and their types to verify</param>
        /// <param name="minimumValidParameters">the minimum number of required keys which must be present</param>
        public ParameterVerificationGroup(Dictionary<string, ParameterType> parameters, int minimumValidParameters)
            : this(parameters, minimumValidParameters, true) { }

        /// <summary>
        /// Constrctor
        /// </summary>
        /// <param name="parameters">The parameters and their types to verify</param>
        /// <param name="minimumValidParameters">the minimum number of required keys which must be present</param>
        /// <param name="isValueRequired">true if the parameter must have a value and not just exist</param>
        public ParameterVerificationGroup(Dictionary<string, ParameterType> parameters, int minimumValidParameters, bool isValueRequired)
            : this(parameters.Keys.ToArray(), minimumValidParameters, isValueRequired, ParameterType.None) {
            _parameters = parameters;
        }

        /// <summary>
        /// Gets the requirements definition for the ParemeterVerificationGroup
        /// </summary>
        /// <returns>the string definition of the ParameterVerificationGroup</returns>
        public string GetDefinition() {
            if (HasMultipleParameterTypes) {
                string parameters = string.Empty;
                foreach (KeyValuePair<string, ParameterType> p in _parameters)
                    parameters += string.Format("{0} ({1}), ", p.Key, p.Value.ToString().ToLower());

                parameters = parameters.Trim(new char[] { ',', ' ' });

                return Settings.GetString("ToStringFormatPVGMultipleTypes", new Dictionary<string, string>(){
                    {"MinimumParameterCount", (_keys.Length == _minimumValidParameters ? "All" : IntToWord(_minimumValidParameters))},
                    {"TotalParametersPlural", (_keys.Length == 1 ? string.Empty : "s")},
                    {"MinimumParametersPlural", (_minimumValidParameters == 1 ? "is" : "are")},
                    {"RequiredType", (_isValueRequired ? " to have a value" : string.Empty)},
                    {"Parameters", parameters}
                });
            }
            else if (_keys.Length == 1)
                return Settings.GetString("ToStringFormatPVGOne", new Dictionary<string, string>(){
                    {"ParameterType", (_type == ParameterType.None ? string.Empty : string.Format("{0} ", _type.ToString().ToLower()))},
                    {"ParameterName", _keys[0]},
                    {"RequiredType", (_isValueRequired ? " to have a value" : string.Empty)}
                });
            else if (_keys.Length > 1)
                return Settings.GetString("ToStringFormatPVGMany", new Dictionary<string, string>(){
                    {"MinimumParameterCount", (_keys.Length == _minimumValidParameters ? "All" : IntToWord(_minimumValidParameters))},
                    {"ParameterType", (_type == ParameterType.None ? string.Empty : string.Format("{0} ", _type.ToString().ToLower()))},
                    {"TotalParametersPlural", (_keys.Length == 1 ? string.Empty : "s")},
                    {"MinimumParametersPlural", (_minimumValidParameters == 1 ? "is" : "are")},
                    {"RequiredType", (_isValueRequired ? " to have a value" : string.Empty)},
                    {"Parameters", string.Join(", ", _keys)}
                });
            else
                return Settings.GetString("ToStringFormatPVGZero");
        }

        /// <summary>
        /// Get a string representation of this ParameterVerificationGroup (aka Definition)
        /// </summary>
        /// <returns>the definition of this ParameterVerificationGroup</returns>
        public override string ToString(){
            return GetDefinition();
        }

        internal ParameterType GetParameterType(string key){
            if (_parameters == null)
                return _type;
            else
                return _parameters[key];
        }

        private string IntToWord(int i) {
            if (i < Settings.GetArray("ToStringFormatIntegerNames").Length)
                return Settings.GetArray("ToStringFormatIntegerNames")[i];
            else
                return i.ToString();
        }
    }
}
