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

        #region BaseState

        protected override void Start()
        {
            _stateInfo = new StateInfo(STATE_NAME, this);

            GStateManager.Instance.EnableLoadingSpinner(true);

            base.Start();

            GPlayerMgr.Instance.GetBlockchainItems(GetUpdatedBlockchainItems);
        }

        #endregion

        #region Public

        public void GetUpdatedBlockchainItems(bool success)
        {
            // First clear all cards
            for (int i = 0; i < HangarContent.childCount; i++)
            {
                Destroy(HangarContent.GetChild(i).gameObject);
            }

            // Find all skins & display them as hangar cards
            HangarPlaneCard card;
            Dictionary<int, int> items = GPlayerMgr.Instance.BlockchainItems;
            PlaneScriptableObject[] planeSkinsDataObjects = Resources.LoadAll<PlaneScriptableObject>("PlaneData");
            foreach (int id in items.Keys)
            {
                PlaneScriptableObject planeDataEntry = planeSkinsDataObjects.Where(x => x.planeID == id).FirstOrDefault();
                if (planeDataEntry != null)
                {
                    card = GEntityFactory.Instance.CreateResourceAtPath("Prefabs/UI/HangarPlaneCard", HangarContent).GetComponent<HangarPlaneCard>();
                    card.LateInit(planeDataEntry, items[id]);
                    card.OnActivateClickedAction += OnSetPlaneID;
                }
                else
                {
                    Debug.Log($"Couldn't find plane skin data with ID: {id}");
                }
            }

            GPlayerMgr.Instance.GetPlayerPlaneIDSkin(ToggleCurrentPlaneIDSkin);
        }

        #endregion

        #region Private

        private void OnSetPlaneID(int planeID)
        {
            GStateManager.Instance.EnableLoadingSpinner(true);

            GPlayerMgr.Instance.SetPlayerPlaneIDSkin(planeID, ToggleCurrentPlaneIDSkin);
        }

        private void ToggleCurrentPlaneIDSkin(int planeID)
        {
            foreach (HangarPlaneCard card in HangarContent.GetComponentsInChildren<HangarPlaneCard>())
            {
                card.ToggleActive(card.PlaneSkinSOData.planeID == planeID);
            }

            GStateManager.Instance.EnableLoadingSpinner(false);
        }

        #endregion
    }
}
