using System.Collections.Generic;
using UnityEngine;
using Gameframework;

using System.IO;
using BrainCloud.LitJson;

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

            

            foreach(PlaneScriptableObject planeDataEntry in Resources.LoadAll("PlaneData", typeof(PlaneScriptableObject)))
            {
                //PlaneScriptableObject planeDataEntry = Resources.Load<PlaneScriptableObject>(Path.Combine("PlaneData", Path.GetFileNameWithoutExtension(f.Name)));
                planeData.Add(planeDataEntry);
            }

            UpdateData();

            //clear list
            content = this.transform.FindDeepChild("Content");
            for (int i = 0; i < content.childCount; ++i)
            {
                Destroy(content.GetChild(i).gameObject);
            }

            GameObject cardGO;
            HangarPlaneCard card;

            for (int i = 0; i < planeData.Count; ++i)
            {
                cardGO = GEntityFactory.Instance.CreateResourceAtPath("Prefabs/UI/HangarPlaneCard", content);
                card = cardGO.GetComponent<HangarPlaneCard>();
                card.LateInit(planeData[i]);
                card.OnActivateClickedAction += OnSetPlaneID;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
        #endregion

        #region Public 
        #endregion

        #region Private

        private void UpdateData()
        {
            GCore.Wrapper.EntityService.GetSingleton("PlaneSkin", OnPlaneIDSuccessCallback, OnPlaneIDFailureCallback);
        }

        private void OnPlaneIDSuccessCallback(string responseData, object cbObject)
        {
            JsonData jsonData = JsonMapper.ToObject(responseData);
            JsonData entry = jsonData["data"]["data"];
            latestVersion = int.Parse(jsonData["data"]["version"].ToString());
            int planeID = int.Parse(entry[GBomberRTTConfigManager.PLANE_SKIN_ID].ToString());
            
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

            GCore.Wrapper.EntityService.UpdateSingleton("PlaneSkin", jsonData.ToString(), "", latestVersion, OnSetPlaneIDSuccess, OnPlaneIDFailureCallback);
        }

        private void OnSetPlaneIDSuccess(string responseData, object cbObject)
        {
            UpdateData();
        }

        #endregion
    }
}
