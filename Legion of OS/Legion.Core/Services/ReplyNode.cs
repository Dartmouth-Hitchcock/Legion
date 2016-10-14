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

namespace Legion.Core.Services {

    /// <summary>
    /// A node in the Reply object
    /// </summary>
    public class ReplyNode {
        private string _nodename;
        private XmlDocument _dom;

        /// <summary>
        /// The base XmlElement
        /// </summary>
        protected XmlElement _node = null;

        /// <summary>
        /// Gets the raw XmlElement backing the ReplyNode
        /// </summary>
        public XmlElement Raw {
            get { return _node; }
            set { _node.InnerXml = value.InnerXml; }
        }

        /// <summary>
        /// Constructor, defaults to visible empty node
        /// </summary>
        /// <param name="dom">The dom to create on</param>
        /// <param name="nodename">This node's name</param>
        public ReplyNode(XmlDocument dom, string nodename) : this(dom, nodename, false) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dom">The dom to create on</param>
        /// <param name="nodename">This node's name</param>
        /// <param name="hideIfEmpty">hide this node if it has no contents</param>
        public ReplyNode(XmlDocument dom, string nodename, bool hideIfEmpty){
            _dom = dom;
            _nodename = nodename;

            if(!hideIfEmpty)
                _node = (XmlElement)_dom.DocumentElement.AppendChild(_dom.CreateElement(_nodename));
        }

        /// <summary>
        /// Creates a new XmlElement of the Reply
        /// </summary>
        /// <param name="name">the name of the new element</param>
        /// <returns>A new XmlElement of the Reply</returns>
        public XmlElement CreateElement(string name) {
            CheckBaseNodeExists();
            return _dom.CreateElement(name);
        }

        /// <summary>
        /// Adds a new element of the provided name to the Document Element
        /// </summary>
        /// <param name="name">the name of the new element</param>
        /// <returns>The new XmlElement</returns>
        public XmlElement AddElement(string name) {
            CheckBaseNodeExists();
            return AddElement(_node, name);
        }

        /// <summary>
        /// Adds a new element to the Reply's DocumentElement
        /// </summary>
        /// <param name="name">the name of the new element</param>
        /// <param name="value">the text value of the new element</param>
        /// <returns>The new XmlElement</returns>
        public XmlElement AddElement(string name, string value) {
            CheckBaseNodeExists();
            return AddElement(name, value, false);
        }

        /// <summary>
        /// Adds a new element to the Reply's DocumentElement
        /// </summary>
        /// <param name="name">the name of the new element</param>
        /// <param name="value">the text value of the new element</param>
        /// <param name="asCDATA">true if the value should be in a CDATA section</param>
        /// <returns>The new XmlElement</returns>
        public XmlElement AddElement(string name, string value, bool asCDATA) {
            CheckBaseNodeExists();
            return AddElement(_node, name, value, asCDATA);
        }

        /// <summary>
        /// Adds a new element of the provided name to the provided parent
        /// </summary>
        /// <param name="parent">the parent element to append to</param>
        /// <param name="name">the name of the new element</param>
        /// <returns>The new XmlElement</returns>
        public XmlElement AddElement(XmlElement parent, string name) {
            CheckBaseNodeExists();
            return (XmlElement)parent.AppendChild(CreateElement(name));
        }

        /// <summary>
        /// Adds a new element to the Reply's DocumentElement
        /// </summary>
        /// <param name="parent">the element to add to</param>
        /// <param name="name">the name of the new element</param>
        /// <param name="value">the text value of the new element</param>
        /// <returns>The new XmlElement</returns>
        public XmlElement AddElement(XmlElement parent, string name, string value) {
            CheckBaseNodeExists();
            return AddElement(parent, name, value, false);
        }

        /// <summary>
        /// Adds a new element to the Reply's DocumentElement
        /// </summary>
        /// <param name="parent">the element to add to</param>
        /// <param name="name">the name of the new element</param>
        /// <param name="value">the text value of the new element</param>
        /// <param name="asCDATA">true if the value should be in a CDATA section</param>
        /// <returns>The new XmlElement</returns>
        public XmlElement AddElement(XmlElement parent, string name, string value, bool asCDATA) {
            CheckBaseNodeExists();

            XmlElement element;

            element = _dom.CreateElement(name);

            if (asCDATA)
                element.AppendChild(_dom.CreateCDataSection(value));
            else
                element.InnerText = value;

            return (XmlElement)parent.AppendChild(element);
        }

        /// <summary>
        /// Adds an element to the Reply's DocumentElement
        /// </summary>
        /// <param name="element">the new element</param>
        /// <returns>The new XmlElement</returns>
        public XmlElement AddElement(XmlElement element) {
            CheckBaseNodeExists();
            return AddElement(_node, element);
        }

        /// <summary>
        /// Adds an element to the provided parent
        /// </summary>
        /// <param name="parent">the parent element to append to</param>
        /// <param name="element">the new element</param>
        /// <returns>The new XmlElement</returns>
        public XmlElement AddElement(XmlElement parent, XmlElement element) {
            CheckBaseNodeExists();
            return (XmlElement)parent.AppendChild(element);
        }

        /// <summary>
        /// Adds a new element to the Reply's DocumentElement as XML to the Document Element
        /// </summary>
        /// <param name="name">the name of the new element</param>
        /// <param name="value">the string XML value of thenew element</param>
        /// <returns>The new XmlElement</returns>
        public XmlElement AddElementXml(string name, string value) {
            return AddElementXml(_node, name, value);
        }

        /// <summary>
        /// Adds a new element to the Reply's DocumentElement as XML
        /// </summary>
        /// <param name="parent">the element to add to</param>
        /// <param name="name">the name of the new element</param>
        /// <param name="value">the string XML value of thenew element</param>
        /// <returns>The new XmlElement</returns>
        public XmlElement AddElementXml(XmlElement parent, string name, string value) {
            CheckBaseNodeExists();

            XmlElement element;

            element = _dom.CreateElement(name);
            element.InnerXml = value;

            return (XmlElement)parent.AppendChild(element);
        }

        /// <summary>
        /// Clears the ReplyNode
        /// </summary>
        public void Clear() {
            CheckBaseNodeExists();
            _node.RemoveAll();
        }

        /// <summary>
        /// Checks that the base node has been created and if not, creates it
        /// </summary>
        protected bool CheckBaseNodeExists() {
            if (_node == null) {
                _node = (XmlElement)_dom.DocumentElement.AppendChild(_dom.CreateElement(_nodename));
                return false;
            }
            else
                return true;
        }
    }
}
