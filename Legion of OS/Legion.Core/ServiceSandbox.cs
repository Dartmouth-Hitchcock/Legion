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
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Legion {
    internal class ServiceSandbox {
        private const string APPDOMAIN_FORMAT = "LegionServiceSandbox // {0} // {1}";

        private AppDomain _sandbox;
        private Service _service;
        private bool _markedForRelease;
        private object _lock;

        public object Lock {
            get { return _lock; }
        }

        public ServiceSandbox(string name) {
            _markedForRelease = false;
            _lock = new object();

            _sandbox = AppDomain.CreateDomain(string.Format(APPDOMAIN_FORMAT, name, DateTime.Now.Ticks));
            _sandbox.AssemblyResolve += ResolveAssembly;
            _service = (Service)_sandbox.CreateInstanceFromAndUnwrap(Assembly.GetExecutingAssembly().Location, typeof(Service).FullName);
            _service.Load(name);
        }

        public bool Invoke(string method) {
            if (!_markedForRelease) {
                _service.Open[method].Invoke();
                return false;
            }
            else
                return false;
        }

        public void Release() {
            Monitor.Enter(_lock);
            try {
                if (_sandbox != null) {
                    try {
                        AppDomain.Unload(_sandbox);
                        _sandbox = null;
                    }
                    catch (Exception) { /* sandbox is being released, suppress exceptions */ }
                }
            }
            finally {
                Monitor.Exit(_lock);
            }
        }

        private Assembly ResolveAssembly(object sender, ResolveEventArgs args) {
            //TODO: This looks at currently loaded assemblies, not the Assemblies directory
            Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in loadedAssemblies)
                if (assembly.FullName == args.Name)
                    return assembly;

            return null;
        }
    }
}
