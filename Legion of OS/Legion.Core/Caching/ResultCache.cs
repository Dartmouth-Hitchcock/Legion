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

using Legion.Core.Databases;
using System;
using System.Configuration;
using System.Data.Linq;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Legion.Core.Caching {

    internal class ResultCache {
        private const string CACHE_TYPE = "resultcache";
        private const string CACHE_KEY_FORMAT = "resultcache//{0}:{1}";

        internal static CachedResult GetCachedResult(int methodid, Binary key) {
            CachedResult result = new CachedResult() { Found = false };

            object oResult = Cache.Get(CACHE_TYPE, GetCacheKey(methodid, key));
            if (oResult == null) {
                using (LegionLinqDataContext legion = new LegionLinqDataContext(ConfigurationManager.ConnectionStrings["LegionConnectionString"].ToString())) {
                    xspGetCachedResultResult cachedResult = legion.xspGetCachedResult(methodid, key).FirstOrDefault();
                    if (cachedResult != default(xspGetCachedResultResult)) {
                        result.Result = cachedResult.Result;
                        result.UpdatedOn = cachedResult.UpdatedOn;
                        result.ExpiresOn = cachedResult.ExpiresOn;
                        result.Found = true;

                        CacheResultLocally(methodid, key, result);
                    }
                }
            }
            else
                result = (CachedResult)oResult;

            return result;
        }

        internal static void CacheResult(int methodid, Binary key, XElement parameters, XElement resultset) {
            DateTime? expiresOn = null;

            using (LegionLinqDataContext legion = new LegionLinqDataContext(ConfigurationManager.ConnectionStrings["LegionConnectionString"].ToString())) {
                legion.xspInsertCachedResult(
                    methodid, 
                    parameters,
                    resultset, 
                    key,
                    ref expiresOn
                );
            }

            CachedResult cachedResult = new CachedResult() {
                Result = resultset,
                UpdatedOn = DateTime.Now,
                ExpiresOn = (expiresOn == null ? DateTime.Now : (DateTime)expiresOn),
                Found = true
            };

            CacheResultLocally(methodid, key, cachedResult);
        }

        internal static void CacheResultLocally(int methodid, Binary key, CachedResult cachedResult){
            TimeSpan cachePadding = new TimeSpan(0, 0, Settings.GetInt("ResultCachingDurationPadding"));

            Cache.Add(
                CACHE_TYPE,
                GetCacheKey(methodid, key),
                cachedResult,
                (cachedResult.ExpiresOn + cachePadding),
                null
            );
        }

        internal static void ExpireResult(int methodid, Binary key, TimeSpan cachePadding) {
            CachedResult result = GetCachedResult(methodid, key);
            if (result.Found) {
                result.ExpiresOn = DateTime.Now;

                Cache.Add(
                    CACHE_TYPE,
                    GetCacheKey(methodid, key),
                    result,
                    (result.ExpiresOn + cachePadding),
                    null
                );
            }
        }

        /// <summary>
        /// Trys to expire the results cache if needed.
        /// NOT THREAD SAFE
        /// </summary>
        internal static void TryExpire() {
            DateTime lastCacheRefreshCheck = (HttpContext.Current.Application["LastCacheResultsExpireCheck"] == null ? DateTime.MinValue : (DateTime)HttpContext.Current.Application["LastCacheResultsExpireCheck"]);
            if (lastCacheRefreshCheck <= DateTime.Now.Subtract(new TimeSpan(0, 0, Settings.GetInt("CacheRefreshCheckInterval")))) {
                using (LegionLinqDataContext db = new LegionLinqDataContext(ConfigurationManager.ConnectionStrings["LegionConnectionString"].ToString())) {
                    ISingleResult<xspGetExpiredCachedResultsResult> expiredResults = db.xspGetExpiredCachedResults(Settings.GetInt("CacheRefreshCheckInterval"));
                    TimeSpan cachePadding = new TimeSpan(0, 0, Settings.GetInt("ResultCachingDurationPadding"));

                    foreach (xspGetExpiredCachedResultsResult expiredResult in expiredResults)
                        ExpireResult(expiredResult.MethodId, expiredResult.CacheKey, cachePadding);

                    HttpContext.Current.Application["LastCacheResultsExpireCheck"] = DateTime.Now;
                }
            }
        }

        private static string GetCacheKey(int methodid, Binary key) {
            return string.Format(CACHE_KEY_FORMAT, methodid, key);
        }
    }
}
