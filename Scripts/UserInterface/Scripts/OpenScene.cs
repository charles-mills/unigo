using UnityEngine;
using UnityEngine.SceneManagement;

namespace Display
{
    public class OpenScene : MonoBehaviour
    {
        public void Open(string sceneName)
        {
            SceneManager.LoadSceneAsync(sceneName);
        }

        public void OpenSingle(string sceneName)
        {
            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        }
    }
}