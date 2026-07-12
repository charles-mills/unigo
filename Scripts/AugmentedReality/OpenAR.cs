using Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Display
{
    public class OpenAR : MonoBehaviour
    {
        public string tag;
        public string arViewScene = "ARView";

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            Debug.Log("OpenAR script started.");
            //WaitForSeconds wait = new WaitForSeconds(5);
            //EditorSceneManager.LoadSceneAsync(sceneToOpenIndex);
        }

        // Update is called once per frame
        private void Update()
        {

            if(StatsUi.isStatsUiOpen) return;

            if (EventSystem.current != null)
            {
                if (EventSystem.current.IsPointerOverGameObject()) return; 
                if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return;
            }


            if (Input.touchCount <= 0 || Input.GetTouch(0).phase != TouchPhase.Began) return;

            var ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;

            Debug.Log("Screen touched");

            if (!Physics.Raycast(ray, out hit)) return;

            Debug.Log("Raycast successful");
            Debug.Log("Raycast hit: " + hit.transform.name);

            if (!hit.transform.CompareTag(tag)) return;

            Debug.Log("Touched AR Object with tag: ");

            var identity = hit.transform.GetComponent<TokenIdentity>();

            if (identity == null) return;

            TokenIdentity.SelectedBrand = identity.brand;

            SceneManager.LoadSceneAsync(arViewScene, LoadSceneMode.Single);
        }
    }
}