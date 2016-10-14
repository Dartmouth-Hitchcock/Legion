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
using System.Threading.Tasks;

namespace Legion.Core.Exceptions {

    /// <summary>
    /// Thrown when a service setting is requested by not found
    /// </summary>
    public class SettingNotFoundException : KeyNotFoundException {

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="settingKey">the setting key</param>
        public SettingNotFoundException(string settingKey)
            : base(Legion.Core.Settings.GetString("ExceptionMessageServiceSettingNotFound", new Dictionary<string, string>(){
                    {"SettingKey", settingKey}
            })) { }
    }
}
