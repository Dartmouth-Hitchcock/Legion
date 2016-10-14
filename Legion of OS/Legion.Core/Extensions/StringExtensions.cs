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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;

namespace Legion.Core.Extensions {
    public static class StringExtensions {

        /// <summary>
        /// Builds a string using the suplied variables
        /// </summary>
        /// <param name="var">Variabl eto replace</param>
        /// <param name="val">Value to replace with</param>
        /// <returns>The built string</returns>
        public static string Build(this string s, string var, string val) {
            if (s != null && var != null) {
                if (val == null)
                    val = "";

                s = s.Replace("{" + var + "}", val);
            }

            return s;
        }

        /// <summary>
        /// Builds a string using the suplied variables
        /// </summary>
        /// <param name="vars">Variables to build into the string</param>
        /// <returns>The built string</returns>
        public static string Build(this string s, Dictionary<string, string> vars) {
            if (s != null) {
                foreach (KeyValuePair<string, string> var in vars) {
                    s = s.Build(var.Key, var.Value);
                }
            }

            return s;
        }

        /// <summary>
        /// Replaves the first occurrece of oldValue with newValue
        /// </summary>
        /// <param name="s">the string to replace on</param>
        /// <param name="oldValue">the string to be replaced</param>
        /// <param name="newValue">the string to replace oldValue</param>
        /// <returns>the new string</returns>
        public static string ReplaceFirst(this string s, string oldValue, string newValue) {
            int index = s.IndexOf(oldValue);

            if (index == -1)
                return s;
            else
                return s.Remove(index, oldValue.Length).Insert(index, newValue);
        }

        /// <summary>
        /// Replaves the last occurrece of oldValue with newValue
        /// </summary>
        /// <param name="s">the string to replace on</param>
        /// <param name="oldValue">the string to be replaced</param>
        /// <param name="newValue">the string to replace oldValue</param>
        /// <returns>the new string</returns>
        public static string ReplaceLast(this string s, string oldValue, string newValue) {
            int index = s.LastIndexOf(oldValue);

            if (index == -1)
                return s;
            else
                return s.Remove(index, oldValue.Length).Insert(index, newValue);
        }

        /// <summary>
        /// Converts an XML string into an XmlDocument
        /// </summary>
        /// <param name="s">the string to convert</param>
        /// <returns>a new XmlDocument</returns>
        public static XmlDocument ToXml(this string s) {
            XmlDocument document = new XmlDocument();
            document.LoadXml(s);
            return document;
        }

        /// <summary>
        /// Transforms the xml string using the supplied xslt and returns the result of the transformation
        /// </summary>
        /// <param name="xml">The xml to be transformed</param>
        /// <param name="xslt">The xslt to transform the xml</param>
        /// <returns>The transformed xml</returns>
        public static string Transform(this string xml, string xslt) {
            // Simple data checks
            if (string.IsNullOrEmpty(xml))
                throw new ArgumentException("Param cannot be null or empty", "xml");
            if (string.IsNullOrEmpty(xslt))
                throw new ArgumentException("Param cannot be null or empty", "xslt");

            // Create required readers for working with xml and xslt
            StringReader xsltInput = new StringReader(xslt);
            StringReader xmlInput = new StringReader(xml);
            XmlTextReader xsltReader = new XmlTextReader(xsltInput);
            XmlTextReader xmlReader = new XmlTextReader(xmlInput);

            // Create required writer for output
            StringWriter stringWriter = new StringWriter();
            XmlTextWriter transformedXml = new XmlTextWriter(stringWriter);

            // Create a XslCompiledTransform to perform transformation
            XslCompiledTransform xsltTransform = new XslCompiledTransform();

            xsltTransform.Load(xsltReader);
            xsltTransform.Transform(xmlReader, transformedXml);

            return stringWriter.ToString();
        }

        /// <summary>
        /// Truncates a string
        /// </summary>
        /// <param name="s">this string</param>
        /// <param name="length">the length to truncate to</param>
        /// <returns>the first 'length' characters of 's' or the entire string if shorter than 'length'</returns>
        public static string Truncate(this string s, int length) {
            return s.GetFirst(length);
        }

        /// <summary>
        /// Gets the first characters in a string
        /// </summary>
        /// <param name="s">this string</param>
        /// <param name="length">the length get</param>
        /// <returns>the first 'length' characters of 's' or the entire string if shorter than 'length'</returns>
        public static string GetFirst(this string s, int length) {
            if (s == null)
                return null;
            else if (s.Length <= length)
                return s;
            else
                return s.Substring(0, length);
        }

        /// <summary>
        /// Gets the last characters in a string
        /// </summary>
        /// <param name="s">this string</param>
        /// <param name="length">the length to get</param>
        /// <returns>the last 'length' characters of 's' or the entire string if longer than 'length'</returns>
        public static string GetLast(this string s, int length) {
            if (s == null)
                return null;
            else if (length >= s.Length)
                return s;
            else
                return s.Substring(s.Length - length);
        }
    }
}
