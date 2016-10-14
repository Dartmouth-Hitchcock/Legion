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
using System.IO;
using System.Reflection;
using System.Threading;

using Legion.Core.Caching;
using Legion.Core.DataStructures;
using Legion.Core.Exceptions;

namespace Legion.Core.Modules {

    /// <summary>
    /// Base class for external modules
    /// </summary>
    public class ExternalFuntionalityModule {
        private static ConcurrentDictionary<string, object> _modules = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// Gets a reflectedinstance of a module by name
        /// </summary>
        /// <param name="name">The internal Legion name for the module</param>
        /// <returns>The module, or null if not found</returns>
        protected static object GetModule(string name) {
            object oModule;
            if (_modules.TryGetValue(name, out oModule))
                return oModule;
            else {
                try {
                    Monitor.Enter(DynamicLock.Get("Module", name));

                    if (_modules.TryGetValue(name, out oModule))
                        return oModule;

                    object module = Cache.Get("Module", name);
                    if (module == null) {
                        string path = string.Format(
                            @"{0}\{1}",
                            Settings.GetString("ModuleRootPath"),
                            Settings.GetString(string.Format("ModuleName{0}", name))
                        );

                        if (File.Exists(path)) {
                            Assembly assembly = Assembly.LoadFrom(path);
                            Type t = assembly.GetType(assembly.GetName().Name + "." + "Module");

                            if (t != null && t.IsClass) {
                                module = Activator.CreateInstance(t);

                                if (module != null) {
                                    _modules.TryAdd(name, module);
                                    Cache.Add("Module", name, module);
                                }
                            }
                            else
                                throw new ModuleNotFoundException(string.Format("DLL specified for module '{0}' does not contain module interface class 'Module'.", name));
                        }
                        else
                            throw new ModuleNotFoundException(string.Format("DLL '{0}' for module '{1}' not found.", path, name));
                    }

                    return module;
                }
                finally {
                    Monitor.Exit(DynamicLock.Get("Module", name));
                }
            }
        }
    }
}
