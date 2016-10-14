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
using System.Threading.Tasks;

using Legion.Core.Modules;

namespace Legion.Core.Services.Tools {

    /// <summary>
    /// Outgoing email class
    /// </summary>
    public static class Email {

        /// <summary>
        /// Email priority
        /// </summary>
        public enum Priority {
            /// <summary>
            /// High priority
            /// </summary>
            High,
            /// <summary>
            /// Normal priority
            /// </summary>
            Normal,
            /// <summary>
            /// Low priority
            /// </summary>
            Low
        }

        /// <summary>
        /// Sends an email
        /// </summary>
        /// <param name="from">The from address</param>
        /// <param name="to">Array of to recipients</param>
        /// <param name="cc">Arrary of cc'ed recipients</param>
        /// <param name="bcc">Arrary of bcc'ed recipients</param>
        /// <param name="subject">The subject line</param>
        /// <param name="body">the body of the email</param>
        /// <param name="attachments">Adday of binary attachments</param>
        /// <param name="priority">The email's priority</param>
        public static void Send(string from, string[] to, string[] cc, string[] bcc, string subject, string body, string[] attachments, Priority priority) {

            Modules.Email.Priority realPriority;
            switch (priority) {
                case Priority.High:
                    realPriority = Modules.Email.Priority.Normal;
                    break;
                case Priority.Low:
                    realPriority = Modules.Email.Priority.Low;
                    break;
                default:
                    realPriority = Modules.Email.Priority.Normal;
                    break;
            }

            Modules.Email.Module.Send(from, to, cc, bcc, subject, body, attachments, realPriority);
        }

        /// <summary>
        /// Sends an email
        /// </summary>
        /// <param name="from">The from address</param>
        /// <param name="to">Array of to recipients</param>
        /// <param name="cc">Arrary of cc'ed recipients</param>
        /// <param name="bcc">Arrary of bcc'ed recipients</param>
        /// <param name="subject">The subject line</param>
        /// <param name="body">the body of the email</param>
        /// <param name="attachments">Array of binary attachments</param>
        public static void Send(string from, string[] to, string[] cc, string[] bcc, string subject, string body, string[] attachments) {
            Send(from, to, cc, bcc, subject, body, attachments, Priority.Normal);
        }

        /// <summary>
        /// Sends an email
        /// </summary>
        /// <param name="from">The from address</param>
        /// <param name="to">Array of to recipients</param>
        /// <param name="cc">Arrary of cc'ed recipients</param>
        /// <param name="bcc">Arrary of bcc'ed recipients</param>
        /// <param name="subject">The subject line</param>
        /// <param name="body">the body of the email</param>
        public static void Send(string from, string[] to, string[] cc, string[] bcc, string subject, string body) {
            Send(from, to, cc, bcc, subject, body, null, Priority.Normal);
        }

        /// <summary>
        /// Sends an email
        /// </summary>
        /// <param name="to">Array of to recipients</param>
        /// <param name="cc">Arrary of cc'ed recipients</param>
        /// <param name="bcc">Arrary of bcc'ed recipients</param>
        /// <param name="subject">The subject line</param>
        /// <param name="body">the body of the email</param>
        public static void Send(string[] to, string[] cc, string[] bcc, string subject, string body) {
            Send(null, to, cc, bcc, subject, body, null, Priority.Normal);
        }

        /// <summary>
        /// Sends an email
        /// </summary>
        /// <param name="from">The from address</param>
        /// <param name="to">Array of to recipients</param>
        /// <param name="subject">The subject line</param>
        /// <param name="body">the body of the email</param>
        public static void Send(string from, string to, string subject, string body) {
            Send(from, new string[] { to }, null, null, subject, body, null, Priority.Normal);
        }

        /// <summary>
        /// Sends an email
        /// </summary>
        /// <param name="to">Array of to recipients</param>
        /// <param name="subject">The subject line</param>
        /// <param name="body">the body of the email</param>
        public static void Send(string to, string subject, string body) {
            Send(null, new string[] { to }, null, null, subject, body, null, Priority.Normal);
        }
    }
}
