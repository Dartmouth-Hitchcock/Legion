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

using Legion.Core.Caching;
using Legion.Core.DataStructures;

namespace Legion.Core.Clients {

    /// <summary>
    /// Rate limiting class
    /// </summary>
    public class RateLimit {
        private const string CACHE_TYPE = "RateLimit";
        private int _capacity;
        private TimeSpan _interval;
        private string _key;
        private RateType _type;

        private static object _locksLock = new object();
        private static Dictionary<string, object> _locks = new Dictionary<string, object>();

        /// <summary>
        /// Is this request rate type (user/host/default) limit exceeded
        /// </summary>
        public bool IsExceeded {
            get {
                lock (DynamicLock.Get(CACHE_TYPE, _key)) {
                    object oCallQueue = Cache.Get(CACHE_TYPE, _key);
                    if (oCallQueue != null) {
                        RollingQueue<DateTime> log = (RollingQueue<DateTime>)oCallQueue;
                        return (log.IsFull && log.Peek() > (DateTime.Now.Subtract(_interval)));
                    }
                    else
                        return false;
                }
            }
        }

        /// <summary>
        /// The max number of requests per interval
        /// </summary>
        public int Limit {
            get { return _capacity; }
        }

        /// <summary>
        /// The interval to rate
        /// </summary>
        public int Interval {
            get { return _interval.Seconds; }
        }

        /// <summary>
        /// Rate via this metric
        /// </summary>
        public RateType Type {
            get { return _type; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="requestor">The requestor to limit</param>
        /// <param name="application">The application being requested</param>
        public RateLimit(Requestor requestor, Application application) {
            _capacity = application.RateLmit;
            _interval = new TimeSpan(0, 0, application.RateInterval);
            _type = application.RateBy;

            switch (_type) {
                case RateType.User:
                    _key = string.Format("application:{0}//user:{1}", application.Id, requestor.ClientIPAddress);
                    break;
                case RateType.Host:
                    _key = string.Format("application:{0}//host:{1}", application.Id, requestor.HostIPAddress);
                    break;
                default:
                    _key = string.Format("host:{0}", requestor.HostIPAddress);
                    break;
            }
        }

        /// <summary>
        /// Increments the rolling rate limit log for this request type
        /// </summary>
        public void BitchSlap() {
            lock (DynamicLock.Get(CACHE_TYPE, _key)) {
                object oCallQueue = Cache.Get(CACHE_TYPE, _key);
                RollingQueue<DateTime> log = ((RollingQueue<DateTime>)oCallQueue ?? new RollingQueue<DateTime>(_capacity));

                log.Enqueue(DateTime.Now);

                Cache.Add(CACHE_TYPE, _key, log, _interval, new System.Web.Caching.CacheItemRemovedCallback(RemoveLock));
            }
        }

        /// <summary>
        /// Callback after removing a rate limit log from cache
        /// </summary>
        /// <param name="key">The cache key</param>
        /// <param name="value">The cache value</param>
        /// <param name="reason">The reason the cache item is being removed</param>
        public void RemoveLock(string key, object value, System.Web.Caching.CacheItemRemovedReason reason) {
            DynamicLock.Remove(CACHE_TYPE, _key);
            Cache.RemoveComponentCacheKey(key, value, reason);
        }

        /// <summary>
        /// String representation of this RateLimit
        /// </summary>
        /// <returns>a string representation of this RateLimit</returns>
        public override string ToString() {
            return Settings.GetString("ToStringFormatRateLimit", new Dictionary<string, string>(){
                {"Limit", Limit.ToString()},
                {"LimitPlural", (Limit == 1 ? string.Empty : "s")},
                {"Interval", (Interval == 1 ? string.Empty : Interval.ToString())},
                {"IntervalUnit", (Interval == 1 ? "second" : " seconds")},
                {"LimitType", (Type == RateType.Default ? string.Empty : string.Format(" per application {0}", Type.ToString().ToLower()))}
            });
        }
    }
}
