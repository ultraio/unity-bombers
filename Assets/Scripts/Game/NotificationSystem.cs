using BrainCloud;
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
        [Header("Announcement")]
        [SerializeField] private RectTransform AnnouncementRect = null;
        [SerializeField] private CanvasGroup AnnouncementCG = null;
        [SerializeField] private TextMeshProUGUI AnnouncementText = null;

        [Header("Popup")]
        [SerializeField] private RectTransform PopupRect = null;
        [SerializeField] private CanvasGroup PopupCG = null;
        [SerializeField] private Image PopupImage = null;
        [SerializeField] private TextMeshProUGUI PopupMessageText = null;
        [SerializeField] private GameObject PopupButton = null;
        //[SerializeField] private TextMeshProUGUI PopupButtonText = null;

        private string currentPlaneSkinID = ""; // Stores Prefab ID for plane skin

        private void Start()
        {
            ResetAnimation();

            currentPlaneSkinID = ""; // Get current ID?

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
                    AnnouncementText.text = "NEW BOMBER AVAILABLE!";
                    PopupMessageText.text = "New Bomber Available!";
                    StartAnimation();
                    return;
                default:
                    Debug.Log("(Blockchain Refresh received - NEED TO SHOW MESSAGE)");
                    return;
            }
        }

        private void ResetAnimation()
        {
            StopAllCoroutines();
            PopupCG.interactable = false;
            AnnouncementRect.gameObject.SetActive(false);
            PopupRect.gameObject.SetActive(false);
        }

        private void StartAnimation()
        {
            ResetAnimation();

            //TODO: Should animation be done via Mecanim? Or scripted?
            StartCoroutine(TempAnimation());
        }

        private IEnumerator TempAnimation()
        {
            AnnouncementRect.gameObject.SetActive(true);

            yield return new WaitForSecondsRealtime(1.5f);

            AnnouncementRect.gameObject.SetActive(false);
            PopupRect.gameObject.SetActive(true);
            PopupCG.interactable = true;

            yield return new WaitForSecondsRealtime(3.0f);

            ResetAnimation();
        }

        public void OnDitchAndSwitchButton()
        {
            ResetAnimation();

            // TODO: Destroy plane and set the new ID to be able to swap to new plane skin
            currentPlaneSkinID = ""; // Store new ID?

            Debug.Log("Ditching & Switching...");
        }
    }
}
