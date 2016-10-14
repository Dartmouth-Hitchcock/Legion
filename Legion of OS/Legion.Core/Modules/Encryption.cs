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

namespace Legion.Core.Modules {

    /// <summary>
    /// Encryption provider module
    /// </summary>
    public abstract class Encryption : ExternalFuntionalityModule {

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
        }

        /// <summary>
        /// The reference to the module
        /// </summary>
        public static Encryption Module {
            get { return ExternalFuntionalityModule.GetModule("Encryption") as Encryption; }
        }

        /// <summary>
        /// Encypts the ClearText of the Packet
        /// </summary>
        /// <param name="key">the key to use to encrypt</param>
        /// <param name="packet">the Packet to encrypt</param>
        /// <returns>The Packet with its CipherBytes populated</returns>
        protected abstract Packet Encrypt(string key, Packet packet);

        /// <summary>
        /// Decrypts the CipherBytes of the Packet
        /// </summary>
        /// <param name="key">the key to use to decrypt</param>
        /// <param name="packet">the Packet to decrypt</param>
        /// <returns>The Packet with its CleatText populated</returns>
        protected abstract Packet Decrypt(string key, Packet packet);

        /// <summary>
        /// Encypts the ClearText of the Packet
        /// </summary>
        /// <param name="packet">the Packet to encrypt</param>
        /// <returns>The Packet with its CipherBytes populated</returns>
        public Packet EncryptString(Packet packet) {
            if (!string.IsNullOrEmpty(packet.ClearText))
                return Encrypt(Settings.GetString("EncryptionKey"), packet);
            else
                throw new EncryptionException("ClearText must be specified in the encyption packet.");
        }

        /// <summary>
        /// Decrypts the CipherBytes of the Packet
        /// </summary>
        /// <param name="packet">the Packet to decrypt</param>
        /// <returns>The Packet with its CleatText populated</returns>
        public Packet DecryptString(Packet packet) {
            if (!string.IsNullOrEmpty(packet.IV) && !string.IsNullOrEmpty(packet.CipherText))
                return Decrypt(Settings.GetString("EncryptionKey"), packet);
            else
                throw new EncryptionException("IV and CipherText must be specified in the encyption packet.");
        }
    }
}
