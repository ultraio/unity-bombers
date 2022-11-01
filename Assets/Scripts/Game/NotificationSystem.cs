using BrainCloud.JsonFx.Json;
using BrainCloud.LitJson;
using BrainCloudUNETExample.Game;
using System.Collections;
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
        [SerializeField] private GameObject Canvas = null;

        [Header("Message")]
        [SerializeField] private Image MessageImage = null;

        [Header("Dialog")]
        [SerializeField] private Image DialogImage = null;
        [SerializeField] private TextMeshProUGUI DialogMessageText = null;
        [SerializeField] private GameObject DialogButton = null;

        [SerializeField] private string BlockchainTestData = null;

        //lazy loading GameManager
        //TODO: GameManager might be better as a singleton 
        private GameManager m_gMan;
        private int m_newPlaneSkin = 0;

        private void Start()
        {
            ResetAnimation();
            if (m_gMan == null) m_gMan = GameObject.Find("GameManager").GetComponent<GameManager>();

            GCore.Wrapper.RTTService.RegisterRTTBlockchainItemEvent(OnBlockchainItemEvent);
        }

        private void OnDestroy()
        {
            ResetAnimation();

            GCore.Wrapper.RTTService.DeregisterRTTBlockchainItemEvent();
        }

#if UNITY_EDITOR
        private void Update()
        {
            //Test blockchain event
            if (Input.GetKeyDown(KeyCode.K))
            {
                OnBlockchainItemEvent(BlockchainTestData);
            }
        }
#endif
        private void OnBlockchainItemEvent(string in_message)
        {
            if (string.IsNullOrEmpty(in_message)) return;

            JsonData jsonData = JsonMapper.ToObject(in_message);
            PlaneScriptableObject[] planeSkinsDataObjects = Resources.LoadAll<PlaneScriptableObject>("PlaneData");
            switch (jsonData["operation"].ToString())
            {
                case "ITEM_EVENT": 
                    if (jsonData["data"]["operation"].ToString() == "INS")
                    {
                        //only looking for INSERT events to show that there is a new item
                        int factoryId = (int)jsonData["data"]["newJSON"]["object"]["token_factory_id"];
                        //find plane skin data with this ID
                        PlaneScriptableObject planeDataEntry = planeSkinsDataObjects.Where(x => x.planeID == factoryId).FirstOrDefault();
                        if (planeDataEntry != null)
                        {
                            m_newPlaneSkin = factoryId;
                            MessageImage.sprite = planeDataEntry.planeThumbnail_green;
                            DialogImage.sprite = planeDataEntry.planeThumbnail_green;
                            DialogMessageText.text = "New Bomber Available!";
                            DialogButton.gameObject.SetActive(true);
                            StartAnimation();
                        }
                        else
                        {
                            Debug.Log("Couldn't find plane skin data with ID: " + factoryId);
                        }
                    }
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
            //disable button 
            DialogButton.gameObject.SetActive(false);

            m_gMan.DitchAndSwitchPlaneSkin(m_newPlaneSkin);

            Debug.Log("Ditching & Switching...");
        }
    }
}
