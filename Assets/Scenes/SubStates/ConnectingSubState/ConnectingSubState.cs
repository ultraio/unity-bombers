using Gameframework;
using BrainCloud;
using BrainCloud.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace BrainCloudUNETExample
{
    public class ConnectingSubState : BaseSubState
    {
        public static string STATE_NAME = "connectingSubState";

        [SerializeField]
        private GameObject Panel = null;

        private static string ms_instructionText = "";
        private static string ms_buttonText = "";
        private static bool ms_canCancel = false;

        public static void PushConnectingSubState(string in_instructionText, string in_buttonText, bool in_canCancel = true)
        {
            GStateManager stateMgr = GStateManager.Instance;
            stateMgr.OnInitializeDelegate += onPushConnectingSubStateLoaded;
            ms_instructionText = in_instructionText;
            ms_buttonText = in_buttonText;
            ms_canCancel = in_canCancel;
            stateMgr.PushSubState(STATE_NAME);
        }

        private static void onPushConnectingSubStateLoaded(BaseState in_state)
        {
            GStateManager stateMgr = GStateManager.Instance;
            if (in_state as ConnectingSubState)
            {
                stateMgr.OnInitializeDelegate -= onPushConnectingSubStateLoaded;
            }
        }

        #region BaseState
        protected override void Start()
        {
            _stateInfo = new StateInfo(STATE_NAME, this);
            base.Start();

            if (!Ultraio.Ultra.Client.Initialized)
            {
                //init ultra
                UltraManager.singleton.OnUltraLoginSuccess += (string username, string token) =>
                {
                    SuccessCallback successCB = (response, cbObject) =>
                    {
                        //ultra cloud auth success
                        Debug.Log("Attaching username: " + username);
                        GCore.Wrapper.IdentityService.AttachUltraIdentity(username, token, onAttachSuccess, onAuthFail);
                        GPlayerMgr.Instance.OnAuthSuccess(response, cbObject);
                        onConnectComplete();
                    };
                    FailureCallback failureCB = (status, code, error, cbObject) =>
                    {
                        //failure
                        Debug.Log(string.Format("ultraCloud login failed | {0} {1} {2}", status, code, error));
                        HudHelper.DisplayMessageDialog("AUTHENTICATION ERROR", string.Format("Ultra login failed | {0} {1} {2}", status, code, error), "OK");
                    };

                    GCore.Wrapper.Init();
                    GCore.Wrapper.AuthenticateUltra(username, token, true, successCB, failureCB);
                };

                UltraManager.singleton.OnUltraLoginFailure += (string error) =>
                {
                    Debug.Log("Ultra login failed: " + error);
                    HudHelper.DisplayMessageDialog("AUTHENTICATION ERROR", "PLEASE MAKE SURE YOU HAVE ACCESS TO ULTRA ENDPOINTS", "TRY AGAIN", () =>
                    {
                        LaunchBrowserLogin();
                    });
                };

                LaunchBrowserLogin();
            }
        }

        public void LaunchBrowserLogin()
        {
            UltraManager.singleton.Init();
        }
        override public void ExitSubState()
        {
            MainMenuState menu = GStateManager.Instance.CurrentState as MainMenuState;
            if (menu != null && !GPlayerMgr.Instance.IsUniversalIdAttached())
            {
                // UniversalID wasn't attached so restore the user's name to its previous value.
                menu.RestoreName();
            }
            base.ExitSubState();
        }
        #endregion

        #region Private


        // call this when you want to close down the state
        private void onCompleteConnectingSubState()
        {
            GCore.Wrapper.SetStoredProfileId(GCore.Wrapper.Client.ProfileId);
            GCore.Wrapper.SetStoredAnonymousId(GCore.Wrapper.Client.AuthenticationService.AnonymousId);

            GStateManager.Instance.EnableLoadingSpinner(false);
            GStateManager.Instance.PopSubState(_stateInfo);

            if (GCore.IsFreshLaunch)
            {
                GCore.IsFreshLaunch = false;
            }

            GStateManager.Instance.ChangeState(MainMenuState.STATE_NAME);
        }

        private void authenticateBraincloud()
        {
            GCore.Wrapper.AuthenticateAnonymous(onAuthSuccess, onAuthFail);
        }

        private void onAuthSuccess(string jsonResponse, object cbObject)
        {
            // pass off to the PlayerMgr
            GPlayerMgr.Instance.OnAuthSuccess(jsonResponse, cbObject);
            onConnectComplete();
        }

        private void onAttachSuccess(string jsonResponse, object cbObject)
        {
            GPlayerMgr.Instance.PlayerData.PlayerName = Ultraio.Ultra.Client.Username;
            if (m_lastAuthType == AuthenticationType.Ultra)
            {
                GPlayerMgr.Instance.PlayerData.UniversalId = Ultraio.Ultra.Client.Username;
                GCore.Wrapper.Client.PlayerStateService.UpdateUserName(Ultraio.Ultra.Client.Username);
            }
            GCore.Instance.ProcessRetryQueue();
            onCompleteConnectingSubState();
        }

        private void displayPlayerInMatchDisplay()
        {
            GStateManager.Instance.EnableLoadingSpinner(true);
            Invoke("authenticateBraincloud", 15.0f);
        }

        private void displayPlayerInMatchMessage()
        {
            HudHelper.DisplayMessageDialog("HOLD ON", "YOUR TEAM IS CURRENTLY COMPETING AGAINST ANOTHER AT THE MOMENT.  RETRYING.", "OK", displayPlayerInMatchDisplay);
        }

        private void displayTokenMisMatchMessage()
        {
            if (m_lastAuthType == AuthenticationType.Universal)
                HudHelper.DisplayMessageDialog("AUTHENTICATION ERROR", "THE USERNAME AND PASSWORD COMBINATION DO NOT MATCH.  PLEASE TRY AGAIN.", "OK");
            else if (m_lastAuthType == AuthenticationType.Email)
                HudHelper.DisplayMessageDialog("AUTHENTICATION ERROR", "THE EMAIL AND PASSWORD COMBINATION DO NOT MATCH.  PLEASE TRY AGAIN.", "OK");

            // Add support for other Authentication types here
        }

        private void displayDuplicateIdentityTypeMessage()
        {
            HudHelper.DisplayMessageDialog("WARNING", "THIS NAME IS ALREADY TAKEN, PLEASE TRY ANOTHER ONE.", "OK");
        }

        private void onAuthFail(int status, int reasonCode, string jsonError, object cbObject)
        {
            // pass off to the PlayerMgr
            if (GPlayerMgr.Instance.onAuthFail(status, reasonCode, jsonError, cbObject))
                return;

            switch (reasonCode)
            {
                case ReasonCodes.TOKEN_DOES_NOT_MATCH_USER:
                    {
                        displayTokenMisMatchMessage();
                    }
                    break;
                case ReasonCodes.PLAYER_IN_MATCH:
                    {
                        displayPlayerInMatchMessage();
                    }
                    break;
                case ReasonCodes.UNABLE_TO_VALIDATE_PLAYER:
                case ReasonCodes.PLAYER_SESSION_EXPIRED:
                case ReasonCodes.NO_SESSION:
                case ReasonCodes.PLAYER_SESSION_LOGGED_OUT:
                    {
                        authenticateBraincloud();
                    }
                    break;
                case ReasonCodes.SWITCHING_PROFILES:
                case ReasonCodes.MISSING_IDENTITY_ERROR:
                    {
                        // lets clear the info. and reauth
                        GCore.Wrapper.ResetStoredProfileId();
                        GCore.Wrapper.ResetStoredAnonymousId();

                        authenticateBraincloud();
                    }
                    break;
                case ReasonCodes.DUPLICATE_IDENTITY_TYPE:
                case ReasonCodes.NEW_CREDENTIAL_IN_USE:
                    displayDuplicateIdentityTypeMessage();
                    break;
                default:
                    break;
            }
        }

        private void onAttachSteamAccount(string in_response, object obj)
        {
            GStateManager.Instance.EnableLoadingSpinner(true);
            // lets try a full re-auth afterwards
            authenticateBraincloud();
        }

        private void onConnectComplete()
        {
            // check the attached identities
#if !STEAMWORKS_ENABLED
            if (GPlayerMgr.Instance.IsEmailAttached())
            {
                GCore.Instance.ProcessRetryQueue();
                onCompleteConnectingSubState();
            }
            else
#endif
            {
                GStateManager.Instance.EnableLoadingSpinner(true);
                GEventManager.StartListening(GEventManager.ON_IDENTITIES_UPDATED, onIdentitiesUpdated);
            }
        }

        private void onIdentitiesUpdated()
        {
            GEventManager.StopListening(GEventManager.ON_IDENTITIES_UPDATED, onIdentitiesUpdated);
#if STEAMWORKS_ENABLED
            if (!GPlayerMgr.Instance.IsSteamIdAttached())
            {
                GSteamAuthManager.Instance.AttachSteamAccount(true, onAttachSteamAccount, onAuthFail);
                m_lastAuthType = AuthenticationType.Steam;
            }
            else 
#endif
            if (GPlayerMgr.Instance.IsUniversalIdAttached())
            {
                // universal IS ATTACHED
                GCore.Instance.ProcessRetryQueue();
                onCompleteConnectingSubState();
            }
            else
            {
                GStateManager.Instance.EnableLoadingSpinner(false);
                Panel.SetActive(true);
            }
        }

        private void onReadPlayerStateAfterMergeIdentitySuccess(string jsonResponse, object cbObject)
        {
            GStateManager.Instance.EnableLoadingSpinner(false);
            GCore.Instance.ProcessRetryQueue();
            onCompleteConnectingSubState();
        }



        #endregion
        private int MIN_CHARACTERS = 3;
        private int MAX_CHARACTERS = 25;
        private AuthenticationType m_lastAuthType = AuthenticationType.Anonymous;
        private AuthenticationType m_defaultAuthType = AuthenticationType.Universal;
    }
}
