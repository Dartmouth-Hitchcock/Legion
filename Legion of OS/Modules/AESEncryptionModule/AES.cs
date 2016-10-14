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
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AESEncryptionModule {

    /// <summary>
    /// AES Rijndael Encryption
    /// </summary>
    public sealed class AES {
        private const int IV_SIZE = 16;
        private const int KEY_SIZE = 32;

        private byte[] _key, _iv;
        private ICryptoTransform _encryptor, _decryptor;
        private static UTF8Encoding _characterEncoder = new UTF8Encoding();

        #region accessors

        /// <summary>
        /// The encryption key used by this AES instance
        /// </summary>
        public Randomness Key { get { return new Randomness(_key); } }

        /// <summary>
        /// The initialization vector used by this AES instance
        /// </summary>
        public Randomness IV { get { return new Randomness(_iv); } }

        #endregion

        #region constructors

        /// <summary>
        /// CONSTRUCTOR
        /// Initializes with a random IV
        /// </summary>
        /// <param name="key">The Base64 encoded 32-byte encryption key</param>
        public AES(string key) : this(Convert.FromBase64String(key), GenerateIV().GetBytes()) { }

        /// <summary>
        /// CONSTRUCTOR
        /// Initializes with a random IV
        /// </summary>
        /// <param name="key">The 32-byte encryption key</param>
        public AES(byte[] key) : this(key, GenerateIV().GetBytes()) { }

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="key">The Base64 encoded 32-byte encryption key</param>
        /// <param name="iv">The Base64 encoded 16-byte initialization vector</param>
        public AES(string key, string iv) : this(Convert.FromBase64String(key), Convert.FromBase64String(iv)) { }

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="key">The 32-byte encryption key</param>
        /// <param name="iv">The 16-byte initialization vector</param>
        public AES(byte[] key, byte[] iv) {
            if (key.Length != KEY_SIZE)
                throw new CryptographicException(string.Format("Key must have a length of {0} bytes.", KEY_SIZE));
            if (iv.Length != IV_SIZE)
                throw new CryptographicException(string.Format("IV must have a length of {0} bytes.", IV_SIZE));

            _key = key;
            _iv = iv;

            using (RijndaelManaged rm = new RijndaelManaged()) {
                _encryptor = rm.CreateEncryptor(_key, _iv);
                _decryptor = rm.CreateDecryptor(_key, _iv);
            }
        }

        #endregion

        #region encryption

        /// <summary>
        /// Encrypts the supplied text
        /// </summary>
        /// <param name="cleartext">The text to encrypt</param>
        /// <returns>The Base64 encoded string of the encrypted text</returns>
        public string EncryptString(string cleartext) {
            return EncryptString(cleartext, false);
        }

        /// <summary>
        /// Encrypts the supplied text
        /// </summary>
        /// <param name="cleartext">The text to encrypt</param>
        /// <param name="compress">Compress the cleartext prior to encryption</param>
        /// <returns>The Base64 encoded string of the encrypted text</returns>
        public string EncryptString(string cleartext, bool compress) {
            return Convert.ToBase64String(Encrypt(cleartext, compress));
        }

        /// <summary>
        /// Encrypts the supplied bytes
        /// </summary>
        /// <param name="clearbytes">The bytes to encrypt</param>
        /// <returns>The Base64 encoded string of the encrypted text</returns>
        public string EncryptString(byte[] clearbytes) {
            return EncryptString(clearbytes, false);
        }

        /// <summary>
        /// Encrypts the supplied bytes
        /// </summary>
        /// <param name="clearbytes">The bytes to encrypt</param>
        /// <param name="compress">Compress the clearbytes prior to encryption</param>
        /// <returns>The Base64 encoded string of the encrypted text</returns>
        public string EncryptString(byte[] clearbytes, bool compress) {
            return Convert.ToBase64String(Encrypt(clearbytes, compress));
        }

        /// <summary>
        /// Encrypts the specified cleartext
        /// </summary>
        /// <param name="cleartext">The text to encrypt</param>
        /// <returns>An array of encrypted bytes</returns>
        public byte[] Encrypt(string cleartext) {
            return Encrypt(cleartext, false);
        }

        /// <summary>
        /// Encrypts the specified cleartext
        /// </summary>
        /// <param name="cleartext">The text to encrypt</param>
        /// <param name="compress">Compress the cleartext prior to encryption</param>
        /// <returns>An array of encrypted bytes</returns>
        public byte[] Encrypt(string cleartext, bool compress) {
            return Encrypt(_characterEncoder.GetBytes(cleartext), compress);
        }

        /// <summary>
        /// Encrypts the supplied bytes
        /// </summary>
        /// <param name="clearbytes">The bytes to encrypt</param>
        /// <param name="compress">Compress the clearbytes prior to encryption</param>
        /// <returns>An array of encrypted bytes</returns>
        public byte[] Encrypt(byte[] clearbytes, bool compress) {
            byte[] cipherbytes;

            if (compress) {
                using (MemoryStream cleartextStream = new MemoryStream(clearbytes)) {
                    using (MemoryStream compressedCleartextStream = new MemoryStream()) {
                        using (GZipStream compressionStream = new GZipStream(compressedCleartextStream, CompressionMode.Compress)) {
                            cleartextStream.CopyTo(compressionStream);
                        }

                        clearbytes = compressedCleartextStream.ToArray();
                    }
                }

            }

            using (MemoryStream ms = new MemoryStream()) {
                using (CryptoStream cs = new CryptoStream(ms, _encryptor, CryptoStreamMode.Write)) {
                    cs.Write(clearbytes, 0, clearbytes.Length);
                    cs.FlushFinalBlock();

                    ms.Position = 0;
                    cipherbytes = new byte[ms.Length];
                    ms.Read(cipherbytes, 0, cipherbytes.Length);
                }
            }

            return cipherbytes;
        }

        #endregion

        #region decryption

        /// <summary>
        /// Decrypts the supplied Base64 encoded text
        /// </summary>
        /// <param name="ciphertext">The Base64 encoded text to decrypt</param>
        /// <returns>The decrypted string</returns>
        public string DecryptString(string ciphertext) {
            return DecryptString(ciphertext, false);
        }

        /// <summary>
        /// Decrypts the supplied Base64 encoded text
        /// </summary>
        /// <param name="ciphertext">The Base64 encoded text to decrypt</param>
        /// <param name="decompress">Decompress the cleartext after decryption</param>
        /// <returns>The decrypted string</returns>
        public string DecryptString(string ciphertext, bool decompress) {
            return _characterEncoder.GetString(Decrypt(ciphertext, decompress));
        }

        /// <summary>
        /// Decrypts the supplied bytes
        /// </summary>
        /// <param name="cipherbytes">The encrypted bytes to decrypt</param>
        /// <returns>The decrypted string</returns>
        public string DecryptString(byte[] cipherbytes) {
            return DecryptString(cipherbytes, false);
        }

        /// <summary>
        /// Decrypts the supplied bytes
        /// </summary>
        /// <param name="cipherbytes">The encrypted bytes to decrypt</param>
        /// <param name="decompress">Decompress the cleartext after decryption</param>
        /// <returns>The decrypted string</returns>
        public string DecryptString(byte[] cipherbytes, bool decompress) {
            return _characterEncoder.GetString(Decrypt(cipherbytes, decompress));
        }

        /// <summary>
        /// Decrypts the supplied Base64 encoded text
        /// </summary>
        /// <param name="ciphertext">The Base64 encoded text to decrypt</param>
        /// <returns>An array of decrypted bytes</returns>
        public byte[] Decrypt(string ciphertext) {
            return Decrypt(ciphertext, false);
        }

        /// <summary>
        /// Decrypts the supplied Base64 encoded text
        /// </summary>
        /// <param name="ciphertext">The Base64 encoded text to decrypt</param>
        /// <param name="decompress">Decompress the clearbytes after decryption</param>
        /// <returns>An array of decrypted bytes</returns>
        public byte[] Decrypt(string ciphertext, bool decompress) {
            return Decrypt(Convert.FromBase64String(ciphertext), decompress);
        }

        /// <summary>
        /// Decrypts the supplied bytes
        /// </summary>
        /// <param name="cipherbytes">The encrypted bytes to decrypt</param>
        /// <param name="decompress">Decompress the clearbytes after decryption</param>
        /// <returns>An array of decrypted bytes</returns>
        public byte[] Decrypt(byte[] cipherbytes, bool decompress) {
            byte[] clearbytes;

            using (MemoryStream ms = new MemoryStream()) {
                using (CryptoStream cs = new CryptoStream(ms, _decryptor, CryptoStreamMode.Write)) {
                    cs.Write(cipherbytes, 0, cipherbytes.Length);
                    cs.FlushFinalBlock();

                    ms.Position = 0;
                    clearbytes = new Byte[ms.Length];
                    ms.Read(clearbytes, 0, clearbytes.Length);
                }
            }

            if (decompress) {
                using(MemoryStream cleartextStream = new MemoryStream()){
                    using (GZipStream compressionStream = new GZipStream(new MemoryStream(clearbytes), CompressionMode.Decompress)) {
                        compressionStream.CopyTo(cleartextStream);
                    }

                    clearbytes = cleartextStream.ToArray();
                }
            }

            return clearbytes;
        }

        #endregion

        #region randomness

        /// <summary>
        /// Generates a random initialization vecotr
        /// </summary>
        /// <returns>A 16-bit initialization vector</returns>
        public static Randomness GenerateIV() {
            return new Randomness(IV_SIZE);
        }

        /// <summary>
        /// Generates a random encryption key
        /// </summary>
        /// <returns>a 256-bit encryption key</returns>
        public static Randomness GenerateKey() {
            return new Randomness(KEY_SIZE);
        }

        #endregion
    }
}
