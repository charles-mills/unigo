using UnityEngine;

namespace Gameplay
{
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;

        [Header("Sources")]
        public AudioSource musicSource;

        public AudioSource sfxSource;

        [Header("Music")]
        public AudioClip menuMusic;

        public AudioClip mapMusic;
        public AudioClip pvpMusic;

        [Header("SFX")]
        public AudioClip swipeThrow;

        public AudioClip catchSuccess;
        public AudioClip catchFail;
        public AudioClip tapped;
        public AudioClip diceRoll;

        public static AudioManager Instance
        {
            get
            {
                if (_instance != null) return _instance;
                var prefab = Resources.Load<GameObject>("Prefabs/AudioManager");
                if (prefab != null) Instantiate(prefab);

                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PlaySfx(AudioClip clip)
        {
            if (clip != null) sfxSource.PlayOneShot(clip);
        }

        public void PlayMusic(AudioClip clip)
        {
            if (musicSource.clip == clip) return;

            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }

        public void StopMusic()
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Stop();
            }
        }
    }
}