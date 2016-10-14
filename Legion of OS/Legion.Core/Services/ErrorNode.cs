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

using Legion.Core.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Legion.Core.Services {
    
    /// <summary>
    /// Legion error node
    /// </summary>
    public class ErrorNode : ReplyNode {
        private XmlElement _xFriendly = null;
        private XmlElement _xDetailed = null;

        /// <summary>
        /// Has the error node been populated?
        /// </summary>
        public bool Exists {
            get { return (_node == null ? false : true); }
        }

        /// <summary>
        /// The ID of the error
        /// </summary>
        public int? Id {
            get { return (_node == null || _node.Attributes["id"] == null ? null : (int?)int.Parse(_node.Attributes["id"].Value)); }
            set {
                CheckMembersCreated();
                if (value == null)
                    _node.RemoveAttribute("id");
                else
                    _node.SetAttribute("id", value.ToString());
            }
        }

        /// <summary>
        /// The error code
        /// </summary>
        public string Code {
            get { return (_node == null || _node.Attributes["code"] == null ? null : _node.Attributes["code"].Value); }
            set {
                CheckMembersCreated();
                if (value == null)
                    _node.RemoveAttribute("code");
                else
                    _node.SetAttribute("code", value);
            }
        }

        /// <summary>
        /// A friendly (user readable) description of the error
        /// </summary>
        public string Friendly {
            get { return (_xFriendly == null ? string.Empty : _xFriendly.InnerText); }
            set {
                CheckMembersCreated();
                _xFriendly.InnerText = value;
            }
        }

        /// <summary>
        /// A detailed (technical) description of the error
        /// </summary>
        public string Detailed {
            get { return (_xDetailed == null ? string.Empty : _xDetailed.InnerText); }
            set {
                CheckMembersCreated();
                _xDetailed.InnerText = value;
            }
        }

        /// <summary>
        /// Build error from Exception
        /// </summary>
        public Exception Exception {
            set {
                Friendly = value.Message;
                Detailed = value.StackTrace;

//                if (value is ThanatosException)
//                    Id = ((ThanatosException)value).EventId;
            }
        }

        /// <summary>
        /// The text of the error
        /// </summary>
        [Obsolete("Use ErrorNode.Friendly instead")]
        public string Text {
            get { return Friendly; }
            set { Friendly = value; }
        }

        /// <summary>
        /// The XML of the error
        /// </summary>
        public string Xml {
            get { return (_node == null ? string.Empty : _node.InnerXml); }
            set { _node.InnerXml = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dom">The dom to create on</param>
        /// <param name="nodename">This node's name</param>
        public ErrorNode(XmlDocument dom, string nodename) : base(dom, nodename, true) { }

        /// <summary>
        /// Sets the error node to this error
        /// </summary>
        /// <param name="error">the error to set</param>
        public void Set(ServiceToService.Error error) {
            Code = error.Code;
            Detailed = error.Detailed;
            Friendly = error.Friendly;
        }

        private void CheckMembersCreated() {
            if (!CheckBaseNodeExists()) {
                _xFriendly = AddElement(Settings.GetString("NodeNameErrorDescription"));
                _xFriendly.SetAttribute("type", Settings.GetString("NodeAttributeErrorDescriptionTypeFriendly"));

                _xDetailed = AddElement(Settings.GetString("NodeNameErrorDescription"));
                _xDetailed.SetAttribute("type", Settings.GetString("NodeAttributeErrorDescriptionTypeDetailed"));
            }
        }
    }
}
