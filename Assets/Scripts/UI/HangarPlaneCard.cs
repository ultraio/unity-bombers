using Gameframework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

namespace BrainCloudUNETExample
{
    public class HangarPlaneCard : BaseBehaviour
    {
        public TextMeshProUGUI Title = null;
        public TextMeshProUGUI Description = null;
        public Button ActivateButton = null;
        public Image UpperImage = null;
        public Image Spinner = null;
        public GameObject UpperBG = null;
        public GameObject ActivatedLabel = null;
        public PlaneScriptableObject planeData;

        public Action<int> OnActivateClickedAction;

        #region public
        public void LateInit(PlaneScriptableObject in_data)
        {
            planeData = in_data;

            Title.text = planeData.planeName;
            Description.text = planeData.planeDescription;


            if (UpperImage != null) UpperImage.sprite = planeData.planeThumbnail_green;
            if (UpperBG != null) UpperBG.SetActive(true);
            if (Spinner != null) Spinner.gameObject.SetActive(false);

            ActivateButton.onClick.AddListener(OnActivateClicked);
        }

        public void ToggleActive(bool active)
        {
            ActivatedLabel.SetActive(active);
            ActivateButton.gameObject.SetActive(!active);
        }

        #endregion

        #region private

        private void OnActivateClicked()
        {
            OnActivateClickedAction?.Invoke(planeData.planeID);
        }

        private void OnDestroy()
        {
            ActivateButton.onClick.RemoveAllListeners();
            OnActivateClickedAction = null;
        }

        #endregion
    }
}
