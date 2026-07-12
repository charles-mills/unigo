using Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapViewStartup : MonoBehaviour
{
    private void Start()
    {
        AudioManager.Instance?.PlayMusic(AudioManager.Instance.mapMusic);
        SceneManager.LoadScene("MapInterface", LoadSceneMode.Additive);
    }
}