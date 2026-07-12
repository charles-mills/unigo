using UnityEngine;

namespace Gameplay
{
    public class CastEffects : MonoBehaviour
    {
        [Header("Cast Effects")] [SerializeField]
        private GameObject[] castEffectPrefabs;

        [Header("Impact Effects")] [SerializeField]
        private GameObject[] impactEffectPrefabs;

        [Header("Heavy Impact Effects")] [SerializeField]
        private GameObject[] heavyImpactEffectPrefabs;

        [Header("Settings")] [SerializeField] private Vector3 effectOffset = new(0, 1.0f, 0.5f);

        /// <summary>
        /// Currently unused (need to find some nice effects to use with it)
        /// </summary>
        /// <param name="attacker"></param>
        public void PlayCastEffect(Transform attacker)
        {
            if (castEffectPrefabs == null || castEffectPrefabs.Length == 0) return;

            var prefab = castEffectPrefabs[Random.Range(0, castEffectPrefabs.Length)];
            var spawnPos = attacker.position + attacker.TransformDirection(effectOffset);
            var effect = Instantiate(prefab, spawnPos, attacker.rotation);
            Destroy(effect, 1.0f);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="defender"></param>
        /// <param name="damage"></param>
        public void PlayImpactEffect(Transform defender, int damage)
        {
            var options = damage >= 4 ? heavyImpactEffectPrefabs : impactEffectPrefabs;

            if (options == null || options.Length == 0) return;

            var prefab = options[Random.Range(0, options.Length)];
            var spawnPos = defender.position + effectOffset;
            var effect = Instantiate(prefab, spawnPos, Quaternion.identity);
            Destroy(effect, 1.0f);
        }
    }
}