using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace World
{
    /**
     *
     */
    public class ARSpawnBox : MonoBehaviour
    {
        private const float OnFailReturnDelay = 0.5f;

        [Header("References")]
        public Camera arCamera;

        public GameObject boxPrefab;
        public GameObject codePanel;
        public GameObject failPanel;

        [Header("Catch Animation")]
        public float boxFlyDuration = 0.5f;

        [Header("Fail Animation")]
        public float failDuration = 0.8f;

        public string mapSceneName = "MapView";

        public void PlayCatchAnimation(GameObject token, Vector3 swipeTarget)
        {
            StartCoroutine(CatchSequence(token, swipeTarget));
        }

        public void PlayFailAnimation(GameObject token, Vector3 swipeTarget)
        {
            StartCoroutine(FailSequence(token, swipeTarget));
        }

        private IEnumerator CatchSequence(GameObject token, Vector3 swipeTarget)
        {
            yield return FlyBoxToTarget(token.transform.position, swipeTarget);

            Destroy(token);

            if (codePanel != null) codePanel.SetActive(true);
        }

        private IEnumerator FailSequence(GameObject token, Vector3 swipeTarget)
        {
            yield return FlyBoxToTarget(swipeTarget);

            if (failPanel != null) failPanel.SetActive(true);

            yield return FloatTokenAway(token);

            Destroy(token);

            yield return new WaitForSeconds(OnFailReturnDelay);
            SceneManager.LoadSceneAsync(mapSceneName, LoadSceneMode.Single);
        }

        private IEnumerator FlyBoxToTarget(Vector3 target, Vector3? swipedPoint = null)
        {
            var startPos = arCamera.transform.position + arCamera.transform.forward * 0.5f + Vector3.down * 0.3f;
            var box = Instantiate(boxPrefab, startPos, Quaternion.identity);

            var elapsed = 0.0f;

            while (elapsed < boxFlyDuration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / boxFlyDuration;
                t = t * t * (3f - 2f * t);

                if (swipedPoint.HasValue)
                {
                    var a = Vector3.Lerp(startPos, swipedPoint.Value, t);
                    var b = Vector3.Lerp(swipedPoint.Value, target, t);
                    box.transform.position = Vector3.Lerp(a, b, t);
                }
                else
                {
                    box.transform.position = Vector3.Lerp(startPos, target, t);
                }

                var scale = Mathf.Lerp(0.15f, 0.25f, t);
                box.transform.localScale = Vector3.one * scale;

                yield return null;
            }

            Destroy(box);
        }

        private IEnumerator FloatTokenAway(GameObject token)
        {
            var startPos = token.transform.position;
            var failPos = startPos + Vector3.up * 5f;
            var startScale = token.transform.localScale;

            var elapsed = 0f;

            while (elapsed < failDuration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / failDuration;
                token.transform.position = Vector3.Lerp(startPos, failPos, t);
                token.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

                yield return null;
            }
        }
    }
}