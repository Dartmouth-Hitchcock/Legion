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
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Legion.Core.Services {
    internal class RawRequest {
        private bool _isThreadSafe = false;

        private string _body;
        private NameValueCollection _querystring;
        private NameValueCollection _form;
        private NameValueCollection _servervariables;

        public string this[string key]{
            get {
                if (_form[key] != null)
                    return _form[key];
                else if (_querystring[key] != null)
                    return _querystring[key];
                else
                    return null;
            }
        }

        public string Body {
            get { return _body; }
        }

        public NameValueCollection Form {
            get { return _form; }
        }

        public NameValueCollection QueryString {
            get { return _querystring; }
        }

        public NameValueCollection ServerVariables {
            get { return _servervariables; }
        }

        public RawRequest(System.Web.HttpRequest originalRequest) {
            _querystring = originalRequest.QueryString;
            _form = originalRequest.Form;
            _servervariables = originalRequest.ServerVariables;
            _body = GetDocumentContents(originalRequest);
        }

        /// <summary>
        /// Makes the Request object safe for use in threads by internalizing members
        /// </summary>
        public void MakeThreadSafe() {
            if (!_isThreadSafe) {
                //copy the  collections from the HttpContext members they are currently referencing
                _querystring = new NameValueCollection(_querystring);
                _form = new NameValueCollection(_form);
                _servervariables = new NameValueCollection(_servervariables);

                _isThreadSafe = true;
            }
        }

        private string GetDocumentContents(System.Web.HttpRequest request) {
            string body;

            using (Stream receiveStream = request.InputStream) {
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8)) {
                    body = readStream.ReadToEnd();
                }
            }

            return body;
        }
    }
}
