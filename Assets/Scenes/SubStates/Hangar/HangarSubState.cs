using BrainCloud;
using BrainCloud.LitJson;
using Gameframework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BrainCloudUNETExample
{
    public class HangarSubState : BaseSubState
    {
        public static string STATE_NAME = "hangar";

        [SerializeField] private Transform HangarContent;

        private int latestVersion;

        #region BaseState

        protected override void Start()
        {
            _stateInfo = new StateInfo(STATE_NAME, this);

            base.Start();

            GetUpdatedBlockchainItems();
        }

        #endregion

        #region Public

        public void GetUpdatedBlockchainItems()
        {
            GStateManager.Instance.EnableLoadingSpinner(true);

            List<PlaneScriptableObject> planeData = new List<PlaneScriptableObject>();
            bool.TryParse(GConfigManager.GetStringValue("suppressDuplicateBomberSkins"), out bool suppressDuplicates);

            SuccessCallback successCB = (string response, object cbObject) =>
            {
                Debug.Log($"Received Blockchain items: {response}");

                JsonData jsonData = JsonMapper.ToObject(response);
                JsonData items = jsonData["data"]["response"]["items"];

                Dictionary<int, int> itemFactoryIds = new Dictionary<int, int>();
                PlaneScriptableObject[] planeSkinsDataObjects = Resources.LoadAll<PlaneScriptableObject>("PlaneData");

                // Add the default Plane Skin ID
                itemFactoryIds.Add(PlaneScriptableObject.DEFAULT_SKIN_ID, 1);

                // Find all skins
                for (int i = 0; i < items.Count; i++)
                {
                    int factoryID = int.Parse(items[i]["json"]["token_factory_id"].ToString());
                    if (itemFactoryIds.ContainsKey(factoryID))
                    {
                        itemFactoryIds[factoryID]++;
                    }
                    else
                    {
                        itemFactoryIds.Add(factoryID, 1);
                    }
                }

                for (int i = 0; i < HangarContent.childCount; ++i)
                {
                    Destroy(HangarContent.GetChild(i).gameObject);
                }

                // Add and initialize the plane skin UI cards
                GameObject cardGO;
                HangarPlaneCard card;
                foreach (KeyValuePair<int, int> kvp in itemFactoryIds)
                {
                    PlaneScriptableObject planeDataEntry = planeSkinsDataObjects.Where(x => x.planeID == kvp.Key).FirstOrDefault();
                    if (planeDataEntry != null)
                    {
                        cardGO = GEntityFactory.Instance.CreateResourceAtPath("Prefabs/UI/HangarPlaneCard", HangarContent);
                        card = cardGO.GetComponent<HangarPlaneCard>();

                        card.LateInit(planeDataEntry, suppressDuplicates ? 1 : kvp.Value);
                        card.OnActivateClickedAction += OnSetPlaneID;
                    }
                    else
                    {
                        Debug.Log($"Couldn't find plane skin data with ID: {kvp.Key}");
                    }
                }

                // Get the users selected PlaneSkinID
                UpdateUserSelection();
            };

            FailureCallback failureCB = (int status, int code, string error, object cbObject) =>
            {
                HudHelper.DisplayMessageDialog("BLOCKCHAIN ERROR", $"Blockchain items failed to load | {status} {code} {error}", "OK");

                DisplayDefaultSkinOnly();

                OnSetPlaneID(PlaneScriptableObject.DEFAULT_SKIN_ID, 1);
            };

            GCore.Wrapper.Client.Blockchain.GetBlockchainItems("default", "{}", successCB, failureCB);
        }

        #endregion

        #region Private

        private void DisplayDefaultSkinOnly()
        {
            if (HangarContent.childCount == 1 &&
                HangarContent.GetComponentInChildren<HangarPlaneCard>() != null &&
                HangarContent.GetComponentInChildren<HangarPlaneCard>().PlaneSkinSOData.IsDefaultSkin)
                return;

            for (int i = 0; i < HangarContent.childCount; ++i)
            {
                Destroy(HangarContent.GetChild(i).gameObject);
            }

            GameObject cardGO;
            HangarPlaneCard card;
            cardGO = GEntityFactory.Instance.CreateResourceAtPath("Prefabs/UI/HangarPlaneCard", HangarContent);
            card = cardGO.GetComponent<HangarPlaneCard>();
            PlaneScriptableObject defaultPlaneData = Resources.Load<PlaneScriptableObject>("PlaneData/00_Default");
            card.LateInit(defaultPlaneData, 1);
            card.OnActivateClickedAction += OnSetPlaneID;
        }

        private void UpdateUserSelection()
        {
            GCore.Wrapper.EntityService.GetSingleton("PlaneSkin", OnPlaneIDSuccessCallback, OnPlaneIDFailure);
        }

        private void OnPlaneIDSuccessCallback(string responseData, object cbObject)
        {
            JsonData jsonData = JsonMapper.ToObject(responseData);
            JsonData entry = jsonData["data"];

            int planeID = PlaneScriptableObject.DEFAULT_SKIN_ID;
            if(entry != null)
            {
                latestVersion = int.Parse(jsonData["data"]["version"].ToString());
                planeID = int.Parse(entry["data"][GBomberRTTConfigManager.PLANE_SKIN_ID].ToString());
            }

            ToggleCurrentPlaneIDSkin(planeID);

            GStateManager.Instance.EnableLoadingSpinner(false);
        }

        private void OnSetPlaneID(int planeID, int count)
        {
            GStateManager.Instance.EnableLoadingSpinner(true);

            Dictionary<string, object> stats = new Dictionary<string, object>
            {
                {GBomberRTTConfigManager.PLANE_SKIN_ID, planeID},
                {GBomberRTTConfigManager.CURRENT_PLANE_SKIN_ID_COUNT, count}
            };

            Dictionary<string, object> acl = new Dictionary<string, object>
            {
                {"other", 1 }
            };

            JsonData jsonData = JsonMapper.ToJson(stats);
            JsonData aclJson = JsonMapper.ToJson(acl);

            GCore.Wrapper.EntityService.UpdateSingleton("PlaneSkin", jsonData.ToString(), aclJson.ToString(), latestVersion, OnSetPlaneIDSuccess, OnPlaneIDFailure);
        }

        private void OnSetPlaneIDSuccess(string responseData, object cbObject)
        {
            UpdateUserSelection();
        }

        private void OnPlaneIDFailure(int status, int reasonCode, string jsonError, object cbObject)
        {
            Debug.LogError($"Couldn't get plane ID of user: {status} {reasonCode}");

            DisplayDefaultSkinOnly();

            GStateManager.Instance.EnableLoadingSpinner(false);
        }

        private void ToggleCurrentPlaneIDSkin(int planeID)
        {
            foreach (HangarPlaneCard card in HangarContent.GetComponentsInChildren<HangarPlaneCard>())
            {
                card.ToggleActive(card.PlaneSkinSOData.planeID == planeID);
            }
        }

        #endregion
    }
}
