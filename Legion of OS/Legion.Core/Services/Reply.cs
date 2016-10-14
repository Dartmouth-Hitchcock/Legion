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

using Newtonsoft.Json;

namespace Legion.Core.Services {

    /// <summary>
    /// A Reply to a Request
    /// </summary>
    public class Reply {
        private struct ATTRIBUTE_NAMES {
            public const string RequestId = "referenceid";
        }

        private XmlDocument _dom;
        private ReplyNode _response, _result;
        private ErrorNode _error = null;

        private string _replyFormat;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="request">The original Request</param>
        public Reply(Request request) : this(request.FormatKey){
            if (request.ReferenceId != null)
                _dom.DocumentElement.SetAttribute(ATTRIBUTE_NAMES.RequestId, request.ReferenceId);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="replyformat">The format to supply the Reply in</param>
        public Reply(string replyformat) {
            _dom = new XmlDocument();
            _dom.AppendChild(_dom.CreateNode(XmlNodeType.XmlDeclaration, "", ""));
            _dom.AppendChild(_dom.CreateElement("", Settings.GetString("NodeNameReply"), ""));

            _result = new ReplyNode(_dom, Settings.GetString("NodeNameResult"));
            _response = new ReplyNode(_dom, Settings.GetString("NodeNameResponse"));
            _error = new ErrorNode(_dom, Settings.GetString("NodeNameError"));

            _replyFormat = (replyformat == null ? "xml" : replyformat.ToLower());
        }

        /// <summary>
        /// The content type of the Reply
        /// </summary>
        public string ContentType {
            get {
                switch (_replyFormat) {
                    case "json":
                        return "application/json";
                    case "xml":
                        return "text/xml";
                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// The Response node of the Reply
        /// </summary>
        public ReplyNode Response {
            get { return _response; }
        }

        /// <summary>
        /// The Result node of the Reply, populated by the requested Method
        /// </summary>
        public ReplyNode Result {
            get { return _result; }
        }

        /// <summary>
        /// The Error node of the reply, populated by the requested Method if there is an error
        /// </summary>
        public ErrorNode Error {
            get { return _error; }
        }

        internal string Format {
            get { return _replyFormat; }
            set { _replyFormat = value; }
        }

        /// <summary>
        /// Flattens the Reply to an XML string
        /// </summary>
        /// <returns>The XML string representation of the object</returns>
        public override string ToString() {
            switch (_replyFormat) {
                case "json":
                    return JsonConvert.SerializeObject(_dom);
                case "xml":
                    return _dom.OuterXml;
                case "system":
                    if (_result.Raw != null)
                        return _result.Raw.InnerText;
                    else
                        return _dom.OuterXml;
                default:
                    return null;
            }
        }
    }
}
