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
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using System.Xml;

using Legion.Core.Modules;


namespace EventLogLoggingModule {
    public class Module : Logging {
        private const string DEFAULT_ENTRY_TAG = "details";
        private const string LOG_SOURCE = "Legion";

        private static EventLog _eventLog = null;

        private static EventLog EventLog {
            get {
                if(_eventLog == null) {
                    _eventLog = new EventLog();
                    _eventLog.Source = LOG_SOURCE;
                    _eventLog.Log = string.Empty; //empty string intentional, is default log
                }

                return _eventLog;
            }
        }

        public override bool IsEventWritingBuffered {
            get { return false; }
        }

        public override int WriteEvent(LoggedEvent e) {
            XmlDocument doc = new XmlDocument();
            XmlNode xEvent = doc.AppendChild(doc.CreateElement("event"));
            xEvent.AppendChild(doc.CreateElement("level")).InnerText = e.Level.ToString();
            xEvent.AppendChild(doc.CreateElement("group")).InnerText = e.Group;
            xEvent.AppendChild(doc.CreateElement("type")).InnerText = e.Type;
            xEvent.AppendChild(doc.CreateElement("details")).InnerText = e.Details;
            xEvent.AppendChild(doc.CreateElement("clientip")).InnerText = e.ClientIp;
            xEvent.AppendChild(doc.CreateElement("hostip")).InnerText = e.HostIp;

            XmlNode xApplication = xEvent.AppendChild(doc.CreateElement("application"));
            xApplication.AppendChild(doc.CreateElement("id")).InnerText = e.ApplicationId.ToString();
            xApplication.AppendChild(doc.CreateElement("name")).InnerText = e.ApplicationName;
            xApplication.AppendChild(doc.CreateElement("type")).InnerText = e.ApplicationType.ToString();

            XmlNode xLoggingUser = xEvent.AppendChild(doc.CreateElement("logginguser"));
            xLoggingUser.AppendChild(doc.CreateElement("id")).InnerText = e.LoggingUserId;
            xLoggingUser.AppendChild(doc.CreateElement("type")).InnerText = e.LoggingUserType;

            XmlNode xAffectedUser = xEvent.AppendChild(doc.CreateElement("affecteduser"));
            xAffectedUser.AppendChild(doc.CreateElement("id")).InnerText = e.AffectedUserId;
            xAffectedUser.AppendChild(doc.CreateElement("type")).InnerText = e.AffectedUserType;

            EventLog.WriteEntry(xEvent.OuterXml, EventLogEntryType.Information);

            //EventLog does not synchronously return IDs for new events, so return 1
            //This module is intended to log to a centralized logging source, not the local event log
            //This module is provided for exabple purposes only
            return 1;
        }

        public override int WriteException(LoggedException e) {
            XmlDocument doc = new XmlDocument();
            XmlNode xException = doc.AppendChild(doc.CreateElement("event"));
            xException.AppendChild(doc.CreateElement("group")).InnerText = e.Group;
            xException.AppendChild(doc.CreateElement("type")).InnerText = e.Type;
            xException.AppendChild(doc.CreateElement("details")).InnerText = e.Details;
            xException.AppendChild(doc.CreateElement("message")).InnerText = e.Message;
            xException.AppendChild(doc.CreateElement("stacktrace")).InnerText = e.StackTrace;
            xException.AppendChild(doc.CreateElement("clientip")).InnerText = e.ClientIp;
            xException.AppendChild(doc.CreateElement("hostip")).InnerText = e.HostIp;

            XmlNode xApplication = xException.AppendChild(doc.CreateElement("application"));
            xApplication.AppendChild(doc.CreateElement("id")).InnerText = e.ApplicationId.ToString();
            xApplication.AppendChild(doc.CreateElement("name")).InnerText = e.ApplicationName;
            xApplication.AppendChild(doc.CreateElement("type")).InnerText = e.ApplicationType.ToString();

            EventLog.WriteEntry(xException.OuterXml, EventLogEntryType.Error);

            //EventLog does not synchronously return IDs for new events, so return 1
            //This module is intended to log to a centralized logging source, not the local event log
            //This module is provided for exabple purposes only
            return 1;
        }

        public override void FlushEventsBuffer() {
            return;
        }
    }
}