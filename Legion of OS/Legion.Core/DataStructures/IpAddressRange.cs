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

using Legion.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Legion.Core.DataStructures {
    public class IpAddressRange {
        private List<IPRange> _ipRanges = new List<IPRange>();

        public IpAddressRange(string sUpper, string sLower) {
            IPRange range;
            IPAddress lower, upper;

            if (IPAddress.TryParse(sLower.Trim(), out lower) && IPAddress.TryParse(sUpper.Trim(), out upper)) {
                range = new IPRange(lower, upper);
                if (range.IsValid)
                    _ipRanges.Add(range);
                else
                    throw new IpAddressException(string.Format("Address families do not match: '{0}-{1}'", sLower, sUpper));
            }
            else
                throw new IpAddressException(string.Format("IP Address range not valid: '{0}-{1}'", sLower, sUpper));

        }

        public IpAddressRange(IPAddress lower, IPAddress upper) {
            IPRange range = new IPRange(lower, upper);
            if (range.IsValid)
                _ipRanges.Add(range);
            else
                throw new IpAddressException(string.Format("Address families do not match: '{0}-{1}'", lower.ToString(), upper.ToString()));
        }

        public IpAddressRange(string iprangeset) {
            IPAddress lower, upper;
            IPRange range;

            if (iprangeset != null) {
                string[] ipranges = iprangeset.Split(new char[] { ',', ';' });
                foreach (string iprange in ipranges) {
                    string[] bounds = iprange.Split('-');
                    if (bounds.Length == 1) {
                        if (IPAddress.TryParse(bounds[0].Trim(), out lower))
                            range = new IPRange(lower, lower);
                        else
                            throw new IpAddressException(string.Format("IP Address range not valid: '{0}'", iprange));
                    }
                    if (bounds.Length == 2) {
                        if (IPAddress.TryParse(bounds[0].Trim(), out lower) && IPAddress.TryParse(bounds[1].Trim(), out upper))
                            range = new IPRange(lower, upper);
                        else
                            throw new IpAddressException(string.Format("IP Address range not valid: '{0}'", iprange));
                    }
                    else
                        throw new IpAddressException(string.Format("IP Address range not valid: '{0}'", iprange));

                    if (range.IsValid)
                        _ipRanges.Add(range);
                    else
                        throw new IpAddressException(string.Format("Address families do not match: '{0}'", iprange));
                }
            }
        }

        public bool IsInRange(string sAddress) {
            IPAddress address;
            if (IPAddress.TryParse(sAddress.Trim(), out address) && IsInRange(address))
                return true;
            else
                return false;
        }

        public bool IsInRange(IPAddress address) {
            foreach (IPRange iprange in _ipRanges)
                if (iprange.IsInRange(address))
                    return true;

            return false;
        }

        private class IPRange {
            private AddressFamily _addressFamily;
            private byte[] _lower, _upper;
            private bool _isValid;

            public bool IsValid {
                get { return _isValid; }
            }

            public IPRange(IPAddress lower, IPAddress upper) {
                if (lower.AddressFamily != upper.AddressFamily)
                    _isValid = false;
                else {
                    _addressFamily = lower.AddressFamily;
                    _lower = lower.GetAddressBytes();
                    _upper = upper.GetAddressBytes();

                    _isValid = true;
                }
            }

            public bool IsInRange(IPAddress address) {
                if (address.AddressFamily != _addressFamily)
                    return false;

                byte[] addressBytes = address.GetAddressBytes();

                bool lowerBoundary = true, upperBoundary = true;

                for (int i = 0; i < this._lower.Length && (lowerBoundary || upperBoundary); i++) {
                    if ((lowerBoundary && addressBytes[i] < _lower[i]) || (upperBoundary && addressBytes[i] > _upper[i]))
                        return false;

                    lowerBoundary &= (addressBytes[i] == _lower[i]);
                    upperBoundary &= (addressBytes[i] == _upper[i]);
                }

                return true;
            }
        }
    }
}
