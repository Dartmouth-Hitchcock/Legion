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
using System.Xml;

namespace Caesar.Legion.Extensions {
    public static class DictionaryExtensions {

        /// <summary>
        /// Deep-clones the dictionary
        /// </summary>
        /// <param name="d">this Dictionary</param>
        /// <returns>a clone of the dictionary</returns>
        public static Dictionary<string, string> Clone(this Dictionary<string, string> d) {
            return d.Clone<string, string>();
        }

        /// <summary>
        /// Deep-clones the dictionary
        /// </summary>
        /// <typeparam name="Tk">The Type of the key</typeparam>
        /// <typeparam name="Tv">the Type of the value</typeparam>
        /// <param name="d">this Dictionary</param>
        /// <returns>a clone of the dictionary</returns>
        public static Dictionary<Tk, Tv> Clone<Tk, Tv>(this Dictionary<Tk, Tv> d) where Tv : ICloneable {
            Dictionary<Tk, Tv> n = new Dictionary<Tk, Tv>(d.Count, d.Comparer);
            foreach (KeyValuePair<Tk, Tv> entry in d) {
                    if (entry.Value == null)
                        n.Add(entry.Key, default(Tv));
                    else
                        n.Add(entry.Key, (Tv)entry.Value.Clone());
            }

            return n;
        }

        /// <summary>
        /// Flattens the dictionary into a key=value string
        /// </summary>
        /// <param name="d">the Dictionary to flatten</param>
        /// <returns>the flattened string</returns>
        public static string Flatten(this Dictionary<string, string> d){
            string flat = "";

            if (d != null) {
                foreach (KeyValuePair<string, string> kvp in d)
                    flat += string.Format("{0}={1}&", kvp.Key, System.Web.HttpUtility.UrlEncode(kvp.Value));

                flat = flat.Trim('&');
            }

            return flat;
        }

        /// <summary>
        /// Serializes a Dictionary to an XmlDocument
        /// </summary>
        /// <returns>an XmlDocument</returns>
        public static XmlDocument ToXmlDocument(this Dictionary<string, string> d) {
            return d.ToXmlDocument("dictionary", false);
        }

        /// <summary>
        /// Serializes a Dictionary to an XmlDocument
        /// </summary>
        /// <param name="dictionaryElementName">the name of the dictionary elements</param>
        /// <returns>an XmlDocument</returns>
        public static XmlDocument ToXmlDocument(this Dictionary<string, string> d, string dictionaryElementName) {
            return d.ToXmlDocument(dictionaryElementName, false);
        }

        /// <summary>
        /// Serializes a Dictionary to an XmlDocument
        /// </summary>
        /// <param name="dictionaryElementName">the name of the dictionary elements</param>
        /// <param name="lowerCaseNodeName">Lower case the dictionary key becoming the node name</param>
        /// <returns>an XmlDocument</returns>
        public static XmlDocument ToXmlDocument(this Dictionary<string, string> d, string dictionaryElementName, bool lowerCaseNodeName) {
            XmlDocument dom = new XmlDocument();
            XmlNode xDictionary = dom.AppendChild(dom.CreateElement(dictionaryElementName));

            d.AddXmlDictionaryNode(dom, xDictionary, lowerCaseNodeName);

            return dom;
        }

        internal static void AddXmlDictionaryNode(this Dictionary<string, string> d, XmlDocument dom, XmlNode xDictionary, bool lowerCaseNodeName) {
            XmlNode xValue;
            foreach (KeyValuePair<string, string> kvp in d) {
                xValue = xDictionary.AppendChild(dom.CreateElement(lowerCaseNodeName ? kvp.Key.ToLower() : kvp.Key));
                xValue.AppendChild(dom.CreateCDataSection(kvp.Value));
            }
        }
    }
}
