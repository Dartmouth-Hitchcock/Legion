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
using System.Text;
using System.Threading.Tasks;

namespace Legion.Core.Services.Tools {

    /// <summary>
    /// Service tools interface to the global encryption module
    /// </summary>
    public class Encryption {

        /// <summary>
        /// The packet of encrypted data
        /// </summary>
        public struct Packet {
            /// <summary>
            /// The IV used for this packet
            /// </summary>
            public string IV;
            /// <summary>
            /// The encrypted cipher bytes
            /// </summary>
            public string CipherText;
            /// <summary>
            /// The unencrypted cleat text
            /// </summary>
            public string ClearText;

            /// <summary>
            /// Implicit cast
            /// </summary>
            /// <param name="p">the pacet to cast</param>
            public static implicit operator Modules.Encryption.Packet(Packet p) {
                return new Modules.Encryption.Packet() {
                    IV = p.IV,
                    CipherText = p.CipherText,
                    ClearText = p.ClearText
                };
            }

            /// <summary>
            /// Implicit cast
            /// </summary>
            /// <param name="p">the pacet to cast</param>
            public static implicit operator Packet(Modules.Encryption.Packet p) {
                return new Modules.Encryption.Packet() {
                    IV = p.IV,
                    CipherText = p.CipherText,
                    ClearText = p.ClearText
                };
            }
        }

        /// <summary>
        /// Encrypts the packet using the global encryption module
        /// </summary>
        /// <param name="packet">The packet to encrypt</param>
        /// <returns>The packet with it's CipherText member populated</returns>
        public Packet Encrypt(Packet packet) {
            if (!string.IsNullOrEmpty(packet.ClearText))
                return Modules.Encryption.Module.EncryptString(packet);
            else
                throw new EncryptionException("ClearText must be specified in the encyption packet.");
        }

        /// <summary>
        /// Decrypts the packet using the global encryption module
        /// </summary>
        /// <param name="packet">The packet to decrypt</param>
        /// <returns>The packet with it's ClearText member populated</returns>
        public Packet Decrypt(Packet packet) {
            if (!string.IsNullOrEmpty(packet.IV) && !string.IsNullOrEmpty(packet.CipherText))
                return Modules.Encryption.Module.DecryptString(packet);
            else
                throw new EncryptionException("IV and CipherText must be specified in the encyption packet.");
        }
    }
}
