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
using System.Xml.Linq;

namespace Legion.Core.Extensions {
    internal static class XmlExtensions {
        /// <summary>
        /// Converts an XDocument to an XmlDocument
        /// </summary>
        /// <param name="document">the source XDocument</param>
        /// <returns>an XmlDocument</returns>
        public static XmlDocument ToXmlDocument(this XDocument document) {
            using (XmlReader reader = document.CreateReader()) {
                XmlDocument dom = new XmlDocument();
                dom.Load(reader);
                return dom;
            }
        }

        /// <summary>
        /// Converts an XmlDocument to an XElement
        /// </summary>
        /// <param name="document">the source XmlDocument</param>
        /// <returns>an XElement</returns>
        public static XElement ToXElement(this XmlDocument document) {
            using (XmlNodeReader reader = new XmlNodeReader(document)) {
                reader.MoveToContent();
                return XElement.Load(reader);
            }
        }

        /// <summary>
        /// Converts an XmlDocument to an XDocument
        /// </summary>
        /// <param name="document">the source XmlDocument</param>
        /// <returns>an XDocument</returns>
        public static XDocument ToXDocument(this XmlDocument document) {
            using (XmlNodeReader reader = new XmlNodeReader(document)) {
                reader.MoveToContent();
                return XDocument.Load(reader);
            }
        }

        /// <summary>
        /// Converts an XmlNode to an XElement
        /// </summary>
        /// <param name="node">The source XmlNode</param>
        /// <returns>an XElement</returns>
        public static XElement ToXElement(this XmlNode node) {
            XElement element = null;
            if (node != null)
                element = XElement.Parse(node.OuterXml);
            return element;
        }

        /// <summary>
        /// Gets the InnerXml of an XElement
        /// </summary>
        /// <param name="node">The source XElement</param>
        /// <returns>the string InnerXml</returns>
        public static string GetInnerXml(this XElement element) {
            XmlReader reader = element.CreateReader();
            reader.MoveToContent();
            return reader.ReadInnerXml();
        }

        /// <summary>
        /// Remove the XML Declaration node from the document if it exists
        /// </summary>
        /// <param name="dom">the target document</param>
        public static void RemoveDeclaration(this XmlDocument dom) {
            if (dom.FirstChild.NodeType == XmlNodeType.XmlDeclaration)
                dom.RemoveChild(dom.FirstChild);
        }

        /// <summary>
        /// Adds an XML Declaration node to the document if it does not exist
        /// </summary>
        /// <param name="dom">the target document</param>
        public static void AddDeclaration(this XmlDocument dom) {
            if (dom.FirstChild.NodeType != XmlNodeType.XmlDeclaration)
                dom.PrependChild(dom.CreateNode(XmlNodeType.XmlDeclaration, null, null));
        }
    }
}
