using BrainCloud.JsonFx.Json;
using BrainCloudUNETExample.Game;
using System.Collections.Generic;
using System;
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
        [Header("Editor Only")]
        [SerializeField] private string BlockchainTestData = null;
#endif

        private GameManager gameManager = null;
        private int newPlaneSkin = -1;
        private bool canSwitchPlanes = false;
        PlaneScriptableObject[] planeSkinsDataObjects = null;

        private void Start()
        {
            ResetAnimation();

            if (gameManager == null) gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            planeSkinsDataObjects = Resources.LoadAll<PlaneScriptableObject>("PlaneData");

            GCore.Wrapper.RTTService.RegisterRTTBlockchainItemEvent(OnBlockchainItemEvent);
        }

        private void OnDestroy()
        {
            GCore.Wrapper.RTTService.DeregisterRTTBlockchainItemEvent();

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

            Dictionary<string, object> jsonMessage = (Dictionary<string, object>)JsonReader.Deserialize(in_message);
            Dictionary<string, object> jsonData = (Dictionary<string, object>)jsonMessage[BrainCloudConsts.JSON_DATA];
            Dictionary<string, object> newItemJSON = (Dictionary<string, object>)jsonData["newJSON"];
            Dictionary<string, object> newItemData = (Dictionary<string, object>)newItemJSON["object"];

            int factoryID = (int)newItemData["token_factory_id"];

            GPlayerMgr.Instance.BlockchainItems.TryGetValue(factoryID, out int currentItemCount);

            Action<bool> onGetBlockchainItems = (bool success) =>
            {
                int updatedItemCount = 0;
                bool isInsertEvent = jsonMessage["operation"] as string == "ITEM_EVENT" && jsonData["operation"] as string == "INS";
                bool ownsSkin = success && GPlayerMgr.Instance.BlockchainItems.TryGetValue(factoryID, out updatedItemCount);
                if (isInsertEvent && ownsSkin && updatedItemCount > currentItemCount)
                {
                    newPlaneSkin = factoryID;
                    PlaneScriptableObject planeDataEntry = planeSkinsDataObjects.Where(x => x.planeID == newPlaneSkin).FirstOrDefault();
                    MessageImage.sprite = planeDataEntry.planeThumbnail_green;
                    DialogImage.sprite = planeDataEntry.planeThumbnail_green;
                    DialogMessageText.text = "New Bomber Available!";

                    StartAnimation();
                }
                else if (!ownsSkin || (updatedItemCount <= 0 && currentItemCount > 0)) // Current skin is no longer on account, need to remove...
                {
                    GPlayerMgr.Instance.SetPlayerPlaneIDSkin(PlaneScriptableObject.DEFAULT_SKIN_ID, null); // Do remove on player too
                }
            };

            GPlayerMgr.Instance.GetBlockchainItems(onGetBlockchainItems);
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

        public void OnDitchAndSwitchButton()
        {
            if (!canSwitchPlanes) return;

            ResetAnimation();

            gameManager.DitchAndSwitchPlaneSkin(newPlaneSkin);

            Debug.Log($"Swapping to Plane Skin ID: {newPlaneSkin}");
        }

        // Gets called by an animation event when the notification disappears
        public void DisablePlaneSwitching()
        {
            canSwitchPlanes = false;
        }
    }
}
