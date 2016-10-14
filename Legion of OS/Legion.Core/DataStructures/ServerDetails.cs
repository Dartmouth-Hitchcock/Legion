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
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Legion.Core.DataStructures {
    internal static class ServerDetails {
        /// <summary>
        /// List of this server's IP Addresses
        /// </summary>
        public static List<IPAddress> IPv4Addresses {
            get {
                List<IPAddress> list = new List<IPAddress>();

                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces()) {
                    foreach (var uipi in ni.GetIPProperties().UnicastAddresses) {
                        if (uipi.Address.AddressFamily == AddressFamily.InterNetwork)
                            if (uipi.IPv4Mask != null) //ignore 127.0.0.1
                                list.Add(uipi.Address);
                    }
                }

                return list;
            }
        }
    }
}
