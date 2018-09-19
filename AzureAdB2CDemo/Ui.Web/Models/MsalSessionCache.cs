namespace WebApp_OpenIDConnect_DotNet.Models
{
    using System;
    using System.Linq;
    using System.Threading;

    using Microsoft.AspNetCore.Http;
    using Microsoft.Identity.Client;

    public class MsalSessionCache
    {
        #region member vars

        private readonly TokenCache _cache = new TokenCache();
        private readonly string _cacheId;
        private readonly HttpContext _httpContext;

        #endregion

        #region constants

        private static readonly ReaderWriterLockSlim SessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        #endregion

        #region constructors and destructors

        public MsalSessionCache(string userId, HttpContext httpcontext)
        {
            // not object, we want the SUB
            _cacheId = userId + "_TokenCache";
            _httpContext = httpcontext;
            Load();
        }

        #endregion

        #region methods

        public TokenCache GetMsalCacheInstance()
        {
            _cache.SetBeforeAccess(BeforeAccessNotification);
            _cache.SetAfterAccess(AfterAccessNotification);
            Load();
            return _cache;
        }

        public void Load()
        {
            SessionLock.EnterReadLock();
            _cache.Deserialize(_httpContext.Session.Get(_cacheId));
            SessionLock.ExitReadLock();
        }

        public void Persist()
        {
            SessionLock.EnterWriteLock();
            // Optimistically set HasStateChanged to false. We need to do it early to avoid losing changes made by a concurrent thread.
            _cache.HasStateChanged = false;
            // Reflect changes in the persistent store
            _httpContext.Session.Set(_cacheId, _cache.Serialize());
            SessionLock.ExitWriteLock();
        }

        public string ReadUserStateValue()
        {
            SessionLock.EnterReadLock();
            var state = _httpContext.Session.GetString(_cacheId + "_state");
            SessionLock.ExitReadLock();
            return state;
        }

        public void SaveUserStateValue(string state)
        {
            SessionLock.EnterWriteLock();
            _httpContext.Session.SetString(_cacheId + "_state", state);
            SessionLock.ExitWriteLock();
        }

        // Triggered right after MSAL accessed the cache.
        private void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (_cache.HasStateChanged)
            {
                Persist();
            }
        }

        // Triggered right before MSAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Load();
        }

        #endregion
    }
}