using BrainCloud.JsonFx.Json;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameframework
{
    public class NotificationSystem : MonoBehaviour
    {
        private static int PLAY_ANIMATION = Animator.StringToHash("PlayAnimation");
        private static int RESET_ANIMATION = Animator.StringToHash("ResetAnimation");

        [Header("Notification System")]
        [SerializeField] private Animator NewBombersAnimator = null;

        [Header("Message")]
        [SerializeField] private Image MessageImage = null;

        [Header("Dialog")]
        [SerializeField] private Image DialogImage = null;
        [SerializeField] private TextMeshProUGUI DialogMessageText = null;

#if UNITY_EDITOR
        [Header("Editor Only"), TextArea(1, 100)]
        [SerializeField] private string BlockchainTestData = null;
#endif

        private bool canSwitchPlanes = false;
        private int newPlaneIDSkin = PlaneScriptableObject.DEFAULT_SKIN_ID;
        private BrainCloudUNETExample.Game.GameManager gameManager = null;
        PlaneScriptableObject[] planeSkinsDataObjects = null;

        private void Start()
        {
            ResetAnimation();

            if (gameManager == null) gameManager = GameObject.Find("GameManager")
                                     .GetComponent<BrainCloudUNETExample.Game.GameManager>();

            planeSkinsDataObjects = Resources.LoadAll<PlaneScriptableObject>("PlaneData");

            GCore.Wrapper?.RTTService.RegisterRTTBlockchainItemEvent(OnBlockchainItemEvent);
        }

        private void OnDestroy()
        {
            GCore.Wrapper?.RTTService.DeregisterRTTBlockchainItemEvent();

            for (int i = 0; i < planeSkinsDataObjects.Length; i++)
            {
                Resources.UnloadAsset(planeSkinsDataObjects[i]);
                planeSkinsDataObjects[i] = null;
            }

            planeSkinsDataObjects = null;

            ResetAnimation();
        }

        private void Update()
        {
#if UNITY_EDITOR
            // Test blockchain event
            if (Input.GetKeyDown(KeyCode.K))
            {
                OnBlockchainItemEvent(BlockchainTestData);
            }
#endif
            if (Input.GetKeyDown(KeyCode.B))
            {
                OnDitchAndSwitchButton();
            }
        }

        private void OnBlockchainItemEvent(string in_message)
        {
            if (string.IsNullOrEmpty(in_message)) return;

            var message = JsonReader.Deserialize<Dictionary<string, object>>(in_message);
            var data = message["data"] as Dictionary<string, object>;

            int factoryID = PlaneScriptableObject.DEFAULT_SKIN_ID;
            string eventType = message["operation"] != null ? (string)message["operation"] : string.Empty;
            if (!string.IsNullOrEmpty(eventType) && eventType == "ITEM_EVENT")
            {
                eventType = (string)data["operation"];
                switch (eventType)
                {
                    case "INS":
                    case "UDP":
                        factoryID = (int)((data["newJSON"] as Dictionary<string, object>)
                                               ["object"] as Dictionary<string, object>)["token_factory_id"];
                        break;
                    case "REM":
                        factoryID = (int)((data["oldJSON"] as Dictionary<string, object>)
                                               ["object"] as Dictionary<string, object>)["token_factory_id"];
                        break;
                    default:
                        break;
                }
            }

            if (planeSkinsDataObjects.Where(x => x.planeID == factoryID).FirstOrDefault() == null)
            {
                return; // Skin Asset doesn't exist so let's exit...
            }

            GPlayerMgr.Instance.BlockchainItems.TryGetValue(factoryID, out int currentItemCount);

            GPlayerMgr.Instance.GetBlockchainItems((bool success) =>
            {
                GPlayerMgr.Instance.GetPlayerPlaneIDSkin((int planeID) =>
                {
                    int updatedItemCount = currentItemCount;
                    bool isInsertEvent = eventType == "INS" || eventType == "UDP";
                    bool ownsSkin = success && GPlayerMgr.Instance.BlockchainItems.TryGetValue(factoryID, out updatedItemCount);
                    if (isInsertEvent && ownsSkin && planeID != factoryID && updatedItemCount > currentItemCount)
                    {
                        newPlaneIDSkin = factoryID;
                        PlaneScriptableObject planeDataEntry = planeSkinsDataObjects.Where(x => x.planeID == factoryID).FirstOrDefault();
                        MessageImage.sprite = planeDataEntry.planeThumbnail_green;
                        DialogImage.sprite = planeDataEntry.planeThumbnail_green;
                        DialogMessageText.text = "New Bomber Available!";

                        StartAnimation();
                    }
                    else if (planeID == factoryID && (!ownsSkin || (updatedItemCount <= 0 && currentItemCount > 0))) // Current skin is no longer on account, need to remove...
                    {
                        newPlaneIDSkin = PlaneScriptableObject.DEFAULT_SKIN_ID;
                        GPlayerMgr.Instance.SetPlayerPlaneIDSkin(newPlaneIDSkin, gameManager.SilentSwitchPlaneSkin); // Update skin to default for everyone
                    }
                });
            });
        }

        private void ResetAnimation()
        {
            NewBombersAnimator.SetTrigger(RESET_ANIMATION);
            NewBombersAnimator.ResetTrigger(PLAY_ANIMATION);
            canSwitchPlanes = false;
        }

        private void StartAnimation()
        {
            ResetAnimation();
            canSwitchPlanes = true;
            NewBombersAnimator.SetTrigger(PLAY_ANIMATION);
        }

        // Gets called by an animation event when the notification disappears
        public void DisablePlaneSwitching()
        {
            canSwitchPlanes = false;
        }
        
        public void OnDitchAndSwitchButton()
        {
            if (!canSwitchPlanes) return;

            ResetAnimation();

            gameManager.DitchAndSwitchPlaneSkin(newPlaneIDSkin);

            Debug.Log($"Swapping to Plane Skin ID: {newPlaneIDSkin}");
        }
    }
}
