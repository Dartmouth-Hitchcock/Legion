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
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.AccessControl;
using System.ServiceProcess;
using System.Text;
using Microsoft.Web.Administration;
using Microsoft.Win32;

namespace Watcher {
    public partial class Watcher : ServiceBase {
        FileSystemWatcher _watcher;

        internal const string BASE_KEY = @"SOFTWARE\Dartmouth-Hitchcock\LegionWatcher";

        internal struct REGISTRY_KEYS {
            public static string WebPool = "ServiceWebPool";
            public static string AssembliesPath = "ServiceAssembliesPath";
        }

        private string _poolName, _watchDir, _connectionString = null;

        public Watcher() {
            InitializeComponent();
            
            if (!System.Diagnostics.EventLog.SourceExists("LegionWatcher"))
                System.Diagnostics.EventLog.CreateEventSource("LegionWatcher", "");

            eventLog.Source = "LegionWatcher";
            eventLog.Log = "";

            RegistryKey key = Registry.LocalMachine.OpenSubKey(BASE_KEY);

            _poolName = key.GetValue(REGISTRY_KEYS.WebPool).ToString();
            _watchDir = key.GetValue(REGISTRY_KEYS.AssembliesPath).ToString();
            _connectionString = ConfigurationManager.ConnectionStrings["LegionConnectionString"].ToString();
        }

        protected override void OnStart(string[] args) {
            _watcher = new FileSystemWatcher();
            _watcher.Path = _watchDir;
            //_watcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime;
            //_watcher.Filter = "*.dll";
            _watcher.Created += new FileSystemEventHandler(RefreshAppPool);
            _watcher.Changed += new FileSystemEventHandler(RefreshAppPool);
            _watcher.Deleted += new FileSystemEventHandler(RefreshAppPool);
            _watcher.EnableRaisingEvents = true;

            string msg = string.Format("Legion Watcher started in '{0}'", _watchDir);
            eventLog.WriteEntry(msg);
            Email(msg);
        }

        protected override void OnStop() {
            _watcher.Dispose();
        }

        private void RefreshAppPool(object sender, FileSystemEventArgs e) {
            if (e.Name.EndsWith(".dll")) { //yes, i know i can use filters, but dfs sucks and causes them not to work correctly
                if (_connectionString != null) {
                    using (LegionLinqDataContext db = new LegionLinqDataContext(_connectionString)) {
                        db.xspSetAssemblyRefreshFlags();
                    }
                }

                string msg = string.Format("'{0}' modified", e.Name);
                Email(msg);
                eventLog.WriteEntry(msg);

                ServerManager serverManager = new ServerManager();
                ApplicationPoolCollection applicationPoolCollection = serverManager.ApplicationPools;
                foreach (ApplicationPool applicationPool in applicationPoolCollection) {
                    if (applicationPool.Name == _poolName) {
                        applicationPool.Recycle();
                        break;
                    }
                }
                serverManager.CommitChanges();
            }
        }

        private void Email(string msg) {
            MailMessage message = new MailMessage();

            message.To.Add("webmaster@hitchcock.org");
            message.From = new MailAddress("webmaster@hitchcock.org", "webmaster@hitchcock.org");
            message.Subject = string.Format("Legion Service Update on {0}", Environment.MachineName);
            message.Body = msg;
            message.IsBodyHtml = false;


            SmtpClient smtp = new SmtpClient("exmailhub.hitchcock.org");
            smtp.Send(message);
        }

        private void eventLog1_EntryWritten(object sender, EntryWrittenEventArgs e) {

        }
    }
}
