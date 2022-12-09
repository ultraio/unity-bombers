using Gameframework;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BrainCloudUNETExample
{
    public class HangarPlaneCard : BaseBehaviour
    {
        [SerializeField] private TextMeshProUGUI Title = null;
        [SerializeField] private TextMeshProUGUI Description = null;
        [SerializeField] private Button ActivateButton = null;
        [SerializeField] private Image UpperImage = null;
        [SerializeField] private Image Spinner = null;
        [SerializeField] private GameObject UpperBG = null;
        [SerializeField] private GameObject ActivatedLabel = null;
        [SerializeField] private GameObject ItemCountBox = null;
        [SerializeField] private TextMeshProUGUI ItemCountText = null;

        public Action<int, int> OnActivateClickedAction;

        public PlaneScriptableObject PlaneSkinSOData { get; private set; }
        public int CurrentPlaneSkinCount { get; private set; }

        #region public
        public void LateInit(PlaneScriptableObject in_data, int count)
        {
            UpdatePlaneSkin(in_data, count);

            Title.text = PlaneSkinSOData.planeName;
            Description.text = PlaneSkinSOData.planeDescription;

            if (UpperImage != null) UpperImage.sprite = PlaneSkinSOData.planeThumbnail_green;
            if (UpperBG != null) UpperBG.SetActive(true);
            if (Spinner != null) Spinner.gameObject.SetActive(false);

            ActivateButton.onClick.AddListener(OnActivateClicked);
        }

        public void UpdatePlaneSkin(PlaneScriptableObject in_data, int count)
        {
            PlaneSkinSOData = in_data;
            CurrentPlaneSkinCount = count;

            if (CurrentPlaneSkinCount > 1)
            {
                ItemCountBox.SetActive(true);
                ItemCountText.text = CurrentPlaneSkinCount.ToString();
            }
            else
            {
                ItemCountBox.SetActive(false);
                ItemCountText.text = string.Empty;
            }
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
            OnActivateClickedAction?.Invoke(PlaneSkinSOData.planeID, CurrentPlaneSkinCount);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            ActivateButton.onClick.RemoveAllListeners();
            OnActivateClickedAction = null;
        }

        #endregion
    }
}
