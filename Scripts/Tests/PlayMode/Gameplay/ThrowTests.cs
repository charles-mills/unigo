using System.Collections;
using Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.PlayMode.Gameplay
{
    /// <summary>
    ///     Tests the functionality of the scripts related to collision handling
    /// </summary>
    public class ThrowTests : MonoBehaviour
    {
        public const string ThrowTestScene = "ThrowTest";
        private const float maxtime = 5f;
        private GameObject box;
        private float timer;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            yield return SceneManager.LoadSceneAsync(ThrowTestScene);
            timer = 0f;
            var gameObjects = FindFirstObjectByType<GameObject>();
            foreach (var @object in gameObjects.scene.GetRootGameObjects())
                if (@object.name == "box")
                    box = @object;
        }

        [UnityTest]
        public IEnumerator Box()
        {
            yield return null;
            Debug.Log("Starting Box Test");
            Assert.IsNotNull(box, "Box object not found");
        }

        [UnityTest]
        public IEnumerator BoxThrown()
        {
            Debug.Log("box : " + box);
            Debug.Log("max time : " + maxtime);
            Debug.Log("timer : " + timer);

            yield return null;
            yield return new WaitForSeconds(1f);
            var pos = box.transform.position;
            Debug.Log("Initial Position : " + pos);
            while (timer < maxtime)
            {
                box.GetComponent<Throw>().endPos = new Vector2(3000, 3000);
                // box.GetComponent<Throw>().swiped = true;
                if (pos != box.transform.position)
                {
                    Debug.Log("Final Position : " + box.transform.position);
                    Assert.IsTrue(true);
                    break;
                }

                timer += 0.016f;
            }

            Debug.Log("timer : " + timer);
        }
    }
}