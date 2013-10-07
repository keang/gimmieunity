using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NativeDialogModes;

namespace Facebook
{
    class IOSFacebook : AbstractFacebook, IFacebook
    {
#if UNITY_IOS
        [DllImport ("__Internal")] private static extern void iosInit(bool cookie, bool logging, bool status, bool frictionlessRequests);
        [DllImport ("__Internal")] private static extern void iosLogin(string scope);
        [DllImport ("__Internal")] private static extern void iosLogout();

        [DllImport ("__Internal")] private static extern void iosSetShareDialogMode(int mode);

        [DllImport ("__Internal")] 
        private static extern void iosFeedRequest(
            int requestId,
            string toId, 
            string link, 
            string linkName, 
            string linkCaption, 
            string linkDescription, 
            string picture, 
            string mediaSource, 
            string actionName, 
            string actionLink, 
            string reference);
        
        [DllImport ("__Internal")] 
        private static extern void iosAppRequest(
            int requestId,
            string message, 
            string[] to = null,
            int toLength = 0,
            string filters = "", 
            string[] excludeIds = null,
            int excludeIdsLength = 0,
            bool hasMaxRecipients = false,
            int maxRecipients = 0, 
            string data = "", 
            string title = "");
        
        [DllImport ("__Internal")] 
        private static extern void iosCallFbApi(
            int requestId,
            string query,
            string method,
            string[] formDataKeys = null,
            string[] formDataVals = null,
            int formDataLen = 0);
        

        //neko, insights stuff
        [DllImport ("__Internal")] 
        private static extern void iosFBInsightsFlush();

        [DllImport ("__Internal")] 
        private static extern void iosFBInsightsLogConversionPixel(
            string pixelID, 
            double value);
        
        [DllImport ("__Internal")] 
        private static extern void iosFBInsightsLogPurchase(
            double purchaseAmount,
            string currency,
            int numParams,
            string[] paramKeys,
            string[] paramVals);
        
        [DllImport ("__Internal")] 
        private static extern void iosFBInsightsSetAppVersion(string version);
        
        [DllImport ("__Internal")] 
        private static extern void iosFBInsightsSetFlushBehavior(int behavior);
        
        [DllImport ("__Internal")] 
        private static extern void iosFBSettingsPublishInstall(int requestId, string appId);
#else
        void iosInit(bool cookie, bool logging, bool status, bool frictionlessRequests) {}
        void iosLogin(string scope) {}
        void iosLogout() {}

        void iosSetShareDialogMode(int mode) {}
        
        void iosFeedRequest(
            int requestId,
            string toId, 
            string link, 
            string linkName, 
            string linkCaption, 
            string linkDescription, 
            string picture, 
            string mediaSource, 
            string actionName, 
            string actionLink, 
            string reference) {}
        
        void iosAppRequest(
            int requestId,
            string message, 
            string[] to = null,
            int toLength = 0,
            string filters = "", 
            string[] excludeIds = null,
            int excludeIdsLength = 0,
            bool hasMaxRecipients = false,
            int maxRecipients = 0, 
            string data = "", 
            string title = "") {}
        
        void iosCallFbApi(
            int requestId,
            string query,
            string method,
            string[] formDataKeys = null,
            string[] formDataVals = null,
            int formDataLen = 0) {}
        
        void iosFBInsightsFlush() {}

        void iosFBInsightsLogConversionPixel(
            string pixelID, 
            double value) {}
        
        void iosFBInsightsLogPurchase(
            double purchaseAmount,
            string currency,
            int numParams,
            string[] paramKeys,
            string[] paramVals) {}
        
        void iosFBInsightsSetAppVersion(string version) {}
        
        void iosFBInsightsSetFlushBehavior(int behavior) {}
        
        void iosFBSettingsPublishInstall(int requestId, string appId) {}
#endif
        
        private class NativeDict
        {
            public NativeDict()
            {
                numEntries = 0;
                keys = null;
                vals = null;
            }
            
            public int numEntries;
            public string[] keys;
            public string[] vals;
        };
        
        public enum FBInsightsFlushBehavior 
        {
            FBInsightsFlushBehaviorAuto,
            FBInsightsFlushBehaviorExplicitOnly,
        };
            
        private int dialogMode = (int)NativeDialogModes.eModes.FAST_APP_SWITCH_SHARE_DIALOG;
        private string appVersion;
        private FBInsightsFlushBehavior flushBehavior;

        private InitDelegate externalInitDelegate;
        
        public string AppVersion{ get {return appVersion;} }
        
        public override int DialogMode
        {
            get { return dialogMode; }
            set { dialogMode = value; iosSetShareDialogMode (dialogMode); }
        }
        
        public FBInsightsFlushBehavior FlushBehavior{ get {return flushBehavior;} }
        
        #region Init
        protected override void OnAwake()
        {
        }

        public override void Init(
            InitDelegate onInitComplete,
            string appId,
            bool cookie = false,
            bool logging = true,
            bool status = true,
            bool xfbml = false,
            string channelUrl = "",
            string authResponse = null,
            bool frictionlessRequests = false,
            Facebook.HideUnityDelegate hideUnityDelegate = null)
        {
            iosInit(cookie, logging, status, frictionlessRequests);
            externalInitDelegate = onInitComplete;
        }
        #endregion

        #region FB public interface
        public override void Login(string scope = "", FacebookDelegate callback = null)
        {
            AddAuthDelegate(callback);
            iosLogin(scope);
        }
        
        public override void Logout()
        {
            iosLogout();
            isLoggedIn = false;
        }

        public override void AppRequest(
            string message,
            string[] to = null,
            string filters = "",
            string[] excludeIds = null,
            int? maxRecipients = null,
            string data = "",
            string title = "",
            FacebookDelegate callback = null)
        {
            iosAppRequest(System.Convert.ToInt32(AddFacebookDelegate(callback)), message, to, to != null?to.Length:0, filters, excludeIds, excludeIds != null?excludeIds.Length:0, maxRecipients.HasValue, maxRecipients.HasValue?maxRecipients.Value:0, data, title);
        }

        public override void FeedRequest(
            string toId = "",
            string link = "",
            string linkName = "",
            string linkCaption = "",
            string linkDescription = "",
            string picture = "",
            string mediaSource = "",
            string actionName = "",
            string actionLink = "",
            string reference = "",
            Dictionary<string, string[]> properties = null,
            FacebookDelegate callback = null)
        {
            iosFeedRequest(System.Convert.ToInt32(AddFacebookDelegate(callback)), toId, link, linkName, linkCaption, linkDescription, picture, mediaSource, actionName, actionLink, reference);
        }

        public override void Pay(
            string product,
            string action = "purchaseitem",
            int quantity = 1,
            int? quantityMin = null,
            int? quantityMax = null,
            string requestId = null,
            string pricepointId = null,
            string testCurrency = null,
            FacebookDelegate callback = null)
        {
            throw new PlatformNotSupportedException("There is no Facebook Pay Dialog on iOS");
        }

        public override void API(
            string query,
            HttpMethod method,
            Dictionary<string, string> formData = null,
            FacebookDelegate callback = null)
        {
            string[] dictKeys = null;
            string[] dictVals = null;

            if(formData != null && formData.Count > 0)
            {
                dictKeys = new string[formData.Count];
                dictVals = new string[formData.Count];
                int idx = 0;
                foreach( KeyValuePair<string, string> kvp in formData )
                {
                    dictKeys[idx] = kvp.Key;
                    dictVals[idx] = System.String.Copy(kvp.Value);
                    idx++;
                }
            }
            iosCallFbApi(System.Convert.ToInt32(AddFacebookDelegate(callback)), query, method!=null?method.ToString():null, dictKeys, dictVals, formData!=null?formData.Count:0);
        }
        #endregion
        
        #region Insights public interface
        public void InsightsFlush() 
        {
            iosFBInsightsFlush();
        }

        public void InsightsLogConversionPixel(
            string pixelID, 
            double val) 
        {
            iosFBInsightsLogConversionPixel(pixelID, val);
        }
        
        public void InsightsLogPurchase(
            double purchaseAmount,
            string currency,
            Dictionary<string, string> properties = null) 
        {
            NativeDict dict = MarshallDict(properties);
            iosFBInsightsLogPurchase(purchaseAmount, currency, dict.numEntries, dict.keys, dict.vals);
        }
        
        public void InsightsSetAppVersion(string version) 
        {
            appVersion = version;
            iosFBInsightsSetAppVersion(version);
        }
        
        public void InsightsSetFlushBehavior(FBInsightsFlushBehavior behavior)
        {
            flushBehavior = behavior;
            iosFBInsightsSetFlushBehavior((int)flushBehavior);
        }
        
        public override void PublishInstall(string appId, FacebookDelegate callback = null)
        {
            iosFBSettingsPublishInstall(System.Convert.ToInt32(AddFacebookDelegate(callback)), appId);
        }
        #endregion
        
        
        
        #region Interal stuff
        private NativeDict MarshallDict(Dictionary<string, string> dict)
        {
            NativeDict res = new NativeDict();
            
            if(dict != null && dict.Count > 0)
            {
                res.keys = new string[dict.Count];
                res.vals = new string[dict.Count];
                res.numEntries=0;
                foreach( KeyValuePair<string, string> kvp in dict )
                {
                    res.keys[res.numEntries] = kvp.Key;
                    res.vals[res.numEntries] = kvp.Value;
                    res.numEntries++;
                }
            }
            return res;
        }

        private void OnInitComplete(string msg)
        {
            if(msg != null && msg.Length > 0)
            {
                OnLogin (msg);
            }
            externalInitDelegate();
        }

        public void OnLogin(string msg)
        {
            int delimIdx = msg.IndexOf(":");

            if (delimIdx > 0)
            {
                isLoggedIn = true;
                userId = msg.Substring (0, delimIdx);
                accessToken = msg.Substring (delimIdx + 1);
            }

            OnAuthResponse(new FBResult(msg));
        }
        
        public void OnLogout(string msg)
        {
            isLoggedIn = false;
        }
        
        public void OnRequestComplete(string msg)
        {
            int delimIdx = msg.IndexOf(":");
            if(delimIdx <= 0)
            {
                FbDebug.Error("Malformed callback from ios.  I expected the form id:message but couldn't find either the ':' character or the id.");
                FbDebug.Error("Here's the message that errored: " + msg);
                return;
            }
            
            string idStr = msg.Substring(0, delimIdx);
            string payload = msg.Substring(delimIdx+1);
            
            FbDebug.Info("id:" + idStr + " msg:" + payload);
            
            OnFacebookResponse(idStr, new FBResult(payload));
        }
        #endregion
    }
}
