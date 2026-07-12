using TMPro;
using UnityEngine;

namespace APIs_Helpers
{
    public class SetCode : MonoBehaviour
    {
        public GameObject tmptext;
        public string CurrentCode { get; private set; }

        private void Start()
        {
            SetNewCode();
        }

        public string GetOrGenerateCode()
        {
            if (string.IsNullOrWhiteSpace(CurrentCode)) SetNewCode();

            return CurrentCode;
        }

        private void SetNewCode()
        {
            CurrentCode = GetRandomCode.GetRandom(6);
            Debug.Log($"Got code: {CurrentCode}");

            if (tmptext == null)
            {
                Debug.LogWarning("SetCode.tmptext is not assigned. Code generated but could not be displayed.");
                return;
            }

            var label = tmptext.GetComponent<TextMeshProUGUI>();
            if (label == null)
            {
                Debug.LogWarning("SetCode.tmptext does not have a TextMeshProUGUI component.");
                return;
            }

            label.text = CurrentCode;
        }
    }
}