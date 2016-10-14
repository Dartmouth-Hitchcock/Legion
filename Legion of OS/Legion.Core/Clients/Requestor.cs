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
using System.Net;
using System.Text.RegularExpressions;

using Legion.Core.Modules;
using Legion.Core.Services;

namespace Legion.Core.Clients {

    /// <summary>
    /// Details about the incoming reqestor
    /// </summary>
    public class Requestor {
        private const string IP_ADDRESS_VALIDATOR = @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";

        private ParameterSet _parameters = null;
        private Account _account = null;
        private string _clientipaddress = null, _hostipaddress = null, _hostname = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="request">The incoming request</param>
        public Requestor(Request request) {
            string requestIpAddress = ClientDetails.Module.IpAddress(request.Raw.ServerVariables);

            _parameters = request.ParameterSet;

            if (_parameters.ContainsKey(Settings.GetString("ParameterKeyRequestorImpersonateIp")) && ClientDetails.Module.IsDatacenter(requestIpAddress) && IsValidIpAddress(_parameters[Settings.GetString("ParameterKeyRequestorImpersonateIp")]))
                _hostipaddress = _parameters[Settings.GetString("ParameterKeyRequestorImpersonateIp")];
            else
                _hostipaddress = requestIpAddress;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clientipaddress">The IP address of the calling user</param>
        /// <param name="hostipaddress">The IP address of the calling host</param>
        /// <param name="hostname">The DNS hostname of the calling host</param>
        /// <param name="parameterSet">The ParameterSet passed in with the request</param>
        public Requestor(string clientipaddress, string hostipaddress, string hostname, ParameterSet parameterSet) {
            _clientipaddress = clientipaddress;
            _hostipaddress = hostipaddress;
            _hostname = hostname;
            _parameters = parameterSet;
        }

        /// <summary>
        /// The authenticated account making this call
        /// Note: null if no auth token passed
        /// </summary>
        public Account Account {
            get {
                if (_account == null && _parameters != null && _parameters.ContainsKey(Settings.GetString("ParameterKeyRequestorAuthToken")))
                    _account = Account.Retrieve(_parameters[Settings.GetString("ParameterKeyRequestorAuthToken")], ClientIPAddress);
                
                return _account;
            }
        }

        /// <summary>
        /// The IP address of the client originating the method request
        /// </summary>
        public string ClientIPAddress {
            get {
                if (_clientipaddress == null) {
                    if (_parameters.Parameters.ContainsKey(Settings.GetString("ParameterKeyRequestorUserIp")))
                        _clientipaddress = _parameters.Parameters[Settings.GetString("ParameterKeyRequestorUserIp")];
                    else if (_parameters.Parameters.ContainsKey(Settings.GetString("ParameterKeyRequestorUserCall")) && !_parameters.GetBool(Settings.GetString("ParameterKeyRequestorUserCall")))
                        _clientipaddress = HostIPAddress;
                }

                return _clientipaddress;
            }
        }

        /// <summary>
        /// The IP address of the client originating the method request
        /// </summary>
        [Obsolete("Use Requestor.ClientIpAddress instead")]
        public string UserIPAddress {
            get { return ClientIPAddress; }
        }

        /// <summary>
        /// The IP Address of the requester
        /// </summary>
        public string HostIPAddress {
            get { return _hostipaddress; }
        }

        /// <summary>
        /// The Hostname of the requester
        /// </summary>
        public string Hostname {
            get {
                if (_hostname == null)
                    _hostname = Dns.GetHostEntry(System.Net.IPAddress.Parse(HostIPAddress)).HostName.ToLower();

                return _hostname;
            }
        }

        /// <summary>
        /// Returns true if the calling host is internal to DH
        /// </summary>
        public bool IsHostInternal {
            get { return ClientDetails.Module.IsInternal(_hostipaddress); }
        }

        /// <summary>
        /// Gets the user's IP from a request object
        /// </summary>
        /// <param name="request">The request object</param>
        /// <returns>The user's IP address</returns>
        internal static string GetUserIPAddress(RawRequest request) {
            return request.Form[Settings.GetString("ParameterKeyRequestorUserIp")] ?? ClientDetails.Module.IpAddress();
        }

        private static bool IsValidIpAddress(string sIp) {
            return (new Regex(IP_ADDRESS_VALIDATOR)).IsMatch(sIp.Trim());
    }
    }
}
