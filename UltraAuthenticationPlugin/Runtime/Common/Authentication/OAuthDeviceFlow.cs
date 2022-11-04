#if !(DOT_NET)
using UnityEngine;
#endif
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Ultraio
{
    public class OAuthDeviceFlow : IAuthenticationFlow
    {
        #region Private Data
        private bool _authenticated = false;
        private string _authUrl = string.Empty;
        private string _clientId = string.Empty;
        private bool _useBrowser = false;
        private UltraToken _ultraToken;
        #endregion

        #region Events
        public event AuthenticationSuccessedHandler AuthenticationSuccessed;
        public event AuthenticationFailedHandler AuthenticationFailed;
        #endregion

        /// <summary>Default constructor of OAuthDeviceFlow</summary>
        /// <param name="authUrl">The authentication server URL (OIDC compliant supporting the device grant flow)</param>
        /// <param name="clientId">Client id of the application</param>
        /// <param name="useBrowser">Set to true to use the browser instead of the Ultra Client (default is false)</param>
        public OAuthDeviceFlow(string authUrl, string clientId, bool useBrowser = false)
        {
            string error = null;
            if (string.IsNullOrEmpty(authUrl))
            {
                error = "authUrl was null or empty";
            }
            else if (string.IsNullOrEmpty(clientId))
            {
                error = "clientId was null or empty";
            }

            if (error != null)
            {
#if !(DOT_NET)
                Debug.LogError($"ERROR | Failed to initialize Ultra Client - {error}");
#else
                Console.WriteLine($"ERROR | Failed to initialize Ultra Client - {error}");
#endif
                return;
            }

            _authUrl = authUrl;
            _clientId = clientId;
            _useBrowser = useBrowser;
        }

        /// <summary>Initiate the OIDC Device Grant Flow</summary>
        /// <returns>An async boolean (true if authentication succeeded, false otherwise)</returns>
        public async Task<bool> Authenticate()
        {
            try
            {
                DeviceInfo deviceInfo = await GetDeviceInfo();
                OpenVerificationDeepLink(deviceInfo.verification_uri_complete);
                _ultraToken = await GetUserToken(deviceInfo);
                _authenticated = true;
                AuthenticationSuccessed(_ultraToken);
                return true;
            }
            catch (Exception error)
            {
#if !(DOT_NET)
                Debug.LogError($"ERROR | Failed to authenticate to Ultra - {error}");
#else 
                Console.WriteLine($"ERROR | Failed to authenticate to Ultra - {error}");
#endif
                AuthenticationFailed(new UltraError(error.Message));
                return false;
            }
        }

        private void OpenVerificationDeepLink(string verificationUri)
        {
            if (_useBrowser)
            {
                Application.OpenURL(verificationUri);
            }
            else
            {
                System.Diagnostics.Process.Start(DeepLinkConstants.Protocol + verificationUri);
            }
            
        }

        private async Task<DeviceInfo> GetDeviceInfo()
        {
            var httpClient = new HttpClient();
            var data = new Dictionary<string, string>
            {
                {OAuthConstants.ClientId, _clientId},
                {OAuthConstants.Scope, OAuthConstants.DefaultScope}
            };

            HttpResponseMessage response = await httpClient.PostAsync(_authUrl + OAuthConstants.DevicePath, new FormUrlEncodedContent(data));
            string responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var deviceInfo = JsonUtility.FromJson<DeviceInfo>(responseContent);
#if !(DOT_NET)
                Debug.Log($"INFO | Device Info result - {responseContent}");
#endif                
                return deviceInfo;
            }
            else
            {
                throw new Exception(responseContent);
            }
        }

        private async Task<UltraToken> GetUserToken(DeviceInfo deviceInfo)
        {
            var httpClient = new HttpClient();
            var data = new Dictionary<string, string>
            {
                {OAuthConstants.DeviceCode, deviceInfo.device_code},
                {OAuthConstants.ClientId, _clientId},
                {OAuthConstants.Scope, OAuthConstants.DefaultScope},
                {OAuthConstants.GrantType, OAuthConstants.DefaultGrantType}
            };

            UltraToken ultraToken = null;
            var timeoutToken = new CancellationTokenSource();
            timeoutToken.CancelAfter(TimeSpan.FromSeconds(deviceInfo.expires_in));
            while (ultraToken == null)
            {
                HttpResponseMessage response = await httpClient.PostAsync(_authUrl + OAuthConstants.TokenPath, new FormUrlEncodedContent(data));
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    ultraToken = JsonUtility.FromJson<UltraToken>(responseContent);
#if !(DOT_NET)
                    Debug.Log($"INFO | Ultra Token - {responseContent}");
#endif     
                }
                else
                {
#if !(DOT_NET)
                    Debug.Log($"INFO | The user's token is not available yet - {responseContent}");
#else 
                    Console.WriteLine($"INFO | The user's token is not available yet - {responseContent}");
#endif
                    if (timeoutToken.IsCancellationRequested)
                    {
                        throw new TimeoutException($"Failed to retrieve the user token after the timeout duration ({deviceInfo.expires_in}s)");
                    }
                    else
                    {
                        await Task.Delay(TimeSpan.FromSeconds(deviceInfo.interval));
                    }
                }
            }
            return ultraToken;
        }

        /// <summary>Fetches User Information from the authentication server</summary>
        /// <returns>
        /// An async UserInfo object representing the authenticated Ultra user
        /// </returns>
        public async Task<UserInfo> GetUserInfo()
        {
            if (!_authenticated)
            {
                string error = "Authentication is required to fetch user information";
#if !(DOT_NET)
                Debug.LogError($"ERROR | {error}");
#else 
                Console.WriteLine($"ERROR | {error}");
#endif
                return null;
            }
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add(OAuthConstants.Authorization, $"{OAuthConstants.Bearer} {_ultraToken.access_token}");
            HttpResponseMessage response = await httpClient.GetAsync(_authUrl + OAuthConstants.UserInfoPath);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var userInfo = JsonUtility.FromJson<UserInfo>(responseContent);
#if !(DOT_NET)
                Debug.Log($"INFO | User Info result - {responseContent}");
#endif            
                return userInfo;
            }
            else
            {
                throw new Exception(responseContent);
            }
        }
    }
}