using BrainCloud.JsonFx.Json;
using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private GameObject Canvas = null;

        [Header("Message")]
        [SerializeField] private Image MessageImage = null;

        [Header("Dialog")]
        [SerializeField] private Image DialogImage = null;
        [SerializeField] private TextMeshProUGUI DialogMessageText = null;
        [SerializeField] private GameObject DialogButton = null;

        private int currentPlaneSkinID = 0; // Stores Prefab ID for plane skin

        private void Start()
        {
            ResetAnimation();

            currentPlaneSkinID = 0; // Get current ID?

            GCore.Wrapper.RTTService.RegisterRTTBlockchainRefresh(OnBlockchainRefresh);
        }

        private void OnDestroy()
        {
            ResetAnimation();

            GCore.Wrapper.RTTService.DeregisterRTTBlockchainRefresh();
        }

        private void OnBlockchainRefresh(string in_message)
        {
            Dictionary<string, object> jsonMessage = (Dictionary<string, object>)JsonReader.Deserialize(in_message);
            switch (jsonMessage["operation"] as string)
            {
                case "BOMBER_AVAILABLE": // TODO: Get proper Json operation
                    // TODO: Be able to get the ID of the new unlocked Bomber from Json
                    // TODO: Populate various components with proper Json data (image, message)
                    MessageImage.sprite = null; // TODO: Grab the sprite based on the bomber ID
                    DialogImage.sprite = null; // TODO: Grab the sprite based on the bomber ID
                    DialogMessageText.text = "New Bomber Available!";
                    DialogButton.gameObject.SetActive(true);
                    StartAnimation();
                    return;
                default:
                    Debug.Log("(Blockchain Refresh received - NEED TO SHOW MESSAGE)");
                    return;
            }
        }

        private void ResetAnimation()
        {
            NewBombersAnimator.SetTrigger(RESET_ANIMATION);
            NewBombersAnimator.ResetTrigger(PLAY_ANIMATION);
        }

        private void StartAnimation()
        {
            ResetAnimation();

            NewBombersAnimator.SetTrigger(PLAY_ANIMATION);
        }

        public void OnDitchAndSwitchButton()
        {
            ResetAnimation();

            // TODO: Destroy plane and set the new ID to be able to swap to new plane skin
            currentPlaneSkinID = 0; // Store new ID?

            Debug.Log("Ditching & Switching...");
        }
    }
}
