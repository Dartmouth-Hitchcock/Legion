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
using System.Xml;

using Newtonsoft.Json;

namespace Legion.Core.Services.Tools {
    /// <summary>
    /// Converts data from one format to another
    /// </summary>
    public static class DataConversion {

        /// <summary>
        /// Convert from JSON to XML
        /// </summary>
        /// <param name="sJson">The original JSON string</param>
        /// <returns>An XmlDocument object</returns>
        public static XmlDocument JsonToXml(string sJson) {
            return JsonToXml(sJson, "json");
        }

        /// <summary>
        /// Convert from JSON to XML
        /// </summary>
        /// <param name="sJson">The original JSON string</param>
        /// <param name="root">The root node to use when converting</param>
        /// <returns>An XmlDocument object</returns>
        public static XmlDocument JsonToXml(string sJson, string root) {
            return JsonConvert.DeserializeXmlNode(sJson, root);
        }

        /// <summary>
        /// Converts from XML to JSON
        /// </summary>
        /// <param name="sXml">The original XML string</param>
        /// <returns>A JSON string</returns>
        public static string XmlToJson(string sXml) {
            return JsonConvert.SerializeObject(sXml);
        }

        /// <summary>
        /// Converts from XML to JSON
        /// </summary>
        /// <param name="dom">The origin XML document</param>
        /// <returns>A JSON string</returns>
        public static string XmlToJson(XmlDocument dom) {
            return JsonConvert.SerializeXmlNode(dom, Newtonsoft.Json.Formatting.None, true);
        }

    }
}