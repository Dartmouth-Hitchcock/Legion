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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

using Legion.Core.Exceptions;
using Legion.Core.Clients;

namespace Legion.Core.Services {

    /// <summary>
    /// The request object to the Legion API
    /// </summary>
    public class Request {
        private const string CURRENT_REQUEST_ID_KEY = "__legion_requestid";

        private static ConcurrentDictionary<Guid, ConcurrentStack<Request>> _globalRequestStackCollecion = new ConcurrentDictionary<Guid, ConcurrentStack<Request>>();
         
        private Guid _id;
        private RawRequest _rawRequest = null;
        private ParameterSet _parameterSet = null;
        private Application _application = null;
        private APIKey _apiKey = null;
        private ServiceDetails _serviceDetails = null;
        private Requestor _requestor = null;
        private RateLimit _rateLimit = null;
        private bool _isServiceToService = false, _isProxied = false, _isComplete = false, _isTheadSafe = false;
        private string _referenceId = null, _serviceKey = null, _methodKey = null, _formatKey = null;

        private Method _method;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">The page variable</param>
        public Request(Page page) : this(page.Request) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="request">The incoming HttpRequest object</param>
        public Request(System.Web.HttpRequest request) {
            Id = Guid.NewGuid();

            _rawRequest = new RawRequest(request);

            if (APIKey != null && APIKey.IsValid)
                _application = APIKey.Application;

            PushRequestToGlobalStack(Id, this);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The unique ID generated for this request</param>
        /// <param name="apiKey">The API key used to make this request</param>
        /// <param name="servicekey">the key of the service to call</param>
        /// <param name="methodkey">the key of the method in the service to call</param>
        /// <param name="parameters">the parameters Dictionary to pass to the method</param>
        /// <param name="clientIpAddress">The IP address of the client making this request</param>
        /// <param name="hostIpAddress">The IP address of the host making this request</param>
        /// <param name="hostname">The name of the host making this request</param>
        /// <param name="isServiceToService">Is this a service to service request</param>
        internal Request(Guid? id, string apiKey, string servicekey, string methodkey, Dictionary<string, string> parameters, string clientIpAddress, string hostIpAddress, string hostname, bool isServiceToService)
            : this(id, apiKey, servicekey, methodkey, new ParameterSet(parameters), clientIpAddress, hostIpAddress, hostname, isServiceToService) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The unique ID generated for this request</param>
        /// <param name="apiKey">The API key used to make this request</param>
        /// <param name="servicekey">the key of the service to call</param>
        /// <param name="methodkey">the key of the method in the service to call</param>
        /// <param name="parameterSet">the ParameterSet to pass to the method</param>
        /// <param name="clientIpAddress">The IP address of the client making this request</param>
        /// <param name="hostIpAddress">The IP address of the host making this request</param>
        /// <param name="hostname">The name of the host making this request</param>
        /// <param name="isServiceToService">Is this a service to service request</param>
        internal Request(Guid? id, string apiKey, string servicekey, string methodkey, ParameterSet parameterSet, string clientIpAddress, string hostIpAddress, string hostname, bool isServiceToService) {
            if (id == null)
                throw new Exception("Invalid request id specified when creating service-to-service request object");
            else
                Id = (Guid)id;

            APIKey = new APIKey(apiKey);
            ServiceKey = servicekey;
            MethodKey = methodkey;
            _parameterSet = parameterSet;
            _requestor = new Requestor(clientIpAddress, hostIpAddress, hostname, parameterSet);
            _isServiceToService = isServiceToService;

            PushRequestToGlobalStack(Id, this);
        }

        /// <summary>
        /// The raw / unparsed incoming request
        /// </summary>
        internal RawRequest Raw {
            get { return _rawRequest; }
        }

        /// <summary>
        /// The unique identifier for this request
        /// </summary>
        public Guid Id {
            get { return _id; }
            private set {
                _id = value;

                if (HttpContext.Current != null)
                    HttpContext.Current.Items[CURRENT_REQUEST_ID_KEY] = value;
            }
        }

        /// <summary>
        /// The client supplied id of this request
        /// </summary>
        public string ReferenceId {
            get {
                if (ParameterSet.ContainsKey(Settings.GetString("ParameterKeyRequestId")))
                    _referenceId = ParameterSet.GetString(Settings.GetString("ParameterKeyRequestId"));

                return _referenceId;
            }
        }

        /// <summary>
        /// The service the application is requesting access to
        /// </summary>
        public string ServiceKey {
            get {
                if (_serviceKey == null) {
                    foreach (string key in Settings.GetArray("ParameterKeySetRequestService"))
                        if (_rawRequest[key] != null) {
                            _serviceKey = _rawRequest[key].ToLower();
                            break;
                        }
                }

                return _serviceKey;
            }
            internal set {
                _serviceKey = value.ToLower();
            }
        }

        /// <summary>
        /// The method the application is requesting access to
        /// </summary>
        public string MethodKey {
            get {
                if (_methodKey == null) {
                    foreach (string key in Settings.GetArray("ParameterKeySetRequestMethod"))
                        if (_rawRequest[key] != null) {
                            _methodKey = _rawRequest[key].ToLower();
                            break;
                        }
                }

                return _methodKey;
            }
            internal set {
                _methodKey = value.ToLower();
            }
        }

        /// <summary>
        /// The API key of the application requesting access
        /// </summary>
        public APIKey APIKey {
            get {
                if (_apiKey == null)
                    _apiKey = new APIKey(Request.GetApplicationKey(_rawRequest));

                return _apiKey;
            }
            internal set {
                _apiKey = value;
            }
        }

        /// <summary>
        /// The body of the request
        /// </summary>
        public string Body {
            get { return Raw.Body; }
        }

        /// <summary>
        /// The format the application is requesting the result in
        /// </summary>
        public string FormatKey {
            get {
                if (_rawRequest != null && _formatKey == null) {
                    foreach (string key in Settings.GetArray("ParameterKeySetRequestFormat"))
                        if (_rawRequest[key] != null)
                            _formatKey = _rawRequest[key].ToLower();
                }

                return _formatKey;
            }
        }

        /// <summary>
        /// Information about the incoming requestor
        /// </summary>
        public Requestor Requestor {
            get {
                if (_requestor == null)
                    _requestor = new Requestor(this);

                return _requestor;
            }
        }

        /// <summary>
        /// The rate limit to apply to this request
        /// </summary>
        public RateLimit RateLimit {
            get {
                if (_rateLimit == null)
                    _rateLimit = new RateLimit(Requestor, Application);

                return _rateLimit;
            }
        }

        /// <summary>
        /// Request originated from a Legion Service rather than a client
        /// </summary>
        public bool IsServiceToService {
            get { return _isServiceToService; }
        }

        /// <summary>
        /// Is this request currently safe for threading?
        /// </summary>
        public bool IsThreadSafe {
            get { return _isTheadSafe; }
        }

        /// <summary>
        /// Is this request proxied through a public server
        /// </summary>
        public bool IsProxied {
            get {
                if (ParameterSet.ContainsKey(Settings.GetString("ParameterKeyRequestIsProxied")))
                    bool.TryParse(ParameterSet[Settings.GetString("ParameterKeyRequestIsProxied")], out _isProxied);

                return _isProxied;
            }
        }

        /// <summary>
        /// Is this Request complete
        /// </summary>
        public bool IsComplete {
            get { return _isComplete; }
        }

        /// <summary>
        /// The set of parameters passed by the request
        /// </summary>
        public ParameterSet ParameterSet {
            get {
                if (_parameterSet == null) {
                    Dictionary<string, string> p = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    if (_rawRequest.Form.Count > 0 || _rawRequest.QueryString.Count > 0) {
                        NameValueCollection parameterCollection = new NameValueCollection(_rawRequest.Form);
                        parameterCollection.Add(_rawRequest.QueryString);

                        p = parameterCollection.Cast<string>()
                            .Select(s => new { Key = s, Value = parameterCollection[s] })
                            .Where(w => !(Settings.GetArray("ParameterKeySetRequestService").Contains(w.Key) || Settings.GetArray("ParameterKeySetRequestMethod").Contains(w.Key) || Settings.GetArray("ParameterKeySetRequestApplication").Contains(w.Key) || Settings.GetArray("ParameterKeySetRequestFormat").Contains(w.Key)))
                            .ToDictionary(d => d.Key, d => d.Value);

                        //foreach (string key in parameterCollection.AllKeys.Where(k => !(_serviceKeys.Contains(k) || _methodKeys.Contains(k) || _applicationKeys.Contains(k) || _formatKeys.Contains(k))))
                        //    p[key] = parameterCollection[key];
                    }

                    _parameterSet = new ParameterSet(p);
                }

                return _parameterSet;
            }
        }

        /// <summary>
        /// The application of the Request
        /// </summary>
        public Application Application {
            get {
                if (_application == null) {
                    if (APIKey != null)
                        _application = APIKey.Application;
                }

                return _application;
            }
        }

        /// <summary>
        /// Provides details about the service
        /// </summary>
        public ServiceDetails Service {
            get {
                if (_serviceDetails == null)
                    _serviceDetails = new ServiceDetails(Method.ServiceId, Method.ServiceKey);

                return _serviceDetails;
            }
        }

        /// <summary>
        /// The parameters of the request
        /// </summary>
        [Obsolete("Use ParameterSet instead")]
        public Dictionary<string, string> Params {
            get { return this.ParameterSet.Parameters; }
        }

        /// <summary>
        /// The IP address of the user originating the method request
        /// </summary>
        [Obsolete("Use Requestor.ClientIPAddress")]
        public string UserIPAddress {
            get { return Requestor.ClientIPAddress; }
        }

        /// <summary>
        /// The IP Address of the requester
        /// </summary>
        [Obsolete("Use Requestor.HostIPAddress")]
        public string HostIPAddress {
            get { return Requestor.HostIPAddress; }
        }

        /// <summary>
        /// the HostName of the requester
        /// </summary>
        [Obsolete("Use Requestor.Hostname")]
        public string HostName {
            get { return _requestor.Hostname; }
        }

        /// <summary>
        /// Returns true if the calling host is internal to DH
        /// </summary>
        [Obsolete("Use Requestor.IsInternal")]
        public bool IsHostInternal {
            get { return Requestor.IsHostInternal; }
        }

        /// <summary>
        /// The method being called by the request
        /// </summary>
        internal Method Method {
            get { return _method; }
            set { _method = value; }
        }

        /// <summary>
        /// Verifies that the specified parameters have been sent in the request
        /// </summary>
        /// <param name="keys">a list of parameters to check for</param>
        /// <returns>true if all specified parameters have been found, false otherwise</returns>
        [Obsolete("Use ParameterSet.Verify(List<string> keys) instead")]
        public bool VerifyParams(List<string> keys) {
            return this.ParameterSet.Verify(keys.ToArray());
        }

        /// <summary>
        /// Verifies that the specified parameters have been sent in the request
        /// </summary>
        /// <param name="keys">an array of parameters to check for</param>
        /// <returns>true if all specified parameters have been found, false otherwise</returns>
        [Obsolete("Use ParameterSet.Verify(string[] keys) instead")]
        public bool VerifyParams(string[] keys) {
            return this.ParameterSet.Verify(new ParameterVerificationGroup(keys));
        }

        /// <summary>
        /// A string representation of the Request
        /// </summary>
        /// <returns>a string representation of the request</returns>
        public override string ToString() {
            string parameters = string.Empty;
            foreach (KeyValuePair<string, string> kvp in ParameterSet.Parameters)
                parameters += string.Format("   {0}: {1}\n", kvp.Key, kvp.Value);

            string s = Settings.GetString("ToStringFormatRequest", new Dictionary<string, string>(){
                {"Id", Id.ToString()},
                {"ServiceKey", ServiceKey},
                {"MethodKey", MethodKey},
                {"FormatKey", FormatKey},
                {"ApplicationId", APIKey.Application.Id.ToString()},
                {"ApplicationName", APIKey.Application.Name},
                {"ApiKey", APIKey.Key},
                {"IsServiceToService", IsServiceToService.ToString()},
                {"ClientIpAddress", Requestor.ClientIPAddress},
                {"HostIpAddress", Requestor.HostIPAddress},
                {"IsHostInternal", Requestor.IsHostInternal.ToString()},
                {"Parameters", parameters}
            });

            return s;
        }

        /// <summary>
        /// An XML representation of the Request
        /// </summary>
        /// <returns>an XML string</returns>
        public string ToXml() {
            XmlDocument dom = new XmlDocument();
            XmlElement root = (XmlElement)dom.AppendChild(dom.CreateElement("request"));
            root.SetAttribute("id", Id.ToString());

            root.AppendChild(dom.CreateElement("servicekey")).InnerText = ServiceKey;
            root.AppendChild(dom.CreateElement("methodkey")).InnerText = MethodKey;
            root.AppendChild(dom.CreateElement("formatkey")).InnerText = FormatKey;

            XmlElement application = (XmlElement)root.AppendChild(dom.CreateElement("application"));
            application.AppendChild(dom.CreateElement("id")).InnerText = APIKey.Application.Id.ToString();
            application.AppendChild(dom.CreateElement("name")).InnerText = APIKey.Application.Name;
            application.AppendChild(dom.CreateElement("apikey")).InnerText = APIKey.Key;

            root.AppendChild(dom.CreateElement("isservicetoservice")).InnerText = IsServiceToService.ToString();
            root.AppendChild(dom.CreateElement("clientipaddress")).InnerText = Requestor.ClientIPAddress;
            root.AppendChild(dom.CreateElement("hostipaddress")).InnerText = Requestor.HostIPAddress;
            root.AppendChild(dom.CreateElement("ishostinternal")).InnerText = Requestor.IsHostInternal.ToString();


            XmlDocumentFragment xFragment = dom.CreateDocumentFragment();
            xFragment.InnerXml = ParameterSet.ToXml().OuterXml;

            root.AppendChild(xFragment);

            return dom.OuterXml;
        }

        /// <summary>
        /// Gets the current Legion.Services.Request object for the currently executing HTTP Request
        /// </summary>
        public static Request Current {
            get {
                if (HttpContext.Current == null)
                    throw new AsynchronousContextException(Settings.GetString("ExceptionMessageAsyncReferenceRequestCurrent"));
                else {
                    if (HttpContext.Current.Items.Contains(CURRENT_REQUEST_ID_KEY)) {
                        Guid id = (Guid)HttpContext.Current.Items[CURRENT_REQUEST_ID_KEY];
                        return GetRequest(id);
                    }
                    else
                        return null;
                }
            }
        }

        /// <summary>
        /// Gets the head Legion.Services.Request object for the currently executing HTTP Request
        /// </summary>
        /// <param name="id">the ID of the request to get</param>
        /// <returns>the head request</returns>
        public static Request GetRequest(Guid id) {
            ConcurrentStack<Request> requestStack;
            Request request = null;

            if (_globalRequestStackCollecion.TryGetValue(id, out requestStack))
                if(requestStack.Count > 0)
                    requestStack.TryPeek(out request);

            return request;
        }

        /// <summary>
        /// Completes the request and pops the request stack if this is a local call
        /// </summary>
        internal void Complete() {
            ConcurrentStack<Request> requestStack;
            if (_globalRequestStackCollecion.TryGetValue(Id, out requestStack)) {
                Request request;
                requestStack.TryPop(out request);

                if(requestStack.Count == 0) {
                    while (!_globalRequestStackCollecion.TryRemove(Id, out requestStack)) ;
                }
            }

            _isComplete = true;
        }

        /// <summary>
        /// Makes the Request object safe for use in threads by internalizing members
        /// </summary>
        internal void MakeThreadSafe() {
            if (!_isTheadSafe) {
                _rawRequest.MakeThreadSafe();
                _isTheadSafe = true;
            }
        }

        internal static string GetApplicationKey(RawRequest request) {
            string applicationKey = null;

            foreach (string key in Settings.GetArray("ParameterKeySetRequestApplication")) {
                if (request[key] != null) {
                    applicationKey = request[key];
                    break;
                }
            }

            return applicationKey;
        }

        internal static Guid? GetRequestId(HttpContext context) {
            if (HttpContext.Current.Items.Contains(CURRENT_REQUEST_ID_KEY))
                return (Guid)HttpContext.Current.Items[CURRENT_REQUEST_ID_KEY];
            else
                return null;
        }

        private static void PushRequestToGlobalStack(Guid id, Request request) {
            _globalRequestStackCollecion.GetOrAdd(id, new ConcurrentStack<Request>()).Push(request);
        }
    }
}
