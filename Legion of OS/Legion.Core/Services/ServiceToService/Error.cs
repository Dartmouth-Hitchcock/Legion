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

using System.Xml;

namespace Legion.Core.Services.ServiceToService {
    /// <summary>
    /// A Legion Error
    /// </summary>
    public class Error {
        public XmlNode _xError;
        private string _code = null, _friendly = null, _detailed = null;

        /// <summary>
        /// The error's code
        /// </summary>
        public string Code {
            get {
                if (_code == null)
                    _code = (_xError.Attributes["code"] == null ? string.Empty : _xError.Attributes["code"].Value);

                return _code;
            }
        }

        /// <summary>
        /// A friendly description of the error
        /// </summary>
        public string Friendly {
            get {
                if (_friendly == null) {
                    XmlNode xFriendly = _xError.SelectSingleNode("./description[@type='friendly']");
                    _friendly = (xFriendly == null ? string.Empty : xFriendly.InnerText);
                }
                return _friendly;
            }
        }

        /// <summary>
        /// A detailed description of the error
        /// </summary>
        public string Detailed {
            get {
                if (_detailed == null) {
                    XmlNode xDetailed = _xError.SelectSingleNode("./description[@type='detailed']");
                    _detailed = (xDetailed == null ? string.Empty : xDetailed.InnerText);
                }

                return _detailed;
            }
        }

        internal Error(XmlNode xError) {
            _xError = xError;
        }

        public override string ToString() {
            return Friendly;
        }

        public static implicit operator string (Error e) {
            return e.ToString();
        }
    }
}
