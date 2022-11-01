using System.Collections.Generic;
using UnityEngine;
using Gameframework;

using System.IO;
using BrainCloud.LitJson;
using BrainCloud;
using System.Linq;

namespace BrainCloudUNETExample
{
    public class HangarSubState : BaseSubState
    {
        public static string STATE_NAME = "hangar";

        private Transform content;

        private string planeEntityID;
        private int latestVersion;

        #region BaseState
        protected override void Start()
        {
            _stateInfo = new StateInfo(STATE_NAME, this);
            base.Start();
            // TODO: fetch actual products
            // get placeholder items for now
            List<PlaneScriptableObject> planeData = new List<PlaneScriptableObject>();

            SuccessCallback successCB = (response, cbObject) =>
            {
                Debug.Log("Got blockchain items: " + response);
                JsonData jsonData = JsonMapper.ToObject(response);
                JsonData items = jsonData["data"]["response"]["items"];

                Dictionary<int, int> itemFactoryIds = new Dictionary<int, int>();
                PlaneScriptableObject[] planeSkinsDataObjects = Resources.LoadAll<PlaneScriptableObject>("PlaneData");
                //add the default plane skin id of 0
                itemFactoryIds.Add(0, 1);

                //loop all found NFTs
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
                //Get the users selected plane skin ID
                UpdateUserSelection();

                //clear list
                content = this.transform.FindDeepChild("Content");
                for (int i = 0; i < content.childCount; ++i)
                {
                    Destroy(content.GetChild(i).gameObject);
                }

                //Add and initialize the plane skin UI cards
                GameObject cardGO;
                HangarPlaneCard card;
                
                foreach(KeyValuePair<int, int> kvp in itemFactoryIds)
                {
                    PlaneScriptableObject planeDataEntry = planeSkinsDataObjects.Where(x => x.planeID == kvp.Key).FirstOrDefault();
                    if (planeDataEntry != null)
                    {
                        cardGO = GEntityFactory.Instance.CreateResourceAtPath("Prefabs/UI/HangarPlaneCard", content);
                        card = cardGO.GetComponent<HangarPlaneCard>();
                        card.LateInit(planeDataEntry, kvp.Value);
                        card.OnActivateClickedAction += OnSetPlaneID;
                    }
                    else
                    {
                        Debug.Log("Couldn't find plane skin data with ID: " + kvp.Key);
                    }
                }
            };

            FailureCallback failureCB = (status, code, error, cbObject) =>
            {
                HudHelper.DisplayMessageDialog("BLOCKCHAIN ERROR", string.Format("Blockchain items failed to load | {0} {1} {2}", status, code, error), "OK");

                UpdateUserSelection();

                //clear list
                content = this.transform.FindDeepChild("Content");
                for (int i = 0; i < content.childCount; ++i)
                {
                    Destroy(content.GetChild(i).gameObject);
                }

                //Just display the default plane
                GameObject cardGO;
                HangarPlaneCard card;
                cardGO = GEntityFactory.Instance.CreateResourceAtPath("Prefabs/UI/HangarPlaneCard", content);
                card = cardGO.GetComponent<HangarPlaneCard>();
                PlaneScriptableObject defaultPlaneData = Resources.Load<PlaneScriptableObject>("PlaneData/00_Default");
                card.LateInit(defaultPlaneData, 1);
                card.OnActivateClickedAction += OnSetPlaneID;
            };

            GCore.Wrapper.Client.Blockchain.GetBlockchainItems("default", "{}",successCB, failureCB);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
        #endregion

        #region Private

        private void UpdateUserSelection()
        {
            GCore.Wrapper.EntityService.GetSingleton("PlaneSkin", OnPlaneIDSuccessCallback, OnPlaneIDFailureCallback);
        }

        private void OnPlaneIDSuccessCallback(string responseData, object cbObject)
        {
            JsonData jsonData = JsonMapper.ToObject(responseData);
            JsonData entry = jsonData["data"];

            int planeID = 0;
            if(entry != null)
            {
                latestVersion = int.Parse(jsonData["data"]["version"].ToString());
                planeID = int.Parse(entry["data"][GBomberRTTConfigManager.PLANE_SKIN_ID].ToString());
            }
            
            foreach (Transform t in content)
            {
                HangarPlaneCard card = t.gameObject.GetComponent<HangarPlaneCard>();
                card.ToggleActive(card.planeData.planeID == planeID);
            }
        }

        private void OnPlaneIDFailureCallback(int status, int reasonCode, string jsonError, object cbObject)
        {
            Debug.LogError("Couldn't get plane ID of user: " + status + " " + reasonCode);
        }

        private void OnSetPlaneID(int planeID)
        {
            Dictionary<string, object> stats = new Dictionary<string, object>
            {
                {GBomberRTTConfigManager.PLANE_SKIN_ID, planeID}
            };

            JsonData jsonData = JsonMapper.ToJson(stats);

            Dictionary<string, object> aclData = new Dictionary<string, object>
            {
                {"other", 1 }
            };
            JsonData aclJson = JsonMapper.ToJson(aclData);

            GCore.Wrapper.EntityService.UpdateSingleton("PlaneSkin", jsonData.ToString(), aclJson.ToString(), latestVersion, OnSetPlaneIDSuccess, OnPlaneIDFailureCallback);
        }

        private void OnSetPlaneIDSuccess(string responseData, object cbObject)
        {
            UpdateUserSelection();
        }

        #endregion
    }
}
